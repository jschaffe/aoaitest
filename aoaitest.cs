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
        private const string prompt = "Extract flight information from the Summary below as JSON: Include departure airport code as DepartureAirportCode, arrival airport code as ArrivalAirportCode, flight path as Path, Airline as Carrier, flight number as Number, departure airport as DepartureAirport, arrival airport as ArrivalAirport, seat number as Seat    Format flight path as XXX-XXX; Provide full departure airport name; Provide full arrival airport name; If seat number is not found return null";
        private const string contentDelimiter = "###";

        [FunctionName("aoaitest")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AOAI Function Test is being executed...");

            var response = new SkillResponse();
            var returnValue = String.Empty;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic skillData = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation(requestBody);

            var skillData = JsonHelper.Deserialize<SkillData>(requestBody);
            foreach (var val in skillData.values)
            {
                try
                {
                    if (val.data.SourceName.Equals("PARIS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        OpenAIHelper openAI = new OpenAIHelper();
                        //returnValue = openAI.GetPromptResponse($"{prompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
                        returnValue = openAI.GetChatPromptResponse($"{prompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
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
                    else
                    {
                        response.values.Add(new aoaifunctest.ResponseEntities.Values
                        {
                            recordId = val.recordId,
                            data = new ResponseEntities.Data
                            {
                                FlightData = val.data.Flight
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    var temp = new aoaifunctest.ResponseEntities.Values
                    {
                        recordId = val.recordId,
                        data = new ResponseEntities.Data
                        {
                            FlightData = "[" + returnValue + "]"
                        }
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
