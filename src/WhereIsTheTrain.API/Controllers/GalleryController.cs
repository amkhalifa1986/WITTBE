using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
public class GalleryController : ControllerBase
{
    private readonly IMediator _mediator;

    public GalleryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/gallery")]
    public async Task<IActionResult> GetActiveGallery()
    {
        var result = await _mediator.Send(new GetActiveDashboardGalleryQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/gallery")]
    public async Task<IActionResult> GetAdminGallery()
    {
        var result = await _mediator.Send(new GetAdminDashboardGalleryQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/gallery")]
    public async Task<IActionResult> Create([FromForm] CreateGalleryRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { Message = "No file uploaded." });

        var extension = Path.GetExtension(request.File.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { Message = "Only image files are allowed (.jpg, .jpeg, .png, .gif, .webp)." });

        string imagePath;
        try
        {
            imagePath = await SaveImageAsync(request.File);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to save file: " + ex.Message });
        }

        var command = new CreateDashboardGalleryItemCommand(
            imagePath,
            request.CaptionAr,
            request.CaptionEn,
            request.IsVisible,
            request.Link
        );

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/gallery/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateGalleryRequest request)
    {
        string? newImagePath = null;
        if (request.File != null && request.File.Length > 0)
        {
            var extension = Path.GetExtension(request.File.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { Message = "Only image files are allowed." });

            try
            {
                newImagePath = await SaveImageAsync(request.File);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to save file: " + ex.Message });
            }
        }

        var command = new UpdateDashboardGalleryItemCommand(
            id,
            newImagePath,
            request.CaptionAr,
            request.CaptionEn,
            request.IsVisible,
            request.Link
        );

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/gallery/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleVisibility(Guid id)
    {
        var result = await _mediator.Send(new ToggleDashboardGalleryVisibilityCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/gallery/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDashboardGalleryItemCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "gallery");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/gallery/{fileName}";
    }
}

public class CreateGalleryRequest
{
    public string CaptionAr { get; set; } = string.Empty;
    public string CaptionEn { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public string? Link { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UpdateGalleryRequest
{
    public string CaptionAr { get; set; } = string.Empty;
    public string CaptionEn { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public string? Link { get; set; }
    public IFormFile? File { get; set; }
}
