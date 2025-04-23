using System;
using System.Diagnostics;

namespace Xiyu.DeepSeek
{
    [DebuggerDisplay("{Error.ToString()}")]
    public class HttpResponseErrorException : Exception
    {
        public HttpResponseErrorException(Error error) : base(error.ToString())
        {
            Error = error;
        }

        public HttpResponseErrorException(string message) : base(message)
        {
            Error = new Error(message, null, null, null);
        }

        public HttpResponseErrorException(string message, Exception exception) : base(message, exception)
        {
            Error = new Error(message, null, null, null);
        }

        public Error Error { get; }

        public override string ToString()
        {
            return string.Concat(Error.ToString(), base.ToString());
        }
    }
}