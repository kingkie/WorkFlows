using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yu3zx.Util
{
    public class Result
    {
        public Result(bool success, string error = null)
        {
            IsSuccess = success;
            ResultCode = 0;
            Message = error;
            Exception = null;
        }

        public Result(bool success, long resultCode, string error)
        {
            IsSuccess = success;
            ResultCode = resultCode;
            Message = error;
            Exception = null;
        }

        public Result(bool success, long resultCode, string error, Exception exception)
            : this(success, resultCode, error)
        {
            Exception = exception;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回错误码
        /// </summary>
        public long ResultCode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsSuccess ? "Success" : "Fail");
            if (!string.IsNullOrEmpty(Message))
            {
                sb.Append(" " + Message);
            }
            sb.Append(" " + ResultCode);

            return sb.ToString();
        }

        public static Result Success()
        {
            return new Result(true, 0, string.Empty);
        }

        public static Result Fail(string message)
        {
            return new Result(false, 0, message);
        }

        public static Result Fail(long resultCode, string message)
        {
            return new Result(false, resultCode, message);
        }


        public static Result Fail(long resultCode, string message, Exception exception)
        {
            return new Result(false, resultCode, message, exception);
        }

        public static Result Fail(string message, Exception exception)
        {
            return Fail(0, message, exception);
        }
    }

    /// <summary>
    ///     封装操作结果 返回数据和错误信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        public bool IsSuccess { get; set; }

        public T Value { get; set; }
        public long ResultCode { get; set; }
        public string Message { get; set; }

        public Exception Exception { get; set; }

        public Result(bool success, T value, long resultCode, string error)
            : this(success, resultCode, error, null, value)
        {
            Value = value;
        }

        public Result(bool success, long resultCode, string error, Exception exception = null, T value = default(T))
        {
            IsSuccess = success;
            ResultCode = resultCode;
            Message = error;
            Exception = exception;
            Value = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(IsSuccess ? "Success" : "Fail");
            if (!string.IsNullOrEmpty(Message))
            {
                sb.Append(" " + Message);
            }

            sb.Append($"Value={Value}");

            return sb.ToString();
        }

        public static Result<T> Success(T value) => new Result<T>(true,value, 0, string.Empty);

        public static Result<T> Fail(string message) => new Result<T>(false, 0, message);

        public static Result<T> Fail(long resultCode, string message) => new Result<T>(false, resultCode, message);

        public static Result<T> Fail(long resultCode, string message, Exception exception) => new Result<T>(false, resultCode, message, exception);

        public static Result<T> Fail(string message, Exception exception) => Fail(0, message, exception);
    }
    public static class ResultExtension
    {
        public static Result Convert<T>(this Result<T> ori) => new Result(ori.IsSuccess, ori.ResultCode, ori.Message, ori.Exception);

        public static Result<T> Convert<T>(this Result ori) => new Result<T>(ori.IsSuccess, ori.ResultCode, ori.Message, ori.Exception, default(T));
    }
}
