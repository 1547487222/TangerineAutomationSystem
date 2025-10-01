
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QStandaedPlatform.Engine.Common.Common
{

    public class PinInfo : ILinkNode<PinInfo>, IPartObject, IMarker<Guid>, IDataTransmit
    {
        private readonly List<PinInfo> linkPins = [];
        private readonly ILogger _logger;

        public PinInfo(string pinName, Tool ownerTool, Type dataType, PinType pinType, string desc = "")
        {
            if (string.IsNullOrEmpty(pinName))
            {
                throw new ArgumentNullException(nameof(pinName), $"{nameof(pinName)}Pin name cannot be empty.");
            }
            if (ownerTool == null)
            {
                throw new ArgumentNullException(nameof(ownerTool), $"{nameof(ownerTool)}Pin attachment tool cannot be empty.");
            }
            if (dataType != typeof(QData) && !typeof(QData).IsAssignableFrom(dataType))
            {
                throw new ArgumentException($"PinInfo: <{dataType}> The type is wrong.");
            }
            InternalStructure(pinName, ownerTool, dataType, pinType, desc);
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger(pinName);
        }

        public event EventHandler<PinStateChangedEventArgs> ChildPinStateChanged;

        public event PinDataTransmitEventHandler OnPinDataTransmit;

        public PinInfo? Parent { get; protected set; }

        public List<PinInfo> LinkPins => linkPins;

        public virtual string Name { get; set; }

        public Tool OwnerTool { get; private set; }

        public string OwnerToolName { get; protected set; } = string.Empty;

        public string OwnerToolDisplayName => OwnerTool.DisplayName;

        public Type PinDataType { get; protected set; }

        public PinType PinType { get; protected set; }

        public PinState PinState { get; private set; }

        public virtual string Description { get; set; }

        public virtual Guid Id { get; set; }

        public Guid ParentId => Parent != null ? Parent.Id : default;

        public bool CanLinkTo(PinInfo pinInfo)
        {

            if (PinType == PinType.Input)
            {
                return PinDataType == typeof(QDynamic)
                    || pinInfo.PinDataType == typeof(QDynamic)
                    || PinDataType == pinInfo.PinDataType
                    || PinDataType.IsAssignableFrom(pinInfo.PinDataType);
            }
            else
            {
                return PinDataType == typeof(QDynamic)
                    || pinInfo.PinDataType == typeof(QDynamic)
                    || PinDataType == pinInfo.PinDataType
                    || pinInfo.PinDataType.IsAssignableFrom(PinDataType);
            }
        }
        public void LinkTo(PinInfo pinInfo, bool isMonitorState = true)
        {
            LinkPins.Add(pinInfo);
            pinInfo.Parent = this;
            if (isMonitorState)
                pinInfo.MonitorChildPinStates = true;
        }
        public bool UnLink(PinInfo pinInfo)
        {
            for (int i = linkPins.Count - 1; i >= 0; i--)
            {
                if (linkPins[i].Id == pinInfo.Id)
                {
                    linkPins.RemoveAt(i);
                    pinInfo.Parent = null;
                    return true;
                }
            }
            return false;
        }

        public void TransmitData(PinInfo sourcePin, QData qData)
        {
            var pinDataTransmitEventArgs = new PinDataTransmitEventArgs()
            {
                SourceOwnerTool = OwnerTool,
                PinData = qData,
                TargetPin = this,
                SourcePin = sourcePin,
            };
            OnPinDataTransmit?.Invoke(pinDataTransmitEventArgs);
        }

        public void TransmitData(PinDataTransmitEventArgs pinDataTransmitEventArgs)
        {
            _logger.LogInformation($"PinInfo: <{Name}> TransmitData: <{pinDataTransmitEventArgs}>");
             OnPinDataTransmit?.Invoke(pinDataTransmitEventArgs);
        }

        public PinCachePipeline PinCachePipeline { get; set; }

        public bool MonitorChildPinStates { get; set; }


        protected virtual void OnChildPinStateChanged(PinStateChangedEventArgs pinStateChangedEventArgs)
        {
            ChildPinStateChanged?.Invoke(this, pinStateChangedEventArgs);
        }

        public void HandleChildPinStateChanged(PinStateChangedEventArgs pinStateChangedEventArgs)
        {
            OnChildPinStateChanged(pinStateChangedEventArgs);
            pinStateChangedEventArgs.PinStates.Push(PinState);
            if (!MonitorChildPinStates)
            {

                return;
            }
            Parent?.HandleChildPinStateChanged(pinStateChangedEventArgs);
        }

        private void InternalStructure(string pinName, Tool ownerTool, Type dataType, PinType pinType, string desc = "")
        {
            Name = pinName;
            OwnerTool = ownerTool;
            OwnerToolName = ownerTool.DefineName;
            Description = desc;
            PinDataType = dataType;
            PinType = pinType;
            Id = Guid.NewGuid();
        }
    }
}
