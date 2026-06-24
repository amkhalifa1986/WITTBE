using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.SystemLogs;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/admin/system-log-archives")]
[Authorize(Roles = "Admin")]
public class SystemLogArchivesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly string _archivesDirectory;

    public SystemLogArchivesController(IMediator mediator)
    {
        _mediator = mediator;
        _archivesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Archives", "SystemLogs");
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerArchive()
    {
        var result = await _mediator.Send(new ArchiveUnarchivedLogsCommand());
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public IActionResult ListArchives()
    {
        if (!Directory.Exists(_archivesDirectory))
        {
            return Ok(new List<ArchiveFileDto>());
        }

        var files = Directory.GetFiles(_archivesDirectory, "*.csv")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.Name)
            .Select(f => new ArchiveFileDto
            {
                FileName = f.Name,
                SizeInBytes = f.Length,
                CreatedAt = f.CreationTimeUtc
            })
            .ToList();

        return Ok(files);
    }

    [HttpPost("download")]
    public IActionResult DownloadArchives([FromBody] DownloadArchivesRequest request)
    {
        if (request.FileNames == null || request.FileNames.Count == 0)
        {
            return BadRequest("No files selected.");
        }

        if (!Directory.Exists(_archivesDirectory))
        {
            return NotFound("Archives directory not found.");
        }

        // Validate filenames to prevent path traversal
        var validFiles = request.FileNames
            .Where(f => !f.Contains("..") && !f.Contains("/") && !f.Contains("\\") && f.EndsWith(".csv"))
            .Select(f => Path.Combine(_archivesDirectory, f))
            .Where(f => System.IO.File.Exists(f))
            .ToList();

        if (validFiles.Count == 0)
        {
            return NotFound("None of the requested files were found.");
        }

        if (validFiles.Count == 1)
        {
            // Single file -> Return CSV directly
            var fileBytes = System.IO.File.ReadAllBytes(validFiles[0]);
            return File(fileBytes, "text/csv", Path.GetFileName(validFiles[0]));
        }
        else
        {
            // Multiple files -> Return ZIP
            using var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in validFiles)
                {
                    var fileInfo = new FileInfo(file);
                    var entry = zipArchive.CreateEntry(fileInfo.Name, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(file);
                    fileStream.CopyTo(entryStream);
                }
            }
            
            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "application/zip", $"SystemLogs_Archives_{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        }
    }

    [HttpDelete]
    public IActionResult DeleteArchives([FromBody] DeleteArchivesRequest request)
    {
        if (request.FileNames == null || request.FileNames.Count == 0)
        {
            return BadRequest("No files selected.");
        }

        if (!Directory.Exists(_archivesDirectory))
        {
            return NotFound("Archives directory not found.");
        }

        int deletedCount = 0;
        foreach (var fileName in request.FileNames)
        {
            // Path traversal protection
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                continue;
            }

            var filePath = Path.Combine(_archivesDirectory, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                deletedCount++;
            }
        }

        return Ok(new { Message = $"Successfully deleted {deletedCount} files." });
    }
}

public class ArchiveFileDto
{
    public string FileName { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DownloadArchivesRequest
{
    public List<string> FileNames { get; set; } = new();
}

public class DeleteArchivesRequest
{
    public List<string> FileNames { get; set; } = new();
}
