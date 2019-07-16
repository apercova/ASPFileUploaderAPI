using ASPFileUploaderAPI.ValueObject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ASPFileUploaderAPI.Controllers
{
    [Route("api/files/")]
    public class FilesController : ApiController
    {
        private static readonly string TEMP_DIR = ConfigurationManager.AppSettings["tempDir"];
        private static readonly string UPLOAD_DIR = ConfigurationManager.AppSettings["uploadDir"];
        private static readonly string TEMP_PATH = ConfigurationManager.AppSettings["tempPath"];
        private static readonly string UPLOAD_PATH = ConfigurationManager.AppSettings["uploadPath"];
        private static readonly string Base64ImageDataRegex = "(?:[Dd][Aa][Tt][Aa]\\s*\\:\\s*([Ii][Mm][Aa][Gg][Ee]\\s*\\/\\s*([^\\s]*?))?\\s*;\\s*[Bb][Aa][Ss][Ee]64\\s*,\\s*)(.+)";



        [HttpGet]
        public IHttpActionResult GetFileMetadata([FromUri(Name = "name")] string fileName, [FromUri] bool temp = false)
        {
            if (fileName == null)
            {
                return BadRequest("missing search filters");
            }

            string uploadDir = temp ? HttpContext.Current.Server.MapPath(TEMP_DIR) : HttpContext.Current.Server.MapPath(UPLOAD_DIR);
            string uploadPath = temp ? TEMP_PATH : UPLOAD_PATH;

            string localFilePath = Path.Combine(uploadDir, fileName);
            if (!File.Exists(localFilePath))
            {
                return NotFound();
            }

            /*Get file length*/
            FileInfo f = new FileInfo(localFilePath);
            long fileLength = f.Length;

            /*Calculate SHA1 hash*/
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] sha1Bytes = sha1.ComputeHash(File.ReadAllBytes(localFilePath));
            string fileSha1Hash = BitConverter.ToString(sha1.ComputeHash(sha1Bytes)).Replace("-", "");

            string fileUri = $"{uploadPath}/{fileName}";
            string fileInfoUri = $"/api/files/?name={fileName}&temp={temp}";

            return Ok(new FormFileResponseVO("", fileName, fileLength, fileSha1Hash, fileUri, fileInfoUri));


        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadFiles([FromUri] bool keepname = false, [FromUri] bool temp = false)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string uploadDir = temp ? HttpContext.Current.Server.MapPath(TEMP_DIR) : HttpContext.Current.Server.MapPath(UPLOAD_DIR);
            string uploadPath = temp ? TEMP_PATH : UPLOAD_PATH;
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var provider = new MultipartFormDataStreamProvider(uploadDir);
            try
            {
                var uploadedFiles = new List<FormFileResponseVO>();
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var file in provider.FileData)
                {

                    string fileName = Path.GetFileName(file.Headers.ContentDisposition.FileName.Trim('"'));
                    keepname = (keepname && fileName != null);

                    /*Calculate filename*/
                    string uploadedFileName = (keepname)
                        ? fileName
                        : $"{Guid.NewGuid()}{Path.GetExtension(fileName)}".Replace("-", "");

                    /*Get file length*/
                    FileInfo f = new FileInfo(file.LocalFileName);
                    long fileLength = f.Length;

                    /*Calculate SHA1 hash*/
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    byte[] sha1Bytes = sha1.ComputeHash(File.ReadAllBytes(file.LocalFileName));
                    string fileSha1Hash = BitConverter.ToString(sha1.ComputeHash(sha1Bytes)).Replace("-", "");

                    
                    /*Calculate fileuri*/
                    string localFilePath = Path.Combine(uploadDir, uploadedFileName);
                    string fileUri = $"{uploadPath}/{uploadedFileName}";
                    string fileInfoUri = $"/api/files/?name={uploadedFileName}&temp={temp}";

                    if (keepname && File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }
                    File.Move(file.LocalFileName, localFilePath);
                    uploadedFiles.Add(
                        new FormFileResponseVO(
                            file.Headers.ContentDisposition.Name.Trim('"'), 
                            uploadedFileName, fileLength, fileSha1Hash, fileUri, fileInfoUri)
                    );

                }
                return Ok(uploadedFiles);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        [Route("api/files/base64-images/")]
        public IHttpActionResult UploadB64images([FromBody] List<Base64ImageRequestVO> images, [FromUri] bool keepname = false, [FromUri] bool temp = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string uploadDir = temp ? HttpContext.Current.Server.MapPath(TEMP_DIR) : HttpContext.Current.Server.MapPath(UPLOAD_DIR);
                string uploadPath = temp ? TEMP_PATH : UPLOAD_PATH;
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var uploadedImages = new List<FormFileResponseVO>();
                foreach (Base64ImageRequestVO image in images)
                {
                    string fileName = Path.GetFileName(image.FileName);
                    keepname = (keepname && fileName != null);

                    /*Calculate filename*/
                    string uploadedFileName = (keepname)
                        ? fileName
                        : $"{Guid.NewGuid()}".Replace("-", "");

                    Match match = Regex.Match(image.Data, @Base64ImageDataRegex, RegexOptions.IgnoreCase);

                    string localFileName;
                    if (match.Success)
                    {
                        string mimeExt = match.Groups[2].Value;
                        string data = match.Groups[3].Value;
                        data = data.Replace('-', '+');
                        data = data.Replace('_', '/');

                        uploadedFileName = Path.HasExtension(uploadedFileName)
                            ? uploadedFileName
                            : (!string.IsNullOrEmpty(mimeExt)
                                    ? $"{uploadedFileName}.{mimeExt}"
                                    : $"{uploadedFileName}{Path.GetExtension(fileName)}"
                              );

                        localFileName = Path.Combine(uploadDir, uploadedFileName);
                        File.WriteAllBytes(localFileName, Convert.FromBase64String(data));
                    }
                    else
                    {
                        uploadedFileName = Path.HasExtension(fileName)
                            ? $"{uploadedFileName}{Path.GetExtension(fileName)}"
                            : uploadedFileName;

                        // save image data as is
                        localFileName = Path.Combine(uploadDir, uploadedFileName);
                        File.WriteAllBytes(localFileName, Convert.FromBase64String(image.Data));
                    }

                    /*Get file length*/
                    FileInfo f = new FileInfo(localFileName);
                    long fileLength = f.Length;

                    /*Calculate SHA1 hash*/
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    byte[] sha1Bytes = sha1.ComputeHash(File.ReadAllBytes(localFileName));
                    string fileSha1Hash = BitConverter.ToString(sha1.ComputeHash(sha1Bytes)).Replace("-", "");

                    string fileUri = $"{uploadPath}/{uploadedFileName}";
                    string fileInfoUri = $"/api/files/?name={uploadedFileName}&temp={temp}";
                    uploadedImages.Add(
                        new FormFileResponseVO(image.Name, uploadedFileName, fileLength, fileSha1Hash, fileUri, fileInfoUri)
                    );
                }
                return Ok(uploadedImages);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}