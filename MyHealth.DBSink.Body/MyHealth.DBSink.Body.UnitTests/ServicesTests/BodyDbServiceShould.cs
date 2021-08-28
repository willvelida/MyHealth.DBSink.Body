using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.DBSink.Body.Services;
using MyHealth.DBSink.Body.UnitTests.TestHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.UnitTests.ServicesTests
{
    public class BodyDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private BodyDbService _sut;

        public BodyDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new BodyDbService(
                _mockCosmosClient.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task AddNutritionDocumentWhenCreateItemAsyncIsCalled()
        {
            // Arrange
            mdl.WeightEnvelope testWeightDocument = new mdl.WeightEnvelope
            {
                Date = DateTime.Parse("2021-05-11")
            };


            _mockContainer.SetupCreateItemAsync<mdl.WeightEnvelope>();

            // Act
            Func<Task> serviceAction = async () => await _sut.AddWeightDocument(testWeightDocument);

            // Assert
            await serviceAction.Should().NotThrowAsync<Exception>();
            _mockContainer.Verify(x => x.CreateItemAsync(
                It.IsAny<mdl.WeightEnvelope>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ThrowExceptionWhenCreateItemAsyncCallFails()
        {
            // Arrange
            mdl.WeightEnvelope testWeightDocument = new mdl.WeightEnvelope
            {
                Date = DateTime.Parse("2021-05-11")
            };


            _mockContainer.SetupCreateItemAsync<mdl.Weight>();
            _mockContainer.Setup(x => x.CreateItemAsync(
                It.IsAny<mdl.WeightEnvelope>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> serviceAction = async () => await _sut.AddWeightDocument(testWeightDocument);

            // Assert
            await serviceAction.Should().ThrowAsync<Exception>();
        }
    }
}
