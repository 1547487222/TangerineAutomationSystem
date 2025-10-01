using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    public interface ITreeNode
    {
        /// <summary>
        /// 获取当前节点的父节点。
        /// </summary>
        ITreeNode? Parent { get; }

        /// <summary>
        /// 获取当前节点的所有子节点。
        /// </summary>
        IEnumerable<ITreeNode> Children { get; }

        /// <summary>
        /// 添加一个子节点到当前节点。
        /// </summary>
        /// <param name="child">要添加的子节点。</param>
        void AddChild(ITreeNode child);

        /// <summary>
        /// 移除一个子节点。
        /// </summary>
        /// <param name="child">要移除的子节点。</param>
        /// <returns>如果成功移除则返回 true；否则返回 false。</returns>
        bool RemoveChild(ITreeNode child);

        /// <summary>
        /// 判断该节点是否为根节点（即没有父节点）。
        /// </summary>
        /// <returns>如果是根节点，则返回 true；否则返回 false。</returns>
        bool IsRoot();

        /// <summary>
        /// 判断该节点是否为叶子节点（即没有子节点）。
        /// </summary>
        /// <returns>如果是叶子节点，则返回 true；否则返回 false。</returns>
        bool IsLeaf();
        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="parent"></param>
        void SetParent(ITreeNode? parent);
        /// <summary>
        /// 节点元构件
        /// </summary>
        IMetaComponents? MetaComponent { get; }
        /// <summary>
        /// 添加构件
        /// </summary>
        /// <param name="component"></param>
        /// <param name="name"></param>
        void AddComponent(IComponent component, string name);

        /// <summary>
        /// 节点属性对象
        /// </summary>
        IPropertyInfo? PropertyInfo { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        void SetPropertyInfo(IPropertyInfo propertyInfo);

        /// <summary>
        /// 节点序列化器
        /// </summary>
        ISerializer<ITreeNode>? Serializer { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        void SetSerializer(ISerializer<ITreeNode> serializer);
    }

    public class TreeNode : ITreeNode
    {
        private readonly List<ITreeNode> _children = [];
        private ITreeNode? _parent;
        private IMetaComponents? _metaComponent;
        private IPropertyInfo? _propertyInfo;
        private ISerializer<ITreeNode>? _serializer;

        public ITreeNode? Parent => _parent;

        public IEnumerable<ITreeNode> Children => _children;

        public IMetaComponents? MetaComponent => _metaComponent;

        public IPropertyInfo? PropertyInfo => _propertyInfo;

        public ISerializer<ITreeNode>? Serializer => _serializer;

        public TreeNode()
        {

        }

        public void AddChild(ITreeNode child)
        {
            ArgumentNullException.ThrowIfNull(child);
            if (child.Parent != null) throw new InvalidOperationException("The node already has a parent.");

            _children.Add(child);
            child.SetParent(this);
        }

        public bool RemoveChild(ITreeNode child)
        {
            if (_children.Remove(child))
            {
                child.SetParent(null);
                return true;
            }
            return false;
        }

        public bool IsRoot() => _parent == null;

        public bool IsLeaf() => _children.Count == 0;

        public void SetParent(ITreeNode? parent)
        {
            _parent = parent;
        }
        public void AddComponent(IComponent component, string name)
        {
            if (_metaComponent == null)
                throw new InvalidOperationException("节点元构件不存在");
            _metaComponent.Set(name, component);

        }
        public void SetMetaComponent(IMetaComponents metaComponents)
        {
            _metaComponent = metaComponents;
        }
        public void SetPropertyInfo(IPropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public void SetSerializer(ISerializer<ITreeNode> serializer)
        {
            _serializer = serializer;
        }
    }
}
