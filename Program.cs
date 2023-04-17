using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI_API;
using SpeechRecognitionEngine;
using static SpeechRecognitionEngine.SRE;

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
        async static Task Main()
        {
            SRE speech = new SRE();
            // Naming your companion
            // Console.WriteLine("SYSTEM: Input a Name for your Companion");
            // charName = TextInput().ToUpper();
            // Prompt Warning
            Console.WriteLine("Edit the prompt.txt file for more specific responses");
            Console.WriteLine("Would you like to read the textfile?");
            if (BoolInput()) {
                systemMessage = File.ReadAllText(promptFile);
            } else {
                Console.WriteLine("Please input a prompt. (Think of this as the personality behind your companion. E.G: You are a AI Language learning)");
                systemMessage = TextInput();
            }

            chat.AppendSystemMessage(systemMessage);
            chat.AppendSystemMessage($"Your name is {charName}. You will respond in 2 to 3 sentences at a time. When you hear End Program, say goodbye.");
            
            // Set up complete, run program
            Console.WriteLine($"SYSTEM: Identity Established {charName}");
            Console.WriteLine($"{charName}: How may I be of Assitance?");
            while (runProgram){
                await speech.SpeechRecognition();
                searchForChoices();
                if(shouldRespond) await textToAI();
            }
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

        public static string charName = "CELESTE";
        public static string systemMessage;
        public static bool runProgram = true;
        public static bool shouldRespond = false;
        static string OPENAI_API_KEY = "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";
        public static OpenAIAPI api = new OpenAIAPI(OPENAI_API_KEY);
        public static OpenAI_API.Chat.Conversation chat = api.Chat.CreateConversation();

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
            // To deal with limited space issues, reset the chat every 100 messages
            if (chat.Messages.Count > 100){
                chat = api.Chat.CreateConversation();
                chat.AppendSystemMessage(systemMessage);
                chat.AppendSystemMessage($"Your name is {charName}. You will respond in 2 to 3 sentences at a time. When you hear End Program, say goodbye.");
            }
            chat.AppendUserInput(transcript.Last<string>());

            string response = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine($"{charName} - RESPONSE : {response} ");
            shouldRespond = false;
        }
    }
}