using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Lookups;

public class DashboardGalleryItemDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string CaptionAr { get; set; } = string.Empty;
    public string CaptionEn { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public string? Link { get; set; }
}

public record GetActiveDashboardGalleryQuery() : IRequest<Result<List<DashboardGalleryItemDto>>>;

public record GetAdminDashboardGalleryQuery() : IRequest<Result<List<DashboardGalleryItemDto>>>;

public record CreateDashboardGalleryItemCommand(string ImagePath, string CaptionAr, string CaptionEn, bool IsVisible, string? Link) : IRequest<Result<DashboardGalleryItemDto>>;

public record UpdateDashboardGalleryItemCommand(Guid Id, string? ImagePath, string CaptionAr, string CaptionEn, bool IsVisible, string? Link) : IRequest<Result<DashboardGalleryItemDto>>;

public record ToggleDashboardGalleryVisibilityCommand(Guid Id) : IRequest<Result<DashboardGalleryItemDto>>;

public record DeleteDashboardGalleryItemCommand(Guid Id) : IRequest<Result<bool>>;

public class DashboardGalleryHandlers :
    IRequestHandler<GetActiveDashboardGalleryQuery, Result<List<DashboardGalleryItemDto>>>,
    IRequestHandler<GetAdminDashboardGalleryQuery, Result<List<DashboardGalleryItemDto>>>,
    IRequestHandler<CreateDashboardGalleryItemCommand, Result<DashboardGalleryItemDto>>,
    IRequestHandler<UpdateDashboardGalleryItemCommand, Result<DashboardGalleryItemDto>>,
    IRequestHandler<ToggleDashboardGalleryVisibilityCommand, Result<DashboardGalleryItemDto>>,
    IRequestHandler<DeleteDashboardGalleryItemCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardGalleryHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<DashboardGalleryItemDto>>> Handle(GetActiveDashboardGalleryQuery request, CancellationToken ct)
    {
        var items = await _unitOfWork.Repository<DashboardGalleryItem>().FindAsync(i => i.IsVisible, ct);
        var dtos = items.Select(i => new DashboardGalleryItemDto
        {
            Id = i.Id,
            ImagePath = i.ImagePath,
            CaptionAr = i.CaptionAr,
            CaptionEn = i.CaptionEn,
            IsVisible = i.IsVisible,
            Link = i.Link
        }).ToList();
        return Result<List<DashboardGalleryItemDto>>.Success(dtos);
    }

    public async Task<Result<List<DashboardGalleryItemDto>>> Handle(GetAdminDashboardGalleryQuery request, CancellationToken ct)
    {
        var items = await _unitOfWork.Repository<DashboardGalleryItem>().GetAllAsync(ct);
        var dtos = items.Select(i => new DashboardGalleryItemDto
        {
            Id = i.Id,
            ImagePath = i.ImagePath,
            CaptionAr = i.CaptionAr,
            CaptionEn = i.CaptionEn,
            IsVisible = i.IsVisible,
            Link = i.Link
        }).ToList();
        return Result<List<DashboardGalleryItemDto>>.Success(dtos);
    }

    public async Task<Result<DashboardGalleryItemDto>> Handle(CreateDashboardGalleryItemCommand request, CancellationToken ct)
    {
        var entity = new DashboardGalleryItem
        {
            ImagePath = request.ImagePath,
            CaptionAr = request.CaptionAr,
            CaptionEn = request.CaptionEn,
            IsVisible = request.IsVisible,
            Link = request.Link
        };

        await _unitOfWork.Repository<DashboardGalleryItem>().AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<DashboardGalleryItemDto>.Success(new DashboardGalleryItemDto
        {
            Id = entity.Id,
            ImagePath = entity.ImagePath,
            CaptionAr = entity.CaptionAr,
            CaptionEn = entity.CaptionEn,
            IsVisible = entity.IsVisible,
            Link = entity.Link
        });
    }

    public async Task<Result<DashboardGalleryItemDto>> Handle(UpdateDashboardGalleryItemCommand request, CancellationToken ct)
    {
        var entity = await _unitOfWork.Repository<DashboardGalleryItem>().GetByIdAsync(request.Id, ct);
        if (entity == null)
            return Result<DashboardGalleryItemDto>.Failure("Gallery item not found.", 404);

        if (!string.IsNullOrEmpty(request.ImagePath))
        {
            entity.ImagePath = request.ImagePath;
        }
        entity.CaptionAr = request.CaptionAr;
        entity.CaptionEn = request.CaptionEn;
        entity.IsVisible = request.IsVisible;
        entity.Link = request.Link;

        await _unitOfWork.Repository<DashboardGalleryItem>().UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<DashboardGalleryItemDto>.Success(new DashboardGalleryItemDto
        {
            Id = entity.Id,
            ImagePath = entity.ImagePath,
            CaptionAr = entity.CaptionAr,
            CaptionEn = entity.CaptionEn,
            IsVisible = entity.IsVisible,
            Link = entity.Link
        });
    }

    public async Task<Result<DashboardGalleryItemDto>> Handle(ToggleDashboardGalleryVisibilityCommand request, CancellationToken ct)
    {
        var entity = await _unitOfWork.Repository<DashboardGalleryItem>().GetByIdAsync(request.Id, ct);
        if (entity == null)
            return Result<DashboardGalleryItemDto>.Failure("Gallery item not found.", 404);

        entity.IsVisible = !entity.IsVisible;

        await _unitOfWork.Repository<DashboardGalleryItem>().UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<DashboardGalleryItemDto>.Success(new DashboardGalleryItemDto
        {
            Id = entity.Id,
            ImagePath = entity.ImagePath,
            CaptionAr = entity.CaptionAr,
            CaptionEn = entity.CaptionEn,
            IsVisible = entity.IsVisible
        });
    }

    public async Task<Result<bool>> Handle(DeleteDashboardGalleryItemCommand request, CancellationToken ct)
    {
        var entity = await _unitOfWork.Repository<DashboardGalleryItem>().GetByIdAsync(request.Id, ct);
        if (entity == null)
            return Result<bool>.Failure("Gallery item not found.", 404);

        // Delete physical file if it's inside local uploads folder
        if (entity.ImagePath.StartsWith("/uploads/gallery/"))
        {
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.ImagePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                try { File.Delete(physicalPath); } catch { /* ignore if locked */ }
            }
        }

        await _unitOfWork.Repository<DashboardGalleryItem>().DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
