using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationS.Entities
{
        public class QuestionTableEntity : TableEntity
        {
            public QuestionTableEntity() { }

            public string Title { get; set; }
            public string Description { get; set; }
            public string PictureUrl { get; set; }
            public string CreatedBy { get; set; }
            public string TopAnswerId { get; set; }
            public bool IsClosed { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    
}
