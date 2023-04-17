using System.Speech.Recognition;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using AI_Companion;

namespace SpeechRecognitionEngine
{
    
    public class SRE
    {
         // Speech Recognition Code
        static string speechKey = ""; //Find on https://portal.azure.com/#home
        static string speechRegion = ""; //Also find on https://portal.azure.com/#home
        public static Queue<string> transcript = new Queue<string>();
        static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: {speechRecognitionResult.Text}");
                    transcript.Enqueue(speechRecognitionResult.Text);
                    break;
                case ResultReason.NoMatch:
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
        }

        public async Task SpeechRecognition()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(speechConfig, audioConfig);

            var phraseList = PhraseListGrammar.FromRecognizer(speechRecognizer);
            phraseList.AddPhrase($"{Program.charName}");
            phraseList.AddPhrase("Tarkan");
            phraseList.AddPhrase("Tarkan Mete");
            phraseList.AddPhrase($"{Program.charName} End Program");
            phraseList.AddPhrase($"{Program.charName} Print Transcript");
            phraseList.AddPhrase($"{Program.charName} Clear Transcript");

            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            OutputSpeechRecognitionResult(speechRecognitionResult);
        }        
    }
}
