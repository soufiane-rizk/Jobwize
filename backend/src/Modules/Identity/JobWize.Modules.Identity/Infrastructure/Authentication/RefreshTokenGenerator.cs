using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JobWize.Modules.Identity.Infrastructure.Authentication
{
    public interface IRefreshTokenGenerator
    {
        string Generate();
    }
    internal class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
