using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

public class SystemSettingDto
{
    public bool LostFoundPostAutoPublish { get; set; }
    public bool LostFoundCommentAutoPublish { get; set; }
    public bool TripLiveUpdateAutoPublish { get; set; }
    /// <summary>When true, a user removal request is applied immediately (no admin review).</summary>
    public bool TripLiveUpdateRemovalAutoApprove { get; set; }
    public string AdsEnabledPages { get; set; } = "{}";
    public bool GpsTrackingEnabled { get; set; }
}

public record GetSystemSettingsQuery() : IRequest<Result<SystemSettingDto>>;

public record UpdateSystemSettingsCommand(
    bool LostFoundPostAutoPublish,
    bool LostFoundCommentAutoPublish,
    bool TripLiveUpdateAutoPublish,
    bool TripLiveUpdateRemovalAutoApprove,
    string AdsEnabledPages,
    bool GpsTrackingEnabled
) : IRequest<Result<SystemSettingDto>>;

public class SystemSettingsHandlers :
    IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingDto>>,
    IRequestHandler<UpdateSystemSettingsCommand, Result<SystemSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingsHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<SystemSettingDto>> Handle(GetSystemSettingsQuery request, CancellationToken ct)
    {
        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault();
        if (settings == null)
        {
            return Result<SystemSettingDto>.Success(new SystemSettingDto
            {
                LostFoundPostAutoPublish = true,
                LostFoundCommentAutoPublish = true,
                TripLiveUpdateAutoPublish = true,
                TripLiveUpdateRemovalAutoApprove = false,
                AdsEnabledPages = "{}",
                GpsTrackingEnabled = true
            });
        }

        return Result<SystemSettingDto>.Success(new SystemSettingDto
        {
            LostFoundPostAutoPublish = settings.LostFoundPostAutoPublish,
            LostFoundCommentAutoPublish = settings.LostFoundCommentAutoPublish,
            TripLiveUpdateAutoPublish = settings.TripLiveUpdateAutoPublish,
            TripLiveUpdateRemovalAutoApprove = settings.TripLiveUpdateRemovalAutoApprove,
            AdsEnabledPages = settings.AdsEnabledPages,
            GpsTrackingEnabled = settings.GpsTrackingEnabled
        });
    }

    public async Task<Result<SystemSettingDto>> Handle(UpdateSystemSettingsCommand request, CancellationToken ct)
    {
        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault();
        if (settings == null)
        {
            settings = new SystemSetting
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000009999")
            };
            await _unitOfWork.Repository<SystemSetting>().AddAsync(settings, ct);
        }

        settings.LostFoundPostAutoPublish = request.LostFoundPostAutoPublish;
        settings.LostFoundCommentAutoPublish = request.LostFoundCommentAutoPublish;
        settings.TripLiveUpdateAutoPublish = request.TripLiveUpdateAutoPublish;
        settings.TripLiveUpdateRemovalAutoApprove = request.TripLiveUpdateRemovalAutoApprove;
        settings.AdsEnabledPages = request.AdsEnabledPages;
        settings.GpsTrackingEnabled = request.GpsTrackingEnabled;

        await _unitOfWork.Repository<SystemSetting>().UpdateAsync(settings, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SystemSettingDto>.Success(new SystemSettingDto
        {
            LostFoundPostAutoPublish = settings.LostFoundPostAutoPublish,
            LostFoundCommentAutoPublish = settings.LostFoundCommentAutoPublish,
            TripLiveUpdateAutoPublish = settings.TripLiveUpdateAutoPublish,
            TripLiveUpdateRemovalAutoApprove = settings.TripLiveUpdateRemovalAutoApprove,
            AdsEnabledPages = settings.AdsEnabledPages,
            GpsTrackingEnabled = settings.GpsTrackingEnabled
        });
    }
}
