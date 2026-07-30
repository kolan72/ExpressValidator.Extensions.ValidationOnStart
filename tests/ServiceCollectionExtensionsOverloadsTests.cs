using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart.Tests
{
	[TestFixture]
	public class ServiceCollectionExtensionsOverloadsTests
	{
		private IConfiguration _configuration = null!;

		[SetUp]
		public void SetUp()
		{
			_configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["TestOptions:Value"] = "50",
					["TestOptions:Name"] = "TestName",
					["TestOptions:Count"] = "100",
					["AdvancedOptions:Timeout"] = "30",
					["AdvancedOptions:MaxRetries"] = "5",
					["InvalidOptions:Value"] = "5"
				})
				.Build();
		}

		#region Overload: Bind with IConfiguration

		[Test]
		public void Should_BindWithIConfiguration_WhenValidConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);
			var section = _configuration.GetSection("TestOptions");

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(10)),
				section);

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Value, Is.EqualTo(50));
			Assert.That(options.Value.Name, Is.EqualTo("TestName"));
		}

		[Test]
		public void Should_ThrowOptionsValidationException_WhenBindWithIConfiguration_AndValidationFails()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);
			var section = _configuration.GetSection("InvalidOptions");

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(10)),
				section);

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var options = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, options.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenBindWithIConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			var section = _configuration.GetSection("TestOptions");

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				section);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		#endregion

		#region Overload: Bind with IConfiguration and Custom Binder Options

		[Test]
		public void Should_BindWithCustomBinderOptions_WhenErrorOnUnknownConfigurationIsTrue()
		{
			// Arrange
			var services = new ServiceCollection();
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Value"] = "50",
					["Name"] = "Test",
					["UnknownProperty"] = "ShouldCauseError"
				})
				.Build();

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				config,
				binderOptions => binderOptions.ErrorOnUnknownConfiguration = true);

			// Assert - Should not throw during registration, but during binding
			var serviceProvider = services.BuildServiceProvider();
			Assert.That(serviceProvider, Is.Not.Null);
		}

		[Test]
		public void Should_BindWithCustomBinderOptions_WhenBindNonPublicPropertiesIsSet()
		{
			// Arrange
			var services = new ServiceCollection();
			var section = _configuration.GetSection("TestOptions");

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				section,
				binderOptions => binderOptions.BindNonPublicProperties = false);

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Value, Is.EqualTo(50));
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenBindWithCustomBinderOptions()
		{
			// Arrange
			var services = new ServiceCollection();
			var section = _configuration.GetSection("TestOptions");

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				section,
				binderOptions => binderOptions.ErrorOnUnknownConfiguration = false);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		#endregion

		#region Overload: Bind by Section Name with Custom Binder Options

		[Test]
		public void Should_BindBySectionNameWithCustomBinderOptions_WhenValidConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(10)),
				"TestOptions",
				binderOptions => binderOptions.ErrorOnUnknownConfiguration = false);

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Value, Is.EqualTo(50));
			Assert.That(options.Value.Name, Is.EqualTo("TestName"));
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenBindBySectionNameWithCustomBinderOptions()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				"TestOptions",
				binderOptions => binderOptions.ErrorOnUnknownConfiguration = false);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		[Test]
		public void Should_ThrowOptionsValidationException_WhenBindBySectionNameWithCustomBinderOptions_AndValidationFails()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(100)),
				"TestOptions",
				binderOptions => binderOptions.ErrorOnUnknownConfiguration = false);

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var options = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, options.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		#endregion

		#region Overload: Manual Configuration (No Config Binding)

		[Test]
		public void Should_ConfigureManually_WhenNoConfigBinding()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(10)),
				options =>
				{
					options.Value = 100;
					options.Name = "ManuallyConfigured";
					options.Count = 50;
				});

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Value, Is.EqualTo(100));
			Assert.That(options.Value.Name, Is.EqualTo("ManuallyConfigured"));
			Assert.That(options.Value.Count, Is.EqualTo(50));
		}

		[Test]
		public void Should_ThrowOptionsValidationException_WhenManualConfiguration_AndValidationFails()
		{
			// Arrange
			var services = new ServiceCollection();

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(50)),
				options =>
				{
					options.Value = 10; // Invalid: less than 50
					options.Name = "Invalid";
				});

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, optionsMonitor.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenManualConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				options => options.Value = 100);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		#endregion

		#region Overload: Bind by Section Name with Post-Configuration

		[Test]
		public void Should_ApplyPostConfiguration_WhenBindBySectionName()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Count).WithValidation(v => v.GreaterThan(150)),
				"TestOptions",
				options =>
				{
					// Post-configure: compute derived value
					options.Count = options.Value * 2; // 50 * 2 = 100, but we need > 150
					options.Count += 100; // Now 200
				});

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Value, Is.EqualTo(50)); // Original from config
			Assert.That(options.Value.Count, Is.EqualTo(200)); // Post-configured
		}

		[Test]
		public void Should_ValidateAfterPostConfiguration_WhenBindBySectionName()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Count).WithValidation(v => v.LessThan(50)),
				"TestOptions",
				options =>
				{
					// Post-configure to make it invalid
					options.Count = 1000; // Invalid: greater than 50
				});

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, optionsMonitor.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenBindBySectionNameWithPostConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				"TestOptions",
				options => options.Count = 999);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		#endregion

		#region Overload: Bind with IConfiguration and Post-Configuration

		[Test]
		public void Should_ApplyPostConfiguration_WhenBindWithIConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);
			var section = _configuration.GetSection("TestOptions");

			// Act
			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Name).WithValidation(v => v.NotEmpty()),
				section,
				options =>
				{
					// Post-configure: transform the name
					options.Name = options.Name?.ToUpper() + "_PROCESSED";
				});

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value, Is.Not.Null);
			Assert.That(options.Value.Name, Is.EqualTo("TESTNAME_PROCESSED"));
		}

		[Test]
		public void Should_ValidateAfterPostConfiguration_WhenBindWithIConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);
			var section = _configuration.GetSection("TestOptions");

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Name).WithValidation(v => v.NotEmpty()),
				section,
				options =>
				{
					// Post-configure to make it invalid
					options.Name = string.Empty; // Invalid: empty string
				});

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, optionsMonitor.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		[Test]
		public void Should_ReturnServiceCollection_WhenBindWithIConfigurationAndPostConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			var section = _configuration.GetSection("TestOptions");

			// Act
			var result = services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0)),
				section,
				options => options.Count = 123);

			// Assert
			Assert.That(result, Is.SameAs(services));
		}

		#endregion

		#region Integration Tests

		[Test]
		public void Should_HandleMultipleValidationRules_WithPostConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddSingleton(_configuration);

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb
					.AddProperty(o => o.Value).WithValidation(v => v.GreaterThan(0))
					.AddProperty(o => o.Name).WithValidation(v => v.NotEmpty().MinimumLength(5))
					.AddProperty(o => o.Count).WithValidation(v => v.InclusiveBetween(100, 300)),
				"TestOptions",
				options =>
				{
					options.Count = options.Value + 150; // 50 + 150 = 200
				});

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();
			
			Assert.That(options.Value.Value, Is.EqualTo(50));
			Assert.That(options.Value.Name, Is.EqualTo("TestName"));
			Assert.That(options.Value.Count, Is.EqualTo(200));
		}

		[Test]
		public void Should_UseValidationMode_WithManualConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();

			services.AddOptionsWithExpressValidation<TestOptions>(
				eb => eb
					.AddProperty(o => o.Value).WithValidation(v => v.LessThan(0)) // Will fail
					.AddProperty(o => o.Count).WithValidation(v => v.LessThan(0)), // Will fail
				options =>
				{
					options.Value = 100;
					options.Count = 200;
				},
				OnFirstPropertyValidatorFailed.Break);

			// Act & Assert
			var serviceProvider = services.BuildServiceProvider();
			var validator = serviceProvider.GetRequiredService<IValidateOptions<TestOptions>>();
			var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();

			var ex = Assert.Throws<OptionsValidationException>(
				() => validator.Validate(Options.DefaultName, optionsMonitor.CurrentValue));

			Assert.That(ex, Is.Not.Null);
			Assert.That(ex!.Failures, Is.Not.Empty);
		}

		[Test]
		public void Should_WorkWithComplexNestedConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			var complexConfig = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["App:Database:Timeout"] = "30",
					["App:Database:MaxRetries"] = "5"
				})
				.Build();

			services.AddSingleton<IConfiguration>(complexConfig);
			var section = complexConfig.GetSection("App:Database");

			// Act
			services.AddOptionsWithExpressValidation<DatabaseOptions>(
				eb => eb
					.AddProperty(o => o.Timeout).WithValidation(v => v.GreaterThan(0))
					.AddProperty(o => o.MaxRetries).WithValidation(v => v.InclusiveBetween(1, 10)),
				section);

			// Assert
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
			
			Assert.That(options.Value.Timeout, Is.EqualTo(30));
			Assert.That(options.Value.MaxRetries, Is.EqualTo(5));
		}

		#endregion

		#region Test Classes

		public class TestOptions
		{
			public int Value { get; set; }
			public string? Name { get; set; }
			public int Count { get; set; }
		}

		public class DatabaseOptions
		{
			public int Timeout { get; set; }
			public int MaxRetries { get; set; }
		}

		#endregion
	}
}
