using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.DbContexts
{
    public interface ISqlCommand
    {
        string ConnectionString { get; }
        Task<bool> CreateTableAsync(string tableName);

        Task<bool> DropTableAsync(string tableName);

        Task<bool> CreateDatabaseAsync(string databaseName);

        Task<bool> DropDatabaseAsync(string databaseName);

        Task<bool> CreateDynamicFieldTableAsync(string tableName, List<DynamicField> fields);
    }
}
