using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Translation.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLPLAbb1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Setup for QnA
            Uri endpoint = new Uri("https://languageservicelabb1.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("be985852ae934efa8bc77bef50a82c64");
            string projectName = "DogFAQ";
            string deploymentName = "production";

            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            // Setup for Translator using regional endpoint
            Uri translateEndpoint = new Uri("https://labb1translate.cognitiveservices.azure.com/");
            string translateKey = "8e6ad8f0ecb2411ba753259dab328603";
            string translateRegion = "westeurope";

            AzureKeyCredential translateCredential = new AzureKeyCredential(translateKey);
            TextTranslationClient translatorClient = new TextTranslationClient(translateCredential, translateRegion);

            Console.WriteLine("Ask a question, type exit to quit");
            while (true)
            {
                Console.WriteLine("Q: ");
                string question = Console.ReadLine();
                if (question.ToLower() == "exit")
                {
                    break;
                }
                try
                {
                    // Translate the question to English
                    string translatedQuestion = await TranslateText(translatorClient, question, "en");

                    // Send the translated question to QnA
                    Response<AnswersResult> response = client.GetAnswers(translatedQuestion, project);

                    // Translate the answer back to the original language
                    string originalLanguage = await DetectLanguage(translatorClient, question);
                    string translatedAnswer = await TranslateText(translatorClient, response.Value.Answers[0].Answer, originalLanguage);

                    Console.WriteLine($"Q: {question}");
                    Console.WriteLine($"A: {translatedAnswer}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                }
            }
        }

        static async Task<string> TranslateText(TextTranslationClient client, string text, string toLanguage)
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await client.TranslateAsync(toLanguage, text).ConfigureAwait(false);
            return response.Value.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? text;
        }

        static async Task<string> DetectLanguage(TextTranslationClient client, string text)
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await client.TranslateAsync("en", text).ConfigureAwait(false);
            return response.Value.FirstOrDefault()?.DetectedLanguage?.Language ?? "en";
        }
    }
}
