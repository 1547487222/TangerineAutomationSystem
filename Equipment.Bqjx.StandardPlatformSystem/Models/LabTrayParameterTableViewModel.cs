using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TrayRegionModel
    {
        [Browsable(false)]
        public long TrayRegionId { get; set; }

        [DisplayName("区域名称")]
        public string RegionName { get; set; } = string.Empty;

        [DisplayName("行数")]
        public int Rows { get; set; }
        [DisplayName("列数")]
        public int Cols { get; set; }
        [DisplayName("行间距")]
        public float SpaceRow { get; set; }
        [DisplayName("列间距")]
        public float SpaceCol { get; set; }

        [DisplayName("行标签")]
        public List<string> RowLabels { get; set; } = [];
        [DisplayName("列标签")]
        public List<string> ColLabels { get; set; } = [];

        [DisplayName("开始位置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition StartPosition { get; set; }=new QPosition();
        [DisplayName("夹爪设置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ClawGraspInfo ClawSetting { get; set; } = new ClawGraspInfo();

        [DisplayName("区域初始类型")]
        public LabTrayDefaultType RegionTrayType { get; set; }

        [DisplayName("是否虚拟区域")]
        public bool VirtualRegion { get; set; }


    }
    public class LabTrayParameterModel
    {
        [DisplayName("行优先")]
        public bool RowFirst { get; set; } = true;

        [DisplayName("托盘分段区域")]
        public List<TrayRegionModel> TrayRegion { get; set; } = [];
    }

    public partial class LabTrayParameterViewModel : ObservableObject, IParameterModel
    {
        private readonly LabTray _labTrayParameter;

        public IParameterTable Table => ParameterModelRepository.LabTrayParameterTableViewModel.Table;

        [ObservableProperty]
        private string _parameterDescription = string.Empty;
        [ObservableProperty]
        private string _labTrayCategory = string.Empty;
        [ObservableProperty]
        private string _labTrayName = string.Empty;
        [ObservableProperty]
 		private long _firstMaterialId = 1;
        [ObservableProperty]
        private string _labTrayCode = string.Empty;
        [ObservableProperty]
        private string _labTrayDescription = string.Empty;


        public IParameter Parameter => _labTrayParameter;

        public LabTray LabTrayParameter => _labTrayParameter;

        public LabTrayParameterViewModel(LabTray labTray)
        {
            _labTrayParameter = labTray;
            this.LabTrayCategory = _labTrayParameter.LabTrayCategory;
            this.LabTrayName = _labTrayParameter.LabTrayName;
            this.LabTrayCode = _labTrayParameter.LabTrayCode;
            this.LabTrayDescription = _labTrayParameter.LabTrayDescription;
            ParameterDescription=$"{_labTrayParameter.LabTrayName}";
        }

        partial void OnLabTrayCategoryChanged(string value)
        {
            _labTrayParameter.LabTrayCategory = value;
            ParameterDescription = $"{_labTrayParameter.LabTrayName}";
        }
        partial void OnLabTrayNameChanged(string value)
        {
            _labTrayParameter.LabTrayName = value;
            ParameterDescription = $"{_labTrayParameter.LabTrayName}";
        }

        partial void OnLabTrayCodeChanged(string value)
        {
            _labTrayParameter.LabTrayCode = value;
            ParameterDescription = $"{_labTrayParameter.LabTrayName}";
        }


        partial void OnFirstMaterialIdChanged(long value)
        {
            ParameterDescription = $"{_labTrayParameter.LabTrayName}";
        }

        partial void OnLabTrayDescriptionChanged(string value)
        {
            _labTrayParameter.LabTrayDescription = value;
            ParameterDescription = $"{_labTrayParameter.LabTrayName}";
        }

        public ICommand EditCommand => new RelayCommand<LabTrayParameterViewModel>(labTrayParameterViewModel =>
        {
            LabTrayParameterModel labTrayParameterModel = new()
            {
                RowFirst = _labTrayParameter.RowFirst,
                TrayRegion = _labTrayParameter.Regions.Select(trayRegion => new TrayRegionModel() 
                {
                     ColLabels = trayRegion.ColLabels,
                     Cols = trayRegion.Cols,
                     RowLabels = trayRegion.RowLabels,
                     Rows = trayRegion.Rows,
                     SpaceCol = trayRegion.SpaceCol,
                     SpaceRow = trayRegion.SpaceRow,
                     VirtualRegion = trayRegion.VirtualRegion,
                     StartPosition = trayRegion.StartPosition,
                     RegionName = trayRegion.TrayRegionName,
                     TrayRegionId = trayRegion.TrayRegionId,
                     RegionTrayType = trayRegion.TrayRegionType,
                     ClawSetting = trayRegion.ClawSetting

                }).ToList()
            };

            PropertyGridEditView propertyGridEditView = new(labTrayParameterModel);
            propertyGridEditView.Closed += (sender, e) =>
            {
                _labTrayParameter.RowFirst = labTrayParameterModel.RowFirst;
                _labTrayParameter.Regions = labTrayParameterModel.TrayRegion.Select(trayRegion => new TrayRegion()
                {
                    ColLabels = trayRegion.ColLabels,
                    Cols = trayRegion.Cols,
                    RowLabels = trayRegion.RowLabels,
                    Rows = trayRegion.Rows,
                    SpaceCol = trayRegion.SpaceCol,
                    SpaceRow = trayRegion.SpaceRow,
                    VirtualRegion = trayRegion.VirtualRegion,
                    StartPosition = trayRegion.StartPosition,
                    TrayRegionName = trayRegion.RegionName,
                    TrayRegionType = trayRegion.RegionTrayType,
                    ClawSetting = trayRegion.ClawSetting,
                    TrayRegionId = trayRegion.TrayRegionId == 0 ? SnowflakeIdGenerator.Instance.GenerateYitId() : trayRegion.TrayRegionId,
                }).ToList();
            };
            propertyGridEditView.ShowDialog();

        });
    }


    public class LabTrayParameterTableViewModel : ObservableObject, IParamterTableChanged, ITableModel
    {
        private readonly LabTrayTable _labTrayTable;
        public LabTrayParameterTableViewModel(LabTrayTable  labTrayTable)
        {
            _labTrayTable = labTrayTable;
            
        }

        public LabTrayTable Table => _labTrayTable;

        public string TableName => _labTrayTable.Name;

        public ICommand AddCommand => new RelayCommand(() =>
        {
            var labTrayParameter = new LabTray() { ParameterId = Guid.NewGuid(), LabTrayId = SnowflakeIdGenerator.Instance.GenerateYitId() };
            var labTrayParameterViewModel = new LabTrayParameterViewModel(labTrayParameter);
            _labTrayTable.AddParameter(labTrayParameter);
            Tables.Add(labTrayParameterViewModel);
            ParamterTableChanged?.Invoke(this);
        });

        //复制选中行
        public ICommand CopyCommand => new RelayCommand<LabTrayParameterViewModel>(labTrayParameterViewModel =>
        {
            if (labTrayParameterViewModel != null)
            {
                var labTrayParameterClone = (LabTray)labTrayParameterViewModel.LabTrayParameter.Clone();
                var labTrayParameterViewModel1 = new LabTrayParameterViewModel(labTrayParameterClone);
                _labTrayTable.AddParameter(labTrayParameterClone);
                Tables.Add(labTrayParameterViewModel1);
                ParamterTableChanged?.Invoke(this);

            }
        });

        public ICommand DeleteCommand => new RelayCommand<LabTrayParameterViewModel>(labTrayParameterViewModel => 
        {
            if (labTrayParameterViewModel != null)
            {
                if (_labTrayTable.LabTrayParameters.Remove(labTrayParameterViewModel.LabTrayParameter))
                {
                    Tables.Remove(labTrayParameterViewModel);
                    ParamterTableChanged?.Invoke(this);
                }
            }
        });

        public BindingList<LabTrayParameterViewModel> Tables { get; internal set; } = [];

        public event ParamterTableChangedHandler? ParamterTableChanged;
        public event Action? Initialized;

        public void Initialize()
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var labTrayParameter in _labTrayTable.LabTrayParameters)
                {
                    Tables.Add(new LabTrayParameterViewModel(labTrayParameter));
                }
                Initialized?.Invoke();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}
