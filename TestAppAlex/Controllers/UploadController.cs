using Microsoft.AspNetCore.Mvc;
using TestAppAlex.Services;
using TestAppAlex.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        [HttpPost("file")]
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

            var userId = GetCurrentUserId();

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
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

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
                System.IO.File.Delete(filePath);

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
            var userId = GetCurrentUserId();
            var files = await _fileService.GetFilesByUserAsync(userId);
            var totalSize = files.Sum(f => f.FileSize);
            return Ok(new { used = totalSize });
        }

        [HttpGet("storage-used")]
        public async Task<IActionResult> GetStorageUsed()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            long used = await _fileService.GetUsedStorageByUserAsync(userId);
            long max = 15L * 1024 * 1024 * 1024; // 15 GB

            var percent = (int)((double)used / max * 100);
            return Ok(new { usedBytes = used, maxBytes = max, percent });
        }

        [HttpPost("rename")]
        public async Task<IActionResult> RenameFile([FromBody] RenameRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldFileName) || string.IsNullOrWhiteSpace(request.NewFileName))
                return BadRequest();

            var userId = GetCurrentUserId();
            var files = await _fileService.GetFilesByUserAsync(userId);

            var file = files.FirstOrDefault(f => f.FileName == request.OldFileName);
            if (file == null)
                return NotFound();

            var ext = Path.GetExtension(file.FileName);
            var newName = Path.GetFileNameWithoutExtension(request.NewFileName) + ext;

            if (files.Any(f => f.FileName == newName && f != file))
                return Conflict("Un fișier cu acest nume există deja.");

            // Redenumire pe disc
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            var oldPath = Path.Combine(uploadPath, request.OldFileName);
            var newPath = Path.Combine(uploadPath, newName);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Move(oldPath, newPath);
            }

            // Actualizează și în DB
            file.FileName = newName;
            file.FilePath = newPath;

            await _fileService.UpdateFileAsync(file);
            return Ok();
        }

        public class RenameRequest
        {
            public string OldFileName { get; set; } = "";
            public string NewFileName { get; set; } = "";
        }

    }
}
