using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Body.Services;
using Newtonsoft.Json;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Functions
{
    public class CreateWeightDocument
    {
        private readonly IConfiguration _configuration;
        private readonly IBodyDbService _bodyDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateWeightDocument(
            IConfiguration configuration,
            IBodyDbService bodyDbService,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _bodyDbService = bodyDbService;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateWeightDocument))]
        public async Task Run([ServiceBusTrigger("myhealthbodytopic", "myhealthbodysubscription", Connection = "ServiceBusConnectionString")]string mySbMsg, ILogger logger)
        {
            try
            {
                var weightDocument = JsonConvert.DeserializeObject<mdl.Weight>(mySbMsg);
                await _bodyDbService.AddWeightDocument(weightDocument);
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
