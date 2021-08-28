using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Mappers
{
    public interface IWeightEnvelopeMapper
    {
        mdl.WeightEnvelope MapWeightToWeightEnvelope(mdl.Weight weight);
    }
}
