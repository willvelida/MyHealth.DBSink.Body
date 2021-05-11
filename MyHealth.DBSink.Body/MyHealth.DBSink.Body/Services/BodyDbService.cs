using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Body.Services
{
    public class BodyDbService : IBodyDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _myHealthContainer;
        private readonly IConfiguration _configuration;

        public BodyDbService(
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _myHealthContainer = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task AddWeightDocument(mdl.Weight weight)
        {
            try
            {
                ItemRequestOptions itemRequestOptions = new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                mdl.WeightEnvelope weightEnvelope = new WeightEnvelope
                {
                    Id = Guid.NewGuid().ToString(),
                    Weight = weight,
                    DocumentType = "Weight"
                };

                await _myHealthContainer.CreateItemAsync(
                    weightEnvelope,
                    new PartitionKey(weightEnvelope.DocumentType),
                    itemRequestOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
