# S3PresignApi

用 C# / .NET 8 實作的 S3 Pre-Signed URL 產生服務：\
客戶端向 API 取得短時效的上傳/下載 URL，不需要持有 AWS 金鑰，直接連 S3 完成檔案傳輸 \
技術：ASP.NET Core、AWS SDK for .NET (S3)、Docker、ECR、ECS Fargate、CloudWatch Logs\
\
使用S3PresignApi有下列幾項優點 - \
安全：前端不需要持有 AWS 憑證，URL 幾分鐘就失效，可以綁定 Content-Type、物件路徑前綴等\
效能：大型檔案不經過API 主機（ECS/Fargate 容器），直接進 S3，此API 只做「簽名」，減少CPU和頻寬壓力\
成本：不會「進 API 一次、再從 API 出去一次」造成雙倍傳輸費，也少了為高吞吐擴容 API 的成本\
追蹤：S3 Access Logs/CloudTrail 很好做追蹤並將權限集中在 IAM/Task Role 管理\
\
Client ──(POST /presign)──> API主機（只算簽名, 幾KB）\
Client ←─(回一條有時效的 S3 URL)\
Client ──(PUT/GET 檔案)──> 直接對 S3 上傳/下載（走 S3 的高擴展管道）

