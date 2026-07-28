using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JobWize.Runtime.UnitTests")]

namespace JobWize.ModuleThree
{
    public sealed class ModuleThreeModule : ModuleBase
    {
        public override string Name => "ModuleThree";

        protected override void Configure(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}
