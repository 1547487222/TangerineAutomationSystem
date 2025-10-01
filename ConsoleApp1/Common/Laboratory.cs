using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    /// <summary>
    /// 实验室
    /// </summary>
    public abstract class Laboratory : TreeNode, ILaboratory
    {
        public static string LaboratoryName { get; set; }//=> PropertyInfo?.Get<string>(nameof(LaboratoryName)) ?? string.Empty;

        public static long LaboratoryId { get; set; }
        public static string LaboratoryCode { get; set; }
        public static string LaboratoryDescription { get; set; }


        /// <summary>
        /// 实验室加载
        /// </summary>
        public void Load()
        {
            OnLoad();
        }

        public abstract void OnLoad();
        /// <summary>
        /// 实验室运行
        /// </summary>
        public abstract void Process();


        public abstract void OnUnLoad();
        /// <summary>
        /// 实验室卸载
        /// </summary>
        public void UnLoad()
        {
            OnUnLoad();
        }


        public void AddPlatform(IPlatform platform)
        {
            AddChild(platform);
        }

        public void RemovePlatform(IPlatform platform)
        {
            RemoveChild(platform);
        }
    }
}
