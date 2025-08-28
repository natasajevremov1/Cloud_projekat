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

    public class QuestionsRepository
    {
        private CloudTable _table;

        public QuestionsRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("Questions");
            _table.CreateIfNotExists();
        }

        // Dohvati sva pitanja
        public IQueryable<Question> GetAllQuestions()
        {
            var query = from q in _table.CreateQuery<Question>()
                        where q.PartitionKey == "Question"
                        select q;
            return query.AsQueryable();
        }

        // Dohvati pitanje po ID-u
        public Question GetQuestionById(string questionId)
        {
            var retrieve = TableOperation.Retrieve<Question>("Question", questionId);
            var result = _table.Execute(retrieve);
            return result.Result as Question;
        }

        // Dodaj novo pitanje
        public void AddQuestion(Question question)
        {
            var insertOperation = TableOperation.Insert(question);
            _table.Execute(insertOperation);
        }

        // Izmeni pitanje (samo autor)
        public void UpdateQuestion(Question question)
        {
            var updateOperation = TableOperation.Replace(question);
            _table.Execute(updateOperation);
        }

        // Obrisi pitanje (samo autor)
        public void DeleteQuestion(Question question)
        {
            var deleteOperation = TableOperation.Delete(question);
            _table.Execute(deleteOperation);
        }

        // Pretraga po naslovu
        public IQueryable<Question> SearchByTitle(string keyword)
        {
            var query = from q in _table.CreateQuery<Question>()
                        where q.PartitionKey == "Question" && q.Title.Contains(keyword)
                        select q;
            return query.AsQueryable();
        }
    }

}