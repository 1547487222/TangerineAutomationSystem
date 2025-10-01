using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface ITangerinePrologueAngryService
    {
        Task StartPrologueAsync();
    }

    public interface ITangerineConcludeAngryService
    {
        Task<ConcludeResult> WaitForConcludeAsync();
    }

    public class ConcludeResult
    {
        public bool IsSuccess { get; set; }

        public bool IsCancel { get; set; }

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
