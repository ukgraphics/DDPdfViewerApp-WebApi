using Microsoft.AspNetCore.Mvc;
using SupportApi.Controllers;

namespace WebAPIApplication1.Controllers
{
    [Route("api/pdf-viewer")]
    [ApiController]
    public class SupportApiController : GcPdfViewerController
    {

    }
}
