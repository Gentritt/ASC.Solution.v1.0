using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace ASC.DataAccess
{
    public class BaseEntity : TableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }

    }
}