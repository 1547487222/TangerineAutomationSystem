using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class DynamicPinTool : ToolBase
    {
       public new DynamicPinToolData DataContext { get; set; }

        public override T Context<T>()
        {
            if (DataContext is T context)
                return context;
            throw new InvalidOperationException("DataContext is not of type " + typeof(T).Name);
        }

        public override void ApplyOnContextChanged(object context)
        {
            var dynamicPinData = context as DynamicPinToolData;
            if (dynamicPinData != null)
            {
                if (this.InputPins.Any() && dynamicPinData.IsUpdateInput)
                {
                    ToolExecutionContext.StopToolExecution();
                    for (global::System.Int32 i = this.InputPins.Count - (1); i >= 0; i--)
                    {
                        if (dynamicPinData.UpdateInputIndex >= i)
                        {
                            continue;
                        }
                        var pin = this.InputPins[i];
                        DeleteInputPin(pin.Name, true);
                    }
                }
                if (this.OutputPins.Any() && dynamicPinData.IsUpdateOutput)
                {
                    for (global::System.Int32 i = this.OutputPins.Count - (1); i >= 0; i--)
                    {
                        if (dynamicPinData.UpdateOutputIndex >= i)
                        {
                            continue;
                        }
                        var pin = this.OutputPins[i];
                        DeleteOutputPin(pin.Name, true);
                    }
                }
                HandleRequestAddNewPin(dynamicPinData);
                ToolExecutionContext.StartToolExecution();
                ApplyOnDynamicPinToolDataChanged(dynamicPinData);
            }
        }
        public abstract void HandleRequestAddNewPin(object dynamicPinData);

        public virtual void ApplyOnDynamicPinToolDataChanged(object dynamicPinData) { }
    }
}
