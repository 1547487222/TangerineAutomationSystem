using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public class ConnectionViewModel:ObservableObject
    {
        public ConnectionViewModel(OutputPortModel source, InputPortModel target)
        {
            Source = source;
            Target = target;
        }
        public ConnectionViewModel()
        {
            
        }

        private OutputPortModel _source = default!;
        public OutputPortModel Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        private InputPortModel _target = default!;
        public InputPortModel Target
        {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        private bool _IsActive = true;

        public bool IsActive
        {
            get { return _IsActive = true;; }
            set => SetProperty(ref _IsActive, value);
        }

    }
}
