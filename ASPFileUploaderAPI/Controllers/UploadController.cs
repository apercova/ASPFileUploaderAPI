using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ASPFileUploaderAPI.Controllers
{
    
    public class UploadController: ApiController
    {

        [HttpPost]
        [Route("api/uploads/")]
        public IHttpActionResult Post()
        {
            return Ok();
        }

        [HttpPost]
        [Route("api/uploads/t/")]
        public IHttpActionResult PostTemp(
            [FromUri] string tid = ""    
        )
        {
            if (string.IsNullOrEmpty(tid))
            {
                tid = $"{Guid.NewGuid().ToString().Replace("-", "")}{Path.GetFileNameWithoutExtension(Path.GetTempFileName())}";
            }
            return Ok(tid);
        }

    }
}