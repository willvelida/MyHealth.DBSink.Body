﻿using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using MyHealth.DBSink.Body.Functions;
using MyHealth.DBSink.Body.Service.Interfaces;
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
        private Mock<IBodyService> _mockBodyService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;

        private CreateWeightDocument _func;

        public CreateWeightDocumentShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration.Setup(x => x["ServiceBusConnectionString"]).Returns("ServiceBusConnectionString");
            _mockBodyService = new Mock<IBodyService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();

            _func = new CreateWeightDocument(
                _mockConfiguration.Object,
                _mockBodyService.Object,
                _mockServiceBusHelpers.Object);
        }

        [Fact]
        public async Task AddActivityDocumentSuccessfully()
        {
            // Arrange
            var fixture = new Fixture();
            var testWeight = fixture.Create<mdl.Weight>();
            var testWeightEnvelope = fixture.Create<mdl.WeightEnvelope>();
            var testActivityDocumentString = JsonConvert.SerializeObject(testWeight);

            _mockBodyService.Setup(x => x.MapWeightToWeightEnvelope(testWeight)).Returns(testWeightEnvelope);
            _mockBodyService.Setup(x => x.AddWeightDocument(It.IsAny<mdl.WeightEnvelope>())).Returns(Task.CompletedTask);

            // Act
            await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockBodyService.Verify(x => x.MapWeightToWeightEnvelope(It.IsAny<mdl.Weight>()), Times.Once);
            _mockBodyService.Verify(x => x.AddWeightDocument(It.IsAny<mdl.WeightEnvelope>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task CatchAndLogExceptionWhenWeightEnvelopeMapperThrowsException()
        {
            // Arrange
            var fixture = new Fixture();
            var testWeight = fixture.Create<mdl.Weight>();
            var testActivityDocumentString = JsonConvert.SerializeObject(testWeight);

            _mockBodyService.Setup(x => x.MapWeightToWeightEnvelope(It.IsAny<mdl.Weight>())).Throws<Exception>();

            // Act
            Func<Task> responseAction = async () => await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockBodyService.Verify(x => x.MapWeightToWeightEnvelope(It.IsAny<mdl.Weight>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task CatchAndLogErrorWhenAddActivityDocumentThrowsException()
        {
            // Arrange
            var fixture = new Fixture();
            var testWeight = fixture.Create<mdl.Weight>();
            var testWeightEnvelope = fixture.Create<mdl.WeightEnvelope>();
            testWeight.Date = "2021-08-28";
            var testActivityDocumentString = JsonConvert.SerializeObject(testWeight);

            _mockBodyService.Setup(x => x.MapWeightToWeightEnvelope(testWeight)).Returns(testWeightEnvelope);
            _mockBodyService.Setup(x => x.AddWeightDocument(It.IsAny<mdl.WeightEnvelope>())).ThrowsAsync(It.IsAny<Exception>());

            // Act
            Func<Task> responseAction = async () => await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockBodyService.Verify(x => x.AddWeightDocument(It.IsAny<mdl.WeightEnvelope>()), Times.Never);
            await responseAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
