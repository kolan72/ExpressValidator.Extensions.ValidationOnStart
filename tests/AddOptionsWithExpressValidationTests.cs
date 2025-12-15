using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static ExpressValidator.Extensions.ValidationOnStart.Tests.ExpressValidationIntegrationTests;

namespace ExpressValidator.Extensions.ValidationOnStart.Tests
{
	[TestFixture]
	internal class AddOptionsWithExpressValidationTests
	{
		private IServiceProvider? _serviceProvider;

		[Test]
		public void Should_Successfully_Resolve_Options_When_Valid()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>()
				{
					["MyOptions:Option1"] = "20",
					["MyOptions:Option2"] = "60"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			// Act
			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Assert
			var options = _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;

			Assert.That(options.Option1, Is.EqualTo(20));
			Assert.That(options.Option2, Is.EqualTo(60));

			// Optional: explicitly validate
			var validator = _serviceProvider.GetServices<IValidateOptions<MyOptions>>().FirstOrDefault();
			var result = validator?.Validate("MyOptions", options);
			Assert.That(result?.Succeeded, Is.True, string.Join(", ", result?.Failures ?? Array.Empty<string>()));
		}

		[Test]
		public void Should_Throw_OptionsValidationException_When_Single_Option_Invalid()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>()
				{
					["MyOptions:Option1"] = "5",
					["MyOptions:Option2"] = "60"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Act & Assert
			var exception = Assert.Throws<OptionsValidationException>(() =>
				_ = _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value);

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception!.OptionsType, Is.EqualTo(typeof(MyOptions)));
			Assert.That(exception.Failures.Any(f => f.Contains("Option1")), Is.True);
		}

		[Test]
		public void Should_AddOptionsWithExpressValidation_MultipleInvalidOptions_ThrowsWithMultipleFailures()
		{
			// Arrange
			var services = new ServiceCollection();
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "3",
					["MyOptions:Option2"] = "10"
				})
				.Build();

			services.AddSingleton<IConfiguration>(config);

			services.Configure<MyOptions>(config.GetSection("MyOptions"));
			services.AddOptionsWithExpressValidation<MyOptions>(
				eb => eb
					.AddProperty(o => o.Option1).WithValidation(v => v.GreaterThan(10))
					.AddProperty(o => o.Option2).WithValidation(v => v.GreaterThan(50)),
				"MyOptions");

			var provider = services.BuildServiceProvider();

			// Act
			var ex = Assert.Throws<OptionsValidationException>(() =>
				_ = provider.GetRequiredService<IOptions<MyOptions>>().Value);

			// Assert
			Assert.That(ex!.Failures, Has.Some.Contains("Option1"));
			Assert.That(ex.Failures, Has.Some.Contains("Option2"));
		}

		[Test]
		public void Should_Successfully_Resolve_All_Options_When_Multiple_Valid()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "20",
					["MyOptions:Option2"] = "60",
					["MyOptions2:Option3"] = "10",
					["MyOptions2:Option4"] = "15",
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			// Act
			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			services.AddOptionsWithExpressValidation<MyOptions2>(
				(eb) => eb.AddProperty(o => o.Option3)
					.WithValidation(o => o.GreaterThan(5))
					.AddProperty(o => o.Option4)
					.WithValidation(o => o.GreaterThan(7)),
				"MyOptions2");

			_serviceProvider = services.BuildServiceProvider();

			// Assert
			var myOptions = _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;
			var myOptions2 = _serviceProvider.GetRequiredService<IOptions<MyOptions2>>().Value;

			Assert.That(myOptions.Option1, Is.EqualTo(20));
			Assert.That(myOptions.Option2, Is.EqualTo(60));
			Assert.That(myOptions2.Option3, Is.EqualTo(10));
			Assert.That(myOptions2.Option4, Is.EqualTo(15));
		}

		[Test]
		public void Should_Throw_AggregateException_When_Multiple_Options_Invalid()
		{
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "20",
					["MyOptions:Option2"] = "30", // Invalid: <= 50
					["MyOptions2:Option3"] = "10",
					["MyOptions2:Option4"] = "6"// Invalid: <= 7
                })
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			services.AddOptionsWithExpressValidation<MyOptions2>(
				(eb) => eb.AddProperty(o => o.Option3)
					.WithValidation(o => o.GreaterThan(5))
					.AddProperty(o => o.Option4)
					.WithValidation(o => o.GreaterThan(7)),
				"MyOptions2");

			_serviceProvider = services.BuildServiceProvider();

			IStartupValidator? validator = _serviceProvider.GetRequiredService<IStartupValidator>();
			var exception =  Assert.Throws<AggregateException>(() => validator.Validate());

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception!.InnerExceptions, Has.Count.EqualTo(2));
			Assert.That(exception.InnerExceptions.All(e => e is OptionsValidationException), Is.True);
		}

		[Test]
		public void Should_Throw_OptionsValidationException_When_Configuration_Section_Missing()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(Array.Empty<KeyValuePair<string, string?>>())
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Act & Assert
			var exception = Assert.Throws<OptionsValidationException>(() =>
				_ =  _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value);

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception!.Failures.Any(f => f.Contains("Option1") || f.Contains("Option2")), Is.True);
		}

		[Test]
		public void Should_Resolve_Options_With_IOptionsSnapshot()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "25",
					["MyOptions:Option2"] = "55"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			// Act
			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Assert
			var optionsSnapshot = _serviceProvider.GetRequiredService<IOptionsSnapshot<MyOptions>>();
			var options = optionsSnapshot.Value;

			Assert.That(options.Option1, Is.EqualTo(25));
			Assert.That(options.Option2, Is.EqualTo(55));
		}

		[Test]
		public void Should_Resolve_Options_With_IOptionsMonitor()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "30",
					["MyOptions:Option2"] = "70"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			// Act
			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.GreaterThan(10))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(50)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Assert
			var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<MyOptions>>();
			var options = optionsMonitor.CurrentValue;

			Assert.That(options.Option1, Is.EqualTo(30));
			Assert.That(options.Option2, Is.EqualTo(70));
		}

		[Test]
		public void Should_Validate_Successfully_With_Custom_Validator()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "100",
					["MyOptions:Option2"] = "200"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			// Act
			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.Custom((value, context) =>
					{
						if (value <= 99)
						{
							context.AddFailure("Option1 must be greater than 99");
						}
					}))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(150)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Assert
			var options = _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;

			Assert.That(options.Option1, Is.EqualTo(100));
			Assert.That(options.Option2, Is.EqualTo(200));
		}

		[Test]
		public void Should_Throw_ValidationException_When_Custom_Validator_Fails()
		{
			// Arrange
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["MyOptions:Option1"] = "50", // Invalid: <= 99
					["MyOptions:Option2"] = "200"
				})
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);

			services.AddOptionsWithExpressValidation<MyOptions>(
				(eb) => eb.AddProperty(o => o.Option1)
					.WithValidation(o => o.Custom((value, context) =>
					{
						if (value <= 99)
						{
							context.AddFailure("Option1 must be greater than 99");
						}
					}))
					.AddProperty(o => o.Option2)
					.WithValidation(o => o.GreaterThan(150)),
				"MyOptions");

			_serviceProvider = services.BuildServiceProvider();

			// Act & Assert
			var exception = Assert.Throws<OptionsValidationException>(() =>
				_ = _serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value);

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception!.Failures.Any(f => f.Contains("Option1 must be greater than 99")), Is.True);
		}
	}
}
