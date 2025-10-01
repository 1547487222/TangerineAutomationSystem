using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class GrpcPfatformProjectOption
    {
        //解决方案根目录
        public string SolutionRootPath { get; set; } = string.Empty;

        //解决方案名称
        public string SolutionName { get; set; } = string.Empty;

        //项目名称

        public string ProjectName { get; set; } = string.Empty;

        //项目描述
        public string ProjectDescription { get; set; } = string.Empty;

        //平台名称
         public string PfatformName { get; set; } = string.Empty;


        public List<GrpcProjectFlowFile> HomeFiles { get; set; } = [];
        public List<GrpcProjectFlowFile> PreperExperimentFiles { get; set; } = [];
        public List<GrpcProjectFlowFile> StartTaskFiles { get; set; } = [];
        public List<GrpcProjectFlowFile> FinalizeFiles { get; set; } = [];
        public List<GrpcProjectFlowFile> MaintenanceFiles { get; set; } = [];
        public List<GrpcProjectFlowFile> SystemStorageFiles { get; set; } = [];

    }

    public class GrpcProjectFlowFile
    {
        public string FlowFilePath { get; set; } = string.Empty;

        public Guid FlowId { get; set; }

        public string FileCopy { get; set; } = string.Empty;

        public string FlowName { get; set; } = string.Empty;
    }

}
