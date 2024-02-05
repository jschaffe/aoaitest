using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using aoaifunctest.Entities;
using aoaifunctest.Helpers;
using aoaifunctest.ResponseEntities;
using System.Net.Http;
using System.Net;

namespace aoaifunctest
{
    public static class aoaitest
    {
        private const string prompt = "Extract flight information from the Summary below as JSON: Include departure airport code as DepartureAirportCode, arrival airport code as ArrivalAirportCode and flight path as Path    Format flight path as XXX-XXX";
        private const string contentDelimiter = "###";

        [FunctionName("aoaitest")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AOAI Function Test is being executed...");

            var response = new SkillResponse();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic skillData = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation(requestBody);

            var skillData = JsonHelper.Deserialize<SkillData>(requestBody);
            OpenAIHelper openAI = new OpenAIHelper();
            foreach (var val in skillData.values)
            {
                try
                {
                    var returnValue = openAI.GetPromptResponse($"{prompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
                    var aoaiResponse = JsonHelper.Deserialize<AOAIResponse>(returnValue);
                    response.values.Add(new aoaifunctest.ResponseEntities.Values
                    {
                        recordId = val.recordId,
                        data = new ResponseEntities.Data
                        {
                            FlightData = "[" + returnValue + "]"
                            //FlightDepartureAirport = aoaiResponse.DepartureAirportCode,
                            //FlightArrivalAirport = aoaiResponse.ArrivalAirportCode,
                            //FlightPath = aoaiResponse.FlightPath
                        }
                    });
                }
                catch (Exception ex)
                {
                    var temp = new aoaifunctest.ResponseEntities.Values
                    {
                        recordId = val.recordId

                    };
                    temp.errors.Add(ex.Message);
                    response.values.Add(temp);
                }
            }

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse.Content = new StringContent(JsonHelper.Serializer<SkillResponse>(response), System.Text.Encoding.UTF8, "application/json");
            return httpResponse;
        }
    }
}
