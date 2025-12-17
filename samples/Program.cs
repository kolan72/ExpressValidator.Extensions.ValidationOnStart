using ExpressValidator.Extensions.ValidationOnStart;
using FluentValidation;
using Microsoft.Extensions.Options;
using Samples;

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

try
{
	var app = builder.Build();
	// This endpoint returns a greeting once all options are validated.
	app.MapGet("/",
		(IOptions<MyOptions1> options1, IOptions<MyOptions2> options2) => "Hello, " +
			$"{options1.Value.Option1}, " +
			$"{options1.Value.Option2}, " +
			$"{options2.Value.Option3}, " +
			$"{options2.Value.Option4}!");

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