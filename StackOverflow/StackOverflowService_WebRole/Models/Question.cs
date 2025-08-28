using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class Question: TableEntity
    {
        public Question(string id)
        {
            PartitionKey = "Question";
            RowKey = id;
        }

        public Question() { }

        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorEmail { get; set; }

        public int TotalVotes { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}