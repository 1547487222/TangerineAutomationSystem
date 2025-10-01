using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{

    public class H5uValueAbreastReaderData : DynamicPinToolData
    {
        /// <summary>
        /// 开始地址
        /// </summary>
        public string StartAddress { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 值数据类型
        /// </summary>
        public DataType  ValueDataType { get; set; }
    }
    [DisplayName("H5U值集合读取器")]
    public class H5uValueAbreastReaderTool : DynamicPinTool
    {
        public override string DefineName => "H5U值集合读取器";

        [ReferencePart]
        public IH5uTcp  h5UTcp { get; set; }
        public override bool InitDataContext()
        {
            DataContext = new H5uValueAbreastReaderData() { IsUpdateInput = false };
            return true;
        }

        public override bool InitPins()
        {
            InsetPin("触发批量读取",this,typeof(QData), PinType.Input);
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var h5UValueAbreastReaderData = Context<H5uValueAbreastReaderData>();
            switch (h5UValueAbreastReaderData.ValueDataType)
            {
                case DataType.QInt16:
                    var shortValues =await h5UTcp?.ReadMultiValueAsync<short>(h5UValueAbreastReaderData.StartAddress,h5UValueAbreastReaderData.Length);
                    SendToPin("输出批量读取值",new QArray<QInt16>(shortValues.Select(p=> new QInt16(p))));
                    break;
                case DataType.QInt:
                    var intValues = await h5UTcp?.ReadMultiValueAsync<int>(h5UValueAbreastReaderData.StartAddress, h5UValueAbreastReaderData.Length);
                    SendToPin("输出批量读取值", new QArray<QInt>(intValues.Select(p => new QInt(p))));
                    break;
                case DataType.QFloat:
                    var floatValues = await h5UTcp?.ReadMultiValueAsync<float>(h5UValueAbreastReaderData.StartAddress, h5UValueAbreastReaderData.Length);
                    SendToPin("输出批量读取值", new QArray<QFloat>(floatValues.Select(p => new QFloat(p))));
                    break;
                case DataType.QBoolean:
                    var boolValues = await h5UTcp?.ReadMultiBooleanAsync(h5UValueAbreastReaderData.StartAddress, h5UValueAbreastReaderData.Length);
                    SendToPin("输出批量读取值", new QArray<QBoolean>(boolValues.Select(p => new QBoolean(p))));
                    break;
            }
            return true;
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is H5uValueAbreastReaderData h5UValueAbreastReaderData)
            {
                switch (h5UValueAbreastReaderData.ValueDataType)
                {
                    case DataType.QInt16:
                        InsetPin($"输出读取组值", this, typeof(QArray<QInt16>), PinType.Output, true);
                        break;
                    case DataType.QInt:
                        InsetPin($"输出读取组值", this, typeof(QArray<QInt>), PinType.Output, true);
                        break;
                    case DataType.QFloat:
                        InsetPin($"输出读取组值", this, typeof(QArray<QFloat>), PinType.Output, true);
                        break;
                    case DataType.QBoolean:
                        InsetPin($"输出读取组值", this, typeof(QArray<QBoolean>), PinType.Output, true);
                        break;
                }
            }
        }
    }
}
