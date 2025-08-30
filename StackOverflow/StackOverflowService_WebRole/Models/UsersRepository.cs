using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflowService_WebRole.Models
{
    public class UsersRepository
    {
        private CloudTable _table;

        public UsersRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("Users");
            _table.CreateIfNotExists();
        }

        public IQueryable<User> GetAllUsers()
        {
            var query = from u in _table.CreateQuery<User>()
                        where u.PartitionKey == "User"
                        select u;
            return query.AsQueryable();
        }

        public void AddUser(User user)
        {
            var insertOperation = TableOperation.Insert(user);
            _table.Execute(insertOperation);
        }

        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var retrieveOperation = TableOperation.Retrieve<User>("User", email);
            var result = _table.Execute(retrieveOperation);
            return result.Result as User;
        }

        public void UpdateUser(User user)
        {
            var updateOperation = TableOperation.Replace(user);
            _table.Execute(updateOperation);
        }

        public void DeleteUser(string email)
        {
            var user = GetUserByEmail(email);
            if (user != null)
            {
                var deleteOperation = TableOperation.Delete(user);
                _table.Execute(deleteOperation);
            }
        }
    }
}
