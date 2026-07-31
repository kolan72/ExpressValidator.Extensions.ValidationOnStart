namespace AdvancedSamples
{
	/// <summary>
	/// Database configuration options with computed connection string
	/// Demonstrates: Post-configuration and derived values
	/// </summary>
	public class DatabaseOptions
	{
		public string Server { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public int Timeout { get; set; }
		public int MaxRetries { get; set; }
		// Computed property - built from other properties
		public string ConnectionString { get; set; } = string.Empty;
	}

	/// <summary>
	/// API configuration with nested settings
	/// Demonstrates: Binding with IConfiguration section
	/// </summary>
	public class ApiOptions
	{
		public string BaseUrl { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;
		public int TimeoutSeconds { get; set; }
		public bool EnableRetry { get; set; }
	}

	/// <summary>
	/// Feature flags configured programmatically
	/// Demonstrates: Manual configuration without config binding
	/// </summary>
	public class FeatureFlagOptions
	{
		public bool EnableNewUI { get; set; }
		public bool EnableBetaFeatures { get; set; }
		public bool EnableAnalytics { get; set; }
		public int MaxConcurrentUsers { get; set; }
	}

	/// <summary>
	/// File storage options with path normalization
	/// Demonstrates: Post-configuration for path processing
	/// </summary>
	public class FileStorageOptions
	{
		public string RelativePath { get; set; } = string.Empty;
		public long MaxFileSizeBytes { get; set; }
		public string[] AllowedExtensions { get; set; } = [];

		// Computed full path
		public string FullPath { get; set; } = string.Empty;
	}

	/// <summary>
	/// Security options with strict binding
	/// Demonstrates: Custom BinderOptions with ErrorOnUnknownConfiguration
	/// </summary>
	public class SecurityOptions
	{
		public string SecretKey { get; set; } = string.Empty;
		public string EncryptionAlgorithm { get; set; } = string.Empty;
		public int TokenExpirationMinutes { get; set; }
		public bool RequireHttps { get; set; }
	}

	/// <summary>
	/// Cache configuration with derived TTL
	/// Demonstrates: Multiple validation rules with post-configuration
	/// </summary>
	public class CacheOptions
	{
		public int DefaultTtlSeconds { get; set; }
		public int MaxTtlSeconds { get; set; }
		public int MaxCacheSize { get; set; }
		public bool EnableDistributedCache { get; set; }

		// Computed - ensures max is always >= default
		public int EffectiveMaxTtl { get; set; }
	}
}
