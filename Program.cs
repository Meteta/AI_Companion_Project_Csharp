using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI_API;
using SpeechRecognitionEngine;
using static SpeechRecognitionEngine.SRE;
using SpeechSynthesisEngine;
using static SpeechSynthesisEngine.SSE;

/**
TODO:
https://liuhongbo.medium.com/how-to-use-chatgpt-api-in-c-d9133a3b8ef9
https://www.nuget.org/packages/OpenAI/
1. Set up AI parameters on load
2. Speech Recognition Software (DONE)
3. Search Text for Commands (DONE)
4. Send text to ChatGPT (or other AI) and get response 
5. AI Voice Synthasizer to read response.
6. Discord Integration.

TODO: Reset Keys on Public Release
dotnet publish --output "..\AI_Companion_Project_Publish\" --runtime win-x64 --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true

ISSUES:
1. Limited Lifespan
    a. Only a certain amount of messages can go to ChatGPT before it exits out due to how big the conversation is.
    b, Resolved by starting a new conversation every 100 lines. (May need to figure out a way to change that to be based on bytes)
**/
namespace AI_Companion
{
    class Program
    {
        static readonly string promptFile = @"promt.txt";
        public static string userName = "Crew";
        public static string charName = "CELESTE";
        public static string systemMessage;
        public static bool runProgram = true;
        public static bool speaking = false;
        public static bool listenWhileSpeaking = false;
        public static bool shouldRespond = false;
        public static string lastResponse;
        static string OPENAI_API_KEY = "";
        public static string speechKey = ""; //Find on https://portal.azure.com/#home
        public static string speechRegion = "australiaeast"; //Also find on https://portal.azure.com/#home
        public static OpenAIAPI api;
        public static OpenAI_API.Chat.Conversation chat;
        async static Task Main()
        {
            //I Could use environment variables to set this, however as this is a personal project, I dont think I need to go that far. 
            if(String.IsNullOrWhiteSpace(OPENAI_API_KEY) || String.IsNullOrWhiteSpace(speechKey)){
                Console.WriteLine("Insert your OPENAI_API_KEY");
                OPENAI_API_KEY = TextInput();
                Console.Clear();
                Console.WriteLine("Insert your SPEECH_KEY");
                speechKey = TextInput();
                Console.Clear();
            }
            InitializeOpenAI();

            SRE speechRecog = new SRE();
            SSE speechSynth = new SSE();

            Console.WriteLine("Use Default Settings? [Y]es/[N]o?");
            if(!BoolInput()){
                Console.WriteLine("Overwrite SPEECH_REGION (australiaeast)? [Y]es/[N]o");
                if (BoolInput()){
                    Console.WriteLine("Insert your SPEECH_REGION");
                }
                speechRegion = TextInput();
                // Naming your companion
                Console.WriteLine("SYSTEM: Input a Name for your Companion");
                charName = TextInput().ToUpper();
                Console.WriteLine("SYSTEM: Input a User Name for your companion to address you by.");
                userName = TextInput();
                // Prompt Warning
                Console.WriteLine("SYSTEM: Edit the prompt.txt file for more specific responses");
                Console.WriteLine("SYSTEM: Would you like to read the textfile?");
                if (BoolInput()) {
                    systemMessage = File.ReadAllText(promptFile);
                } else {
                    Console.WriteLine("Please input a prompt. (Think of this as the personality behind your companion. E.G: You are a AI Language learning)");
                    systemMessage = TextInput();
                }
                Console.WriteLine($"SYSTEM: Would you like {charName} to listen to you while they are speaking");
                Console.WriteLine($"SYSTEM: This will in various cases result in {charName} hearing their own voice, which can cause issues. This is issue is dependant on your sound set up");
                listenWhileSpeaking = BoolInput();
            } else {
                systemMessage = File.ReadAllText(promptFile);
            }
            InitPrompt();
            
            // Set up complete, run program
            Console.Clear();
            Console.WriteLine($"SYSTEM: Identity Established {charName}");
            Console.WriteLine($"{charName}: How may I be of Assitance?");
            while (runProgram){
                await speechRecog.SpeechRecognition();
                searchForChoices();
                if(shouldRespond) {
                    await textToAI();
                    if (listenWhileSpeaking){ // If this setting is set, the program will listen while speaking, results in less accurate transcripts
                        speechSynth.SpeechSynthesis();
                    } else { // Else, wait for AI to finish speaking before continuing
                        await speechSynth.SpeechSynthesis();
                    }
                }
            }
            while(speaking){ /** Keeps the program open until speech has finished.**/ }
        }

