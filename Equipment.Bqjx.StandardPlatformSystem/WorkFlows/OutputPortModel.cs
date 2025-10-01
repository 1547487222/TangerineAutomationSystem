using CommunityToolkit.Mvvm.ComponentModel;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public class OutputPortModel: PortBaseModel
    {

        private bool _isConnected = true;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public ObservableCollection<PinInfo> OutputPins { get; set; } = [];

        public ObservableCollection<InputPortModel> ConnectedInputPorts { get; set; } = [];

        public ObservableCollection<OutputMatchConnectPin> MatchConnectPins { get; private set; } = [];

        private readonly object _lock = new ();
       public  override void UpdateMatchConnectPins()
        {
            lock (_lock)
            {
                if (MatchConnectPins.Count == 0)
                {
                    foreach (var item in OutputPins)
                    {
                        MatchConnectPins.Add(new OutputMatchConnectPin { OutputSourcePin = item });
                    }
                }
                if (ConnectedInputPorts.Count == 0)
                {
                    MatchConnectPins.Clear();
                    foreach (var item in OutputPins)
                    {
                        MatchConnectPins.Add(new OutputMatchConnectPin { OutputSourcePin = item });
                    }
                    return;
                }
                if (MatchConnectPins.Count != OutputPins.Count)
                {
                    foreach (var pinInfo in OutputPins)
                    {
                        if (MatchConnectPins.All(p => p.OutputSourcePin.Id != pinInfo.Id))
                        {
                            MatchConnectPins.Add(new OutputMatchConnectPin { OutputSourcePin = pinInfo });
                        }
                    }
                    for (global::System.Int32 i = MatchConnectPins.Count - (1); i >= 0; i--)
                    {
                        var item = MatchConnectPins[i];
                        if (!OutputPins.Any(p => p.Id == item.OutputSourcePin.Id))
                        {
                            foreach (var matchConnectPin in item.InputTargetMatchPins)
                            {
                                matchConnectPin.InputMatchConnected = false;
                            }
                            MatchConnectPins.RemoveAt(i);
                        }
                    }
                }
                for (global::System.Int32 i = MatchConnectPins.Count - (1); i >= 0; i--)
                {
                    var item = MatchConnectPins[i];
                    if (item.InputPorts.Count > 0)
                    {
                        for (global::System.Int32 j = item.InputPorts.Count - (1); j >= 0; j--)
                        {
                            var x = item.InputPorts[j];
                            if (!ConnectedInputPorts.Any(p => p.OwnerId == x.OwnerId))
                            {
                                for (global::System.Int32 k = item.InputTargetMatchPins.Count - (1); k >= 0; k--)
                                {
                                    if (item.InputTargetMatchPins[k].OwnerInputPort.OwnerId == x.OwnerId)
                                    {
                                        item.InputTargetMatchPins.RemoveAt(k);
                                    }
                                }
                                item.InputPorts.RemoveAt(j);
                            }
                        }
                    }
                    foreach (var inputPort in ConnectedInputPorts)
                    {
                        var pins = inputPort.InputPins.Where(p => p.CanLinkTo(item.OutputSourcePin));
                        if (pins.Any())
                        {
                            if (item.InputPorts.Count == 0 || item.InputPorts.All(p => p.OwnerId != inputPort.OwnerId))
                            {
                                item.InputPorts.Add(inputPort);

                            }
                            foreach (var pin in pins)
                            {
                                if (item.InputTargetMatchPins.All(p => p.TargetInputPin != pin))
                                    item.InputTargetMatchPins.Add(new InputMatchPinViewModel
                                    {
                                        OwnerInputPort = inputPort,
                                        OwnerSourcePin = item.OutputSourcePin,
                                        TargetInputPin = pin,
                                    });
                            }
                            foreach (var inputPin in inputPort.InputPins)
                            {
                                if (inputPin.LinkPins.Count != 0)
                                {
                                    foreach (var linkPin in inputPin.LinkPins)
                                    {
                                        foreach (var inputMatch in item.InputTargetMatchPins)
                                        {
                                            if (inputMatch.OwnerInputPort.OwnerId == inputPort.OwnerId && inputMatch.OwnerSourcePin.LinkPins.Any(p => p.Id == inputPin.Id)
                                                && inputMatch.TargetInputPin== inputPin
                                                && !inputMatch.InputMatchConnected)
                                            {
                                                inputMatch.InputMatchConnected = true;
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
