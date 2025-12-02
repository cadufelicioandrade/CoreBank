using CoreBank.Ledger.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBank.Ledger.API.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLedgerInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<LedgerDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("LedgerConnection"));
            });

            return services;
        }
    }
}
