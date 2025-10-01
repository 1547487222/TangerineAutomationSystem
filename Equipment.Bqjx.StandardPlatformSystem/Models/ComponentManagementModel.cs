using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
   public partial class ComponentManagementModel:ObservableObject
    {
        public ObservableCollection<PartMenuModel> PartMenuModels { get; set; } = [];

        public ObservableCollection<PartModel> PartModels { get; set; } = [];

        public ComponentManagementModel()
        {
            foreach (var (partName, desc) in App.ToolEngine.GetPartDescriptions())
            {
                PartMenuModels.Add(new PartMenuModel
                {
                    PartName = partName,
                    Desc = desc
                });
            }
            foreach (var item in App.ToolEngine.GetPartMappers())
            {
                if (item != null)
                {
                    PartModels.Add(new PartModel(item));
                }
            }
        }
        [RelayCommand]
        private void CreatePart(PartMenuModel partMenuModel)
        {
            if (App.ToolEngine.RegisterPart(partMenuModel.PartName, out var partMapper))
            {
                if (partMapper != null)
                    PartModels.Add(new PartModel(partMapper));
            }
        }
        public ICommand RemovePartCommand => new RelayCommand<PartModel>((partModel) =>
        {
            if (partModel != null)
            {
                if (App.ToolEngine.RemovePart(partModel.OwnerPartMapper))
                {
                    App.ToolEngine.RaisePartCollectionChanged();
                    PartModels.Remove(partModel);
                }
                else
                {
                    MessageBox.Show("移除部件失败");
                }
            }
        });
    }

    public class PartMenuModel
    {
        public string PartName { get; set; }

        public string Desc { get; set; }
    }

    public class PartModel:ObservableObject
    {
        private readonly PartMapper _partMapper;
        public PartModel(PartMapper partMapper)
        {
            _partMapper = partMapper;
            this.PartName = _partMapper.PartName;
            this.Desc = _partMapper.Description;
            this.PartOption = _partMapper.PartOption;
            _partMapper.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_partMapper.IsInitialized))
                {
                    this.IsInitialized = _partMapper.IsInitialized;
                }

            };
        }
        public string PartName { get; set; }

        private string _Desc = string.Empty;

        public string Desc
        {
            get { return _Desc; }
            set
            {
                if (SetProperty(ref _Desc, value))
                {
                    _partMapper.Description = _Desc;
                }
            }
        }
        public PartMapper OwnerPartMapper => _partMapper;

        public object PartOption { get; set; }

        private bool _IsInitialized;

        public bool IsInitialized
        {
            get { return _IsInitialized; }
            set => SetProperty(ref _IsInitialized, value);
        }

        public ICommand SettingEditCommand => new RelayCommand(() => 
        {
            PropertyGridEditView propertyGridEditView = new(_partMapper.PartOption);
            propertyGridEditView.ShowDialog();
        });
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        public ICommand InitializeCommand => new RelayCommand(async () =>
        {
            await Task.Delay(0).ContinueWith(async _ => 
            {
                await semaphoreSlim.WaitAsync();
                if (!this.IsInitialized)
                    _partMapper.Initialize();
                this.IsInitialized = _partMapper.IsInitialized;
                OnPropertyChanged(nameof(IsInitialized));
                semaphoreSlim.Release();
            });
           
        },()=> semaphoreSlim.CurrentCount>0);
        public ICommand UnInitializeCommand => new RelayCommand(() =>
        {
            if (this.IsInitialized)
                _partMapper.UnInitialize();
            this.IsInitialized = _partMapper.IsInitialized;
            OnPropertyChanged(nameof(IsInitialized));
        });

        public ICommand RefreshStatusCommand => new RelayCommand(() =>
        {
            this.IsInitialized = _partMapper.GetIsInitialized();
            OnPropertyChanged(nameof(IsInitialized));
        });
    }
}
