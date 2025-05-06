using Microsoft.AspNetCore.Mvc;
using TestAppAlex.Services;
using TestAppAlex.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TestAppAlex.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase

    {
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;

        public UploadController(IWebHostEnvironment env, IFileService fileService)
        {
            _env = env;
            _fileService = fileService;
        }

        [HttpPost]
        [Route("file")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


            var fileItem = new FileItem
            {
                FileName = file.FileName,
                FileSize = file.Length,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _fileService.AddFileAsync(fileItem);

            return Ok(new { name = file.FileName, size = file.Length });
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null. User not authenticated.");
                return Unauthorized("User not authenticated.");
            }

            var files = await _fileService.GetFilesByUserAsync(userId);

            var result = files.Select(f => new
            {
                name = f.FileName,
                size = f.FileSize,
                uploadedAt = f.UploadedAt
            });

            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Missing file name.");

            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var success = await _fileService.DeleteFileAsync(fileName);

            if (!success)
                return NotFound("File metadata not found in database.");

            return Ok();
        }
        
        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("File name is required.");

            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        
        [HttpGet("storage")]
        public async Task<IActionResult> GetStorageUsage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var files = await _fileService.GetFilesByUserAsync(userId);
            var totalSize = files.Sum(f => f.FileSize);

            return Ok(new { used = totalSize });
        }

        [HttpGet("storage-used")]
        public async Task<IActionResult> GetStorageUsed()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            long used = await _fileService.GetUsedStorageByUserAsync(userId);
            long max = 15L * 1024 * 1024 * 1024; // 15 GB în bytes

            var percent = (int)((double)used / max * 100);

            return Ok(new { usedBytes = used, maxBytes = max, percent });
        }

    }
}
