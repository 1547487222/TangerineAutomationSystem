using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using ProtoBuf;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class CascadeFlowManager
    {
        private readonly Flow _flow;
        public CascadeFlowManager(Flow flow)
        {
            _flow = flow;
            ModuleDataManager = new ModuleDataManager(_flow);
        }

        public void InitManager()
        {
            ModuleDataManager.Init();
        }
        public ModuleDataManager  ModuleDataManager { get;  }

    }

    [ProtoContract]
    public class ByteArraysContainer
    {
        [ProtoMember(1)]
        public List<byte[]> ByteArrays { get; set; } = new List<byte[]>();
    }

    public class ModuleDataManager(Flow flow)
    {
        private IDbContextFactory<ModuleDbContext>? _dbContextFactory;
        private readonly Flow _flow = flow;

        private HttpClient _httpClient;

        private int _httpTimeOut = 60 * 5;

        private string _mongoWebApiBaseAdress = "http://123.56.45.66:7166";


        //private readonly string ConnectionString = $@"Data Source=d:/Module_databases/{flow.TableName}.sqlite";
        public readonly string ConnectionString = $@"Server=123.56.45.66,1433;Database={flow.FlowId};User ID=sa;Password=SZTest123456;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=200;Connection Timeout=300;TrustServerCertificate=True;";
        public void Init()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModuleDbContext>();
            optionsBuilder.UseSqlServer(ConnectionString);
            _dbContextFactory = new PooledDbContextFactory<ModuleDbContext>(optionsBuilder.Options,5);
            using var context = GetDbContext();
            context.Database.EnsureCreated();
        }

        public void UnInit()
        {
           //
        }


        public ModuleDbContext GetDbContext()
        {
            if (_dbContextFactory == null)
                throw new Exception($"{_flow.FlowName}:DbContextFactory is null");
            return _dbContextFactory.CreateDbContext();
        }


        public async Task<List<ModuleReportRunDataEntity>> GetRunDataAsync()
        {
            var entity = new MongoDataQueryDto
            {
                TableName = _flow.FlowId.ToString().Replace("-", "_"),
            };
            var jsonContent = new StringContent(
                       JsonConvert.SerializeObject(entity),
                       Encoding.UTF8,
                       "application/json"
                   );

            // 2. 调用 API
            var response = await _httpClient.PostAsync("api/DataTransferMongo/getModuleRunData", jsonContent);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();

            var container = ProtoBuf.Serializer.Deserialize<ByteArraysContainer>(stream);
            var byteArray = container.ByteArrays;
            var tempList = new List<ModuleReportRunDataEntity>();
            foreach (var item in byteArray)
            {
                var text = DecompressWithDeflate(item);
                var tempEntity = JsonConvert.DeserializeObject<ModuleReportRunDataEntity>(text);
                if (tempEntity != null)
                    tempList.Add(tempEntity);
            }
            return tempList;
        }

        public static string DecompressWithDeflate(byte[] compressedBytes)
        {
            using var memoryStream = new MemoryStream(compressedBytes);
            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outputStream);
            }
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

    }

    public class ModuleDbContext : DbContext
    {
        public ModuleDbContext(DbContextOptions options):base(options)
        {

        }
        public DbSet<ModuleReportAlarmEntity>  ModuleReportAlarmEntities { get; set; }

        public DbSet<ModuleReportMonitorTaskDataItemEntity>  ModuleReportMonitorTaskDataItemEntities { get; set; }

        public DbSet<ModuleReportMonitorTaskDataOnceEntity>  ModuleReportMonitorTaskEntities { get; set; }

        public DbSet<ModuleReportEbrDataEntity>  ModuleReportEbrDataEntities { get; set; }

        public DbSet<ModuleReportPrecisionEntity>  ModuleReportPrecisionEntities { get; set; }

        public DbSet<ModuleReportRunDataEntity>  ModuleReportRunDataEntities { get; set; }


        public List<ModuleReportRunDataEntity> GetModuleReportRunDataEntities()
        {
            var data = ModuleReportRunDataEntities
                 .Include(p => p.PrecisionDatas)
                 .Include(p => p.Alarms)
                 .Include(p => p.EbrDatas)
                 .Include(p => p.TaskDatas)
                 .ThenInclude(p => p.ModuleReportCollectTaskDataItemEntities);
            return [.. data];
        }
    }

    public class MongoDataQueryDto
    {
        /// <summary>
        /// 数据库名称，格式不带'-'符号
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
        /// <summary>
        /// 数据表名称：一般为flowId,格式不带'-'符号
        /// </summary>
        public string TableName { get; set; }

        public string KeyInfo { get; set; }
        /// <summary>
        /// 起始时间，如果为空，返回所有
        /// </summary>
        public DateOnly? StartDate { get; set; }
        /// <summary>
        /// 结束时间，如果为空，返回StartDate当天
        public DateOnly? EndDate { get; set; }
    }
}
