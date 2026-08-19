using JobWize.Shared.Application.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace JobWize.Shared.Infrastructure.Security
{
    internal sealed class HttpUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                string? userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userId, out Guid id))
                {
                    throw new InvalidOperationException("The current authenticated user does not have a valid user ID.");
                }

                return id;
            }
        }
    }
}
