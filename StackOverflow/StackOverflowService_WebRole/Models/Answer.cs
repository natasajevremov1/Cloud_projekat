using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class Answer : TableEntity
    {
        public Answer(string questionId, string answerId)
        {
            PartitionKey = questionId;
            RowKey = answerId;
        }

        public Answer() { }

        public string Content { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorName { get; set; }
        public int Votes { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}