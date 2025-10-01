using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public interface ICallService
    {

    }



    public class CallServiceBase : ICallService
    {
        public virtual void InitializeConfiguration() { }
    }

    public class CallStatus
    {
        public int Code { get; set; }

        public string Message { get; set; } = string.Empty;


        public CallStatus(bool result,string message)
        {
            Code = result ? 0 : -1;
            Message = message;
        }



        public CallStatus() { }
        public static CallStatus Success(string message = "Success")
        {
            return new CallStatus {  Message = message };
        }

        public static CallStatus Fail(int code = -1, string message = "Fail")
        {
            return new CallStatus { Code = code, Message = message };
        }

    }
}
