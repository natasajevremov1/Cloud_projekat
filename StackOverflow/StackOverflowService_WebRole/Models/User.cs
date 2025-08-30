using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflowService_WebRole.Models
{
    public class User : TableEntity
    {
        public User() { }

        public User(string email)
        {
            PartitionKey = "User"; // fiksni PartitionKey
            RowKey = email;         // jedinstveni RowKey
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PasswordHash { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email
        {
            get { return RowKey; }
            set { RowKey = value; }
        }
    }
}
