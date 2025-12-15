using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart.Tests
{
	[TestFixture]
	public class ServiceCollectionExtensionsTests
	{
		[Test]
		public void Should_RegisterOptionsWithExpressValidation()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				builder => { /* configuration */ },
				"TestSection");

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		[Test]
		public void Should_ConfigureOptionsCorrectly_WhenAddOptionsWithExpressValidationIsCalled()
		{
			// Arrange
			var services = new ServiceCollection();

			var root = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					//.AddEnvironmentVariables()
					.Build();

			services.AddSingleton<IConfiguration>(root);
			//HACK
			services.Configure<TestSection>((opt) => root.GetSection("TestSection").Bind(opt));

			// Act
			services.AddOptionsWithExpressValidation<TestSection>(
				(builder) => builder.AddProperty(o => o.Age).WithValidation((o) => o.GreaterThan(6)),
				"TestSection");

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<TestSection>>();
			var validator = serviceProvider.GetService<IValidateOptions<TestSection>>();

			_ = Assert.Throws<OptionsValidationException>(() => validator!.Validate("TestSection", optionsMonitor!.CurrentValue));

			Assert.That(optionsMonitor, Is.Not.Null);
			Assert.That(validator, Is.Not.Null);
		}

		public class TestOptions
		{
			public string? Name { get; set; }
			public int Age { get; set; }
		}

		public class TestSection
		{
			public int Age { get; set; }
		}
	}
}
