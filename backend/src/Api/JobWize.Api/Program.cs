using JobWize.Modules.Identity;
using JobWize.Shared;
using JobWize.Shared.Endpoints;

namespace JobWize.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddShared();
            builder.Services.AddIdentityModule(builder.Configuration);

            var app = builder.Build();

            app.MapEndpoints();

            app.Run();
        }
    }
}
