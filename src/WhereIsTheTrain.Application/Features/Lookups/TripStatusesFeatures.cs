using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;
using WhereIsTheTrain.Application.Features.Trips.DTOs;

namespace WhereIsTheTrain.Application.Features.Lookups;

public record GetTripStatusesQuery() : IRequest<Result<List<TripStatusLookupDto>>>;

public record UpdateTripStatusLookupCommand(Guid Id, string NameAr, string NameEn, string Color) : IRequest<Result<TripStatusLookupDto>>;

public class TripStatusLookupHandlers :
    IRequestHandler<GetTripStatusesQuery, Result<List<TripStatusLookupDto>>>,
    IRequestHandler<UpdateTripStatusLookupCommand, Result<TripStatusLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public TripStatusLookupHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<TripStatusLookupDto>>> Handle(GetTripStatusesQuery request, CancellationToken ct)
    {
        var list = await _unitOfWork.Repository<TripStatusLookup>().GetAllAsync(ct);
        var dtos = list.Select(l => new TripStatusLookupDto
        {
            Id = l.Id,
            Code = l.Code,
            NameAr = l.NameAr,
            NameEn = l.NameEn,
            Color = l.Color
        }).ToList();
        return Result<List<TripStatusLookupDto>>.Success(dtos);
    }

    public async Task<Result<TripStatusLookupDto>> Handle(UpdateTripStatusLookupCommand request, CancellationToken ct)
    {
        var lookup = await _unitOfWork.Repository<TripStatusLookup>().GetByIdAsync(request.Id, ct);
        if (lookup == null) return Result<TripStatusLookupDto>.Failure("Trip Status not found", 404);

        lookup.NameAr = request.NameAr;
        lookup.NameEn = request.NameEn;
        lookup.Color = request.Color;

        await _unitOfWork.Repository<TripStatusLookup>().UpdateAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TripStatusLookupDto>.Success(new TripStatusLookupDto
        {
            Id = lookup.Id,
            Code = lookup.Code,
            NameAr = lookup.NameAr,
            NameEn = lookup.NameEn,
            Color = lookup.Color
        });
    }
}
