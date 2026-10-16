using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Common;

public class AdminNotificationHelper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _hubService;

    public AdminNotificationHelper(IUnitOfWork unitOfWork, INotificationHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task NotifyAdminsAsync(string message, string messageAr, string? link, CancellationToken ct = default)
    {
        // Get all Admins and SuperAdmins
        var admins = await _unitOfWork.Repository<AdminUser>().GetAllAsync(ct);

        var notifications = new List<AdminNotification>();
        foreach (var admin in admins)
        {
            var notification = new AdminNotification
            {
                AdminUserId = admin.Id,
                Message = message,
                MessageAr = messageAr,
                Link = link,
                IsRead = false
            };
            
            await _unitOfWork.Repository<AdminNotification>().AddAsync(notification, ct);
            notifications.Add(notification);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Send real-time updates via SignalR
        foreach (var n in notifications)
        {
            var notificationDto = new NotificationDto
            {
                Id = n.Id,
                Message = n.Message, // We will just send the English message, frontend can use translations or display this
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
            // Note: Since AdminUsers connect to the same Hub using their JWT (where ClaimTypes.NameIdentifier is their AdminUser.Id)
            // we can reuse the existing SendNotificationToUserAsync method.
            await _hubService.SendNotificationToUserAsync(n.AdminUserId, notificationDto);
        }
    }
}
