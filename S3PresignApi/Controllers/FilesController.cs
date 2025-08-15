using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

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

        // 產生上傳(PUT)的 pre-signed URL
        [HttpPost("presign-upload")]
        public IActionResult PresignUpload([FromQuery] string key, [FromQuery] string contentType = "application/octet-stream", [FromQuery] int expiresMinutes = 5)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest("key required");

            using var s3 = new AmazonS3Client(_region); // 會自動讀本機/環境的 AWS 認證
            var req = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                ContentType = contentType
            };
            var url = s3.GetPreSignedURL(req);
            return Ok(new { uploadUrl = url, key, contentType, expiresAt = req.Expires });
        }

        // 產生下載(GET)的 pre-signed URL
        [HttpGet("{key}/presign-download")]
        public IActionResult PresignDownload([FromRoute] string key, [FromQuery] int expiresMinutes = 5)
        {
            using var s3 = new AmazonS3Client(_region);
            var req = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes)
            };
            var url = s3.GetPreSignedURL(req);
            return Ok(new { downloadUrl = url, key, expiresAt = req.Expires });
        }
    }
}
