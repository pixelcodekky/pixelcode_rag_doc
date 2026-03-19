using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RagBot.Application.Interfaces;
using RagBot.Infrastructure.Persistence;
using RagBot.Infrastructure.Services;

namespace RagBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.UseVector()));

        services.AddScoped<IVectorStore, VectorStore>();
        services.AddScoped<ILLMService, OpenAILLMService>();

        return services;
    }
}
