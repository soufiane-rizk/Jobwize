using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JobWize.Runtime.UnitTests")]

namespace JobWize.ModuleTwo
{
    public sealed class ModuleTwoModule : ModuleBase
    {
        public override string Name => "ModuleOne";

        protected override void Configure(IServiceCollection services, IConfiguration configuration)
        {
            
        }
    }
}
