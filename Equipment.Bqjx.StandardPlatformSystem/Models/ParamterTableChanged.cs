
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public delegate void ParamterTableChangedHandler(object sender);


    public interface IParamterTableChanged
    {
        event ParamterTableChangedHandler? ParamterTableChanged;
    }


    /// <summary>
    /// 表示一个可管理数据项的表格视图模型
    /// </summary>
    public interface ITableModel
    {
        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; }
        /// <summary>
        /// 初始化表格
        /// </summary>
        void Initialize();
        /// <summary>
        ///  当表格数据初始化完成后触发
        /// </summary>
        event Action? Initialized;
        /// <summary>
        ///  添加新项的命令
        /// </summary>
        ICommand AddCommand { get; }
        /// <summary>
        /// 删除选中项的命令
        /// </summary>
        ICommand DeleteCommand { get; }
    }


   
}
