using ASPFileUploaderAPI.ValueObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ASPFileUploaderAPI.Controllers
{
    [Route("api/forms/")]
    public class CompositeFormController : ApiController
    {
        public async Task<IHttpActionResult> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string uploadDir = HttpContext.Current.Server.MapPath("~/images/");
            var provider = new MultipartFormDataStreamProvider(uploadDir);
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                var form = new CompositeForm()
                {
                    name = provider.FormData.Get("name"),
                    email = provider.FormData.Get("email"),
                    photo = provider.FileData
                    .AsParallel()
                    .Where(f => f.Headers.ContentDisposition.Name.Replace("\"", "").Equals("photo"))
                    .Select(f => f.Headers.ContentDisposition.FileName.Replace("\"", ""))
                    .FirstOrDefault()
                };

                return Ok(form);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}
