using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.SampleEntitys
{

    public enum SampleTraceStatus
    {
        Success,
        Alert,// 有报警（无论是失败、警告、异常等）
    }
    /// <summary>
    /// 样品执行log
    /// </summary>
    public class SampleTraceEntity
    {
        /// <summary>
        /// 工艺流程ID
        /// </summary>
        public string ProcessflowId { get; set; } = string.Empty;
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 平台任务ID
        /// </summary>
        public long PlatformTaskId { get; set; }
        /// <summary>
        /// 任务Id
        /// </summary>
        public long SamplingTaskId { get; set; }
        /// <summary>
        /// 进样ID
        /// </summary>
        public long SamplingId { get; set; } 

        /// <summary>
        /// 样品绑定的SN
        /// </summary>
        public string SampleSn { get; set; } = string.Empty;

        /// <summary>
        /// 样品名称
        /// </summary>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// 样品类型
        /// </summary>
        public string SampleType { get; set; } = string.Empty;

        /// <summary>
        /// 样品备注
        /// </summary>
        public string SampleRemarks { get; set; } = string.Empty;

        //样品器皿名称
        public string LabwareName { get; set; } = string.Empty;

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 模块动作id
        /// </summary>
        public string ModuleActionId { get; set; } = string.Empty;

        /// <summary>
        /// 模块序列号
        /// </summary>
        public int ModuleSerialNumber { get; set; }

        /// <summary>
        /// 模块动作描述
        /// </summary>
        public string ModuleActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 完成状态
        /// </summary>
        public SampleTraceStatus Status { get; set; } = SampleTraceStatus.Success;

        /// <summary>
        /// 输入参数
        /// </summary>
        public string InputParametersJson { get; set; } = string.Empty;

        /// <summary>
        /// 警报信息
        /// </summary>
        public string AlertMessage { get; set; } = string.Empty;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 执行时间/ms
        /// </summary>
        [JsonIgnore]
        public double ElapsedTime => EndTime > StartTime
     ? (EndTime - StartTime).TotalMilliseconds
     : 0;
        public void SetBasicInfo(SampleTaskInfo sampleTaskInfo)
        {
            ProcessflowId = sampleTaskInfo.ProcessflowId;
            PlatformId = sampleTaskInfo.PlatformId;
            PlatformTaskId = sampleTaskInfo.PlatformTaskId;
            PlatformName = sampleTaskInfo.PlatformName;
            SamplingTaskId = sampleTaskInfo.SamplingTaskId;
            SamplingId = sampleTaskInfo.SamplingId;
            SampleName = sampleTaskInfo.SampleName;
            SampleType = sampleTaskInfo.SampleType;
            SampleRemarks = sampleTaskInfo.SampleRemarks;
        }

        public void SetSampleSn(string sampleSn)
        {
            SampleSn = sampleSn;
        }

        public void SetSampleLabwareName(string labwareName)
        {
            LabwareName = labwareName;
        }

        public void SetAlertMessage(string alertMessage)
        {
            AlertMessage = alertMessage;
        }

        public void SetStatus(SampleTraceStatus status)
        {
            Status = status;
        }

        public void SetInputParametersJson(string inputParametersJson)
        {
            InputParametersJson = inputParametersJson;
        }

        public void SetInputParameters(Dictionary<string, float> inputParameters)
        {
            InputParametersJson = JsonConvert.SerializeObject(inputParameters);
        }

        public void SetModuleInfo(Guid actionid,ModuleFuncCodeParameter parameter)
        {
            this.ModuleActionId = actionid.ToString();
            this.ModuleName = parameter.ModuleInfoParameter?.ModuleName??string.Empty;
            if (int.TryParse(parameter.ModuleInfoParameter?.ModuleSerialNumber, out int moduleSerialNumber))
            {
                ModuleSerialNumber = moduleSerialNumber;
            }
            this.ModuleActionDescription = parameter.FuncCodeDescription;
        }

        public void SetStartTime()
        {
            StartTime = DateTime.Now;
        }

        public void SetEndTime()
        {
            EndTime = DateTime.Now;
        }

        /// <summary>
        /// 样品相关任务数据列表
        /// </summary>
        public List<SampleTaskDataEntity> TaskEbrDataEntities { get; set; } = [];

        public void SetEbrDatas(List<SampleTaskDataEntity> sampleTaskDataEntities)
        {
            TaskEbrDataEntities.AddRange(sampleTaskDataEntities);
        }

        public void SetEbrData(SampleTaskDataEntity sampleTaskDataEntity)
        {
            TaskEbrDataEntities.Add(sampleTaskDataEntity);
        }


        public SampleTraceEntity Clone()
        {
            SampleTraceEntity sampleTraceEntity = new()
            {
                SamplingId = SamplingId,
                LabwareName = LabwareName,
                ModuleSerialNumber = ModuleSerialNumber,
                PlatformId = PlatformId,
                PlatformTaskId = PlatformTaskId,
                ProcessflowId = ProcessflowId,
                PlatformName = PlatformName,
                SampleName = SampleName,
                SampleRemarks = SampleRemarks,
                SampleSn = SampleSn,
                SamplingTaskId = SamplingTaskId,
                SampleType = SampleType,
                ModuleName = ModuleName,
                ModuleActionId = ModuleActionId,
                ModuleActionDescription = ModuleActionDescription,
                InputParametersJson = InputParametersJson,
                AlertMessage = AlertMessage,
                StartTime = StartTime,
                EndTime = EndTime,
                Status = Status,
                TaskEbrDataEntities =TaskEbrDataEntities.Select(p=>(SampleTaskDataEntity)p.Clone()).ToList(),
            };
            return sampleTraceEntity;
        }
    }
}

