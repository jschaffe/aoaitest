using Azure;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;

namespace aoaifunctest.Helpers
{
    public class OpenAIHelper
    {
        public OpenAIHelper()
        {

        }
        //static string key = "9f4d0240980a416c9ac4996342e7bc43";
        //static string endpoint = "https://jschaffeopenai.openai.azure.com/";
        static string key = "bced092b215a4768bcabe21028e00719";
        static string endpoint = "https://openaikesteph.openai.azure.com/";
        string completion = "";

        public string GetPromptResponse(string prompt)
        {
            OpenAIClient client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            if (prompt != "")
            {
                try
                {
                    CompletionsOptions completionsOptions = new CompletionsOptions
                    {
                        DeploymentName = "davinci-summary",
                        //DeploymentName = "gpt35turbo",
                        Prompts = { prompt },
                        MaxTokens = 1000
                    };
                    Response<Completions> completionsResponse = client.GetCompletions(completionsOptions);
                    completion = completionsResponse.Value.Choices[0].Text;
                }
                catch (Exception ex)
                {
                    completion = ex.Message;
                }
            }
            return completion;
        }
    }
}
