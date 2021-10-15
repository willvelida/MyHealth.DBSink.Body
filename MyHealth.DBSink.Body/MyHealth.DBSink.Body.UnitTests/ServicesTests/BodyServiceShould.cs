using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using MyHealth.DBSink.Body.Repository.Interfaces;
using MyHealth.DBSink.Body.Service;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.UnitTests.ServicesTests
{
    public class BodyServiceShould
    {
        private Mock<IBodyRepository> _mockBodyRepository;

        private BodyService _sut;

        public BodyServiceShould()
        {
            _mockBodyRepository = new Mock<IBodyRepository>();

            _sut = new BodyService(
                _mockBodyRepository.Object);
        }

        [Fact]
        public async Task AddNutritionDocumentWhenCreateItemAsyncIsCalled()
        {
            // Arrange
            mdl.WeightEnvelope testWeightDocument = new mdl.WeightEnvelope
            {
                Date = "2021-05-11"
            };

            // Act
            Func<Task> serviceAction = async () => await _sut.AddWeightDocument(testWeightDocument);

            // Assert
            await serviceAction.Should().NotThrowAsync<Exception>();
        }

        [Fact]
        public async Task ThrowExceptionWhenCreateWeightCallFails()
        {
            // Arrange
            mdl.WeightEnvelope testWeightDocument = new mdl.WeightEnvelope
            {
                Date = "2021-05-11"
            };

            _mockBodyRepository.Setup(x => x.CreateWeight(It.IsAny<mdl.WeightEnvelope>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> serviceAction = async () => await _sut.AddWeightDocument(testWeightDocument);

            // Assert
            await serviceAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public void ThrowExceptionWhenIncomingWeightObjectIsNull()
        {
            Action weightEnvelopeMapperAction = () => _sut.MapWeightToWeightEnvelope(null);

            weightEnvelopeMapperAction.Should().Throw<Exception>().WithMessage("No Weight Document to Map!");
        }

        [Fact]
        public void MapWeightToWeightEnvelopeCorrectly()
        {
            var fixture = new Fixture();
            var testWeight = fixture.Create<mdl.Weight>();
            testWeight.Date = "2021-08-28";

            var expectedWeightEnvelope = _sut.MapWeightToWeightEnvelope(testWeight);

            using (new AssertionScope())
            {
                expectedWeightEnvelope.Should().BeOfType<mdl.WeightEnvelope>();
                expectedWeightEnvelope.Weight.Should().Be(testWeight);
                expectedWeightEnvelope.DocumentType.Should().Be("Weight");
                expectedWeightEnvelope.Date.Should().Be(testWeight.Date);
            }
        }
    }
}
