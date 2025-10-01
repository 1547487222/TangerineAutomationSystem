using CommunityToolkit.Mvvm.ComponentModel;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public class PinViewModel:ObservableObject
    {
		private string _PinName;
		public string PinName
        {
			get { return _PinName; }
			set =>SetProperty(ref _PinName, value);
		}
        public Type PinDataType { get; set; }

        public string PinDataTypeName { get; set; }

        public PinType  PinType { get; set; }

        public string OwnerNodeName { get; set; }

        public string OwnerNodeDisplayName  { get; set; }

        private string _Description;

        public string Description
        {
            get { return _Description; }
            set => SetProperty(ref _Description, value);
        }
        public PinViewModel Parent { get; set; }

        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public string UniqueId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<PinViewModel> ConnectPins { get; set; } = [];

        public bool CanConnect(PinViewModel pin)
        {
            if (PinType == PinType.Input)
            {
                return PinDataType == pin.PinDataType || PinDataType.IsAssignableFrom(pin.PinDataType);
            }
            else
            {
                return PinDataType == pin.PinDataType || pin.PinDataType.IsAssignableFrom(PinDataType);
            }
        }
    }
}
