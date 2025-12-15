using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;

namespace ExpressValidator.Extensions.ValidationOnStart.Tests
{
	[TestFixture]
	public class OptionsBuilderExtensionsTests
	{
		[Test]
		public void Should_AddExpressValidation_ToOptionsBuilder()
		{
			// Arrange
			var services = new ServiceCollection();
			var optionsBuilder = services.AddOptions<TestOptions>();

			// Act
			var result = optionsBuilder.ExpressValidate(builder => { /* configuration */ });

			// Assert
			Assert.That(result, Is.SameAs(optionsBuilder));
			Assert.That(services.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Should_RegisterValidatorService_WhenExpressValidateIsCalled()
		{
			// Arrange
			var services = new ServiceCollection();
			var optionsBuilder = services.AddOptions<TestOptions>();

			// Act
			optionsBuilder.ExpressValidate(_ => { /* configuration */ });

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetService<IValidateOptions<TestOptions>>();
			Assert.That(validator, Is.Not.Null);
			Assert.That(validator, Is.TypeOf<ExpressOptionsValidator<TestOptions>>());
		}

		public class TestOptions
		{
			public string? Name { get; set; }
			public int Age { get; set; }
		}
	}

	[TestFixture]
	public class ExpressValidationIntegrationTests
	{
		[Test]
		public void AddOptionsWithExpressValidation_TwoOptionTypes_OneInvalid_ThrowsAggregateOrSingle()
		{
			// This depends on whether your library validates all at once (e.g., via ValidateOnStart)
			// Since your sample uses try/catch for both single and AggregateException,
			// assume you call something like `ValidateAllOptionsOnStartup`

			// For pure integration test without web host, we'll validate manually
			var services = new ServiceCollection();
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "5",   // invalid
					["MyOptions:Option2"] = "60",
					["MyOptions2:Option3"] = "3", // invalid
					["MyOptions2:Option4"] = "10"
				})
				.Build();

			services.AddSingleton<IConfiguration>(config);

			services.Configure<MyOptions>(config.GetSection("MyOptions"));
			services.Configure<MyOptions2>(config.GetSection("MyOptions2"));

			services.AddOptionsWithExpressValidation<MyOptions>(
				eb => eb.AddProperty(o => o.Option1).WithValidation(v => v.GreaterThan(10)), "MyOptions");
			services.AddOptionsWithExpressValidation<MyOptions2>(
				eb => eb.AddProperty(o => o.Option3).WithValidation(v => v.GreaterThan(5)), "MyOptions2");

			var provider = services.BuildServiceProvider();

			var myOptValidator = provider.GetRequiredService<IValidateOptions<MyOptions>>();
			var myOpt2Validator = provider.GetRequiredService<IValidateOptions<MyOptions2>>();

			var result1 = myOptValidator.Validate("MyOptions", config.GetSection("MyOptions").Get<MyOptions>()!);
			var result2 = myOpt2Validator.Validate("MyOptions2", config.GetSection("MyOptions2").Get<MyOptions2>()!);

			Assert.Multiple(() =>
			{
				Assert.That(result1.Succeeded, Is.False);
				Assert.That(result2.Succeeded, Is.False);
				Assert.That(result1.Failures, Has.Some.Contains("Option1"));
				Assert.That(result2.Failures, Has.Some.Contains("Option3"));
			});
		}

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
	}
}
