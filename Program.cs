using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SpeechRecognitionEngine;
using static SpeechRecognitionEngine.SRE;

/**
TODO:
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
        async static Task Main()
        {
            SRE speech = new SRE();
            // Console.WriteLine("SYSTEM: Input a Name for your Companion");
            // charName = TextInput();
            Console.WriteLine($"SYSTEM: Identity Established {charName}");
            Console.WriteLine($"{charName}: How may I be of Assitance?");
            while (runProgram){
                await speech.SpeechRecognition();
                searchForChoices();
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
        public static bool runProgram = true;

        // TEXT OPERATIONS
        static bool compareIgnoreCase(string text, string compare){
            return text.Contains(compare, System.StringComparison.CurrentCultureIgnoreCase);
        }
        public static void searchForChoices(){
            string currentString = transcript.Last<string>();
            if(transcript.Count > 10) transcript.Dequeue();
            // Does the AI hear their name during the conversation?
            if(compareIgnoreCase(currentString, Program.charName)){
                Console.WriteLine($"{Program.charName}: Recognised Name.");

                // Does the latest transcript contain a choice
                if (compareIgnoreCase(currentString, "End Program")){
                    Program.runProgram = false;
                } else if (compareIgnoreCase(currentString, "Print Transcript")){
                    int line = 0;
                    foreach( string text in transcript){
                        line++;
                        Console.WriteLine($"{line}: {text}");
                    }
                } else if (compareIgnoreCase(currentString, "Celeste Clear Transcript")){
                    transcript.Clear();
                };

            }
        }
       
    }
}