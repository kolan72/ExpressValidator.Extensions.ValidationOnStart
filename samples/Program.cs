using ExpressValidator.Extensions.ValidationOnStart;
using FluentValidation;
using Microsoft.Extensions.Options;
using Samples;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddOptionsWithExpressValidation<MyOptions>(
	(eb) =>
			eb.AddProperty(o => o.Option1)
			.WithValidation(o => o.GreaterThan(10))
			.AddProperty(o => o.Option2)
			.WithValidation(o => o.GreaterThan(50))
, "MyOptions")

	.AddOptionsWithExpressValidation<MyOptions2>(
	(eb) =>
			eb.AddProperty(o => o.Option3)
			.WithValidation(o => o.GreaterThan(5))
			.AddProperty(o => o.Option4)
			.WithValidation(o => o.GreaterThan(7))
, "MyOptions2");

var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try
{
	var app = builder.Build();
	app.MapGet("/", () => "Hello!");

	await app.RunAsync();
}
//Handles the standard exception thrown when a single set of options (e.g., MyOptions) fails validation.
catch (OptionsValidationException ove)
{
	foreach (var failure in ove.Failures)
	{
		logger.LogCritical(ove, failure);
	}
}
//Handles the case where validation errors from multiple option types (e.g., both MyOptions and MyOptions2)
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