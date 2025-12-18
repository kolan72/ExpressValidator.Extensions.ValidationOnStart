A lightweight library that brings **expressive, fail-fast validation** to .NET configuration options using the power of `FluentValidation` via the [ExpressValidator](https://github.com/kolan72/ExpressValidator) library.  

## ✨ Key Features

- **Fail-fast validation on startup** with the `AddOptionsWithExpressValidation<MyOptions>` method
- **Startup validation** - Validation failures are converted to a standard `ValidateOptionsResult` with detailed messages
- **Detailed error reporting** - `OptionsValidationException` or `AggregateException` can be handled at startup
- **Uses `IValidateOptions<TOptions>`** under the hood.
- **Configurable validation**: stop on the first error or continue validating an option using `ExpressValidator.OnFirstPropertyValidatorFailed`.
- **Zero boilerplate** - no need to create separate `AbstractValidator` classes 
- **Fluent API** - for property-level rules 

## ⚡ Quick Start

```csharp
// 1. Define Your Options Classes
public class MyOptions1
{
	public int Option1 { get; set; }
	public int Option2 { get; set; }
}

public class MyOptions2
{
	public int Option3 { get; set; }
	public int Option4 { get; set; }
}

// 2. Configure Validation in Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddOptionsWithExpressValidation<MyOptions1>(
	(eb) =>
			// Select the options properties to validate
			eb.AddProperty(o => o.Option1)
			// Define FluentValidation rules
			.WithValidation(o => o.GreaterThan(10))
			.AddProperty(o => o.Option2)
			.WithValidation(o => o.GreaterThan(20))
	// Configuration section name
	, "MyOptions1"
	// Fail fast when validation fails (optional; default behavior is to collect all errors)
	, ExpressValidator.OnFirstPropertyValidatorFailed.Break)

	.AddOptionsWithExpressValidation<MyOptions2>(
	(eb) =>
			eb.AddProperty(o => o.Option3)
			.WithValidation(o => o.GreaterThan(30))
			.AddProperty(o => o.Option4)
			.WithValidation(o => o.GreaterThan(40))
	, "MyOptions2");

var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

 // 3. Handle Validation Errors
try
{
	var app = builder.Build();
	// This endpoint returns a message when all options are valid.
	app.MapGet("/",
		(IOptions<MyOptions1> options1, IOptions<MyOptions2> options2) => "The option values " +
			$"{options1.Value.Option1}, " +
			$"{options1.Value.Option2}, " +
			$"{options2.Value.Option3}, " +
			$"and {options2.Value.Option4} are correct!");

	await app.RunAsync();
}
// Single options type validation failure.
catch (OptionsValidationException ove)
{
	foreach (var failure in ove.Failures)
	{
		logger.LogCritical(
			"Options validation failure: {Failure}",
			failure
		);
	}

	logger.LogCritical(ove, "Options validation exception thrown");
}
// Multiple options types validation failures (e.g., both MyOptions1 and MyOptions2)
catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OptionsValidationException))
{
	foreach (var failure in ae
			.Flatten()
			.InnerExceptions
			.Cast<OptionsValidationException>()
			.SelectMany(ex => ex.Failures))
	{
		logger.LogCritical(
			"Options validation failure: {Failure}",
			failure
		);
	}

	logger.LogCritical(ae, "AggregateException thrown");
}
catch (Exception ex)
{
	logger.LogCritical(ex, "An unhandled exception occurred during application startup.");
}
```

## 🧩 How It Works

### `AddOptionsWithExpressValidation<TOptions>`

 - Registers options 
 - Attaches express validation, and enables `ValidateOnStart`
 - Binds configuration values using the provided section name

### `ExpressOptionsValidator<TOptions>`

 - Implements `IValidateOptions<TOptions>`
 - Runs the express validator against your options.
 - Converts validation results into `ValidateOptionsResult`
 - Produces per-property error messages

## 🏆 Sample

See samples folder for concrete example.