using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace S3PresignApi.Controllers
{
    [Route("files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly string _bucket;
        private readonly RegionEndpoint _region;

        public FilesController(IConfiguration cfg)
        {
            _bucket = cfg["AWS:Bucket"] ?? throw new Exception("BUCKET env missing");
            _region = RegionEndpoint.GetBySystemName(cfg["AWS:Region"] ?? "ap-northeast-1");
        }

        private static string NormalizeKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required");
            var decoded = WebUtility.UrlDecode(key);
            decoded = decoded.TrimStart('/').Replace('\\', '/');
            return decoded;
        }

        public record PresignUploadRequest(
           string Key,
           string ContentType = "application/octet-stream",
           int ExpiresMinutes = 5
       );

        [HttpPost("presign-upload")]
        public IActionResult PresignUpload([FromBody] PresignUploadRequest req)
        {
            var objectKey = NormalizeKey(req.Key);

            using var s3 = new AmazonS3Client(_region);
            var pre = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(req.ExpiresMinutes),
                ContentType = req.ContentType
            };

            var url = s3.GetPreSignedURL(pre);
            return Ok(new
            {
                uploadUrl = url,
                key = objectKey,
                contentType = req.ContentType,
                expiresAt = pre.Expires
            });
        }
                

        /// <summary>
        /// 下載
        /// GET /files/presign-download?key=uploads/hello.txt&expiresMinutes=5
        /// </summary>
        [HttpGet("presign-download")]
        public IActionResult PresignDownloadByQuery([FromQuery] string key, [FromQuery] int expiresMinutes = 5)
        {
            var objectKey = NormalizeKey(key);

            using var s3 = new AmazonS3Client(_region);
            var pre = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes)
            };

            var url = s3.GetPreSignedURL(pre);
            return Ok(new { downloadUrl = url, key = objectKey, expiresAt = pre.Expires });
        }




    }
}
