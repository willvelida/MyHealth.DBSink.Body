using MyHealth.Common.Models;
using System.Threading.Tasks;

namespace MyHealth.DBSink.Body.Service.Interfaces
{
    public interface IBodyService
    {
        Task AddWeightDocument(WeightEnvelope weightEnvelope);
        WeightEnvelope MapWeightToWeightEnvelope(Weight weight);
    }
}
