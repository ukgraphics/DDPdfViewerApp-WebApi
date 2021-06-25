using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Connection
{
    public class Message
    {

        public Message()
        {
            correlationId = "empty";
        }

        public Message(string correlationId)
        {
            this.correlationId = correlationId;
        }

        public string correlationId { get; set; }

        public dynamic data;

    }
}
