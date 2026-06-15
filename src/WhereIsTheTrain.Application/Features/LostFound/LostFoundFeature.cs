using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.LostFound;

public class LostFoundCommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LostFoundPostDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? TrainNumber { get; set; }
    public string? ContactInfo { get; set; }
    public bool IsResolved { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<LostFoundCommentDto> Comments { get; set; } = new();
}

// --- Get Lost & Found List (Public - Only Published) ---
public record GetLostFoundListQuery(LostFoundType? TypeFilter = null) : IRequest<Result<List<LostFoundPostDto>>>;

public class GetLostFoundListQueryHandler : IRequestHandler<GetLostFoundListQuery, Result<List<LostFoundPostDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLostFoundListQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<LostFoundPostDto>>> Handle(GetLostFoundListQuery request, CancellationToken ct)
    {
        var posts = request.TypeFilter.HasValue
            ? await _unitOfWork.Repository<LostFoundPost>().FindAsync(p => p.Type == request.TypeFilter.Value && p.Status == LostFoundStatus.Published, ct)
            : await _unitOfWork.Repository<LostFoundPost>().FindAsync(p => p.Status == LostFoundStatus.Published, ct);

        // We need author info, so we load users
        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
        var users = await _unitOfWork.Users.FindAsync(u => authorIds.Contains(u.Id), ct);
        var userDict = users.ToDictionary(u => u.Id);

        var dtos = posts.OrderByDescending(p => p.CreatedAt).Select(p => new LostFoundPostDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = userDict.ContainsKey(p.AuthorId) ? userDict[p.AuthorId].DisplayName : "Unknown",
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Type = p.Type.ToString(),
            TrainNumber = p.TrainNumber,
            ContactInfo = p.ContactInfo,
            IsResolved = p.IsResolved,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt
        }).ToList();

        return Result<List<LostFoundPostDto>>.Success(dtos);
    }
}

// --- Get Lost & Found Details (Public - Only Published, Only Non-Hidden Comments) ---
public record GetLostFoundDetailsQuery(Guid PostId) : IRequest<Result<LostFoundPostDto>>;

public class GetLostFoundDetailsQueryHandler : IRequestHandler<GetLostFoundDetailsQuery, Result<LostFoundPostDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLostFoundDetailsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<LostFoundPostDto>> Handle(GetLostFoundDetailsQuery request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null || post.Status != LostFoundStatus.Published)
            return Result<LostFoundPostDto>.Failure("Post not found.", 404);

        var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, ct);

        // Load comments that are not hidden
        var comments = await _unitOfWork.Repository<LostFoundComment>().FindAsync(c => c.PostId == post.Id && !c.IsHidden, ct);
        var commentAuthorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var commentAuthors = await _unitOfWork.Users.FindAsync(u => commentAuthorIds.Contains(u.Id), ct);
        var commentAuthorDict = commentAuthors.ToDictionary(u => u.Id);

        var commentDtos = comments.OrderBy(c => c.CreatedAt).Select(c => new LostFoundCommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            AuthorName = commentAuthorDict.ContainsKey(c.AuthorId) ? commentAuthorDict[c.AuthorId].DisplayName : "Unknown",
            Content = c.Content,
            IsHidden = c.IsHidden,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Result<LostFoundPostDto>.Success(new LostFoundPostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            Type = post.Type.ToString(),
            TrainNumber = post.TrainNumber,
            ContactInfo = post.ContactInfo,
            IsResolved = post.IsResolved,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt,
            Comments = commentDtos
        });
    }
}

// --- Create Lost & Found Post ---
public record CreateLostFoundPostCommand(
    Guid AuthorId, string Title, string Description, string? ImageUrl,
    LostFoundType Type, string? TrainNumber, string? ContactInfo
) : IRequest<Result<LostFoundPostDto>>;

public class CreateLostFoundPostCommandHandler : IRequestHandler<CreateLostFoundPostCommand, Result<LostFoundPostDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLostFoundPostCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<LostFoundPostDto>> Handle(CreateLostFoundPostCommand request, CancellationToken ct)
    {
        var author = await _unitOfWork.Users.GetByIdAsync(request.AuthorId, ct);
        if (author == null)
            return Result<LostFoundPostDto>.Failure("User not found.", 404);

        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault() ?? new SystemSetting();
        var post = new LostFoundPost
        {
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Type = request.Type,
            TrainNumber = request.TrainNumber,
            ContactInfo = request.ContactInfo,
            Status = settings.LostFoundPostAutoPublish ? LostFoundStatus.Published : LostFoundStatus.New
        };

        await _unitOfWork.Repository<LostFoundPost>().AddAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<LostFoundPostDto>.Success(new LostFoundPostDto
        {
            Id = post.Id,
            AuthorId = author.Id,
            AuthorName = author.DisplayName,
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            Type = post.Type.ToString(),
            TrainNumber = post.TrainNumber,
            ContactInfo = post.ContactInfo,
            IsResolved = false,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt
        }, 201);
    }
}

