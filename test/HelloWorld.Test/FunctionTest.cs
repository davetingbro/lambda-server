using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

namespace HelloWorld.Tests
{
  public class FunctionTest
  {
      [Fact]
    public void TestHelloWorldFunctionHandler()
    {
            var request = new APIGatewayProxyRequest();
            var context = new TestLambdaContext();
            var message =
                $"Hello David - the time on the server is {DateTime.Now.ToShortTimeString()} on {DateTime.Now.ToLongDateString()}";
            var body = new Dictionary<string, string>
            {
                { "message", message },
            };

            var expectedResponse = new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            var function = new Function();
            var response = function.HelloHandler(request, context);

            Assert.Equal(expectedResponse.Body, response.Body);
            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }
  }
}