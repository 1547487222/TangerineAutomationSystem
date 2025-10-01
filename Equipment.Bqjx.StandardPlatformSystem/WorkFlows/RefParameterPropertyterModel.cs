using CommunityToolkit.Mvvm.ComponentModel;
using Equipment.Bqjx.StandardPlatformSystem.Models;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public partial class RefParameterPropertyterModel:ObservableObject
    {
        public ObservableCollection<IParameterModel>   Parameters { get; set; }

        public RefParameterProperty  OwnerRef { get; set; }

        [ObservableProperty]
        private IParameterModel? _selectedParameter;

        partial void OnSelectedParameterChanged(IParameterModel? value)
        {
            if (value == null) 
            {
                OwnerRef.UnInstallRef();
                return;
            }
            OwnerRef.Parameter = value.Parameter;
            OwnerRef.RefParameterId = value.Parameter.ParameterId;
            OwnerRef.RefParameterTableId = value.Table.Id;
            OwnerRef.InstallRef();
        }

    }

}
