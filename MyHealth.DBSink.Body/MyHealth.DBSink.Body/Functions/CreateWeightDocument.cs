using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Body.Service.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Functions
{
    public class CreateWeightDocument
    {
        private readonly IConfiguration _configuration;
        private readonly IBodyService _bodyService;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateWeightDocument(
            IConfiguration configuration,
            IBodyService bodyService,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _bodyService = bodyService;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateWeightDocument))]
        public async Task Run([ServiceBusTrigger("myhealthbodytopic", "myhealthbodysubscription", Connection = "ServiceBusConnectionString")] string mySbMsg, ILogger logger)
        {
            try
            {
                var weightDocument = JsonConvert.DeserializeObject<mdl.Weight>(mySbMsg);
                var weightEnvelope = _bodyService.MapWeightToWeightEnvelope(weightDocument);
                await _bodyService.AddWeightDocument(weightEnvelope);
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
