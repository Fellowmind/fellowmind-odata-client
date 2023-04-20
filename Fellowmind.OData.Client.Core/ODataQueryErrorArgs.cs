using System;
using System.Collections.Generic;
using System.Text;

namespace Fellowmind.OData.Client.Core
{
    public class ODataQueryErrorArgs
    {
        public ODataQueryErrorArgs(string message, ODataQueryErrorHandling errorHandling, int retryCount, Exception ex)
        {
            Message = message;
            ErrorHandling = errorHandling;
            RetryCount = retryCount;
            Exception = ex;
        }

        public string Message { get; private set; }
        public int RetryCount { get; private set; }
        public Exception Exception { get; private set; }
        public ODataQueryErrorHandling ErrorHandling { get; set; }
    }
}
