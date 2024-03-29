﻿using ASC.DataAccess.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        public bool disposed;
        public bool completed;
        private Dictionary<string, object> _repositories;
        public Queue<Task<Action>> RollBackActions { get; set; }

        public string ConnectionString { get; set; }
        public UnitOfWork(string connectionString) 
        {
            ConnectionString = connectionString;
            RollBackActions = new Queue<Task<Action>>();
        }
        public void RollBackTransaction() 
        { 
            while(RollBackActions.Count > 0)
            {
                var undoAction = RollBackActions.Dequeue();
                undoAction.Result();
            }
        
        }
        public string connectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CommitTransaction()
        {
            completed = true;
        }
        ~UnitOfWork()
        {
            Dispose(false);
        }
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!completed) RollBackTransaction();
                }
                finally
                {
                    RollBackActions.Clear();
                }
            }
            completed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public IRepository<T> Repository<T>() where T : TableEntity
        {
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();

            var type = typeof(T).Name;

            if (_repositories.ContainsKey(type)) return (IRepository<T>)_repositories[type];

            var repositoryType = typeof(Repository<>);

            var repositoryInstance =
                Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), this);

            _repositories.Add(type, repositoryInstance);
            return (IRepository<T>)_repositories[type];
        }
    }
}
