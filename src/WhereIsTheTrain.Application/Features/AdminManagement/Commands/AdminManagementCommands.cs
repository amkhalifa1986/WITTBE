using System;
using System.Collections.Generic;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminManagement.DTOs;

namespace WhereIsTheTrain.Application.Features.AdminManagement.Commands;

public record CreateAdminRoleCommand(string Name, string? Description, List<AdminRolePrivilegeDto> Privileges) : IRequest<Result<Guid>>;
public record UpdateAdminRoleCommand(Guid Id, string Name, string? Description, List<AdminRolePrivilegeDto> Privileges) : IRequest<Result<string>>;
public record DeleteAdminRoleCommand(Guid Id) : IRequest<Result<string>>;

public record CreateAdminUserCommand(string Email, string DisplayName, string Password, Guid? RoleId) : IRequest<Result<Guid>>;
public record UpdateAdminUserCommand(Guid Id, string Email, string DisplayName, string? Password, Guid? RoleId) : IRequest<Result<string>>;
public record DeleteAdminUserCommand(Guid Id) : IRequest<Result<string>>;
