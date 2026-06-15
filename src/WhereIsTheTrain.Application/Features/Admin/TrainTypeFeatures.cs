using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

public class TrainTypeDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? MarkerPngUrl { get; set; }
}

public record GetTrainTypesQuery() : IRequest<Result<List<TrainTypeDto>>>;

public record CreateTrainTypeCommand(
    string NameAr, string NameEn, string? MarkerPngUrl
) : IRequest<Result<TrainTypeDto>>;

public record UpdateTrainTypeCommand(
    Guid Id, string NameAr, string NameEn, string? MarkerPngUrl
) : IRequest<Result<TrainTypeDto>>;

public record DeleteTrainTypeCommand(Guid Id) : IRequest<Result<bool>>;

public class TrainTypeHandlers :
    IRequestHandler<GetTrainTypesQuery, Result<List<TrainTypeDto>>>,
    IRequestHandler<CreateTrainTypeCommand, Result<TrainTypeDto>>,
    IRequestHandler<UpdateTrainTypeCommand, Result<TrainTypeDto>>,
    IRequestHandler<DeleteTrainTypeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public TrainTypeHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<TrainTypeDto>>> Handle(GetTrainTypesQuery request, CancellationToken ct)
    {
        var types = await _unitOfWork.Repository<TrainType>().GetAllAsync(ct);
        var dtos = types.Select(t => new TrainTypeDto
        {
            Id = t.Id,
            NameAr = t.NameAr,
            NameEn = t.NameEn,
            MarkerPngUrl = t.MarkerPngUrl
        }).ToList();

        return Result<List<TrainTypeDto>>.Success(dtos);
    }

    public async Task<Result<TrainTypeDto>> Handle(CreateTrainTypeCommand request, CancellationToken ct)
    {
        var type = new TrainType
        {
            Id = Guid.NewGuid(),
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            MarkerPngUrl = request.MarkerPngUrl
        };

        await _unitOfWork.Repository<TrainType>().AddAsync(type, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TrainTypeDto>.Success(new TrainTypeDto
        {
            Id = type.Id,
            NameAr = type.NameAr,
            NameEn = type.NameEn,
            MarkerPngUrl = type.MarkerPngUrl
        });
    }

    public async Task<Result<TrainTypeDto>> Handle(UpdateTrainTypeCommand request, CancellationToken ct)
    {
        var type = await _unitOfWork.Repository<TrainType>().GetByIdAsync(request.Id, ct);
        if (type == null)
            return Result<TrainTypeDto>.Failure("Train type not found.", 404);

        type.NameAr = request.NameAr;
        type.NameEn = request.NameEn;
        type.MarkerPngUrl = request.MarkerPngUrl;

        await _unitOfWork.Repository<TrainType>().UpdateAsync(type, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TrainTypeDto>.Success(new TrainTypeDto
        {
            Id = type.Id,
            NameAr = type.NameAr,
            NameEn = type.NameEn,
            MarkerPngUrl = type.MarkerPngUrl
        });
    }

    public async Task<Result<bool>> Handle(DeleteTrainTypeCommand request, CancellationToken ct)
    {
        var type = await _unitOfWork.Repository<TrainType>().GetByIdAsync(request.Id, ct);
        if (type == null)
            return Result<bool>.Failure("Train type not found.", 404);

        // Disassociate related trains
        var trains = await _unitOfWork.Repository<Train>().FindAsync(t => t.TrainTypeId == request.Id, ct);
        foreach (var t in trains)
        {
            t.TrainTypeId = null;
            await _unitOfWork.Repository<Train>().UpdateAsync(t, ct);
        }

        await _unitOfWork.Repository<TrainType>().DeleteAsync(type, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