        static void InitializeOpenAI(){
            api = new OpenAIAPI(OPENAI_API_KEY);
            chat = api.Chat.CreateConversation();
        }

        static void InitPrompt(){
            chat.AppendSystemMessage(systemMessage);
            chat.AppendSystemMessage($"Your name is {charName}. You will respond in 2 to 3 sentences at a time. When you hear End Program, say goodbye. You are speaking with {userName}");
            chat.AppendSystemMessage("Indicate emotion once at the start of each sentence encompassed with {}. Choose one emotion from your available emotions which are: affectionate, angry, assistant, calm, chat, cheerful, depressed, disgruntled, embarrassed, empathetic, envious, excited, fearful, friendly, gentle, hopeful, lyrical, sad, serious, shouting, whispering, terrified, unfriendly");
        }

        static string TextInput(){
            string temp = null;
            while(string.IsNullOrWhiteSpace(temp)){
                temp = Console.ReadLine().ToString();
                if (string.IsNullOrWhiteSpace(temp)){
                    Console.WriteLine("Please enter a valid text value");
                }
            }
            return temp;
        }
        static bool BoolInput(){
            string temp = null;
            while(string.IsNullOrWhiteSpace(temp) || !(temp.ToUpper() == "Y" || temp.ToUpper() == "N")){
                temp = Console.ReadLine().ToString();
                if (string.IsNullOrWhiteSpace(temp) || !(temp.ToUpper() == "Y" || temp.ToUpper() == "N")){
                    Console.WriteLine("Please enter a valid text value");
                }
            }
            if(temp.ToUpper() == "Y") return true;
            else return false;
        }

        // TEXT OPERATIONS
        static bool compareIgnoreCase(string text, string compare){
            return text.Contains(compare, System.StringComparison.CurrentCultureIgnoreCase);
        }
        public static void searchForChoices(){
            shouldRespond = false;
            string currentString = transcript.Last<string>();
            if(transcript.Count > 10) transcript.Dequeue();
            // Does the AI hear their name during the conversation?
            if(compareIgnoreCase(currentString, Program.charName)){
                shouldRespond = true;
                // Console.WriteLine($"{Program.charName}: Recognised Name.");

                // Does the latest transcript contain a choice
                if (compareIgnoreCase(currentString, "End Program")){
                    Program.runProgram = false;
                } else if (compareIgnoreCase(currentString, "Print Transcript")){
                    shouldRespond = false;
                    int line = 0;
                    foreach( string text in transcript){
                        line++;
                        Console.WriteLine($"SLOT {line}: {text}");
                    }
                } else if (compareIgnoreCase(currentString, "Clear Transcript")){
                    transcript.Clear();
                    shouldRespond = false;
                } else if (compareIgnoreCase(currentString, "List Responses")){
                    int line = 0;
                    foreach(OpenAI_API.Chat.ChatMessage text in chat.Messages){
                        line++;
                        Console.WriteLine($"SYSTEM AICHATMESSAGE {line}: {text.Content}");
                    }
                    shouldRespond = false;
                };

            }
        }

        static async Task textToAI(){
            try{
                // To deal with limited space issues, reset the chat every 100 messages
                if (chat.Messages.Count > 100){
                    chat = api.Chat.CreateConversation();
                    InitPrompt();
                }

                // Add transcript to user input, then clear transcript
                foreach(string text in transcript){
                    chat.AppendUserInputWithName(userName, text);
                }
                // chat.AppendUserInputWithName(userName, transcript.Last<string>());
                transcript.Clear();

                lastResponse = await chat.GetResponseFromChatbotAsync();
                Console.WriteLine($"{charName} - RESPONSE : {lastResponse} ");
                shouldRespond = false;
            }
            catch{
                Console.WriteLine("SYSTEM: Caught error attempting to speak when there were no words.");
            }
        }
    }
}