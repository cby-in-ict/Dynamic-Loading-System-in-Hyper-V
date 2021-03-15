using System;
using System.Collections.Generic;
using Nest;

namespace Load_Balancer_Server
{
    public class ElasticsearchClient
    {
        private readonly ElasticClient _client;
        private const string DefaultIndexName = "test";

        public ElasticsearchClient()
        {
            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node).DefaultIndex(DefaultIndexName);
            _client = new ElasticClient(settings);
            //CreateDefaultIndex();
        }

        /// <summary>
        /// Writes the document to elasticsearch with the default index
        /// </summary>
        /// <param name="data">Document data to insert</param>
        public void WriteToEs(PerfData data)
        {
            _client.IndexDocument(data);
        }

        /// <summary>
        /// Searches the elastichsearch database for all records given the container name
        /// </summary>
        /// <param name="name">Container name</param>
        /// <returns>Returns matched records</returns>
        public IReadOnlyCollection<PerfData> Search(string name)
        {
            var searchResponse = _client.Search<PerfData>(s => s
                .From(0)
                .Size(10)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.ContainerName)
                        .Query(name)
                    )
                )
            );
            var data = searchResponse.Documents;
            return data;
        }

    }
}
