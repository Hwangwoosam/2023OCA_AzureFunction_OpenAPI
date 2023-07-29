using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace week3
{
    public class ChatGPTCaller
    {
        private readonly ILogger _logger;

        public ChatGPTCaller(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ChatGPTCaller>();
        }

        [Function("ChatGPTCaller")]
        [OpenApiOperation(operationId: nameof(ChatGPTCaller.Run), tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "The request body")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "completions")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var prompt = req.ReadAsString();

            var endpoint = Environment.GetEnvironmentVariable("AOAI_EndPoint");
            var apiKey = Environment.GetEnvironmentVariable("AOAI_ApiKey");
            var Model = Environment.GetEnvironmentVariable("AOAI_DeploymentId");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization",$"Bearer {apiKey}");

            var request = new
            {   
                model = Model,
                messages = new []{
                    new {role = "system", content =  "You are a helpful assistant. You are very good at summarizing the given text into 2-3 bullet points."},
                    new {role = "user", content = prompt}
                },
                temperature = 0.7
            };         

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var GPTresponse = await httpClient.PostAsync(endpoint,content);
            var responseBody = await GPTresponse.Content.ReadAsStringAsync();
            dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseBody);
            string result = item.choices[0].message.content;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(result);

            return response;
        }
    }
}
