using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{
    [DisplayName("Json解构器")]
    public class JsonDeconstructorTool : DynamicPinTool
    {
        public JsonDeconstructorData jsonDeconstructorData = new();

        public override string DefineName => "Json解构器";

        public override bool InitDataContext()
        {
            DataContext = jsonDeconstructorData;
            return true;
        }

        public override bool InitPins()
        {
            InsetPin("输入json字符串", this, typeof(QString), PinType.Input);
            return true;
        }

        public override  Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name != "输入json字符串") return Task.FromResult(false);

            var json = JsonConvert.DeserializeObject(pinData.ToString());
            if (json == null) return Task.FromResult(false);

            var token = JToken.FromObject(json);
            EmitJson(jsonDeconstructorData.JsonToken, token, null);

            return Task.FromResult(true);
        }

        private void EmitJson(JsonToken schema, JToken data, string prefix)
        {
            prefix = prefix != null ? $"{prefix}_" : "";

            if (schema.NodeType == JsonNodeType.Value)
            {
                EmitValue(schema.TokenDataType, $"{prefix}{schema.Name}", data);
            }
            else if (schema.NodeType == JsonNodeType.Object && data is JObject obj)
            {
                foreach (var child in schema.Children)
                {
                    if (obj.TryGetValue(child.Name, out var value))
                    {
                        EmitJson(child, value, $"{prefix}{schema.Name}");
                    }
                }
            }
            else if (schema.NodeType == JsonNodeType.Array && data is JArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    foreach (var child in schema.Children)
                    {
                        var indexPrefix = $"{prefix}{schema.Name}_{i}";
                        EmitJson(child, arr[i][child.Name], indexPrefix);
                    }
                }
            }
        }

        private void EmitValue(DataType type, string name, JToken token)
        {
            try
            {
                switch (type)
                {
                    case DataType.QInt: SendToPin($"JsonToken_{name}", (QInt)token.Value<int>()); break;
                    case DataType.QFloat: SendToPin($"JsonToken_{name}", (QFloat)token.Value<float>()); break;
                    case DataType.QDouble: SendToPin($"JsonToken_{name}", (QDouble)token.Value<double>()); break;
                    case DataType.QDateTime: SendToPin($"JsonToken_{name}", (QDateTime)token.Value<DateTime>()); break;
                    case DataType.QString: SendToPin($"JsonToken_{name}", (QString)token.Value<string>()); break;
                    case DataType.QBoolean: SendToPin($"JsonToken_{name}", (QBoolean)token.Value<bool>()); break;
                }
            }
            catch { /* 忽略解析失败 */ }
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is JsonDeconstructorData d)
            {
                BuildOutputPins(d.JsonToken, null);
            }
        }

        private void BuildOutputPins(JsonToken token, string prefix)
        {
            prefix = prefix != null ? $"{prefix}_" : "";

            if (token.NodeType == JsonNodeType.Value)
            {
                var pinName = $"JsonToken_{prefix}{token.Name}";
                InsetPin(pinName, this, MapType(token.TokenDataType), PinType.Output, true);
            }
            else if (token.NodeType == JsonNodeType.Object || token.NodeType == JsonNodeType.Array)
            {
                foreach (var child in token.Children)
                {
                    BuildOutputPins(child, $"{prefix}{token.Name}");
                }
            }
        }

        private static Type MapType(DataType type)
        {
            return type switch
            {
                DataType.QInt => typeof(QInt),
                DataType.QFloat => typeof(QFloat),
                DataType.QDouble => typeof(QDouble),
                DataType.QDateTime => typeof(QDateTime),
                DataType.QString => typeof(QString),
                DataType.QBoolean => typeof(QBoolean),
                _ => typeof(QData)
            };
        }
    }

    public class JsonDeconstructorData : DynamicPinToolData
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public JsonToken JsonToken { get; set; } = new JsonToken();
    }
    public enum JsonNodeType { Value,Object, Array }

    public class JsonToken
    {
        public string Name { get; set; }
        public DataType TokenDataType { get; set; }
        public JsonNodeType NodeType { get; set; }
        public List<JsonToken> Children { get; set; } = [];
    }
 }
