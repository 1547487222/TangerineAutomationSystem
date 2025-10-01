using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartMapper:INotifyPropertyChanged
    {
        private  IPart? _part;
        private readonly PartBackup _backup;
        private string _description=string.Empty;
        private bool _IsInitialized;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public PartMapper(PartBackup partBackup)
        {
            _backup = partBackup;
            PartOption = _backup.PartOption;
            this.Description = _backup.Description;
        }

        //激活
        public void Activate()
        {
            _part ??= PartStructurer.Structurer(_backup.PartType, PartOption) ?? throw new ArgumentException("构造部件失败", _backup.PartName);
            PartManager.Instance.AddPart(_backup.PartId, _part);
        }


        public void Initialize()
        {
            _part = PartStructurer.Structurer(_backup.PartType, PartOption) ?? throw new ArgumentException("构造部件失败", _backup.PartName);
            _part.Initialize();
            IsInitialized = _part.IsInitialized;
        }

        public void UnInitialize()
        {
            if (_part != null)
            {
                _part.Shutdown();
                PartManager.Instance.RemovePart(_backup.PartId);
                IsInitialized = _part.IsInitialized;
            }
            else
                IsInitialized = false;
        }

        public string PartName => _backup.PartName;


        public string Description
        {
            get { return _description; }
            set 
            {
                _description = value;
                _backup.Description = _description;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public Guid PartId => _backup.PartId;

        public Type? PartType => _part != null ? _part.GetType() : default;
        public object PartOption { get; set; }


        public bool IsInitialized
        {
            get { return _IsInitialized; }
            set 
            {
                _IsInitialized = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInitialized)));
            }
        }

        public bool GetIsInitialized()
        {
            if (_part != null)
                return IsInitialized = _part.IsInitialized;
            else
                return false;
        }


        public IPart? Part => _part;

        public T? As<T>() where T : class, IPart
        {
            return _part as T;
        }
    }
}
