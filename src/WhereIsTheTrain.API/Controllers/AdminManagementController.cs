using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.AdminManagement.Commands;
using WhereIsTheTrain.Application.Features.AdminManagement.DTOs;
using WhereIsTheTrain.Application.Features.AdminManagement.Queries;
using WhereIsTheTrain.API.Filters;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/admin/management")]
[Authorize(Roles = "Admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdminManagementController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminManagementController(IMediator mediator) => _mediator = mediator;

    // ==========================================
    // 🎭 ROLES ENDPOINTS
    // ==========================================

    [HttpGet("roles")]
    [AdminPermission("AdminManagement", "View")]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _mediator.Send(new GetAdminRolesQuery());
        return Ok(result);
    }

    [HttpPost("roles")]
    [AdminPermission("AdminManagement", "Add")]
    public async Task<IActionResult> CreateRole([FromBody] CreateAdminRoleRequestDto dto)
    {
        var result = await _mediator.Send(new CreateAdminRoleCommand(dto.Name, dto.Description, dto.Privileges));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("roles/{id:guid}")]
    [AdminPermission("AdminManagement", "Edit")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateAdminRoleRequestDto dto)
    {
        var result = await _mediator.Send(new UpdateAdminRoleCommand(id, dto.Name, dto.Description, dto.Privileges));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("roles/{id:guid}")]
    [AdminPermission("AdminManagement", "Delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await _mediator.Send(new DeleteAdminRoleCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 👥 ADMINS ENDPOINTS
    // ==========================================

    [HttpGet("admins")]
    [AdminPermission("AdminManagement", "View")]
    public async Task<IActionResult> GetAdmins()
    {
        var result = await _mediator.Send(new GetAdminUsersQuery());
        return Ok(result);
    }

    [HttpPost("admins")]
    [AdminPermission("AdminManagement", "Add")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminUserRequestDto dto)
    {
        var result = await _mediator.Send(new CreateAdminUserCommand(dto.Email, dto.DisplayName, dto.Password, dto.RoleId));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("admins/{id:guid}")]
    [AdminPermission("AdminManagement", "Edit")]
    public async Task<IActionResult> UpdateAdmin(Guid id, [FromBody] UpdateAdminUserRequestDto dto)
    {
        var result = await _mediator.Send(new UpdateAdminUserCommand(id, dto.Email, dto.DisplayName, dto.Password, dto.RoleId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("admins/{id:guid}")]
    [AdminPermission("AdminManagement", "Delete")]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
        var result = await _mediator.Send(new DeleteAdminUserCommand(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
