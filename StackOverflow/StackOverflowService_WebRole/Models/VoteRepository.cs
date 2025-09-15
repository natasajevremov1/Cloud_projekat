using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflowService_WebRole.Models
{
    public class VotesRepository
    {
        private CloudTable table;

        public VotesRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("AnswerVotes");
            table.CreateIfNotExists();
        }

        // Proverava da li je korisnik već glasao za dati odgovor
        public bool HasUserVoted(string answerId, string userEmail)
        {
            var retrieve = TableOperation.Retrieve<AnswerVote>(answerId, userEmail);
            var result = table.Execute(retrieve);
            return result.Result != null;
        }

        // Dodavanje glasa
        public void AddVote(string answerId, string userEmail)
        {
            if (!HasUserVoted(answerId, userEmail))
            {
                var vote = new AnswerVote(answerId, userEmail)
                {
                    VotedAt = DateTime.UtcNow
                };
                var insert = TableOperation.Insert(vote);
                table.Execute(insert);
            }
        }

        // Broj glasova za dati odgovor
        public int GetVoteCount(string answerId)
        {
            var query = new TableQuery<AnswerVote>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, answerId));
            return table.ExecuteQuery(query).Count();
        }
    }
}
