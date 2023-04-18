using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using AI_Companion;

namespace SpeechRecognitionEngine
{
    
    public class SRE
    {
         // Speech Recognition Code
        public static Queue<string> transcript = new Queue<string>();
        static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"{Program.userName.ToUpper()}: {speechRecognitionResult.Text}");
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
            var speechConfig = SpeechConfig.FromSubscription(Program.speechKey, Program.speechRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";
            
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            // var audioStream = AudioInputStream.CreatePushStream();
            // using var audioConfig = AudioConfig.FromStreamInput(audioStream);
            // using var micConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(speechConfig, audioConfig);

            var phraseList = PhraseListGrammar.FromRecognizer(speechRecognizer);
            phraseList.AddPhrase($"{Program.charName}");
            phraseList.AddPhrase("Tarkan");
            phraseList.AddPhrase("Tarkan Mete");
            phraseList.AddPhrase($"{Program.charName} End Program");
            phraseList.AddPhrase($"{Program.charName} Print Transcript");
            phraseList.AddPhrase($"{Program.charName} Clear Transcript");
            phraseList.AddPhrase($"{Program.charName} List Responses");

            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            OutputSpeechRecognitionResult(speechRecognitionResult);
        }        
    }
}

