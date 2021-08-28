using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using MyHealth.DBSink.Body.Mappers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.UnitTests.MapperTests
{
    public class WeightEnvelopeMapperShould
    {
        private WeightEnvelopeMapper _sut;

        public WeightEnvelopeMapperShould()
        {
            _sut = new WeightEnvelopeMapper();
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
                expectedWeightEnvelope.Date.Should().Be(DateTime.Parse(testWeight.Date));
            }
        }
    }
}
