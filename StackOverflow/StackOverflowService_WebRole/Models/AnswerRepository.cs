using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Linq;

    public class AnswersRepository
    {
        private CloudTable _table;

        public AnswersRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("Answers");
            _table.CreateIfNotExists();
        }

        // Dohvati sve odgovore za određeno pitanje
        public IQueryable<Answer> GetAnswersByQuestionId(string questionId)
        {
            var query = from a in _table.CreateQuery<Answer>()
                        where a.PartitionKey == questionId
                        select a;
            return query.AsQueryable();
        }

        // Dohvati odgovor po ID-u
        public Answer GetAnswerById(string questionId, string answerId)
        {
            var retrieve = TableOperation.Retrieve<Answer>(questionId, answerId);
            var result = _table.Execute(retrieve);
            return result.Result as Answer;
        }

        // Dodaj novi odgovor
        public void AddAnswer(Answer answer)
        {
            var insertOperation = TableOperation.Insert(answer);
            _table.Execute(insertOperation);
        }

        // Glasanje za odgovor
        public void VoteAnswer(Answer answer, int voteChange)
        {
            answer.Votes += voteChange;
            var updateOperation = TableOperation.Replace(answer);
            _table.Execute(updateOperation);
        }

        // Oznaci odgovor kao najbolji
        public void MarkAsAccepted(Answer answer)
        {
            answer.IsAccepted = true;
            var updateOperation = TableOperation.Replace(answer);
            _table.Execute(updateOperation);
        }

        // Brisanje odgovora (samo autor)
        public void DeleteAnswer(Answer answer)
        {
            var deleteOperation = TableOperation.Delete(answer);
            _table.Execute(deleteOperation);
        }
    }

}