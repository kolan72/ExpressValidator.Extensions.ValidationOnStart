using AdvancedSamples;
using ExpressValidator.Extensions.ValidationOnStart;
using FluentValidation;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Note: The configuration is in appsettings.json in the AdvancedSamples folder

var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("=== Advanced Samples: Demonstrating New AddOptionsWithExpressValidation Overloads ===\n");

// ============================================================================
// Example 1: Post-Configuration with Connection String Building
// ============================================================================
logger.LogInformation("Example 1: Database Options with Post-Configuration");
builder.Services.AddOptionsWithExpressValidation<DatabaseOptions>(
	eb => eb
		.AddProperty(o => o.Server).WithValidation(v => v.NotEmpty())
		.AddProperty(o => o.Database).WithValidation(v => v.NotEmpty())
		.AddProperty(o => o.Timeout).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.MaxRetries).WithValidation(v => v.InclusiveBetween(1, 10))
		.AddProperty(o => o.ConnectionString).WithValidation(v => v.NotEmpty().MinimumLength(20)),
	"Database",
	options =>
	{
		// Post-configure: Build connection string from parts
		options.ConnectionString = $"Server={options.Server};Database={options.Database};" +
		                          $"User Id={options.Username};Password={options.Password};" +
		                          $"Connection Timeout={options.Timeout};";
		logger.LogInformation("  ✓ Connection string built in post-configuration");
	});

// ============================================================================
// Example 2: Binding with IConfiguration Section (Nested Configuration)
// ============================================================================
logger.LogInformation("\nExample 2: API Options with Nested IConfiguration Section");
var apiSection = builder.Configuration.GetSection("App:Api");
builder.Services.AddOptionsWithExpressValidation<ApiOptions>(
	eb => eb
		.AddProperty(o => o.BaseUrl).WithValidation(v => v.NotEmpty().Must(url => url.StartsWith("https://")))
		.AddProperty(o => o.ApiKey).WithValidation(v => v.NotEmpty().MinimumLength(32))
		.AddProperty(o => o.TimeoutSeconds).WithValidation(v => v.GreaterThan(0)),
	apiSection);
logger.LogInformation("  ✓ Bound from nested configuration path: App:Api");

// ============================================================================
// Example 3: Manual Configuration (No Config Binding)
// ============================================================================
logger.LogInformation("\nExample 3: Feature Flags with Manual Configuration");
builder.Services.AddOptionsWithExpressValidation<FeatureFlagOptions>(
	eb => eb
		.AddProperty(o => o.MaxConcurrentUsers).WithValidation(v => v.GreaterThan(0)),
	options =>
	{
		// Configure programmatically based on environment
		var isDevelopment = builder.Environment.IsDevelopment();
		options.EnableNewUI = isDevelopment;
		options.EnableBetaFeatures = isDevelopment;
		options.EnableAnalytics = !isDevelopment;
		options.MaxConcurrentUsers = isDevelopment ? 10 : 1000;
	});

// ============================================================================
// Example 4: Post-Configuration with Path Normalization
// ============================================================================
logger.LogInformation("\nExample 4: File Storage with Path Normalization");
builder.Services.AddOptionsWithExpressValidation<FileStorageOptions>(
	eb => eb
		.AddProperty(o => o.RelativePath).WithValidation(v => v.NotEmpty())
		.AddProperty(o => o.MaxFileSizeBytes).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.FullPath).WithValidation(v => v.NotEmpty()),
	"FileStorage",
	options =>
	{
		// Post-configure: Convert relative path to full path
		options.FullPath = Path.GetFullPath(Path.Combine(
			builder.Environment.ContentRootPath,
			options.RelativePath));
	});

// ============================================================================
// Example 5: Custom Binder Options with Strict Validation
// ============================================================================
logger.LogInformation("\nExample 5: Security Options with Strict Binding");
builder.Services.AddOptionsWithExpressValidation<SecurityOptions>(
	eb => eb
		.AddProperty(o => o.SecretKey).WithValidation(v => v.NotEmpty().MinimumLength(32))
		.AddProperty(o => o.EncryptionAlgorithm).WithValidation(v => v.NotEmpty())
		.AddProperty(o => o.TokenExpirationMinutes).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.RequireHttps).WithValidation(v => v.Equal(true)),
	"Security",
	binderOptions =>
	{
		// Strict binding - fail if unknown configuration keys exist
		binderOptions.ErrorOnUnknownConfiguration = true;
		logger.LogInformation("  ✓ Strict binding enabled (ErrorOnUnknownConfiguration = true)");
	});

