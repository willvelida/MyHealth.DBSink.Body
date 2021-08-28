using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Body.Mappers;
using MyHealth.DBSink.Body.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Functions
{
    public class CreateWeightDocument
    {
        private readonly IConfiguration _configuration;
        private readonly IBodyDbService _bodyDbService;
        private readonly IWeightEnvelopeMapper _weightEnvelopeMapper;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateWeightDocument(
            IConfiguration configuration,
            IBodyDbService bodyDbService,
            IWeightEnvelopeMapper weightEnvelopeMapper,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _bodyDbService = bodyDbService;
            _weightEnvelopeMapper = weightEnvelopeMapper;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateWeightDocument))]
        public async Task Run([ServiceBusTrigger("myhealthbodytopic", "myhealthbodysubscription", Connection = "ServiceBusConnectionString")] string mySbMsg, ILogger logger)
        {
            try
            {
                var weightDocument = JsonConvert.DeserializeObject<mdl.Weight>(mySbMsg);
                var weightEnvelope = _weightEnvelopeMapper.MapWeightToWeightEnvelope(weightDocument);
                await _bodyDbService.AddWeightDocument(weightEnvelope);
                logger.LogInformation($"Weight document with {weightDocument.Date} has been persisted");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception thrown in {nameof(CreateWeightDocument)}: {ex}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                throw ex;
            }
        }
    }
}
