using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Framework;

namespace TangerineAutomationSystem.Models
{
    public interface ITreeNode
    {
        long Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        ITreeNode Owner { get; set; }
        object Tag { get; set; }
        ObservableCollection<ITreeNode> Children { get; }

        bool CanAddChild(ITreeNode treeNode);
        void AddChild(ITreeNode treeNode);
        void RemoveChild(ITreeNode treeNode);
    }

    public abstract class TreeNode : ITreeNode
    {
        private readonly ObservableCollection<ITreeNode> _children = new ObservableCollection<ITreeNode>();

        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ITreeNode Owner { get; set; }
        public object Tag { get; set; }
        public ObservableCollection<ITreeNode> Children => _children;

        public virtual bool CanAddChild(ITreeNode treeNode)
        {
            return true;
        }

        public virtual void AddChild(ITreeNode treeNode)
        {
            if (CanAddChild(treeNode))
            {
                _children.Add(treeNode);
                treeNode.Owner = this;
            }
            else
            {
                throw new InvalidOperationException($"Cannot add {treeNode.GetType().Name} to {GetType().Name}");
            }
        }

        public virtual void RemoveChild(ITreeNode treeNode)
        {
            if (!_children.Contains(treeNode))
            {
                return;
            }
            _children.Remove(treeNode);
            treeNode.Owner = null;
        }
    }


    public class Project : TreeNode
    {
        public string ProjectPath { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0.0";

        public ObservableCollection<Laboratory> Laboratories { get; set; } = new ObservableCollection<Laboratory>();

        public override bool CanAddChild(ITreeNode treeNode)
        {
            return treeNode is Laboratory;
        }
    }

    public class Laboratory : TreeNode
    {
        public string Location { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public ObservableCollection<ProductionLine> ProductionLines { get; set; } = new ObservableCollection<ProductionLine>();

        public override bool CanAddChild(ITreeNode treeNode)
        {
            return treeNode is ProductionLine;
        }
    }
    public class ProductionLine : TreeNode
    {
        public int Capacity { get; set; }
        public string LineType { get; set; } = "Standard";
        public ObservableCollection<Platform> Platforms { get; set; } = new ObservableCollection<Platform>();

        public override bool CanAddChild(ITreeNode treeNode)
        {
            return treeNode is Platform;
        }
    }
    public class Platform : TreeNode
    {
        public string PlatformType { get; set; } = "Standard";
        public int MaxExecuteCount { get; set; } = 2;
        public int MaxCacheCount { get; set; } = 2;
        public ObservableCollection<IModuleInstance> Modules { get; set; } = new ObservableCollection<IModuleInstance>();

        public override bool CanAddChild(ITreeNode treeNode)
        {
            return treeNode is IModuleInstance;
        }
    }

}
