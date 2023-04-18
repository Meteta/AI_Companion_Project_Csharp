using Microsoft.CognitiveServices.Speech;
using AI_Companion;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace SpeechSynthesisEngine
{
    class SSE 
    {
        static SpeechConfig speechConfig = SpeechConfig.FromSubscription(Program.speechKey, Program.speechRegion);
        static SpeechSynthesizer? speechSynthesizer = null;

        static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }
        static string ssmlVoiceGeneration(string text)
        {
            // Define the regular expression pattern to match text within curly braces {}
            string pattern1 = "{(.*?)}";
            Match match = Regex.Match(text, pattern1);
            string emotion = "";


            // If a match is found, extract the emotion text and remove it from the input string
            if (match.Success)
            {
                emotion = match.Groups[1].Value.ToLower();
                text = text.Replace(match.Value, "");
            }

            string pattern2 = "&";
            string replacement = "";
            string output = Regex.Replace(text, pattern2, replacement);

            // Construct the SSML string with the extracted emotion text
            string ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'><voice name='en-US-JennyNeural' style='{emotion}'>{output}</voice></speak>";

            return ssml;

        }
        
        

        public async Task SpeechSynthesis()
        {
            Program.endProgram = false;
            if(speechSynthesizer != null){
                await speechSynthesizer.StopSpeakingAsync();
                speechSynthesizer = null;
            }
            
            // The language of the voice that speaks.
            speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural"; 

            speechSynthesizer = new SpeechSynthesizer(speechConfig);
           
            // Get text from the AI Response.
            string text = Program.lastResponse;

            // var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
            ssmlVoiceGeneration(text);
            var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(ssmlVoiceGeneration(text));
            OutputSpeechSynthesisResult(speechSynthesisResult, text);
            Program.endProgram = true;
        }
    }
}