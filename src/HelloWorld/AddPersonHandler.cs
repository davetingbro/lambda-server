using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using HelloWorld.Interfaces;
using Newtonsoft.Json;

namespace HelloWorld
{
    public class AddPersonHandler
    {
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;

        public AddPersonHandler()
        {
            var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
            _dbHandler = new DynamoDbHandler(dbContext);
            _logger = new LambdaFnLogger();
        }

        public AddPersonHandler(IDbHandler dbHandler, ILogger logger)
        {
            _dbHandler = dbHandler;
            _logger = logger;
        }

        public async Task<APIGatewayProxyResponse> AddPerson(APIGatewayProxyRequest request)
        {
            _logger.Log($"API GATEWAY REQUEST: {JsonConvert.SerializeObject(request)}");
            try
            {
                var isRequestValid = Validator.ValidateRequest(request.Body);
                return isRequestValid ? await CreateResponse(request) : CreateBadRequestResponse();
            }
            catch (Exception e)
            {
                var response = DefaultServerResponse.CreateServerErrorResponse();
                _logger.Log($"API GATEWAY RESPONSE: {JsonConvert.SerializeObject(response)}");
                _logger.Log(e.ToString()); 
                return response;
            }
        }

        private async Task<APIGatewayProxyResponse> CreateResponse(APIGatewayProxyRequest request)
        {
            var requestBody = request.Body;
            var id = Guid.NewGuid().ToString();
            await _dbHandler.AddPersonAsync(id, requestBody);
            var response = new APIGatewayProxyResponse
            {
                Body = requestBody,
                StatusCode = 200,
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"}, 
                    {"Location", $"{request.Path}/{id}"},
                }
            };
            _logger.Log($"API GATEWAY RESPONSE: {JsonConvert.SerializeObject(response)}");
            return response;
        }

        private APIGatewayProxyResponse CreateBadRequestResponse()
        {
            var response = new APIGatewayProxyResponse
                {StatusCode = 400, Body = "Invalid request - name must be between 0 and 30 characters"};
            _logger.Log($"API GATEWAY RESPONSE: {JsonConvert.SerializeObject(response)}");
            return response;
        }
    }
}