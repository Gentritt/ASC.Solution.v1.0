using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Queue<Task<Action>> RollBackActions { get; set; }
        string connectionString { get; set; }
        IRepository<T> Repository<T>() where T : TableEntity;
        void CommitTransaction();
    }
}
