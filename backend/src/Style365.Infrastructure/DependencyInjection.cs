using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Infrastructure.Authentication;
using Style365.Infrastructure.CsvParsing;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.ImageProcessing;
using Style365.Infrastructure.Repositories;
using Style365.Infrastructure.Storage;

namespace Style365.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<Style365DbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(Style365DbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Repository Registration
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();

        // Authentication Settings
        services.Configure<AuthSettings>(authSettings =>
            configuration.GetSection(AuthSettings.SectionName).Bind(authSettings));

        // AWS Configuration (shared credentials for Cognito and S3)
        var awsOptions = configuration.GetAWSOptions("AWS");
        services.AddDefaultAWSOptions(awsOptions);

        // AWS Cognito
        services.AddAWSService<IAmazonCognitoIdentityProvider>();
        services.AddScoped<IAuthenticationService, CognitoAuthenticationService>();

        // AWS S3 Storage
        services.Configure<S3Settings>(s3Settings =>
            configuration.GetSection(S3Settings.SectionName).Bind(s3Settings));
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IStorageService, S3StorageService>();

        // Image Processing
        services.Configure<ImageProcessingSettings>(imageSettings =>
            configuration.GetSection(ImageProcessingSettings.SectionName).Bind(imageSettings));
        services.AddScoped<IImageProcessingService, ImageSharpProcessingService>();

        // CSV Parsing
        services.AddScoped<ICsvParserService, CsvParserService>();

        return services;
    }
}