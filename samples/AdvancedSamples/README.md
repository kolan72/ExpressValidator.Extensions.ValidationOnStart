# Advanced Samples - New Overloads Demonstration

This advanced sample project demonstrates all the new overloads added to the `AddOptionsWithExpressValidation` method.

## What's Demonstrated

### Example 1: Post-Configuration with Connection String Building
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, sectionName, postConfigure)`

Demonstrates how to:
- Bind configuration from a section
- Apply post-configuration to compute derived values
- Build a connection string from individual properties
- Validate the computed value

```csharp
builder.Services.AddOptionsWithExpressValidation<DatabaseOptions>(
    eb => eb
        .AddProperty(o => o.ConnectionString).WithValidation(v => v.NotEmpty()),
    "Database",
    options => {
        options.ConnectionString = $"Server={options.Server};Database={options.Database};...";
    });
```

### Example 2: Binding with IConfiguration Section
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, IConfiguration)`

Demonstrates how to:
- Bind from a nested configuration path using `IConfiguration`
- Access deeply nested settings (e.g., `App:Api`)
- Validate URLs and API keys with custom rules

```csharp
var apiSection = builder.Configuration.GetSection("App:Api");
builder.Services.AddOptionsWithExpressValidation<ApiOptions>(
    eb => eb.AddProperty(o => o.BaseUrl).WithValidation(v => v.Must(url => url.StartsWith("https://"))),
    apiSection);
```

### Example 3: Manual Configuration (No Config Binding)
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, configureOptions)`

Demonstrates how to:
- Configure options programmatically without configuration files
- Use environment-specific logic
- Set feature flags based on deployment environment

```csharp
builder.Services.AddOptionsWithExpressValidation<FeatureFlagOptions>(
    eb => eb.AddProperty(o => o.MaxConcurrentUsers).WithValidation(v => v.GreaterThan(0)),
    options => {
        options.EnableBetaFeatures = builder.Environment.IsDevelopment();
        options.MaxConcurrentUsers = isDevelopment ? 10 : 1000;
    });
```

### Example 4: Post-Configuration with Path Normalization
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, sectionName, postConfigure)`

Demonstrates how to:
- Convert relative paths to absolute paths
- Use environment information in post-configuration
- Validate computed file paths

```csharp
builder.Services.AddOptionsWithExpressValidation<FileStorageOptions>(
    eb => eb.AddProperty(o => o.FullPath).WithValidation(v => v.NotEmpty()),
    "FileStorage",
    options => {
        options.FullPath = Path.GetFullPath(Path.Combine(
            builder.Environment.ContentRootPath,
            options.RelativePath));
    });
```

### Example 5: Custom Binder Options with Strict Validation
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, sectionName, binderOptions)`

Demonstrates how to:
- Enable strict configuration binding
- Fail on unknown configuration keys
- Enforce security best practices

```csharp
builder.Services.AddOptionsWithExpressValidation<SecurityOptions>(
    eb => eb
        .AddProperty(o => o.SecretKey).WithValidation(v => v.MinimumLength(32))
        .AddProperty(o => o.RequireHttps).WithValidation(v => v.Equal(true)),
    "Security",
    binderOptions => {
        binderOptions.ErrorOnUnknownConfiguration = true;
    });
```

### Example 6: IConfiguration with Post-Configuration
**Overload Used:** `AddOptionsWithExpressValidation<TOptions>(action, IConfiguration, postConfigure)`

Demonstrates how to:
- Combine IConfiguration binding with post-configuration
- Compute derived values that depend on multiple properties
- Ensure consistency constraints (e.g., max >= default)

```csharp
var cacheSection = builder.Configuration.GetSection("Cache");
builder.Services.AddOptionsWithExpressValidation<CacheOptions>(
    eb => eb
        .AddProperty(o => o.EffectiveMaxTtl).WithValidation(v => v.GreaterThanOrEqualTo(300)),
    cacheSection,
    options => {
        options.EffectiveMaxTtl = Math.Max(options.MaxTtlSeconds, options.DefaultTtlSeconds);
    });
