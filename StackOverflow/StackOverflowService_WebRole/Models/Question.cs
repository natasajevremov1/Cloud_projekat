using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace StackOverflowService_WebRole.Models
{
    public class Question : TableEntity
    {
        public Question() { }

        public Question(string id)
        {
            PartitionKey = "Question";
            RowKey = id; // jedinstveni ID
        }
        public string AuthorName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorEmail { get; set; }
        public int TotalVotes { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Dodatno
        public int AnswersCount { get; set; } = 0;
        public string BestAnswerId { get; set; }
    }
}