// ============================================================================
// Example 6: IConfiguration with Post-Configuration and Complex Validation
// ============================================================================
logger.LogInformation("\nExample 6: Cache Options with Derived Values");
var cacheSection = builder.Configuration.GetSection("Cache");
builder.Services.AddOptionsWithExpressValidation<CacheOptions>(
	eb => eb
		.AddProperty(o => o.DefaultTtlSeconds).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.MaxTtlSeconds).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.MaxCacheSize).WithValidation(v => v.GreaterThan(0))
		.AddProperty(o => o.EffectiveMaxTtl).WithValidation(v => v.GreaterThanOrEqualTo(300)),
	cacheSection,
	options =>
	{
		// Post-configure: Ensure MaxTtl is always >= DefaultTtl
		options.EffectiveMaxTtl = Math.Max(options.MaxTtlSeconds, options.DefaultTtlSeconds);
	});

try
{
	var app = builder.Build();

	logger.LogInformation("All options validated successfully at startup! ✓");

	// ============================================================================
	// Display all configured options
	// ============================================================================

	app.MapGet("/", () => "Advanced Samples - Check console for configuration details");

	app.MapGet("/database", (IOptions<DatabaseOptions> options) => new
	{
		options.Value.Server,
		options.Value.Database,
		options.Value.Timeout,
		options.Value.MaxRetries,
		options.Value.ConnectionString
	});

	app.MapGet("/api", (IOptions<ApiOptions> options) => new
	{
		options.Value.BaseUrl,
		ApiKey = $"{options.Value.ApiKey.AsSpan(0, 8)}...", // Masked
		options.Value.TimeoutSeconds,
		options.Value.EnableRetry
	});

	app.MapGet("/features", (IOptions<FeatureFlagOptions> options) => new
	{
		options.Value.EnableNewUI,
		options.Value.EnableBetaFeatures,
		options.Value.EnableAnalytics,
		options.Value.MaxConcurrentUsers
	});

	app.MapGet("/storage", (IOptions<FileStorageOptions> options) => new
	{
		options.Value.RelativePath,
		options.Value.FullPath,
		MaxFileSizeMB = options.Value.MaxFileSizeBytes / (1024.0 * 1024.0),
		options.Value.AllowedExtensions
	});

	app.MapGet("/security", (IOptions<SecurityOptions> options) => new
	{
		options.Value.EncryptionAlgorithm,
		options.Value.TokenExpirationMinutes,
		options.Value.RequireHttps,
		SecretKeyLength = options.Value.SecretKey.Length
	});

	app.MapGet("/cache", (IOptions<CacheOptions> options) => new
	{
		options.Value.DefaultTtlSeconds,
		options.Value.MaxTtlSeconds,
		options.Value.EffectiveMaxTtl,
		options.Value.MaxCacheSize,
		options.Value.EnableDistributedCache
	});

	app.MapGet("/all", (
		IOptions<DatabaseOptions> db,
		IOptions<ApiOptions> api,
		IOptions<FeatureFlagOptions> features,
		IOptions<FileStorageOptions> storage,
		IOptions<SecurityOptions> security,
		IOptions<CacheOptions> cache) =>
	{
		return new
		{
			Message = "All options configured and validated successfully!"
		};
	});

	logger.LogInformation("Starting web application...");
	logger.LogInformation("Available endpoints:");
	logger.LogInformation("  GET / - Welcome message");
	logger.LogInformation("  GET /all - All configurations summary");
	logger.LogInformation("  GET /database - Database options");
	logger.LogInformation("  GET /api - API options");
	logger.LogInformation("  GET /features - Feature flags");
	logger.LogInformation("  GET /storage - Storage options");
	logger.LogInformation("  GET /security - Security options");
	logger.LogInformation("  GET /cache - Cache options\n");

	await app.RunAsync();
}
catch (OptionsValidationException ove)
{
	logger.LogCritical("❌ Options Validation Failed at Startup!");

	foreach (var failure in ove.Failures)
	{
		logger.LogCritical("  ❌ {Failure}", failure);
	}


	logger.LogCritical(ove, "Options validation exception thrown");
}
catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OptionsValidationException))
{

	logger.LogCritical("❌ Multiple Options Validation Failures at Startup!");

	foreach (var failure in ae
		.Flatten()
		.InnerExceptions
		.Cast<OptionsValidationException>()
		.SelectMany(ex => ex.Failures))
	{
		logger.LogCritical("  ❌ {Failure}", failure);
	}

	logger.LogCritical(ae, "AggregateException thrown");
}
catch (Exception ex)
{
	logger.LogCritical("❌ Unhandled Exception at Startup!");
	logger.LogCritical(ex, "An unhandled exception occurred during application startup.");
}
