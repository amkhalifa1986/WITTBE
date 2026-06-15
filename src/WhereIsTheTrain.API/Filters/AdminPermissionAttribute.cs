using System;
using Microsoft.AspNetCore.Mvc;

namespace WhereIsTheTrain.API.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AdminPermissionAttribute : TypeFilterAttribute
{
    public AdminPermissionAttribute(string module, string action) : base(typeof(AdminPermissionFilter))
    {
        Arguments = new object[] { module, action };
    }
}
