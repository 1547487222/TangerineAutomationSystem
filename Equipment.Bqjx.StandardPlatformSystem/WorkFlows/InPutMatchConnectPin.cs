using CommunityToolkit.Mvvm.ComponentModel;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{

    public class InPutMatchConnectPin
    {

        public PinInfo InputSourcePin { get; set; }

        public ObservableCollection<OutputPortModel> OutputPorts { get; set; } = [];


        public ObservableCollection<OutputMatchPinViewModel> OutputTargetMatchPins { get; set; } = [];
    }

    public class OutputMatchPinViewModel : ObservableObject
    {
        public OutputPortModel OwnerOutputPort { get; set; }

        public PinInfo OwnerSourcePin { get; set; }

        public PinInfo TargetOutputPin { get; set; }

        private bool _OutputMatchConnected;

        public bool OutputMatchConnected
        {
            get { return _OutputMatchConnected; }
            set
            {
                if (SetProperty(ref _OutputMatchConnected, value))
                {
                    if (_OutputMatchConnected)
                    {
                        if (!OwnerSourcePin.LinkPins.Any(p => p.Id == TargetOutputPin.Id))
                        {
                            OwnerSourcePin.LinkTo(TargetOutputPin);
                        }
                        foreach (var item in OwnerOutputPort.MatchConnectPins)
                        {
                            foreach (var inputMatchPin in item.InputTargetMatchPins)
                            {
                                if (inputMatchPin.OwnerSourcePin.Id == TargetOutputPin.Id
                                    && inputMatchPin.TargetInputPin.Id == OwnerSourcePin.Id)
                                {
                                    inputMatchPin.InputMatchConnected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (OwnerSourcePin.LinkPins.Any(p => p.Id == TargetOutputPin.Id))
                        {
                            OwnerSourcePin.UnLink(TargetOutputPin);
                        }
                        foreach (var item in OwnerOutputPort.MatchConnectPins)
                        {
                            foreach (var inputMatchPin in item.InputTargetMatchPins)
                            {
                                if (inputMatchPin.OwnerSourcePin.Id == TargetOutputPin.Id
                                    && inputMatchPin.TargetInputPin.Id == OwnerSourcePin.Id)
                                {
                                    inputMatchPin.InputMatchConnected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
