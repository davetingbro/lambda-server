using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using HelloWorld.Interfaces;
using Moq;
using Xunit;

namespace HelloWorld.Tests
{
    public class UpdatePersonHandlerTest
    {
        private readonly Mock<IDbHandler> _mockDbHandler;
        private readonly Mock<ILogger> _mockLogger;
        private readonly APIGatewayProxyRequest _updateRequest = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Body = "New_Name",
            PathParameters = new Dictionary<string, string>{ {"id", "1"} }
        };

        public UpdatePersonHandlerTest()
        {
            _mockDbHandler = new Mock<IDbHandler>();
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public async Task UpdatePerson_ShouldCallDbHandlerUpdatePersonAsyncOnce()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            await handler.UpdatePerson(_updateRequest);
            _mockDbHandler.Verify(db => db.UpdatePersonAsync("1", "New_Name"), Times.Once);
        }

        [Fact]
        public async Task UpdatePerson_ShouldLogReceivedRequest()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            await handler.UpdatePerson(_updateRequest);
            _mockLogger.Verify(logger => logger.Log(
                $"API Gateway request received - HttpMethod: {_updateRequest.HttpMethod}  Path: {_updateRequest.Path}"));
        }

        [Fact]
        public async Task UpdatePerson_ShouldLogResponseCreated_IfSuccessful()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var response = await handler.UpdatePerson(_updateRequest);
            _mockLogger
                .Verify(logger => logger.Log(
                        $"API Gateway response produced - StatusCode: {response.StatusCode}  Body: {response.Body}")
                    , Times.Once);
        }

        [Fact]
        public async Task UpdatePerson_ShouldReturnResponseStatusCode301_IfUpdateSuccessfully()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var response = await handler.UpdatePerson(_updateRequest);

            Assert.Equal(301, response.StatusCode);
        }
        
        [Fact]
        public async Task UpdatePerson_ShouldReturnResponseStatusCode400_IfRequestBodyLengthGreaterThan30()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var request = new APIGatewayProxyRequest
            {
                Body = "the_length_of_the_request_body_is_greater_than_30",
                PathParameters = new Dictionary<string, string>{ {"id", "1"} }
            };
            var response = await handler.UpdatePerson(request);
            Assert.Equal(400, response.StatusCode);
        }
        
        [Fact]
        public async Task UpdatePerson_ShouldLogBadRequestResponse_IfRequestBodyLengthGreaterThan30()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var request = new APIGatewayProxyRequest
            {
                Body = "the_length_of_the_request_body_is_greater_than_30",
                PathParameters = new Dictionary<string, string>{ {"id", "1"} }
            };
            var response = await handler.UpdatePerson(request);
            _mockLogger.Verify(logger => logger.Log(
                    $"API Gateway response produced - StatusCode: {response.StatusCode}  Body: {response.Body}"),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePerson_ShouldNotCallDbHandlerUpdatePersonAsync_IfRequestBodyLengthGreaterThan30()
        {
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var request = new APIGatewayProxyRequest
            {
                Body = "the_length_of_the_request_body_is_greater_than_30",
                PathParameters = new Dictionary<string, string>{ {"id", "1"} }
            };
            await handler.UpdatePerson(request);
            _mockDbHandler
                .Verify(db => db.UpdatePersonAsync("1", It.IsAny<string>()),
                    Times.Never);
        }

        [Fact]
        public async Task UpdatePerson_ShouldReturnResponseStatusCode404_IfNullReferenceExceptionIsCaught()
        {
            _mockDbHandler
                .Setup(db => db.UpdatePersonAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new NullReferenceException());
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            var response = await handler.UpdatePerson(_updateRequest);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePerson_ShouldLogResponse_IfExceptionThrown()
        {
            _mockDbHandler
                .Setup(db => db.UpdatePersonAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var handler = new UpdatePersonHandler(_mockDbHandler.Object, _mockLogger.Object);
            await Assert.ThrowsAsync<Exception>(async () => await handler.UpdatePerson(_updateRequest));
            _mockLogger.Verify(logger => logger.Log(
                $"API Gateway response produced - StatusCode: 500  Body: Internal server error"));
        }
    }
}