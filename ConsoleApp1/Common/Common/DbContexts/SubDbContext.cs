using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Data.Sqlite;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using QStandaedPlatform.Engine.Extensions;
using System.Runtime.Loader;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System.Data;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MySqlConnector;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;


namespace QStandaedPlatform.Engine.Common.Common.DbContexts
{
    /// <summary>
    /// Sqlserver Database
    /// </summary>
    public class SubDbContext : DbContext
    {
        private  string _connectionString;

        private DatabaseType _databaseType;

        private readonly string _dllOutputPath = $"{AppContext.BaseDirectory}/Module_databases/dlls/";

        private readonly string _exportOutputPath = $"{AppContext.BaseDirectory}/Module_databases/exports/";
 
        public string ConnectionString => _connectionString;

        private static readonly ConcurrentDictionary<Type, (MethodInfo,object)> AddAsyncMethods = new();
        private static readonly ConcurrentDictionary<Type, MethodInfo> SetAsyncMethods = new();
        private static readonly ConcurrentDictionary<string, Type> entityTypes = new();

        public DbSet<FlowMainInfoEntity> FlowMainInfoEntitySets { get; set; }

        public SubDbContext(DbContextOptions options) : base(options)
        {
            EnsureDirectoriesExist();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            AddDbSets(modelBuilder);
           
        }

        #region 根据表名称，导出到csv
        public async Task<bool> QueryDynamicTableAndExportAsync(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");

                string entityClassName = $"{tableName}Entity";

                var entityType =  GetOrAndTypeByName(entityClassName);

                var setMethod =  SetAsyncMethods.GetOrAdd(entityType, t =>
                {
                    var method = typeof(DbContext).GetMethod(nameof(DbContext.Set),Type.EmptyTypes);
                    return method.MakeGenericMethod(t);
                });

                var dbSet = setMethod.Invoke(this, null);

                var queryable = dbSet as IQueryable<object>;

                const int pageSize = 1000;
                var allData = new List<object>();
                int pageNumber = 1;
                bool hasMoreData = true;

                // 获取实体属性
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var columnNames = properties.Select(p => p.Name).ToList();

                // 准备 CSV 文件
                string csvPath = Path.Combine(_exportOutputPath, $"{tableName}_{DateTime.Now:yyyyMMddHHmmss}.csv");
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine(string.Join(",", columnNames.Select(name => $"\"{name}\"")));

                while (hasMoreData)
                {
                    var query = queryable
                        .OrderBy(e => EF.Property<int>(e, "Id"))
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);

                    var result = await query.ToListAsync();
                    Console.WriteLine($"第 {pageNumber} 页获取到 {result.Count} 条记录。");

                    foreach (var entity in result)
                    {
                        var values = new List<string>();
                        foreach (var prop in properties)
                        {
                            var value = prop.GetValue(entity);
                            string formattedValue = value == null ? "" : $"\"{value.ToString().Replace("\"", "\"\"")}\"";
                            values.Add(formattedValue);
                        }
                        csvBuilder.AppendLine(string.Join(",", values));
                    }

