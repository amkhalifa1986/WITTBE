using System.Collections.Generic;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminManagement.DTOs;

namespace WhereIsTheTrain.Application.Features.AdminManagement.Queries;

public record GetAdminRolesQuery() : IRequest<Result<List<AdminRoleDetailsDto>>>;
public record GetAdminUsersQuery() : IRequest<Result<List<AdminUserDetailsDto>>>;
