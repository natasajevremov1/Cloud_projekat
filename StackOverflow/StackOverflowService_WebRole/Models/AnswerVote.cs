using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflowService_WebRole.Models
{
    public class AnswerVote : TableEntity
    {
        public AnswerVote() { }

        public AnswerVote(string answerId, string userEmail)
        {
            PartitionKey = answerId; // AnswerId
            RowKey = userEmail;      // jedinstveno po korisniku
        }

        public string AnswerId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string UserEmail
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public DateTime VotedAt { get; set; }
    }
}
