using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Admin;
using WhereIsTheTrain.Application.Features.LostFound;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.API.Filters;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    // ==========================================
    // 📍 STOPS ENDPOINTS
    // ==========================================

    [HttpGet("stops")]
    [AdminPermission("Stops", "View")]
    public async Task<IActionResult> GetAllStops()
    {
        var result = await _mediator.Send(new GetAllStopsQuery());
        return Ok(result);
    }

    [HttpPost("stops")]
    [AdminPermission("Stops", "Add")]
    public async Task<IActionResult> CreateStop([FromBody] CreateStopCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("stops/{id:guid}")]
    [AdminPermission("Stops", "Edit")]
    public async Task<IActionResult> UpdateStop(Guid id, [FromBody] UpdateStopRequest request)
    {
        var result = await _mediator.Send(new UpdateStopCommand(
            id, request.NameAr, request.NameEn, request.Code, request.Latitude, request.Longitude, request.CityId, request.DescriptionAr, request.DescriptionEn, request.RailwayPathIds));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("stops/{id:guid}")]
    [AdminPermission("Stops", "Delete")]
    public async Task<IActionResult> DeleteStop(Guid id)
    {
        var result = await _mediator.Send(new DeleteStopCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🏙️ CITIES ENDPOINTS
    // ==========================================

    [HttpGet("cities")]
    [AdminPermission("Lookups", "View")]
    public async Task<IActionResult> GetAllCities()
    {
        var result = await _mediator.Send(new GetAllCitiesQuery());
        return Ok(result);
    }

    [HttpPost("cities")]
    [AdminPermission("Lookups", "Add")]
    public async Task<IActionResult> CreateCity([FromBody] CreateCityCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("cities/{id:guid}")]
    [AdminPermission("Lookups", "Edit")]
    public async Task<IActionResult> UpdateCity(Guid id, [FromBody] UpdateCityRequest request)
    {
        var result = await _mediator.Send(new UpdateCityCommand(id, request.NameAr, request.NameEn, request.GovernorateId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("cities/{id:guid}")]
    [AdminPermission("Lookups", "Delete")]
    public async Task<IActionResult> DeleteCity(Guid id)
    {
        var result = await _mediator.Send(new DeleteCityCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🏛️ GOVERNMENTS ENDPOINTS
    // ==========================================

    [HttpGet("governments")]
    [AdminPermission("Lookups", "View")]
    public async Task<IActionResult> GetAllGovernorates()
    {
        var result = await _mediator.Send(new GetAllGovernoratesQuery());
        return Ok(result);
    }

    [HttpPost("governments")]
    [AdminPermission("Lookups", "Add")]
    public async Task<IActionResult> CreateGovernorate([FromBody] CreateGovernorateCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("governments/{id:guid}")]
    [AdminPermission("Lookups", "Edit")]
    public async Task<IActionResult> UpdateGovernorate(Guid id, [FromBody] UpdateGovernorateRequest request)
    {
        var result = await _mediator.Send(new UpdateGovernorateCommand(id, request.NameAr, request.NameEn));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("governments/{id:guid}")]
    [AdminPermission("Lookups", "Delete")]
    public async Task<IActionResult> DeleteGovernorate(Guid id)
    {
        var result = await _mediator.Send(new DeleteGovernorateCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🚊 TRAIN TYPE ENDPOINTS
    // ==========================================

    [HttpGet("train-types")]
    [AdminPermission("Lookups", "View")]
    public async Task<IActionResult> GetAllTrainTypes()
    {
        var result = await _mediator.Send(new GetTrainTypesQuery());
        return Ok(result);
    }

    [HttpPost("train-types")]
    [AdminPermission("Lookups", "Add")]
    public async Task<IActionResult> CreateTrainType([FromBody] CreateTrainTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("train-types/{id:guid}")]
    [AdminPermission("Lookups", "Edit")]
    public async Task<IActionResult> UpdateTrainType(Guid id, [FromBody] UpdateTrainTypeRequest request)
    {
        var result = await _mediator.Send(new UpdateTrainTypeCommand(id, request.NameAr, request.NameEn, request.MarkerPngUrl));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("train-types/{id:guid}")]
    [AdminPermission("Lookups", "Delete")]
    public async Task<IActionResult> DeleteTrainType(Guid id)
    {
        var result = await _mediator.Send(new DeleteTrainTypeCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("train-types/upload-marker")]
    [AdminPermission("Lookups", "Edit")]
    public async Task<IActionResult> UploadMarker(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Only image files (.png, .jpg, .jpeg, .svg) are allowed.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "markers");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/markers/{fileName}";
        return Ok(new { isSuccess = true, data = fileUrl });
    }

    // ==========================================
    // ⚙️ SYSTEM SETTINGS & MODERATION ENDPOINTS
    // ==========================================

    [HttpGet("system-settings")]
    [AdminPermission("Settings", "View")]
    public async Task<IActionResult> GetSystemSettings()
    {
        var result = await _mediator.Send(new GetSystemSettingsQuery());
        return Ok(result);
    }

    [HttpPut("system-settings")]
    [AdminPermission("Settings", "Edit")]
    public async Task<IActionResult> UpdateSystemSettings([FromBody] UpdateSystemSettingsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("analytics/ads")]
    [AdminPermission("Settings", "View")]
    public async Task<IActionResult> GetAdAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? trainNumber)
    {
        var result = await _mediator.Send(new GetAdAnalyticsQuery(startDate, endDate, trainNumber));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("trips/updates/pending")]
    [AdminPermission("Updates", "View")]
    public async Task<IActionResult> GetPendingLiveUpdates()
    {
        var result = await _mediator.Send(new GetPendingLiveUpdatesQuery());
        return Ok(result);
    }

    [HttpPut("trips/updates/{id:guid}/approve")]
    [AdminPermission("Updates", "Edit")]
    public async Task<IActionResult> ApproveLiveUpdate(Guid id)
    {
        var result = await _mediator.Send(new ApproveLiveUpdateCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("trips/updates/{id:guid}")]
    [AdminPermission("Updates", "Delete")]
    public async Task<IActionResult> DeleteLiveUpdate(Guid id)
    {
        var result = await _mediator.Send(new DeleteLiveUpdateCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("trips/updates/removal-requests")]
    [AdminPermission("Updates", "View")]
    public async Task<IActionResult> GetRemovalRequestedLiveUpdates()
    {
        var result = await _mediator.Send(new GetRemovalRequestedLiveUpdatesQuery());
        return Ok(result);
    }

    [HttpPost("trips/updates/{id:guid}/deny-removal")]
    [AdminPermission("Updates", "Edit")]
    public async Task<IActionResult> DenyLiveUpdateRemoval(Guid id)
    {
        var result = await _mediator.Send(new DenyLiveUpdateRemovalCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🚂 TRAINS ENDPOINTS
    // ==========================================

    [HttpGet("trains")]
    [AdminPermission("Trains", "View")]
    public async Task<IActionResult> GetAllTrains()
    {
        var result = await _mediator.Send(new GetAllTrainsQuery());
        return Ok(result);
    }

    [HttpPost("trains")]
    [AdminPermission("Trains", "Add")]
    public async Task<IActionResult> CreateTrain([FromBody] CreateTrainCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("trains/{id:guid}")]
    [AdminPermission("Trains", "Edit")]
    public async Task<IActionResult> UpdateTrain(Guid id, [FromBody] UpdateTrainRequest request)
    {
        var result = await _mediator.Send(new UpdateTrainCommand(
            id, request.TrainNumber, request.NameAr, request.NameEn, request.DescriptionAr, request.DescriptionEn, request.TrainTypeId, request.RouteStops));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("trains/{id:guid}")]
    [AdminPermission("Trains", "Delete")]
    public async Task<IActionResult> DeleteTrain(Guid id)
    {
        var result = await _mediator.Send(new DeleteTrainCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 📅 TRIPS ENDPOINTS
    // ==========================================

    [HttpGet("trips")]
    [AdminPermission("Trips", "View")]
    public async Task<IActionResult> GetAllTrips()
    {
        var result = await _mediator.Send(new GetAllTripsQuery());
        return Ok(result);
    }

    [HttpPost("trips")]
    [AdminPermission("Trips", "Add")]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("trips/{id:guid}/status")]
    [AdminPermission("Trips", "Edit")]
    public async Task<IActionResult> UpdateTripStatus(Guid id, [FromBody] UpdateTripStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateTripStatusCommand(
            id, request.StatusId, request.ActualDeparture, request.ActualArrival));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("trips/{id:guid}")]
    [AdminPermission("Trips", "Delete")]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        var result = await _mediator.Send(new DeleteTripCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("trips/clear-ended-telemetry")]
    [AdminPermission("Trips", "Delete")]
    public async Task<IActionResult> ClearEndedTripsTelemetry()
    {
        var result = await _mediator.Send(new WhereIsTheTrain.Application.Features.Trips.Commands.ClearEndedTripsTelemetryCommand());
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("trips/{id:guid}/clear-telemetry")]
    [AdminPermission("Trips", "Delete")]
    public async Task<IActionResult> ClearTripTelemetry(Guid id)
    {
        var result = await _mediator.Send(new WhereIsTheTrain.Application.Features.Trips.Commands.ClearTripTelemetryCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 📩 SUGGESTIONS ENDPOINTS
    // ==========================================

    [HttpGet("suggestions/trains")]
    [AdminPermission("Suggestions", "View")]
    public async Task<IActionResult> GetPendingTrainSuggestions()
    {
        var result = await _mediator.Send(new GetPendingTrainSuggestionsQuery());
        return Ok(result);
    }

    [HttpGet("suggestions/stops")]
    [AdminPermission("Suggestions", "View")]
    public async Task<IActionResult> GetPendingStopSuggestions()
    {
        var result = await _mediator.Send(new GetPendingStopSuggestionsQuery());
        return Ok(result);
    }

    [HttpPut("suggestions/trains/{id:guid}/review")]
    [AdminPermission("Suggestions", "Edit")]
    public async Task<IActionResult> ReviewTrainSuggestion(Guid id, [FromBody] ReviewTrainSuggestionRequest request)
    {
        var result = await _mediator.Send(new ReviewTrainSuggestionCommand(
            id,
            request.Status,
            request.AdminNotes,
            request.TrainNumber,
            request.NameAr,
            request.NameEn,
            request.DescriptionAr,
            request.DescriptionEn,
            request.RouteDescriptionEn
        ));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("suggestions/stops/{id:guid}/review")]
    [AdminPermission("Suggestions", "Edit")]
    public async Task<IActionResult> ReviewStopSuggestion(Guid id, [FromBody] ReviewStopSuggestionRequest request)
    {
        var result = await _mediator.Send(new ReviewStopSuggestionCommand(
            id,
            request.Status,
            request.AdminNotes,
            request.Code,
            request.NameAr,
            request.NameEn,
            request.CityId,
            request.NewCityNameAr,
            request.NewCityNameEn,
            request.NewCityGovernorateId,
            request.Latitude,
            request.Longitude,
            request.DescriptionAr,
            request.DescriptionEn
        ));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🔍 LOST & FOUND MODERATION ENDPOINTS
    // ==========================================

    private Guid GetUserId() => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("lost-found/posts")]
    [AdminPermission("LostFound", "View")]
    public async Task<IActionResult> GetAdminLostFoundPosts()
    {
        var result = await _mediator.Send(new GetAdminLostFoundPostsQuery());
        return Ok(result);
    }

    [HttpPut("lost-found/posts/{id:guid}/status")]
    [AdminPermission("LostFound", "Edit")]
    public async Task<IActionResult> UpdateLostFoundPostStatus(Guid id, [FromBody] UpdatePostStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateLostFoundPostStatusCommand(id, request.Status));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("lost-found/posts/{id:guid}")]
    [AdminPermission("LostFound", "Edit")]
    public async Task<IActionResult> AdminUpdateLostFoundPost(Guid id, [FromBody] AdminUpdatePostRequest request)
    {
        var result = await _mediator.Send(new AdminUpdateLostFoundPostCommand(
            id, request.Title, request.Description, request.Type, request.TrainNumber, request.ContactInfo));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("lost-found/posts/{id:guid}")]
    [AdminPermission("LostFound", "Delete")]
    public async Task<IActionResult> AdminDeleteLostFoundPost(Guid id)
    {
        var result = await _mediator.Send(new AdminDeleteLostFoundPostCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("lost-found/comments/{id:guid}/hide")]
    [AdminPermission("LostFound", "Edit")]
    public async Task<IActionResult> AdminHideLostFoundComment(Guid id, [FromBody] AdminHideCommentRequest request)
    {
        var result = await _mediator.Send(new AdminHideLostFoundCommentCommand(id, request.IsHidden));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("lost-found/comments/{id:guid}")]
    [AdminPermission("LostFound", "Edit")]
    public async Task<IActionResult> AdminUpdateLostFoundComment(Guid id, [FromBody] AdminUpdateCommentRequest request)
    {
        var result = await _mediator.Send(new UpdateLostFoundCommentCommand(id, GetUserId(), request.Content));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("lost-found/comments/{id:guid}")]
    [AdminPermission("LostFound", "Delete")]
    public async Task<IActionResult> AdminDeleteLostFoundComment(Guid id)
    {
        var result = await _mediator.Send(new DeleteLostFoundCommentCommand(id, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 👥 USER MANAGEMENT ENDPOINTS
    // ==========================================

    [HttpGet("users")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    [HttpGet("users/analytics/registrations")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetRegistrationAnalytics([FromQuery] string timeframe = "Day", [FromQuery] string genderFilter = "All")
    {
        var result = await _mediator.Send(new GetRegistrationAnalyticsQuery(timeframe, genderFilter));
        return Ok(result);
    }

    [HttpGet("users/analytics/engagement")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetEngagementAnalytics([FromQuery] string timeframe = "Day", [FromQuery] DateTime? dateContext = null)
    {
        var result = await _mediator.Send(new GetEngagementAnalyticsQuery(timeframe, dateContext ?? DateTime.UtcNow));
        return Ok(result);
    }

    [HttpPut("users/{id:guid}/suspend")]
    [AdminPermission("Users", "Edit")]
    public async Task<IActionResult> ToggleUserSuspension(Guid id, [FromBody] ToggleSuspensionRequest request)
    {
        var result = await _mediator.Send(new ToggleUserSuspensionCommand(id, request.IsSuspended));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("users/{id:guid}/role")]
    [AdminPermission("Users", "Edit")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var result = await _mediator.Send(new ChangeUserRoleCommand(id, request.Role));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 📂 BULK CSV IMPORT ENDPOINTS
    // ==========================================

    [HttpPost("stops/import")]
    [AdminPermission("Stops", "Add")]
    public async Task<IActionResult> ImportStops(IFormFile file, [FromQuery] bool ignoreDuplicates = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var result = await _mediator.Send(new ImportStopsCommand(content, ignoreDuplicates));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("trains/import")]
    [AdminPermission("Trains", "Add")]
    public async Task<IActionResult> ImportTrains(IFormFile file, [FromQuery] bool ignoreDuplicates = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var result = await _mediator.Send(new ImportTrainsCommand(content, ignoreDuplicates));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🚨 SERVICE DISRUPTION ENDPOINTS
    // ==========================================

    [HttpGet("disruptions")]
    [AdminPermission("Disruptions", "View")]
    public async Task<IActionResult> GetDisruptions()
    {
        var result = await _mediator.Send(new GetDisruptionsQuery());
        return Ok(result);
    }

    [HttpPost("disruptions")]
    [AdminPermission("Disruptions", "Add")]
    public async Task<IActionResult> CreateDisruption([FromBody] CreateDisruptionRequest request)
    {
        var result = await _mediator.Send(new CreateDisruptionCommand(
            request.TitleAr, request.TitleEn, request.DescriptionAr, request.DescriptionEn, request.AffectedLine));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("disruptions/{id:guid}/deactivate")]
    [AdminPermission("Disruptions", "Edit")]
    public async Task<IActionResult> DeactivateDisruption(Guid id)
    {
        var result = await _mediator.Send(new DeactivateDisruptionCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 👥 FOLLOWERS MANAGEMENT ENDPOINTS
    // ==========================================

    [HttpGet("users/{userId:guid}/following")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetUserFollowings(Guid userId)
    {
        var result = await _mediator.Send(new GetUserFollowingsQuery(userId));
        return Ok(result);
    }

    [HttpDelete("users/{userId:guid}/following/trains/{trainId:guid}")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> AdminUnfollowTrain(Guid userId, Guid trainId)
    {
        var result = await _mediator.Send(new AdminUnfollowTrainCommand(userId, trainId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("users/{userId:guid}/following/trips/{tripId:guid}")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> AdminUnfollowTrip(Guid userId, Guid tripId)
    {
        var result = await _mediator.Send(new AdminUnfollowTripCommand(userId, tripId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("trains/{trainId:guid}/followers")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetTrainFollowers(Guid trainId)
    {
        var result = await _mediator.Send(new GetTrainFollowersQuery(trainId));
        return Ok(result);
    }

    [HttpDelete("trains/{trainId:guid}/followers/{userId:guid}")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> DeleteTrainFollower(Guid trainId, Guid userId)
    {
        var result = await _mediator.Send(new DeleteTrainFollowersCommand(trainId, userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("trains/{trainId:guid}/followers")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> DeleteAllTrainFollowers(Guid trainId)
    {
        var result = await _mediator.Send(new DeleteTrainFollowersCommand(trainId, null));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("trips/{tripId:guid}/followers")]
    [AdminPermission("Users", "View")]
    public async Task<IActionResult> GetTripFollowers(Guid tripId)
    {
        var result = await _mediator.Send(new GetTripFollowersQuery(tripId));
        return Ok(result);
    }

    [HttpDelete("trips/{tripId:guid}/followers/{userId:guid}")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> DeleteTripFollower(Guid tripId, Guid userId)
    {
        var result = await _mediator.Send(new DeleteTripFollowersCommand(tripId, userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("trips/{tripId:guid}/followers")]
    [AdminPermission("Users", "Delete")]
    public async Task<IActionResult> DeleteAllTripFollowers(Guid tripId)
    {
        var result = await _mediator.Send(new DeleteTripFollowersCommand(tripId, null));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 🛤️ RAILWAY PATHS ENDPOINTS
    // ==========================================

    [HttpGet("railway-paths")]
    [AdminPermission("RailwayPaths", "View")]
    public async Task<IActionResult> GetAllRailwayPaths()
    {
        var result = await _mediator.Send(new GetAllRailwayPathsQuery());
        return Ok(result);
    }

    [HttpGet("railway-paths/{id:guid}")]
    [AdminPermission("RailwayPaths", "View")]
    public async Task<IActionResult> GetRailwayPathById(Guid id)
    {
        var result = await _mediator.Send(new GetRailwayPathByIdQuery(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("railway-paths")]
    [AdminPermission("RailwayPaths", "Add")]
    public async Task<IActionResult> CreateRailwayPath([FromBody] CreateRailwayPathRequest request)
    {
        var result = await _mediator.Send(new CreateRailwayPathCommand(request.StartStationId, request.EndStationId, request.Code, request.GeoJsonContent));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("railway-paths/{id:guid}")]
    [AdminPermission("RailwayPaths", "Edit")]
    public async Task<IActionResult> UpdateRailwayPath(Guid id, [FromBody] UpdateRailwayPathRequest request)
    {
        var result = await _mediator.Send(new UpdateRailwayPathCommand(id, request.GeoJsonContent));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("railway-paths/{id:guid}")]
    [AdminPermission("RailwayPaths", "Delete")]
    public async Task<IActionResult> DeleteRailwayPath(Guid id)
    {
        var result = await _mediator.Send(new DeleteRailwayPathCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}

// Request Classes
public class UpdateStopRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid CityId { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public List<Guid> RailwayPathIds { get; set; } = new();
}

public class UpdateCityRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Guid GovernorateId { get; set; }
}

public class UpdateGovernorateRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class UpdateTrainRequest
{
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public Guid? TrainTypeId { get; set; }
    public List<TrainRouteStopInput> RouteStops { get; set; } = new();
}

public class UpdateTripStatusRequest
{
    public Guid StatusId { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }
}

public class ReviewTrainSuggestionRequest
{
    public SuggestionStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? RouteDescriptionEn { get; set; }
}

public class ReviewStopSuggestionRequest
{
    public SuggestionStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Guid? CityId { get; set; }
    public string? NewCityNameAr { get; set; }
    public string? NewCityNameEn { get; set; }
    public Guid? NewCityGovernorateId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
}

public class UpdatePostStatusRequest
{
    public LostFoundStatus Status { get; set; }
}

public class AdminUpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LostFoundType Type { get; set; }
    public string? TrainNumber { get; set; }
    public string? ContactInfo { get; set; }
}

public class AdminHideCommentRequest
{
    public bool IsHidden { get; set; }
}

public class AdminUpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class ToggleSuspensionRequest
{
    public bool IsSuspended { get; set; }
}

public class ChangeRoleRequest
{
    public UserRole Role { get; set; }
}

public class CreateDisruptionRequest
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string? AffectedLine { get; set; }
}

public class CreateRailwayPathRequest
{
    public Guid StartStationId { get; set; }
    public Guid EndStationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string GeoJsonContent { get; set; } = string.Empty;
}

public class UpdateRailwayPathRequest
{
    public string GeoJsonContent { get; set; } = string.Empty;
}

public class UpdateTrainTypeRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? MarkerPngUrl { get; set; }
}
