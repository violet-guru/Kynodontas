using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kynodontas.Basic
{
    public interface IDatabaseHelper<T> where T : BaseDocument
    {
        List<T> MockDocuments { get; set; }
        Task<List<T>> SelectDocumentsWhere(Expression<Func<T, bool>> predicate, bool enableScanInQuery,
            string commentOfEnableScanInQuery);
        Task CreateDocument(T document);
        Task ReplaceDocument(string previousDocumentId, T document);
        Task DeleteDocument(string documentId);
    }

    /// <summary>
    /// Based on 'Open and run a sample .NET app' in cosmos portal
    /// </summary>
    public class DatabaseHelper<T> : IDatabaseHelper<T> where T : BaseDocument
    {
        public List<T> MockDocuments { get; set; }

        private readonly string _collectionId;
        private readonly DocumentClient _client;
        private readonly Common _common = new Common();

        public DatabaseHelper()
        {
            _collectionId = typeof(T).Name;
            _client = new DocumentClient(new Uri(_common.DatabaseEndpoint), _common.DatabaseDocumentKey);
        }

        /// <summary>
        /// Retrieve db documents
        /// </summary>
        public async Task<List<T>> SelectDocumentsWhere(Expression<Func<T, bool>> predicate, bool enableScanInQuery, string commentOfEnableScanInQuery)
        {
            if (!commentOfEnableScanInQuery.StartsWith(@"//true, ") && !commentOfEnableScanInQuery.StartsWith(@"//false, "))
            {
                throw new Exception("appDeveloper: Comment is not well formed");
            }

            var feedOptions = new FeedOptions { MaxItemCount = -1 };
            if (enableScanInQuery)
            {
                feedOptions.EnableScanInQuery = true;
            }

            var query =
                _client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_common.DatabaseId, _collectionId), feedOptions)
                .Where(predicate)
                .AsDocumentQuery();

            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task CreateDocument(T document)
        {
            var outputDocument = await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_common.DatabaseId, _collectionId), document);
        }

        public async Task ReplaceDocument(string previousDocumentId, T document)
        {
            var outputDocument = await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_common.DatabaseId, _collectionId, previousDocumentId), document);
        }

        public async Task DeleteDocument(string documentId)
        {
            var outputDocument = await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_common.DatabaseId, _collectionId, documentId));
        }
    }

    public class MockDatabaseHelper<T> : IDatabaseHelper<T> where T : BaseDocument
    {
        public List<T> MockDocuments { get; set; } = new List<T>();

        public async Task<List<T>> SelectDocumentsWhere(Expression<Func<T, bool>> predicate, bool enableScanInQuery, string commentOfEnableScanInQuery)
        {
            return MockDocuments.AsQueryable().Where(predicate).ToList();
        }

        public async Task CreateDocument(T document)
        {
            MockDocuments.Add(document);
        }

        public async Task ReplaceDocument(string previousDocumentId, T document)
        {
            var currentDocument = MockDocuments.First(x => x.id == previousDocumentId);
            MockDocuments.Remove(currentDocument);
            MockDocuments.Add(document);
        }

        public async Task DeleteDocument(string documentId)
        {
            var currentDocument = MockDocuments.First(x => x.id == documentId);
            MockDocuments.Remove(currentDocument);
        }
    }
}
