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
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;


namespace QStandaedPlatform.Engine.Common.Common.DbContexts
{
    /// <summary>
    /// Sqlserver Database
    /// </summary>
    public class BaseDbContext : DbContext
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

        public BaseDbContext(DbContextOptions options) : base(options)
        {
            EnsureDirectoriesExist();
        }



        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer(_connectionString);
        //        base.OnConfiguring(optionsBuilder);
        //    }
        //}

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

                var entityType = await GetOrAndTypeByName(entityClassName);

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

                var entityType = await GetOrAndTypeByName(entityClassName);

                var entity = Activator.CreateInstance(entityType);

                foreach (var item in entityType.GetProperties())
                {
                    if (valDic.TryGetValue(item.Name, out float tempValue))
                    {
                        item.SetValue(entity, tempValue);
                    }
                    if (item.PropertyType == typeof(DateTime) || Nullable.GetUnderlyingType(item.PropertyType) == typeof(DateTime))
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

                var con = this;
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

        public async Task<Type> GetOrAndTypeByName(string name)
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

        #region 根据表名、字段，创建Entity
        public async Task<bool> CreateDynamicEntityAndDllAsync(string databaseName,string tableName, List<DynamicField> fields)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");
                if (fields == null || fields.Count == 0)
                    throw new ArgumentException("字段列表不能为空。");

                // 生成实体类代码
                string entityClassName = $"{tableName}Entity";
                string entityCode = GenerateEntityClassCode(databaseName,entityClassName, fields);

        
                // 编译为 DLL
                string dllPath = Path.Combine(_dllOutputPath, $"{entityClassName}.dll");

                bool compileSuccess = await CompileToDllAsync(entityCode, dllPath);
                if (!compileSuccess)
                {
                    Console.WriteLine($"编译实体 {entityClassName} 为 DLL 失败。");
                    return false;
                }

                Console.WriteLine($"实体 {entityClassName} 创建成功，DLL 已保存到 {dllPath}。");



                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建动态实体{tableName} 失败: {ex.Message}");
                throw;
            }
        }

        private string GenerateEntityClassCode(string databaseName,string className, List<DynamicField> fields)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
            sb.AppendLine();
            sb.AppendLine("namespace DynamicEntities");
            sb.AppendLine("{");
            sb.AppendLine($"    [Table(\"{className.Replace("Entity","")}\",Schema = \"{databaseName}\")]");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");

            foreach (var field in fields)
            {
                string csharpType = field.SqlType switch
                {
                    SqlFieldType.String => field.IsNotNull ? "string?" : "string",
                    SqlFieldType.Int => field.IsNotNull ? "int?" : "int",
                    SqlFieldType.Int16 => field.IsNotNull ? "short?" : "short",
                    SqlFieldType.Int32 => field.IsNotNull ? "int?" : "int",
                    SqlFieldType.Int64 => field.IsNotNull ? "long?" : "long",
                    SqlFieldType.Float => field.IsNotNull ? "float?" : "float",
                    SqlFieldType.Double => field.IsNotNull ? "double?" : "double",
                    SqlFieldType.DateTime => field.IsNotNull ? "DateTime?" : "DateTime",
                    SqlFieldType.DateOnly => field.IsNotNull ? "DateOnly?" : "DateOnly",
                    _ => throw new ArgumentException($"不支持的 SqlFieldType: {field.SqlType}")
                };

                if (field.IsPrimaryKey)
                    sb.AppendLine("        [Key]");
                sb.AppendLine($"        public {csharpType} {field.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private async Task<bool> CompileToDllAsync(string code, string dllPath)
        {
            try
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
                string assemblyName = Path.GetFileNameWithoutExtension(dllPath);

                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.KeyAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DateOnly).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location)
                };

                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var stream = new FileStream(dllPath, FileMode.Create))
                {
                    var result = compilation.Emit(stream);
                    if (!result.Success)
                    {
                        var errors = string.Join("\n", result.Diagnostics
                            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                            .Select(d => d.GetMessage()));
                        Console.WriteLine($"编译 DLL 失败: {errors}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"编译 DLL 失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 表结构体操作

        public async Task<bool> AddDynamicFieldTableAsync(string dataseName, string tableName, List<DynamicField> fields, bool isGenralEntitys = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dataseName))
                    throw new ArgumentException("数据库名不能为空。");
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");
                if (fields == null || fields.Count == 0)
                    throw new ArgumentException("字段列表不能为空。");

              
                var existsFields = await GetTableFieldNames(dataseName, tableName);

                var nonFields = fields.Where(s => !existsFields.Contains(s.Name.ToLower())).ToList();

                if (nonFields == null)
                {
                    return true;
                }
                var list = new List<string>();
                foreach (var field in nonFields)
                {

                    var type = GetSqlDataType(field);

                    string sql = _databaseType switch
                    {
                        DatabaseType.mysql => $"ALTER TABLE {tableName} ADD COLUMN {field.Name} {type}, ALGORITHM=INPLACE, LOCK=NONE;",
                        DatabaseType.sqlserver => $"ALTER TABLE {tableName} ADD {field.Name} {type} NULL WITH (ONLINE = ON);",
                        DatabaseType.sqllite => $"ALTER TABLE {tableName} ADD COLUMN {field.Name} {type};",
                        _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                    };

                    list.Add(sql);
                }

                var ret = await ExecuteNonQueryTemplate(_connectionString, list, dataseName);

                Console.WriteLine($"动态添加表字段 {tableName}，{string.Join(',', nonFields.Select(w => w.Name))} 创建成功。");
                if (isGenralEntitys)
                {
                    var b = await CreateDynamicEntityAndDllAsync(dataseName, tableName, fields);

                    if (b)
                        Console.WriteLine($"动态Entity {tableName}Entity 创建成功。");
                    return b;
                }

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建动态表 {tableName} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetTableFieldNames(string databaseName, string tableName)
        {
            try
            {
                string masterConnectionString = _databaseType switch
                {
                    DatabaseType.mysql => _connectionString.Replace($"=master;", "=sys;"),
                    DatabaseType.sqlserver => _connectionString,
                    _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                };

                string query = _databaseType switch
                {
                    DatabaseType.sqlserver => $"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}' ",
                    DatabaseType.mysql => $"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}' and table_schema = '{databaseName}' ;",
                    _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                };
                var tempList = await ExecuteReaderTemplate(masterConnectionString, query);

                return tempList;

            }
            catch (Exception ex)
            {

                return null;
            }
        }
       
        #endregion

        #region 创建_删除库、表
        public async Task<bool> CreateTableAsync(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建表 {tableName} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DropTableAsync(string databaseName,string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");

                string dropTableQuery = $"DROP TABLE {tableName}";

                var ret = await ExecuteNonQueryTemplate(_connectionString, new List<string> { dropTableQuery }, databaseName);

                if (ret)
                    Console.WriteLine($"删除表 {tableName} 成功。");
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除表 {tableName} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateDatabaseAsync(string databaseName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new ArgumentException("数据库名不能为空。");

                string createDbQuery = $"CREATE DATABASE {databaseName}";

                var tempDatabase = _databaseType == DatabaseType.mysql ? "sys" : string.Empty;

                var ret = await ExecuteNonQueryTemplate(_connectionString, new List<string> { createDbQuery }, tempDatabase);

                if (ret)
                    Console.WriteLine($"数据库 {databaseName} 创建成功。");
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建数据库 {databaseName} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DropDatabaseAsync(string databaseName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new ArgumentException("数据库名不能为空。");

                string dropDbQuery = $"DROP DATABASE {databaseName}";

                var tempDatabase = _databaseType == DatabaseType.mysql ? "sys" : string.Empty;

                var ret = await ExecuteNonQueryTemplate(_connectionString, new List<string> { dropDbQuery }, tempDatabase);

                if(ret)
                    Console.WriteLine($"数据库 {databaseName} 删除成功。");
                return ret;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除数据库 {databaseName} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateDynamicFieldTableAsync(string databaseName,string tableName, List<DynamicField> fields, bool isGenralEntitys = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new ArgumentException("表名不能为空。");
                if (fields == null || fields.Count == 0)
                    throw new ArgumentException("字段列表不能为空。");

             
                var createTableQuery = new StringBuilder($"CREATE TABLE {tableName} (");
                var columns = new List<string>();
                string primaryKey = null;
                var indexes = new List<string>();
                var indexFields = new List<string>();
                foreach (var field in fields)
                {
                    if (string.IsNullOrWhiteSpace(field.Name))
                        throw new ArgumentException("字段名不能为空。");
                 
                    var columnDefinition = GetCreateTableByField(field);

                    columns.Add(columnDefinition);

                    if (field.IsIndex)
                    {
                        indexes.Add($"CREATE INDEX IX_{tableName}_{field.Name} ON {tableName} ({field.Name});");
                        indexFields.Add(field.Name);
                    }
                }
                //indexes.Add($"CREATE INDEX INDEX IX_{tableName}_{string.Join('_', indexFields)} ON {tableName} ({string.Join(',', indexFields)});");


                createTableQuery.Append(string.Join(", ", columns));
               
                createTableQuery.Append(");");

                var list = new List<string> { createTableQuery.ToString() };

                foreach (var index in indexes)
                {
                    list.Add(index);
                }
                var ret = await ExecuteNonQueryTemplate(_connectionString,list, databaseName);

                Console.WriteLine($"动态表 {tableName} 创建成功。");

                if (isGenralEntitys)
                {
                    var b = await CreateDynamicEntityAndDllAsync(databaseName, tableName, fields);
                    var msg = b ? "成功" : "失败";
                   
                    Console.WriteLine($"动态Entity {tableName}Entity 创建{msg}");
                    return b;
                }

                return ret;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建动态表 {tableName} 失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 是否存在
   
        public async Task<bool> DataBaseExistsAsync( string databaseName)
        {
            try
            {
                string masterConnectionString = _databaseType switch
                {
                    DatabaseType.mysql => _connectionString.Replace($"=master;", "=sys;"),
                    DatabaseType.sqlserver => _connectionString,
                    _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                };
                string queryString = _databaseType switch
                {
                    DatabaseType.mysql => $"SELECT count(1) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}';",
                    DatabaseType.sqlserver => $"SELECT count(1) FROM sys.databases WHERE name = '{databaseName}';",
                    _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                };

                var ret = await ExecuteScalarTemplate(masterConnectionString, queryString);
                return ret;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task<bool> TableExistsAsync(string databaseName,string tableName)
        {
            try
            {
                string queryString = _databaseType switch
                {
                    DatabaseType.mysql => $"SELECT count(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}' and table_schema = '{databaseName}';",
                    DatabaseType.sqlserver => $"SELECT count(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}';",
                    _ => throw new ArgumentOutOfRangeException(nameof(_databaseType), _databaseType, null)
                };

               
                var ret = await ExecuteScalarTemplate(_connectionString, queryString,databaseName);
                return ret;
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }

        public async Task<bool> IsRefreshTableFileds(string dataseName, string tableName, List<DynamicField> fields)
        {
            try
            {
                var existsFields = await GetTableFieldNames(dataseName, tableName);
                if (existsFields == null && existsFields?.Count == 0)
                {
                    return false;
                }
                var isUpdate = fields.Any(s => !existsFields.Contains(s.Name.ToLower()));

                return isUpdate;
            }
            catch (Exception ex)
            {

                return false;
            }
            
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

        public void AddDbSets(ModelBuilder modelBuilder)
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
                            return count >= 0;
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
                            return count >= 0;
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




    public  class DynamicField
    {
        public string Name { get; set; }

        public SqlFieldType  SqlType { get; set; }

        public bool IsIndex { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsNotNull { get; set; } = true;

        public int Length { get; set; } = 0;

        public bool HasDefaultValue { get; set; } = false;

        public string DefaultValue { get; set; } = string.Empty;
    }

    public enum SqlFieldType
    {
        String,

        Int,
        Int16,
        Int32,
        Int64,

        Float,
        Double,

        DateTime,
        DateOnly,
    }

    public enum DatabaseType
    {
        sqlserver,
        mysql,
        sqllite,
        unknown
    }

    public enum DataTableType
    {
        /// <summary>
        /// 监控表前缀
        /// </summary>
        t_monitor_datas,
        /// <summary>
        /// 报警表前缀
        /// </summary>
        t_alarm_datas,
        /// <summary>
        /// 精度表前缀
        /// </summary>
        t_precision_datas,
        /// <summary>
        /// EBR
        /// </summary>
        t_ebr_datas
    }

    public static class SqlTypeExtensions
    {
        public static string ToSqlServerString(this SqlFieldType sqlType)
        {
            return sqlType switch
            {
                SqlFieldType.String => "NVARCHAR(MAX)",
                SqlFieldType.Int => "INT",
                SqlFieldType.Int16 => "SMALLINT",
                SqlFieldType.Int32 => "INT",
                SqlFieldType.Int64 => "BIGINT",
                SqlFieldType.Float => "REAL",
                SqlFieldType.Double => "FLOAT",
                SqlFieldType.DateTime => "DATETIME2",
                SqlFieldType.DateOnly => "DATE",
                _ => throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null)
            };
        }

        public static string ToMySqlString(this SqlFieldType sqlType)
        {
            return sqlType switch
            {
                SqlFieldType.String => "TEXT",
                SqlFieldType.Int => "INT",
                SqlFieldType.Int16 => "SMALLINT",
                SqlFieldType.Int32 => "INT",
                SqlFieldType.Int64 => "BIGINT",
                SqlFieldType.Float => "FLOAT",
                SqlFieldType.Double => "DOUBLE",
                SqlFieldType.DateTime => "DATETIME",
                SqlFieldType.DateOnly => "DATE",
                _ => throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null)
            };
        }

        public static string ToSqliteString(this SqlFieldType sqlType)
        {
            return sqlType switch
            {
                SqlFieldType.String => "TEXT",
                SqlFieldType.Int => "INTEGER",
                SqlFieldType.Int16 => "INTEGER",
                SqlFieldType.Int32 => "INTEGER",
                SqlFieldType.Int64 => "INTEGER",
                SqlFieldType.Float => "REAL",
                SqlFieldType.Double => "REAL",
                SqlFieldType.DateTime => "TEXT", // 存储为 ISO8601 字符串
                SqlFieldType.DateOnly => "TEXT", // 同上
                _ => throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null)
            };
        }

    
        

    }
}

