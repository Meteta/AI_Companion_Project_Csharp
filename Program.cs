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
            // charName = TextInput();
            // Prompt Warning
            Console.WriteLine("Edit the prompt.txt file for more specific responses");
            systemMessage = File.ReadAllText(promptFile);
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
            return temp.ToUpper();
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
                        Console.WriteLine($"{line}: {text}");
                    }
                } else if (compareIgnoreCase(currentString, "Celeste Clear Transcript")){
                    transcript.Clear();
                    shouldRespond = false;
                };

            }
        }

        static async Task textToAI(){
            
            foreach( string text in transcript ){
                chat.AppendUserInput(text);
            }
            // chat.AppendUserInput(transcript.Last<string>());
            string response = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine($"{charName} - RESPONSE : {response} ");
            shouldRespond = false;
        }
    }
}