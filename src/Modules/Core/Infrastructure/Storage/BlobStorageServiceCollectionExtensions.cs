using Core.Application.Storage;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Storage;

/// <summary>Registers blob storage implementations from configuration.</summary>
public static class BlobStorageServiceCollectionExtensions
{
    /// <summary>Adds <see cref="IBlobStorage"/> according to <see cref="BlobStorageOptions.Provider"/>.</summary>
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<BlobStorageOptions>(configuration, BlobStorageOptions.SectionName);

        services.AddSingleton<IBlobStorage>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlobStorageOptions>>().Value;
            return options.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase)
                ? new AzureBlobStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlobStorageOptions>>())
                : new LocalFileBlobStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlobStorageOptions>>());
        });

        return services;
    }
}
