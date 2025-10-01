using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    using Newtonsoft.Json;
    using System;

    public class LongToStringConverter : JsonConverter<long>
    {
        public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
        public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value?.ToString();
                if (long.TryParse(value, out long result))
                {
                    return result;
                }
                throw new JsonReaderException($"无法将 '{value}' 转换为 long");
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt64(reader.Value);
            }
            else
            {
                throw new JsonReaderException($"意外的 token 类型: {reader.TokenType}，期望 String 或 Integer");
            }
        }
    }
}
