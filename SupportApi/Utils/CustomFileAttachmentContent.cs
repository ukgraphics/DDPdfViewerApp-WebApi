using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SupportApi.Utils
{
    public class CustomFileAttachmentContent : HttpContent
    {

        private readonly Stream _Stream = new MemoryStream();
        public CustomFileAttachmentContent(Stream fileContent, string fileName)
        {
            Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            string contentDispositionValues = $"attachment;filename=\"{fileName}\";filename*=\"{fileName}\"";
            Headers.Add("content-disposition", contentDispositionValues);
            _Stream = fileContent;
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
