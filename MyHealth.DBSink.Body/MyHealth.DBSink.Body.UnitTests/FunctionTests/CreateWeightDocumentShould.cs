using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using MyHealth.DBSink.Body.Functions;
using MyHealth.DBSink.Body.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.UnitTests.FunctionTests
{
    public class CreateWeightDocumentShould
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IBodyDbService> _mockBodyDbService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;

        private CreateWeightDocument _func;

        public CreateWeightDocumentShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration.Setup(x => x["ServiceBusConnectionString"]).Returns("ServiceBusConnectionString");
            _mockBodyDbService = new Mock<IBodyDbService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();

            _func = new CreateWeightDocument(
                _mockConfiguration.Object,
                _mockBodyDbService.Object,
                _mockServiceBusHelpers.Object);
        }

        [Fact]
        public async Task AddActivityDocumentSuccessfully()
        {
            // Arrange
            var testWeightEnvelope = new mdl.WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Weight = new mdl.Weight
                {
                    Date = "2020-12-31"
                },
                DocumentType = "Test"
            };

            var testActivityDocumentString = JsonConvert.SerializeObject(testWeightEnvelope);

            _mockBodyDbService.Setup(x => x.AddWeightDocument(It.IsAny<mdl.Weight>())).Returns(Task.CompletedTask);

            // Act
            await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockBodyDbService.Verify(x => x.AddWeightDocument(It.IsAny<mdl.Weight>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task CatchAndLogErrorWhenAddActivityDocumentThrowsException()
        {
            // Arrange
            var testWeightEnvelope = new mdl.WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Weight = new mdl.Weight
                {
                    Date = "2020-12-31"
                },
                DocumentType = "Test"
            };

            var testActivityDocumentString = JsonConvert.SerializeObject(testWeightEnvelope);

            _mockBodyDbService.Setup(x => x.AddWeightDocument(It.IsAny<mdl.Weight>())).ThrowsAsync(It.IsAny<Exception>());

            // Act
            Func<Task> responseAction = async () => await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockBodyDbService.Verify(x => x.AddWeightDocument(It.IsAny<mdl.Weight>()), Times.Never);
            await responseAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
