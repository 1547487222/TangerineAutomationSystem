using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IWorkFlowEngine
    {
        IReadOnlyList<(string toolName, string desc)> GetToolDescriptions();

        IReadOnlyList<(string partName, string desc)> GetPartDescriptions();

        IReadOnlyList<FlowFileDescription> GetFlowFileDescriptions();

        void Initialize();

        void ShutDown();

        IEnumerable<PartMapper> GetPartMappers();

        event Action OnPartCollectionChanged;

        bool RegisterPart(string partName, out PartMapper? partMapper);

        bool RemovePart(PartMapper partMapper);
 
        void SaveAllPart();

        bool TryCreateFlowByFlowOptions(FlowInfoOptions flowInfoOptions, out Flow? flow);

        Flow CreateNewFlow(string flowName, Guid flowId, string desc = "");

        void DeleteFlow(Flow flow);

        void SaveFlow(Flow flow, string path);

        bool ReadFlow(FlowFileDescription flowFileDescription, out Flow? flow);

        bool ReadFlow(string flowPath, out Flow? flow);
   
        bool FlowDeepCopy(FlowInfoOptions options,out Flow? flow);

        bool FlowDeepCopy(Flow sourceflow, out Flow? flow);

        Flow[] GetAllFlows();
 
        void RaisePartCollectionChanged();
    }
}
