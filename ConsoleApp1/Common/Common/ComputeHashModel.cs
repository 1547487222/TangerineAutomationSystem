using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class ContentBasedHashableObject
    {
        /// <summary>
        /// 生成基于对象所有属性值的哈希值
        /// </summary>
        public string ComputeContentHash()
        {
            StringBuilder contentBuilder = new();
            System.Reflection.PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (System.Reflection.PropertyInfo prop in properties)
            {
                if (prop.GetIndexParameters().Length == 0)
                {
                    object? value = prop.GetValue(this);
                    string valueStr = value?.ToString() ?? string.Empty;

                    contentBuilder.Append($"{prop.Name}={valueStr}|");
                }
            }
            string content = contentBuilder.ToString();
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            StringBuilder hashBuilder = new();
            foreach (byte b in hashBytes)
            {
                hashBuilder.Append(b.ToString("x2"));
            }
            return hashBuilder.ToString();
        }

        /// <summary>
        /// 判断两个对象的内容是否一致
        /// </summary>
        public bool IsSameContent(ContentBasedHashableObject other)
        {
            return this.ComputeContentHash() == other?.ComputeContentHash();
        }
    }
}
