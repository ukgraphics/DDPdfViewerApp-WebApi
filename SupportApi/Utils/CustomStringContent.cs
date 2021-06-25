using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SupportApi.Utils
{
    public class CustomStringContent : HttpContent
    {

        private readonly MemoryStream _Stream = new MemoryStream();
        public CustomStringContent(object value)
        {
            Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            var sw = new StreamWriter(_Stream);
            string s = value != null ? (string)value : "";
            sw.Write(s);
            sw.Flush();
            _Stream.Position = 0;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _Stream.Length;
            return true;
        }

    }

}
