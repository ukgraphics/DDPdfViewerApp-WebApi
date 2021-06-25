using System;
using System.Runtime.Serialization;

namespace SupportApi.Connection
{
    [Serializable]
    internal class ClientConnectionException : Exception
    {
        public ClientConnectionException()
        {
        }

        public ClientConnectionException(string message) : base(message)
        {
        }

        public ClientConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}