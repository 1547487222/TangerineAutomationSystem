using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{


    public class ProcessDebug
    {
        public string ProcessName { get; set; } = string.Empty;
        public ObservableCollection<DebugCommand> Commands { get; set; } = [];
    }


    public class DebugCommand(string commandName,Func<Task> func)
    {
        public string CommandName { get; } = commandName;

        public ICommand Command { get; } = new AsyncRelayCommand(async () => await func());
    }
}
