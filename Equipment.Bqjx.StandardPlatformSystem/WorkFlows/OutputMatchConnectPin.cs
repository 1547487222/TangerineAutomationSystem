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
    public class OutputMatchConnectPin
    {
        public PinInfo OutputSourcePin { get; set; }

        public ObservableCollection<InputPortModel> InputPorts { get; set; } = [];

        public ObservableCollection<InputMatchPinViewModel> InputTargetMatchPins { get; set; } = [];
    }


    public class InputMatchPinViewModel : ObservableObject
    {

        public InputPortModel OwnerInputPort { get; set; }
        public PinInfo OwnerSourcePin { get; set; }
        public PinInfo TargetInputPin { get; set; }

        private bool _InputMatchConnected;

        public bool InputMatchConnected
        {
            get { return _InputMatchConnected; }
            set
            {
                if (SetProperty(ref _InputMatchConnected, value))
                {
                    if (_InputMatchConnected)
                    {
                        if (!OwnerSourcePin.LinkPins.Any(p => p.Id == TargetInputPin.Id))
                        {
                            OwnerSourcePin.LinkTo(TargetInputPin);
                        }

                        foreach (var item in OwnerInputPort.MatchConnectPins)
                        {
                            foreach (var outputMatchPin in item.OutputTargetMatchPins)
                            {
                                if (outputMatchPin.OwnerSourcePin.Id == TargetInputPin.Id
                                     && outputMatchPin.TargetOutputPin.Id == OwnerSourcePin.Id)
                                {
                                    outputMatchPin.OutputMatchConnected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (OwnerSourcePin.LinkPins.Any(p => p.Id == TargetInputPin.Id))
                        {
                            OwnerSourcePin.UnLink(TargetInputPin);
                        }

                        foreach (var item in OwnerInputPort.MatchConnectPins)
                        {
                            foreach (var outputMatchPin in item.OutputTargetMatchPins)
                            {
                                if (outputMatchPin.OwnerSourcePin.Id == TargetInputPin.Id
                                  && outputMatchPin.TargetOutputPin.Id == OwnerSourcePin.Id)
                                {
                                    outputMatchPin.OutputMatchConnected = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
