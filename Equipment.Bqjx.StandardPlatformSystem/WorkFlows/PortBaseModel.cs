using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
   public abstract class PortBaseModel:ObservableObject
    {
        public string OwnerId { get; set; }

        private Point _anchor;
        public Point Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        private Size _size;

        public Size Size
        {
            get { return _size; }
            set => SetProperty(ref _size, value);
        }
        public abstract void UpdateMatchConnectPins();
    }
}
