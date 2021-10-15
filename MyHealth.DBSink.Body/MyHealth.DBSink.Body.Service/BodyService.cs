using MyHealth.Common.Models;
using MyHealth.DBSink.Body.Repository.Interfaces;
using MyHealth.DBSink.Body.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyHealth.DBSink.Body.Service
{
    public class BodyService : IBodyService
    {
        private readonly IBodyRepository _bodyRepository;

        public BodyService(IBodyRepository bodyRepository)
        {
            _bodyRepository = bodyRepository;
        }

        public async Task AddWeightDocument(WeightEnvelope weightEnvelope)
        {
            try
            {
                await _bodyRepository.CreateWeight(weightEnvelope);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public WeightEnvelope MapWeightToWeightEnvelope(Weight weight)
        {
            if (weight == null)
                throw new Exception("No Weight Document to Map!");

            WeightEnvelope weightEnvelope = new WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Weight = weight,
                DocumentType = "Weight",
                Date = weight.Date
            };

            return weightEnvelope;
        }
    }
}