                    if (result.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        allData.AddRange(result);
                        pageNumber++;
                    }
                }

                // 写入 CSV 文件
                await File.WriteAllTextAsync(csvPath, csvBuilder.ToString(), Encoding.UTF8);
                Console.WriteLine($"表 {tableName} 数据已导出到 {csvPath}。");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询并导出表 {tableName} 失败: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 向表中插入数据
        public async Task<bool> DynamicAddDataAsync(DbContext context,string tableName,Dictionary<string,float> valDic)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("表名不能为空。");
            if(valDic == null)
                throw new ArgumentException("插入数据不能为空。");

            string entityClassName = $"{tableName}Entity";
          
           
            try
            {

                var entityType =  GetOrAndTypeByName(entityClassName);

                var entity = Activator.CreateInstance(entityType);

                foreach (var item in entityType.GetProperties())
                {
                    if (valDic.TryGetValue(item.Name, out float tempValue))
                    {
                        item.SetValue(entity, tempValue);
                    }
                    if (item.PropertyType == typeof(DateTime) ||  Nullable.GetUnderlyingType(item.PropertyType) == typeof(DateTime))
                    {
                        item.SetValue(entity, DateTime.Now);
                    }
                    if (item.PropertyType == typeof(DateOnly) || Nullable.GetUnderlyingType(item.PropertyType) == typeof(DateOnly))
                    {
                        item.SetValue(entity, DateOnly.FromDateTime(DateTime.Now));
                    }
                }

                var genericMethod = AddAsyncMethods.GetOrAdd(entityType, t =>
                {

                    // 获取泛型 Set<> 方法
                    MethodInfo setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);

                    // 创建特定类型的 DbSet
                    var genericSetMethod = setMethod.MakeGenericMethod(t);
                    object dbSet = genericSetMethod.Invoke(context, null);

                    // 获取 DbSet<T>.AddAsync 方法
                    MethodInfo dbSetAddAsyncMethod = dbSet.GetType().GetMethod("AddAsync", new[] { t, typeof(CancellationToken) });

                    return (dbSetAddAsyncMethod, dbSet);
                });

                // 调用AddAsync
                if (genericMethod.Item1 == null) throw new Exception("没有找到AddAsync方法");
               
                var task =  genericMethod.Item1.Invoke(genericMethod.Item2, new object[] { entity, CancellationToken.None });

                var dynamicTaks = (dynamic)task;

                await dynamicTaks;

                await context.SaveChangesAsync();

                
                return true;
                
              
            }
            catch (DbUpdateException ex)
            {
                 Console.WriteLine($"数据库更新失败: {ex.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作失败: {ex.Message}");
                return false;
            }

        }

        public  Type GetOrAndTypeByName(string name)
        {
          
            try
            {
                if (!entityTypes.ContainsKey(name))
                {
                    string dllPath = Path.Combine(_dllOutputPath, $"{name}.dll");
                    if (!File.Exists(dllPath))
                        throw new ArgumentException($"实体 DLL {dllPath} 不存在。");

                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    var entityType = assembly.GetType($"DynamicEntities.{name}");
                    if (entityType == null)
                        throw new Exception($"无法加载实体类型 {name}。");
                    return entityTypes.GetOrAdd(name, entityType);
                }
                else
                {
                    return entityTypes.GetValueOrDefault(name);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return null;
        }

        #endregion



        #region 公共方法

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(_dllOutputPath))
            {
                Directory.CreateDirectory(_dllOutputPath);
            }
            if (!Directory.Exists(_exportOutputPath))
            {
                Directory.CreateDirectory(_exportOutputPath);
            }
        }

        private void AddDbSets(ModelBuilder modelBuilder)
        {
            try
            {
                if (!Directory.Exists(_dllOutputPath))
                {
                    throw new DirectoryNotFoundException($"目录不存在: {_dllOutputPath}");
                }
                var files = Directory.GetFiles(_dllOutputPath, "*.dll");
                foreach (var dllPath in files)
                {
                    try
                    {
                        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                        var types = assembly.GetExportedTypes()
                            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
                            .ToList();

                        foreach (var type in types)
                        {
                            // 这里可以根据你的实体类特征添加更多过滤条件
                            if (type.GetProperties().Any() && modelBuilder.Model.FindEntityType(type) == null)
                            {
                                modelBuilder.Model.AddEntityType(type);

                                var tableName = type.GetCustomAttribute<TableAttribute>().Name;
                               
                                var databaseName = type.GetCustomAttribute<TableAttribute>().Schema;
                                // 设置表名和schema
                                modelBuilder.Entity(type)
                                    .ToTable(tableName, databaseName.ToLower());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载DLL {Path.GetFileName(dllPath)} 失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        private string GetSqlDataType(DynamicField field)
        {
            string sqlType = _databaseType switch
            {
                DatabaseType.sqlserver => field.SqlType.ToSqlServerString(),
                DatabaseType.mysql => field.SqlType.ToMySqlString(),
                DatabaseType.sqllite => field.SqlType.ToSqliteString(),
                _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
            };
            if (field.SqlType == SqlFieldType.String && field.Length > 0)
                sqlType = $"NVARCHAR({field.Length})";

            return sqlType;
        }

        private string GetCreateTableByField(DynamicField field)
        {
            string sqlType = GetSqlDataType(field);

            var isnull = field.IsIndex || field.IsNotNull || field.IsPrimaryKey ? "NOT NULL" : "NULL";
            var PrimaryKey = _databaseType switch
            {
                DatabaseType.sqlserver => " IDENTITY(1,1) PRIMARY KEY ",
                DatabaseType.mysql => " AUTO_INCREMENT PRIMARY KEY ",
                DatabaseType.sqllite => "PRIMARY KEY AUTOINCREMENT",
                _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
            };
            var isPrimary = field.IsPrimaryKey ? PrimaryKey : string.Empty;
            return $"{field.Name} {sqlType} {isnull} {isPrimary}";
        }

        public void SetConnectionStringAndDatabaseType(DbContextOptions options)
        {
            // 方法2：从Relational配置扩展中获取
            var relationalExtension = options.Extensions
                .OfType<RelationalOptionsExtension>()
                .FirstOrDefault();


            _databaseType = relationalExtension switch
            {
                SqlServerOptionsExtension _ => DatabaseType.sqlserver,
                MySqlOptionsExtension _ => DatabaseType.mysql,
                SqliteOptionsExtension _ => DatabaseType.sqllite,
                _ => DatabaseType.unknown
            };

            _connectionString = relationalExtension.ConnectionString;
            
        }

        public async Task<bool> ExecuteScalarTemplate(string connectionString,string queryString,string databaseName = "")
        {

            try
            {
                if (!string.IsNullOrEmpty(databaseName))
                    connectionString = connectionString.Replace("=master;", $"={databaseName};").Replace("=sys;", $"={databaseName};");

                switch (_databaseType)
                {
                    case DatabaseType.sqlserver:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            using (var command = new SqlCommand(queryString, connection))
                            {
                                var count = (long) await command.ExecuteScalarAsync();
                                return count > 0;
                            }
                        }
                    case DatabaseType.mysql:
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            using (var command = new MySqlCommand(queryString, connection))
                            {
                                var count = (long) await command.ExecuteScalarAsync();
                                return count > 0;
                            }
                        }
                        break;
                    case DatabaseType.sqllite:
                        return false;

                    case DatabaseType.unknown:
                        return false;
                    default:
                        return false;
                }

            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task<List<string>> ExecuteReaderTemplate(string connectionString, string queryString, string databaseName = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(databaseName))
                    connectionString = connectionString.Replace("=master;", $"={databaseName};").Replace("=sys;", $"={databaseName};");

                var tempList = new List<string>();

                switch (_databaseType)
                {
                    case DatabaseType.sqlserver:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            using (var command = new SqlCommand(queryString, connection))
                            {
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var name = reader.GetString(0);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            tempList.Add(name.ToLower());
                                        }
                                    }
                                }

                            }
                            return tempList;
                        }
                    case DatabaseType.mysql:
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            using (var command = new MySqlCommand(queryString, connection))
                            {
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var name = reader.GetString(0);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            tempList.Add(name.ToLower());
                                        }
                                    }
                                }
                            }
                            return tempList;
                        }
                        break;
                    case DatabaseType.sqllite:
                        return tempList;

                    case DatabaseType.unknown:
                        return tempList;
                    default:
                        return tempList;
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<bool> ExecuteNonQueryTemplate(string connectionString, List<string> queryString, string databaseName = "")
        {
            try
            {
                long count = 0;
                if (!string.IsNullOrEmpty(databaseName))
                    connectionString = connectionString.Replace("=master;", $"={databaseName};").Replace("=sys;", $"={databaseName};");

                switch (_databaseType)
                {
                    case DatabaseType.sqlserver:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                           
                            foreach (var item in queryString)
                            {
                                using (var command = new SqlCommand(item, connection))
                                {
                                    count += (long)await command.ExecuteNonQueryAsync();
                                    await Task.Delay(200);
                                }
                            }
                            return count > 0;
                        }
                    case DatabaseType.mysql:
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                          
                            foreach (var item in queryString)
                            {
                                using (var command = new MySqlCommand(item, connection))
                                {
                                    count += (long)await command.ExecuteNonQueryAsync();
                                    await Task.Delay(200);
                                }
                            }
                            return count > 0;
                        }
                        break;
                    case DatabaseType.sqllite:
                        return false;

                    case DatabaseType.unknown:
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        #endregion
    }
 
}

