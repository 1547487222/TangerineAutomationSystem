using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public class ModuleChannelGroupModel
    {
        /// <summary>
        /// 通道行数
        /// </summary>
        [DisplayName("通道行数")]
        public int Rows { get; set; }
        /// <summary>
        /// 通道列数
        /// </summary>
        [DisplayName("通道列数")]
        public int Cols { get; set; }
        /// <summary>
        /// 通道行间距
        /// </summary>
        [DisplayName("通道行间距")]
        public float SpaceRow { get; set; }
        /// <summary>
        /// 通道列间距
        /// </summary>
        [DisplayName("通道列间距")]
        public float SpaceCol { get; set; }
        /// <summary>
        /// 起始位置
        /// </summary>
        [DisplayName("起始位置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition StartPosition { get; set; } = new();
        /// <summary>
        /// 夹爪设置
        /// </summary>
        [DisplayName("夹爪设置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ClawGraspInfo ClawSetting { get; set; } = new();
        /// <summary>
        /// 是否行优先
        /// </summary>
        [DisplayName("是否行优先")]
        public bool RowFirst { get; set; } = true;
    }


    public partial class ModuleChannelGroupParameterModel : ObservableObject, IParameterModel
    {
        public IParameterTable Table => ParameterModelRepository.ModuleChannelGroupParameterTableModel.Table;

        public IParameter Parameter => _modulePortGroup;


        private readonly ModuleChannelGroup _modulePortGroup;
        public ModuleChannelGroupParameterModel(ModuleChannelGroup modulePortGroup)
        {
            _modulePortGroup = modulePortGroup;
            ParameterDescription = modulePortGroup.ModuleChannelGroupName;
            ModuleChannelGroupName = modulePortGroup.ModuleChannelGroupName;
            this.ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
            SelectedModuleInfoParameter = this.ModuleInfoParameterModels.FirstOrDefault(x => x.ModuleInfoParameter.ModuleInfoId == _modulePortGroup.ModuleInfoId);
        }

        public ModuleChannelGroup ModuleChannelGroup => _modulePortGroup;

        [ObservableProperty]
        private string _parameterDescription = string.Empty;
        [ObservableProperty]
        private string _moduleChannelGroupName = string.Empty;
        [ObservableProperty]
        private ModuleInfoParameterModel? _selectedModuleInfoParameter;

        public ObservableCollection<ModuleInfoParameterModel> ModuleInfoParameterModels { get; set; }

        partial void OnModuleChannelGroupNameChanged(string value)
        {
            _modulePortGroup.ModuleChannelGroupName = value;
            ParameterDescription = value;
        }
        partial void OnSelectedModuleInfoParameterChanged(ModuleInfoParameterModel? value)
        {
            if (value != null)
                _modulePortGroup.ModuleInfoId = value.ModuleInfoParameter.ModuleInfoId;
        }
        public ICommand EditCommand => new RelayCommand(() => 
        {
            ModuleChannelGroupModel model = new()
            {
                Rows = _modulePortGroup.Rows,
                Cols = _modulePortGroup.Cols,
                SpaceCol = _modulePortGroup.SpaceCol,
                SpaceRow = _modulePortGroup.SpaceRow,
                StartPosition = _modulePortGroup.StartPosition,
                ClawSetting = _modulePortGroup.ClawSetting,
                RowFirst = _modulePortGroup.RowFirst
            };
            PropertyGridEditView propertyGridEditView = new(model);
            propertyGridEditView.Closed += (s, e) => 
            {
                _modulePortGroup.Rows = model.Rows;
                _modulePortGroup.Cols = model.Cols;
                _modulePortGroup.SpaceCol = model.SpaceCol;
                _modulePortGroup.SpaceRow = model.SpaceRow;
                _modulePortGroup.StartPosition = model.StartPosition;
                _modulePortGroup.ClawSetting = _modulePortGroup.ClawSetting;
                _modulePortGroup.RowFirst = model.RowFirst;
            };
            propertyGridEditView.ShowDialog();
        });

    }


    public partial class ModuleChannelGroupParameterTableModel : ObservableObject, IParamterTableChanged, ITableModel
    {
        private readonly ModuleChannelGroupTable _moduleChannelGroupTable;
        public ModuleChannelGroupParameterTableModel(ModuleChannelGroupTable moduleChannelGroupTable)
        {
            _moduleChannelGroupTable = moduleChannelGroupTable;
            
        }
        public string TableName => _moduleChannelGroupTable.Name;

        public ICommand AddCommand => new RelayCommand(() =>
        {
            var moduleChannelGroup = new ModuleChannelGroup() { ParameterId = Guid.NewGuid(), };
            var model = new ModuleChannelGroupParameterModel(moduleChannelGroup);
            _moduleChannelGroupTable.AddParameter(moduleChannelGroup);
            Tables.Add(model);
            ParamterTableChanged?.Invoke(this);
        });

        public ICommand DeleteCommand => new RelayCommand<ModuleChannelGroupParameterModel>(model => 
        {
            if (model!= null)
            {
                if (_moduleChannelGroupTable.ModuleChannels.Remove(model.ModuleChannelGroup))
                {
                    Tables.Remove(model);
                    ParamterTableChanged?.Invoke(this);
                }
            }
        });

        public event ParamterTableChangedHandler? ParamterTableChanged;
        public event Action? Initialized;

        public ModuleChannelGroupTable Table => _moduleChannelGroupTable;

        public BindingList<ModuleChannelGroupParameterModel> Tables { get; set; } = [];

        public void Initialize()
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var moduleChannelGroup in _moduleChannelGroupTable.ModuleChannels)
                {
                    Tables.Add(new ModuleChannelGroupParameterModel(moduleChannelGroup));
                }
                Initialized?.Invoke();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}
