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

        private string GetCurrentUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

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
                await file.CopyToAsync(stream);

            var fileItem = new FileItem
            {
                FileName = file.FileName,
                FileSize = file.Length,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                UserId = GetCurrentUserId()
            };

            await _fileService.AddFileAsync(fileItem);
            return new JsonResult(new { name = file.FileName, size = file.Length });
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var files = await _fileService.GetFilesByUserAsync(userId);

            return Ok(files.Select(f => new {
                name = f.FileName,
                size = f.FileSize,
                uploadedAt = f.UploadedAt,
                folderId = f.FolderId 
            }));
        }




        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest();

            var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            var success = await _fileService.DeleteFileAsync(fileName);
            if (!success) return NotFound("File metadata not found in database.");

            return Ok();
        }

        [HttpPost("rename")]
        public async Task<IActionResult> RenameFile([FromBody] RenameRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldFileName) || string.IsNullOrWhiteSpace(request.NewFileName))
                return BadRequest();

            var userId = GetCurrentUserId();
            var files = await _fileService.GetFilesByUserAsync(userId);
            var file = files.FirstOrDefault(f => f.FileName == request.OldFileName);
            if (file == null) return NotFound();

            var ext = Path.GetExtension(file.FileName);
            var newName = Path.GetFileNameWithoutExtension(request.NewFileName) + ext;

            if (files.Any(f => f.FileName == newName && f != file))
                return Conflict("Un fișier cu acest nume există deja.");

            var oldPath = Path.Combine(_env.WebRootPath, "uploads", file.FileName);
            var newPath = Path.Combine(_env.WebRootPath, "uploads", newName);

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Move(oldPath, newPath);

            file.FileName = newName;
            file.FilePath = newPath;

            await _fileService.UpdateFileAsync(file);
            return Ok();
        }

        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest();

            var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpGet("storage")]
        public async Task<IActionResult> GetStorageUsage()
        {
            var userId = GetCurrentUserId();
            var total = await _fileService.GetUsedStorageByUserAsync(userId);
            return Ok(new { used = total });
        }

        [HttpPost("create-folder")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(request?.FolderName)) return BadRequest("Invalid name.");

            try
            {
                var folder = await _fileService.CreateFolderAsync(userId, request.FolderName);
                return Ok(new { folder.Id, folder.Name, folder.CreatedAt });
            }
            catch
            {
                return Conflict("Folder already exists.");
            }
        }

        [HttpGet("folders")]
        public async Task<IActionResult> GetFolders()
        {
            var userId = GetCurrentUserId();
            var folders = await _fileService.GetFoldersByUserAsync(userId);

            return Ok(folders.Select(f => new {
                id = f.Id,
                name = f.Name,
                createdAt = f.CreatedAt
            }));
        }

        [HttpDelete("delete-folder")]
        public async Task<IActionResult> DeleteFolder([FromQuery] int id)
        {
            var userId = GetCurrentUserId();
            var folder = await _fileService.GetFolderByIdAsync(id);
            if (folder == null || folder.UserId != userId) return NotFound();

            await _fileService.DeleteFolderAsync(folder);
            return Ok();
        }

        [HttpGet("folder-files")]
        public async Task<IActionResult> GetFilesInFolder([FromQuery] int id)
        {
            var userId = GetCurrentUserId();
            var files = await _fileService.GetFilesByFolderAsync(userId, id);

            return Ok(files.Select(f => new {
                name = f.FileName,
                size = f.FileSize,
                uploadedAt = f.UploadedAt
            }));
        }




        [HttpGet("download-folder")]
        public async Task<IActionResult> DownloadFolder([FromQuery] int id)
        {
            var userId = GetCurrentUserId();
            var folder = await _fileService.GetFolderByIdAsync(id);
            if (folder == null || folder.UserId != userId) return NotFound();

            var files = await _fileService.GetFilesByFolderAsync(userId, id);
            if (!files.Any()) return NotFound("Folderul este gol.");

            using var zipStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var path = Path.Combine(_env.WebRootPath, "uploads", file.FileName);
                    if (!System.IO.File.Exists(path)) continue;

                    var entry = archive.CreateEntry(file.FileName, System.IO.Compression.CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            zipStream.Position = 0;
            return File(zipStream.ToArray(), "application/zip", $"{folder.Name}.zip");
        }
        public class MoveToFolderRequest
        {
            public List<string> FileNames { get; set; } = new();
            public int FolderId { get; set; }
        }

        [HttpPost("move-to-folder")]
        public async Task<IActionResult> MoveToFolder([FromBody] MoveFilesRequest request)
        {
            var userId = GetCurrentUserId();
            var files = await _fileService.GetFilesByUserAsync(userId);

            var filesToMove = files.Where(f => request.FileNames.Contains(f.FileName)).ToList();

            foreach (var file in filesToMove)
            {
                file.FolderId = request.FolderId;
            }

            await _fileService.UpdateFilesAsync(filesToMove); // <- Ai această linie?
            return Ok();
        }
        
        
        [HttpGet("debug-files")]
        public async Task<IActionResult> DebugFiles()
        {
            var files = await _fileService.GetAllFilesAsync();
            return Ok(files.Select(f => new {
                f.FileName,
                f.FolderId,
                f.UserId
            }));
        }

        [HttpPost("delete-multiple")]
        public async Task<IActionResult> DeleteMultipleFiles([FromBody] List<string> fileNames)
        {
            if (fileNames == null || !fileNames.Any())
                return BadRequest("No file names provided.");

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(uploadPath, fileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            var success = await _fileService.DeleteFilesAsync(fileNames);
            if (!success)
                return NotFound("Some files were not found in the database.");

            return Ok();
        }

        [HttpPost("download-multiple")]
        public async Task<IActionResult> DownloadMultipleFiles([FromBody] List<string> fileNames)
        {
            if (fileNames == null || !fileNames.Any())
                return BadRequest("No file names provided.");

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            using var zipStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var fileName in fileNames)
                {
                    var filePath = Path.Combine(uploadPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        var entry = archive.CreateEntry(fileName, System.IO.Compression.CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            zipStream.Position = 0;
            return File(zipStream.ToArray(), "application/zip", "files.zip");
        }


        public class MoveFilesRequest
        {
            public List<string> FileNames { get; set; } = new();
            public int FolderId { get; set; }
        }


        public class RenameRequest
        {
            public string OldFileName { get; set; } = "";
            public string NewFileName { get; set; } = "";
        }

        public class CreateFolderRequest
        {
            public string FolderName { get; set; } = "";
        }
    }
}
