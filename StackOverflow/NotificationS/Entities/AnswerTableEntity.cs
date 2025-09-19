using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace NotificationS.Entities
{
    public class AnswerTableEntity : TableEntity
    {
        public AnswerTableEntity() { }

        public string QuestionId { get; set; }
        public string Description { get; set; }
        public int NumberOfVotes { get; set; }
        public string AnsweredByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}



