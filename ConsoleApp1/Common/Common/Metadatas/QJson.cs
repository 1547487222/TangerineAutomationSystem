using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QJson : QData
    {
        private readonly object _syncRoot = new object();
        private JObject _json;

        #region 构造函数
        // 基础构造
        public QJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON字符串不能为空", nameof(json));

            try
            {
                _json = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                throw new FormatException("JSON格式无效", ex);
            }
        }

        // 深拷贝构造
        public QJson(QJson other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            lock (other._syncRoot)
            {
                _json = (JObject)other._json.DeepClone();
            }
        }

        // 从JObject直接构造（内部使用）
        internal QJson(JObject jObject)
        {
            _json = jObject ?? throw new ArgumentNullException(nameof(jObject));
        }
        #endregion

        #region 核心访问方法
        // 线程安全索引器
        public JToken this[string key]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token)
                        ? token
                        : null;
                }
            }
        }

        // 安全获取方法（带默认值）
        public T GetValue<T>(string key, T defaultValue = default)
        {
            lock (_syncRoot)
            {
                try
                {
                    return _json.Value<T>(key) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        // 类型化获取方法
        public string GetString(string key, string defaultValue = "")
            => GetValue(key, defaultValue);

        public int GetInt(string key, int defaultValue = 0)
            => GetValue(key, defaultValue);

        public float GetFloat(string key, float defaultValue = 0f)
            => GetValue(key, defaultValue);

        public double GetDouble(string key, double defaultValue = 0d)
            => GetValue(key, defaultValue);

        public bool GetBool(string key, bool defaultValue = false)
            => GetValue(key, defaultValue);

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
            => GetValue(key, defaultValue);

        #endregion

        #region 复杂结构访问
        public QJson GetObject(string key)
        {
            lock (_syncRoot)
            {
                var obj = _json[key] as JObject;
                return obj != null ? new QJson(obj) : null;
            }
        }

        public QArray<QJson> GetArray(string key)
        {
            lock (_syncRoot)
            {
                var array = _json[key] as JArray;
                if (array == null) return QArray<QJson>.Empty;

                var result = new QArray<QJson>();
                foreach (var item in array)
                {
                    if (item is JObject jobj)
                        result.Add(new QJson(jobj));
                }
                return result;
            }
        }
        #endregion

        #region 数据操作
        // 合并JSON
        public void Merge(QJson other, bool overwrite = true)
        {
            if (other == null) return;

            lock (_syncRoot)
            {
                lock (other._syncRoot)
                {
                    _json.Merge(other._json, new JsonMergeSettings
                    {
                        MergeArrayHandling = overwrite
                            ? MergeArrayHandling.Replace
                            : MergeArrayHandling.Concat
                    });
                }
            }
        }

        // 添加/修改属性
        public void AddProperty(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("键名不能为空", nameof(key));

            lock (_syncRoot)
            {
                _json[key] = value != null
                    ? JToken.FromObject(value)
                    : JValue.CreateNull();
            }
        }
        #endregion

        #region 序列化与转换
        public override string ToString() => _json.ToString();

        public JObject ToJObject() => (JObject)_json.DeepClone();

        public static bool TryParse(string json, out QJson result)
        {
            try
            {
                result = new QJson(json);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        #endregion

        #region 高级功能
        // 路径查询
        public JToken SelectToken(string jsonPath)
        {
            lock (_syncRoot)
            {
                return _json.SelectToken(jsonPath);
            }
        }

        // 转换为字典
        public Dictionary<string, object> ToDictionary()
        {
            lock (_syncRoot)
            {
                var dict = new Dictionary<string, object>();
                foreach (var pair in _json)
                {
                    dict[pair.Key] = ConvertJToken(pair.Value);
                }
                return dict;
            }
        }

        private object ConvertJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return new QJson((JObject)token);
                case JTokenType.Array:
                    var array = new List<object>();
                    foreach (var item in (JArray)token)
                        array.Add(ConvertJToken(item));
                    return array;
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Date:
                    return token.Value<DateTime>();
                case JTokenType.String:
                    return token.Value<string>();
                default:
                    return token.ToString();
            }
        }
        #endregion
    }
}