```

## Expected Output

When you run the sample, you should see console output showing:

```
=== Advanced Samples: Demonstrating New AddOptionsWithExpressValidation Overloads ===

Example 1: Database Options with Post-Configuration
  ✓ Connection string built in post-configuration

Example 2: API Options with Nested IConfiguration Section
  ✓ Bound from nested configuration path: App:Api

Example 3: Feature Flags with Manual Configuration
  ✓ Configured programmatically for Development environment

Example 4: File Storage with Path Normalization
  ✓ Full path computed: D:\MyProjects\...\samples\uploads

Example 5: Security Options with Strict Binding
  ✓ Strict binding enabled (ErrorOnUnknownConfiguration = true)

Example 6: Cache Options with Derived Values
  ✓ Effective max TTL computed: 3600s

======================================================================
All options validated successfully at startup! ✓
======================================================================
```

## Triggering Validation Failures

To see validation failures, modify `appsettings.json`:

### Example: Invalid Database Timeout
```json
"Database": {
  "Timeout": 0  // Invalid: must be > 0
}
```

### Example: Invalid API Key Length
```json
"Api": {
  "ApiKey": "short"  // Invalid: must be at least 32 characters
}
```

### Example: Invalid Security Settings
```json
"Security": {
  "RequireHttps": false  // Invalid: must be true
}
```

## Configuration File Structure

The sample uses `appsettings.json` with this structure:

```json
{
  "Database": {
    "Server": "localhost",
    "Database": "MyAppDb",
    "Username": "admin",
    "Password": "P@ssw0rd123",
    "Timeout": 30,
    "MaxRetries": 3
  },
  "App": {
    "Api": {
      "BaseUrl": "https://api.example.com",
      "ApiKey": "your-api-key-here-min-32-chars-long",
      "TimeoutSeconds": 60,
      "EnableRetry": true
    }
  },
  "FileStorage": {
    "RelativePath": "uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [ ".jpg", ".png", ".pdf", ".docx" ]
  },
  "Security": {
    "SecretKey": "this-is-a-very-long-secret-key-at-least-32-characters",
    "EncryptionAlgorithm": "AES256",
    "TokenExpirationMinutes": 60,
    "RequireHttps": true
  },
  "Cache": {
    "DefaultTtlSeconds": 300,
    "MaxTtlSeconds": 3600,
    "MaxCacheSize": 1000,
    "EnableDistributedCache": false
  }
}
```

## Key Takeaways

1. **Post-Configuration** allows you to compute derived values after binding
2. **IConfiguration sections** enable binding to nested configuration paths
3. **Manual configuration** is perfect for environment-specific feature flags
4. **Custom BinderOptions** provide strict validation and binding control
5. **All overloads** maintain fail-fast validation on startup
6. **Validation happens last** - after all configuration and post-configuration steps

## Comparison with Basic Sample

| Feature | Basic Sample | Advanced Sample |
|---------|-------------|-----------------|
| Configuration Binding | ✓ Section name only | ✓ Multiple binding strategies |
| Post-Configuration | ✗ | ✓ Computed values |
| Manual Configuration | ✗ | ✓ No config file needed |
| Custom Binder Options | ✗ | ✓ Strict binding |
| Nested Config Paths | ✗ | ✓ IConfiguration sections |
| Real-World Scenarios | Basic | Production-ready |

## Use Cases by Overload

| Overload | Best For |
|----------|----------|
| IConfiguration binding | Nested or dynamic configuration paths |
| Custom BinderOptions | Strict security settings, preventing typos |
| Manual configuration | Feature flags, environment-specific settings |
| Post-configuration (section) | Connection strings, derived values |
| Post-configuration (IConfiguration) | Complex transformations with nested configs |

