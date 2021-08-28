using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Mappers
{
    public class WeightEnvelopeMapper : IWeightEnvelopeMapper
    {
        public WeightEnvelope MapWeightToWeightEnvelope(Weight weight)
        {
            if (weight == null)
                throw new Exception("No Weight Document to Map!");

            mdl.WeightEnvelope weightEnvelope = new WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Weight = weight,
                DocumentType = "Weight",
                Date = DateTime.ParseExact(weight.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return weightEnvelope;
        }
    }
}
