using CommunityToolkit.Mvvm.ComponentModel;
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

    public class InputPortModel : PortBaseModel
    {

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public ObservableCollection<PinInfo> InputPins { get; set; } = [];

        public ObservableCollection<OutputPortModel> ConnectedOutputPorts { get; set; } = [];

        public ObservableCollection<InPutMatchConnectPin> MatchConnectPins { get;  set; } = [];

        private readonly object _lock = new ();
        public override void UpdateMatchConnectPins()
        {
            lock (_lock)
            {
                if (MatchConnectPins.Count == 0)
                {
                    foreach (var item in InputPins)
                    {
                        MatchConnectPins.Add(new InPutMatchConnectPin { InputSourcePin = item });
                    }
                }
                if (ConnectedOutputPorts.Count == 0)
                {
                    MatchConnectPins.Clear();
                    foreach (var item in InputPins)
                    {
                        MatchConnectPins.Add(new InPutMatchConnectPin { InputSourcePin = item });
                    }
                    return;
                }
                if (MatchConnectPins.Count != InputPins.Count)
                {
                    foreach (var pinInfo in InputPins)
                    {
                        if (MatchConnectPins.All(p => p.InputSourcePin.Id != pinInfo.Id))
                        {
                            MatchConnectPins.Add(new InPutMatchConnectPin { InputSourcePin = pinInfo });
                        }
                    }
                    for (global::System.Int32 i = MatchConnectPins.Count - (1); i >= 0; i--)
                    {
                        var item = MatchConnectPins[i];
                        if (!InputPins.Any(p => p.Id == item.InputSourcePin.Id))
                        {
                            foreach (var matchConnectPin in item.OutputTargetMatchPins)
                            {
                                matchConnectPin.OutputMatchConnected = false;
                            }
                            MatchConnectPins.RemoveAt(i);
                        }
                    }
                }
                for (global::System.Int32 i = MatchConnectPins.Count - (1); i >= 0; i--)
                {
                    var item = MatchConnectPins[i];
                    if (item.OutputPorts.Count > 0)
                    {
                        for (global::System.Int32 j = item.OutputPorts.Count - (1); j >= 0; j--)
                        {
                            var x = item.OutputPorts[j];
                            if (!ConnectedOutputPorts.Any(p => p.OwnerId == x.OwnerId))
                            {
                                for (global::System.Int32 k = item.OutputTargetMatchPins.Count - (1); k >= 0; k--)
                                {
                                    if (item.OutputTargetMatchPins[k].OwnerOutputPort.OwnerId == x.OwnerId)
                                    {
                                        item.OutputTargetMatchPins.RemoveAt(k);
                                    }
                                }
                                item.OutputPorts.RemoveAt(j);
                            }
                        }
                    }
                    foreach (var outputPort in ConnectedOutputPorts)
                    {
                        var pins = outputPort.OutputPins.Where(p => p.CanLinkTo(item.InputSourcePin));
                        if (pins.Any())
                        {
                            if (item.OutputPorts.Count == 0 || item.OutputPorts.All(p => p.OwnerId != outputPort.OwnerId))
                            {
                                item.OutputPorts.Add(outputPort);

                            }
                            foreach (var pin in pins)
                            {
                                if (item.OutputTargetMatchPins.All(p => p.TargetOutputPin != pin))
                                    item.OutputTargetMatchPins.Add(new OutputMatchPinViewModel
                                    {
                                        OwnerOutputPort = outputPort,
                                        OwnerSourcePin = item.InputSourcePin,
                                        TargetOutputPin = pin,
                                    });

                            }
                            foreach (var inputPin in outputPort.OutputPins)
                            {
                                if (inputPin.LinkPins.Count != 0)
                                {
                                    foreach (var linkPin in inputPin.LinkPins)
                                    {
                                        foreach (var inputMatch in item.OutputTargetMatchPins)
                                        {
                                            if (inputMatch.OwnerOutputPort.OwnerId == outputPort.OwnerId && inputMatch.OwnerSourcePin.LinkPins.Any(p => p.Id == inputPin.Id)
                                                && inputMatch.TargetOutputPin==inputPin
                                                && !inputMatch.OutputMatchConnected)
                                            {
                                                inputMatch.OutputMatchConnected = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
        }
    }
}
