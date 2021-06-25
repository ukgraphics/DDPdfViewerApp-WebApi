using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Utils
{
    public class NotLicensedException : Exception
    {

        public NotLicensedException(string message): base(message)
        {

        }
    }

    public class DocumentLoaderNotFoundException : Exception
    {

        public DocumentLoaderNotFoundException(string message) : base(message)
        {

        }
    }

}