// --- Mark Post Resolved ---
public record MarkLostFoundResolvedCommand(Guid PostId, Guid UserId) : IRequest<Result<string>>;

public class MarkLostFoundResolvedCommandHandler : IRequestHandler<MarkLostFoundResolvedCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkLostFoundResolvedCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(MarkLostFoundResolvedCommand request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null)
            return Result<string>.Failure("Post not found.", 404);

        if (post.AuthorId != request.UserId)
            return Result<string>.Failure("Only the author can mark this post as resolved.", 403);

        post.IsResolved = true;
        post.Status = LostFoundStatus.Closed; // Resolved posts are automatically Closed
        await _unitOfWork.Repository<LostFoundPost>().UpdateAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success("Post marked as resolved.");
    }
}

// --- Add Comment ---
public record AddLostFoundCommentCommand(Guid PostId, Guid AuthorId, string Content) : IRequest<Result<LostFoundCommentDto>>;

public class AddLostFoundCommentCommandHandler : IRequestHandler<AddLostFoundCommentCommand, Result<LostFoundCommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddLostFoundCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<LostFoundCommentDto>> Handle(AddLostFoundCommentCommand request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null || (post.Status != LostFoundStatus.Published && post.Status != LostFoundStatus.New))
            return Result<LostFoundCommentDto>.Failure("Post not found or unavailable.", 404);

        var author = await _unitOfWork.Users.GetByIdAsync(request.AuthorId, ct);
        if (author == null)
            return Result<LostFoundCommentDto>.Failure("User not found.", 404);

        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault() ?? new SystemSetting();
        var comment = new LostFoundComment
        {
            PostId = request.PostId,
            AuthorId = request.AuthorId,
            Content = request.Content,
            IsHidden = !settings.LostFoundCommentAutoPublish
        };

        await _unitOfWork.Repository<LostFoundComment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<LostFoundCommentDto>.Success(new LostFoundCommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = author.DisplayName,
            Content = comment.Content,
            IsHidden = comment.IsHidden,
            CreatedAt = comment.CreatedAt
        }, 201);
    }
}

// --- Update Comment ---
public record UpdateLostFoundCommentCommand(Guid CommentId, Guid UserId, string Content) : IRequest<Result<LostFoundCommentDto>>;

public class UpdateLostFoundCommentCommandHandler : IRequestHandler<UpdateLostFoundCommentCommand, Result<LostFoundCommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLostFoundCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<LostFoundCommentDto>> Handle(UpdateLostFoundCommentCommand request, CancellationToken ct)
    {
        var comment = await _unitOfWork.Repository<LostFoundComment>().GetByIdAsync(request.CommentId, ct);
        if (comment == null)
            return Result<LostFoundCommentDto>.Failure("Comment not found.", 404);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<LostFoundCommentDto>.Failure("User not found.", 404);

        // Check if author or admin
        if (comment.AuthorId != request.UserId && user.Role != UserRole.Admin)
            return Result<LostFoundCommentDto>.Failure("Unauthorized to edit this comment.", 403);

        comment.Content = request.Content;
        await _unitOfWork.Repository<LostFoundComment>().UpdateAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var author = await _unitOfWork.Users.GetByIdAsync(comment.AuthorId, ct);

        return Result<LostFoundCommentDto>.Success(new LostFoundCommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Content = comment.Content,
            IsHidden = comment.IsHidden,
            CreatedAt = comment.CreatedAt
        });
    }
}

// --- Delete Comment ---
public record DeleteLostFoundCommentCommand(Guid CommentId, Guid UserId) : IRequest<Result<bool>>;

public class DeleteLostFoundCommentCommandHandler : IRequestHandler<DeleteLostFoundCommentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLostFoundCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteLostFoundCommentCommand request, CancellationToken ct)
    {
        var comment = await _unitOfWork.Repository<LostFoundComment>().GetByIdAsync(request.CommentId, ct);
        if (comment == null)
            return Result<bool>.Failure("Comment not found.", 404);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<bool>.Failure("User not found.", 404);

        // Check if author or admin
        if (comment.AuthorId != request.UserId && user.Role != UserRole.Admin)
            return Result<bool>.Failure("Unauthorized to delete this comment.", 403);

        await _unitOfWork.Repository<LostFoundComment>().DeleteAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
