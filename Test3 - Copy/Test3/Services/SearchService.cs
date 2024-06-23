using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Test3.Models;

namespace Test3.Services
{
    public class SearchService
    {
        private readonly IConfiguration _configuration;
        private readonly SearchClient _searchClient;

        public SearchService(IConfiguration configuration)
        {
            _configuration = configuration;

            string serviceName = _configuration["AzureCognitiveSearch:SearchServiceName"];
            string queryApiKey = _configuration["AzureCognitiveSearch:QueryApiKey"];
            string indexName = _configuration["AzureCognitiveSearch:IndexName"];

            var endpoint = new Uri($"https://{serviceName}.search.windows.net");
            var credential = new AzureKeyCredential(queryApiKey);

            _searchClient = new SearchClient(endpoint, indexName, credential);
        }

        public async Task<List<Product>> SearchProductsAsync(string searchText, string category)
        {
            var options = new SearchOptions
            {
                IncludeTotalCount = true,
                Size = 10,
                Filter = $"Category eq '{category}'"
            };

            var results = await _searchClient.SearchAsync<Product>(searchText, options);
            List<Product> products = new List<Product>();

            await foreach (SearchResult<Product> result in results.Value.GetResultsAsync())
            {
                products.Add(result.Document);
            }

            return products;
        }
    }
}
