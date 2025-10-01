using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tangerine.ModuleRunDataTransferApi.Models
{
    #region 通用MongoDB查询、添加实体，版本2

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

    /// <summary>
    /// 客户端添加数据实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoDataAddEntity<T> where T : class
    {
        /// <summary>
        /// 数据库名称，格式不带'-'符号
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
        /// <summary>
        /// 数据表名称：一般为flowId,格式不带'-'符号
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 添加的数据
        /// </summary>
        public T AddDataModel { get; set; }
    }



    #endregion


    public class MongoDataByteBaseEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateOnly CreateDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public byte[] ByteDatas { get; set; }
    }

    public class CommonkeyInfoEntity : MongoDataByteBaseEntity
    {
        public string KeyInfo { get; set; } = string.Empty;
    }


    public class SampleTraceKeyEntity : MongoDataByteBaseEntity
{
        public long  SamplingId {get;set;}

        public long GlobalSampleId { get; set; }
        public long SamplingTaskId { get; set; }
        public string  ProcessflowId       {get;set; } = string.Empty;
        public string  PlatformName        {get;set; } = string.Empty;
        public bool  IsAlarm { get; set; }
    }

    public class SampleTraceByteDataEntity
    {
        public string SampleType { get; set; } = string.Empty;
        public string SampleSn { get; set; } = string.Empty;
        public long PlatformTaskId { get; set; }

        public string LabwareName { get; set; } = string.Empty;
        public int ModuleSerialNumber      {get;set;}
        public long PlatformId              {get;set;}
        public string SampleName              {get;set; } = string.Empty;
        public string SampleRemarks           {get;set; } = string.Empty;
        public string ModuleName              {get;set; } = string.Empty;
        public string ModuleActionId          {get;set; } = string.Empty;
        public string ModuleActionDescription {get;set; } = string.Empty;
        public string InputParametersJson     {get;set; } = string.Empty;
        public string AlertMessage            {get;set; } = string.Empty;
        public DateTime StartTime               {get;set;}
        public DateTime EndTime                 {get;set;}
        public SampleTraceStatus Status                  {get;set;}
        public List<SampleTaskDataEntity> TaskEbrDataEntities { get; set; } = [];
    }
}
