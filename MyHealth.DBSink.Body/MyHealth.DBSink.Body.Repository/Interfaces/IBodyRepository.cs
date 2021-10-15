using MyHealth.Common.Models;
using System.Threading.Tasks;

namespace MyHealth.DBSink.Body.Repository.Interfaces
{
    public interface IBodyRepository
    {
        Task CreateWeight(WeightEnvelope weightEnvelope);
    }
}
