A lightweight library that brings **expressive, fail-fast validation** to .NET configuration options using the [ExpressValidator](https://github.com/kolan72/ExpressValidator) library.  

## ✨ Key Features

- **Fail-fast validation on startup** with `ValidateOnStart`  
- **Startup validation** - catch configuration issues immediately when your app starts
- **Fluent API** - express validation rules in a clean, readable way
- **Expressive syntax** for property-level rules 
- **Seamless integration** with `IValidateOptions<TOptions>` and the Microsoft `Options` pipeline  
- **Configurable validation** mode - stop on first error or continue
- **Detailed error reporting** - with property names
- **No `AbstractValidator` required** – define rules inline, directly in your service registration  

## ⚡ Quick Start

### 1. Define Your Options Class

```csharp
public class MyOptions
{
	public int Option1 { get; set; }
	public int Option2 { get; set; }
}

public class MyOptions2
{
	public int Option3 { get; set; }
	public int Option4 { get; set; }
}
```

### 2. Configure Validation in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

//// All validation in one place, no extra classes needed
builder.Services
	.AddOptionsWithExpressValidation<MyOptions>(
	(eb) =>
		eb.AddProperty(o => o.Option1)
		.WithValidation(o => o.GreaterThan(10))
		.AddProperty(o => o.Option2)
		.WithValidation(o => o.GreaterThan(50)),
			"MyOptions"); // Configuration section name
	.AddOptionsWithExpressValidation<MyOptions2>(
		(eb) =>
		eb.AddProperty(o => o.Option3)
		.WithValidation(o => o.GreaterThan(5))
		.AddProperty(o => o.Option4)
		.WithValidation(o => o.GreaterThan(7)), 
			"MyOptions2");
```

### 3. Handle Validation Errors at Startup

```csharp
var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try
{
	var app = builder.Build();
	app.MapGet("/", () => "Hello!");
	await app.RunAsync();
}
// Handles the standard exception thrown when a single set of options (e.g., MyOptions) fails validation.
catch (OptionsValidationException ove)
{
	foreach (var failure in ove.Failures)
	{
		logger.LogCritical(ove, failure);
	}
}
// Handles the case where validation errors from multiple option types (e.g., both MyOptions and MyOptions2)
catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OptionsValidationException))
{
	foreach (var failure in ae.Flatten().InnerExceptions)
	{
		logger.LogCritical(ae, "A critical exception occurred: {Message}", failure.Message);
	}
}
catch (Exception ex)
{
	logger.LogCritical(ex, "An unhandled exception occurred during application startup.");
}
```

## 🧩 How It Works

### `AddOptionsWithExpressValidation<TOptions>`
Registers options, binds configuration, attaches express validation, and enables `ValidateOnStart`.

```csharp
services.AddOptionsWithExpressValidation<MyOptions>(
	eb => eb.AddProperty(o => o.Option1).WithValidation(o => o.GreaterThan(10)),
	"MyOptions");
```

### `ExpressOptionsValidator<TOptions>`
Implements `IValidateOptions<TOptions>` and runs the express validator against your options.

- Converts validation results into `ValidateOptionsResult`
- Produces per-property error messages
- Supports short-circuit or collect-all failure modes

### `IValidateOptions<TOptions>`
The standard Microsoft interface for options validation.  

This ensures **fail-fast startup validation** and protects against misconfiguration.

## 🏆 Sample

See samples folder for concrete example.