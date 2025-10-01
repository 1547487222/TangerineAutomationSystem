using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Devices
{
    public class Result<T> : IResult<T>
    {
        public T Content { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public int ErrorCode { get; set; }

        public static Result<T> CreateErrorResult(string errorMessage = "")
        {
            return new Result<T> { IsSuccess = false, Message = errorMessage };
        }
        public static Result<T> CreateFinishContentResult(T content)
        {
            return new Result<T>
            {
                Content = content,
                IsSuccess = true
            };
        }
    }

    public class Result : IResult<object>
    {
        public object Content { get; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }

        public static Result CreateErrorResult(string errorMessage = "")
        {
            return new Result { IsSuccess = false, Message = errorMessage };
        }
    }
}
