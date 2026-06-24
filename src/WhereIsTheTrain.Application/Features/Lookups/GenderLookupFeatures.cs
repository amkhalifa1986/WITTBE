using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Lookups;

public class GenderLookupDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}

public record GetGendersQuery() : IRequest<Result<List<GenderLookupDto>>>;

public record CreateGenderCommand(string NameEn, string NameAr) : IRequest<Result<GenderLookupDto>>;

public record UpdateGenderCommand(Guid Id, string NameEn, string NameAr) : IRequest<Result<GenderLookupDto>>;

public record DeleteGenderCommand(Guid Id) : IRequest<Result<bool>>;

public class GenderLookupHandlers :
    IRequestHandler<GetGendersQuery, Result<List<GenderLookupDto>>>,
    IRequestHandler<CreateGenderCommand, Result<GenderLookupDto>>,
    IRequestHandler<UpdateGenderCommand, Result<GenderLookupDto>>,
    IRequestHandler<DeleteGenderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenderLookupHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<GenderLookupDto>>> Handle(GetGendersQuery request, CancellationToken cancellationToken)
    {
        var genders = await _unitOfWork.Repository<GenderLookup>().GetAllAsync(cancellationToken);
        var dtos = genders.Select(g => new GenderLookupDto
        {
            Id = g.Id,
            NameEn = g.NameEn,
            NameAr = g.NameAr
        }).ToList();
        return Result<List<GenderLookupDto>>.Success(dtos);
    }

    public async Task<Result<GenderLookupDto>> Handle(CreateGenderCommand request, CancellationToken cancellationToken)
    {
        var gender = new GenderLookup
        {
            NameEn = request.NameEn,
            NameAr = request.NameAr
        };
        await _unitOfWork.Repository<GenderLookup>().AddAsync(gender, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<GenderLookupDto>.Success(new GenderLookupDto
        {
            Id = gender.Id,
            NameEn = gender.NameEn,
            NameAr = gender.NameAr
        });
    }

    public async Task<Result<GenderLookupDto>> Handle(UpdateGenderCommand request, CancellationToken cancellationToken)
    {
        var gender = await _unitOfWork.Repository<GenderLookup>().GetByIdAsync(request.Id, cancellationToken);
        if (gender == null) return Result<GenderLookupDto>.Failure("Gender not found");

        gender.NameEn = request.NameEn;
        gender.NameAr = request.NameAr;

        await _unitOfWork.Repository<GenderLookup>().UpdateAsync(gender, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<GenderLookupDto>.Success(new GenderLookupDto
        {
            Id = gender.Id,
            NameEn = gender.NameEn,
            NameAr = gender.NameAr
        });
    }

    public async Task<Result<bool>> Handle(DeleteGenderCommand request, CancellationToken cancellationToken)
    {
        var gender = await _unitOfWork.Repository<GenderLookup>().GetByIdAsync(request.Id, cancellationToken);
        if (gender == null) return Result<bool>.Failure("Gender not found");

        await _unitOfWork.Repository<GenderLookup>().DeleteAsync(gender, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
