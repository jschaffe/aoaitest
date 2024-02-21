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
        //static string key = "bced092b215a4768bcabe21028e00719";
        //static string endpoint = "https://openaikesteph.openai.azure.com/";
        static string key = "872e38c195a74f5986d02c37ef10b85d";
        static string endpoint = "https://openai-rapid.openai.azure.us/";
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
                        //DeploymentName = "gpt-4-1106preview",
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

        public string GetChatPromptResponse(string prompt)
        {
            OpenAIClient client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            if (prompt != "")
            {
                try
                {
                    Response<ChatCompletions> responseWithoutStream = client.GetChatCompletions(
                        new ChatCompletionsOptions()
                        {
                            DeploymentName = "gpt-4-1106preview",
                            Messages =
                            {
                                new ChatRequestSystemMessage(@"You are an AI assistant that helps people find information."),
                                new ChatRequestUserMessage(prompt)
                            },
                            Temperature = (float)0.7,
                            MaxTokens = 1000,
                            NucleusSamplingFactor = (float)0.95,
                            FrequencyPenalty = 0,
                            PresencePenalty = 0,
                        });

                    ChatCompletions response = responseWithoutStream.Value;
                    completion = response.Choices[0].Message.Content.Replace("```json", "").Replace("```", "");
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
