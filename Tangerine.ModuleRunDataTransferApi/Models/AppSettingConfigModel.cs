using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.ModuleRunDataTransferApi.Models
{
    public class AppSettingConfigModel
    {
        public DBConfig DBSet { get; set; }

        public NoSqlConfig NoSqlSet { get; set; }
    }

    public class DBConfig
    {
        public string DbType { get; set; }

        public string MySqlConnStr { get; set; } = string.Empty;

        public string SqlServerConnStr { get; set; } = string.Empty;

        public string SqlLiteConnStr { get; set; } = string.Empty;

    }

    public class NoSqlConfig
    {
        public RedisConfig RedisSet { get; set; }

        public MongoConfig MongoConfigSet { get; set; }

        public InfluxDbConfig InfluxDbSet { get; set; }
    }

    public class RedisConfig
    {
        public string SingleConfig { get; set; }

        public List<string> MasterSlaveConfigs { get; set; }

        //    public List<string> ClusterConfigs { get; set; }

    }

    public class MongoConfig
    {
        public string Standalone { get; set; }

        public string Cluster { get; set; }

        public string ReplicaSet { get; set; }

        public string DataBaseName { get; set; }

        public string ExpireDays { get; set; }

    }

    public class InfluxDbConfig
    {
        public string SingleConfig { get; set; }

        public List<string> MasterSlaveConfigs { get; set; }

        //    public List<string> ClusterConfigs { get; set; }

    }
}
