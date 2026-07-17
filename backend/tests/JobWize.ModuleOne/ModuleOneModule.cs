using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JobWize.Runtime.UnitTests")]

namespace JobWize.ModuleOne
{
    public sealed class ModuleOneModule : ModuleBase
    {
        public override string Name => "ModuleOne";

        protected override void Configure(IServiceCollection services, IConfiguration configuration)
        {
        }
    }
}