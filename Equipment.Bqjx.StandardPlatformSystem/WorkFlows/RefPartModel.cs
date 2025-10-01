using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
   public partial class RefPartPropertyModel:ObservableObject
    {
        [ObservableProperty]
        private RefPartModel? _refPartModel;
        public ObservableCollection<RefPartModel> RefPartPropertyModels { get; set; } = [];

        partial void OnRefPartModelChanged(RefPartModel? value)
        {
            if (value != null)
            {
                try
                {
                    if (OwnerRef.CanRef(value.OwnerPart))
                    {
                        if (OwnerRef.PartId != Guid.Empty)
                        {
                            OwnerRef.PartId = Guid.Empty;
                            if (OwnerRef.RefPart != null)
                            {
                                OwnerRef.OwnerTool.OnRefPartPropertyUnInstalled(OwnerRef);
                                OwnerRef.UnInstallRef();
                            }
                        }
                        OwnerRef.PartId = value.OwnerPart.PartId;
                        OwnerRef.InstallRef();
                        if (OwnerRef.RefPart != null)
                        {
                            OwnerRef.OwnerTool.OnRefPartPropertyInstalled(OwnerRef);
                        }
                    }
                    else
                    {
                        this.RefPartModel = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("建立引用异常：" + ex.ToString());
                    this.RefPartModel = null;
                }
            }
            else
            {

                if (OwnerRef.PartId != Guid.Empty)
                {
                    OwnerRef.PartId = Guid.Empty;
                    if (OwnerRef.RefPart != null)
                    {
                        OwnerRef.OwnerTool.OnRefPartPropertyUnInstalled(OwnerRef);
                        OwnerRef.UnInstallRef();
                    }
                }
            }
        }

        public RefPartProperty OwnerRef { get; set; }

        [RelayCommand]
        public void UnInstallRef()
        {
            this.RefPartModel = null;
        }
    }

    public class RefPartModel:ObservableObject
    {
        public RefPartModel(PartMapper partMapper)
        {
            OwnerPart = partMapper;
            this.PartName = partMapper.PartName;
            this.PartDesc = partMapper.Description;
            partMapper.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(partMapper.Description))
                {
                   this.PartDesc= partMapper.Description;
                }
            };
        }

        public string PartName { get; set; }

        private string _PartDesc;

        public string PartDesc
        {
            get { return _PartDesc; }
            set => SetProperty(ref _PartDesc, value);
        }
        public PartMapper OwnerPart { get; private set; }
    }
}
