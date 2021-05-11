using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Services
{
    public interface IBodyDbService
    {
        Task AddWeightDocument(mdl.Weight weight);
    }
}
