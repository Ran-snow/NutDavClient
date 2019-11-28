using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace WebDAVClient.Helpers
{
    [Serializable]
    public class HttpException : Exception
    {
        private const int FACILITY_WIN32 = 7;
        private int _httpCode;
        private int _webEventCode;

        internal static int HResultFromLastError(int lastError)
        {
            return lastError >= 0 ? lastError & (int)ushort.MaxValue | 458752 | int.MinValue : lastError;
        }

        /// <summary>
        ///   创建一个新 <see cref="T:System.Web.HttpException" /> 异常基于从 Win32 API 返回的错误代码 <see langword="GetLastError()" /> 方法。
        /// </summary>
        /// <param name="message">显示给客户端，当引发异常的错误消息。</param>
        /// <returns>
        ///   <see cref="T:System.Web.HttpException" /> 基于 Win32 api 调用时返回的错误代码 <see langword="GetLastError()" /> 方法。
        /// </returns>
        public static HttpException CreateFromLastError(string message)
        {
            return new HttpException(message, HttpException.HResultFromLastError(Marshal.GetLastWin32Error()));
        }

        /// <summary>
        ///   新实例初始化 <see cref="T:System.Web.HttpException" /> 类，并创建一个空 <see cref="T:System.Web.HttpException" /> 对象。
        /// </summary>
        public HttpException()
        {
        }

        /// <summary>
        ///   初始化的新实例<see cref="T:System.Web.HttpException" />类使用提供的错误消息。
        /// </summary>
        /// <param name="message">引发异常时显示到客户端的错误消息。</param>
        public HttpException(string message)
          : base(message)
        {
        }

        internal HttpException(string message, Exception innerException, int code)
          : base(message, innerException)
        {
            this._webEventCode = code;
        }

        /// <summary>
        ///   新实例初始化 <see cref="T:System.Web.HttpException" /> 类使用一条错误消息和异常代码。
        /// </summary>
        /// <param name="message">显示给客户端，当引发异常的错误消息。</param>
        /// <param name="hr">定义该错误的异常代码。</param>
        public HttpException(string message, int hr)
          : base(message)
        {
            this.HResult = hr;
        }

        /// <summary>
        ///   新实例初始化 <see cref="T:System.Web.HttpException" /> 类使用一条错误消息和 <see cref="P:System.Exception.InnerException" /> 属性。
        /// </summary>
        /// <param name="message">显示给客户端，当引发异常的错误消息。</param>
        /// <param name="innerException">
        ///   引发了当前异常的 <see cref="P:System.Exception.InnerException" />（如果有）。
        /// </param>
        public HttpException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        /// <summary>
        ///   新实例初始化 <see cref="T:System.Web.HttpException" /> 类使用 HTTP 响应状态代码、 错误消息，与 <see cref="P:System.Exception.InnerException" /> 属性。
        /// </summary>
        /// <param name="httpCode">客户端上显示的 HTTP 响应状态代码。</param>
        /// <param name="message">显示给客户端，当引发异常的错误消息。</param>
        /// <param name="innerException">
        ///   引发了当前异常的 <see cref="P:System.Exception.InnerException" />（如果有）。
        /// </param>
        public HttpException(int httpCode, string message, Exception innerException)
          : base(message, innerException)
        {
            this._httpCode = httpCode;
        }

        /// <summary>
        ///   初始化的新实例<see cref="T:System.Web.HttpException" />类使用的 HTTP 响应状态代码和一条错误消息。
        /// </summary>
        /// <param name="httpCode">发送到客户端对应于此错误的 HTTP 响应状态代码。</param>
        /// <param name="message">引发异常时显示到客户端的错误消息。</param>
        public HttpException(int httpCode, string message)
          : base(message)
        {
            this._httpCode = httpCode;
        }

        /// <summary>
        ///   新实例初始化 <see cref="T:System.Web.HttpException" /> 类使用 HTTP 响应状态代码、 错误消息，以及异常代码。
        /// </summary>
        /// <param name="httpCode">客户端上显示的 HTTP 响应状态代码。</param>
        /// <param name="message">显示给客户端，当引发异常的错误消息。</param>
        /// <param name="hr">定义该错误的异常代码。</param>
        public HttpException(int httpCode, string message, int hr)
          : base(message)
        {
            this.HResult = hr;
            this._httpCode = httpCode;
        }

        /// <summary>
        ///   用序列化数据初始化 <see cref="T:System.Web.HttpException" /> 类的新实例。
        /// </summary>
        /// <param name="info">
        ///   <see cref="T:System.Runtime.Serialization.SerializationInfo" />，它保存关于所引发异常的序列化对象数据。
        /// </param>
        /// <param name="context">
        ///   <see cref="T:System.Runtime.Serialization.StreamingContext" /> 保存有关源或目标的上下文信息。
        /// </param>
        protected HttpException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            this._httpCode = info.GetInt32(nameof(_httpCode));
        }

        /// <summary>
        ///   获取有关异常的信息，并将其添加到 <see cref="T:System.Runtime.Serialization.SerializationInfo" /> 对象。
        /// </summary>
        /// <param name="info">
        ///   <see cref="T:System.Runtime.Serialization.SerializationInfo" />，它保存关于所引发异常的序列化对象数据。
        /// </param>
        /// <param name="context">
        ///   <see cref="T:System.Runtime.Serialization.StreamingContext" /> 保存有关源或目标的上下文信息。
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_httpCode", this._httpCode);
        }

        /// <summary>获取要返回到客户端的 HTTP 响应状态代码。</summary>
        /// <returns>
        ///   表示异常的非零 HTTP 代码或 <see cref="P:System.Exception.InnerException" /> 代码; 否则，HTTP 响应状态代码 500。
        /// </returns>
        public int GetHttpCode()
        {
            return HttpException.GetHttpCodeForException((Exception)this);
        }

        internal static int GetHttpCodeForException(Exception e)
        {
            if (e is HttpException)
            {
                HttpException httpException = (HttpException)e;
                if (httpException._httpCode > 0)
                    return httpException._httpCode;
            }
            else
            {
                if (e is UnauthorizedAccessException)
                    return 401;
                if (e is PathTooLongException)
                    return 414;
            }
            if (e.InnerException != null)
                return HttpException.GetHttpCodeForException(e.InnerException);
            return 500;
        }

        /// <summary>获取与 HTTP 异常相关联的事件代码。</summary>
        /// <returns>表示 Web 事件代码的整数。</returns>
        public int WebEventCode
        {
            get
            {
                return this._webEventCode;
            }
            internal set
            {
                this._webEventCode = value;
            }
        }
    }
}