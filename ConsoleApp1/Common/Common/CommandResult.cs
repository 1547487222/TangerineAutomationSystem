using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class CommandResult(string ownerToolName, int commandStatus, string desc = "")
    {
        public enum CommandResultCode
        {
            /// <summary>
            /// 空结果
            /// </summary>
            None,
            Ok,
            Error,
        }
        public string OwnerToolName { get; set; } = ownerToolName;

        public int CommandStatus { get; set; } = commandStatus;

        public string ResultDescription { get; set; } = desc;

        public static CommandResult Ok(string ownerToolName, string desc = "")
        {
            return new CommandResult(ownerToolName, (int)CommandResultCode.Ok, desc);
        }
        public static CommandResult Empty(string ownerToolName, string desc = "")
        {
            return new CommandResult(ownerToolName, (int)CommandResultCode.None, desc);
        }
        public static CommandResult Error(string ownerToolName, string desc = "")
        {
            return new CommandResult(ownerToolName, (int)CommandResultCode.Error, desc);
        }
    }
}
