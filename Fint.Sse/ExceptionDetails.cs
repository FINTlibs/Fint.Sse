using System;

namespace Fint.Sse
{
    internal class ExceptionDetails
    {
        public static String InnermostMessage(Exception ex)
        {
            Exception inner = ex.InnerException;
            String message = ex.Message;
            while (inner != null)
            {
                message = inner.Message;
                inner = inner.InnerException;
            }
            return message;
        }
    }
}
