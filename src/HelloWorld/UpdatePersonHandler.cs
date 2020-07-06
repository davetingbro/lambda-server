using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HelloWorld.Interfaces;
using Newtonsoft.Json;

namespace HelloWorld
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatePersonHandler
    {
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;

        public UpdatePersonHandler()
        {
            var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
            _dbHandler = new DynamoDbHandler(dbContext);
            _logger = new LambdaFnLogger();
        }

        public UpdatePersonHandler(IDbHandler dbHandler, ILogger logger)
        {
            _dbHandler = dbHandler;
            _logger = logger;
        }

        public async Task<APIGatewayProxyResponse> UpdatePerson(APIGatewayProxyRequest request)
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
            var id = request.PathParameters["id"];
            var newName = request.Body;
            APIGatewayProxyResponse response;
            try
            {
                await _dbHandler.UpdatePersonAsync(id, newName);
                response = new APIGatewayProxyResponse { StatusCode = 301};
            }
            catch (NullReferenceException)
            {
                response = new APIGatewayProxyResponse { StatusCode = 404, Body = "Resource not found" };
            }
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