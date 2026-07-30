using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpressValidator.Extensions.ValidationOnStart
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds options with express validation and binds configuration using the specified section name.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> action,
			string configurationSection,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			services
		   .AddOptions<TOptions>()
		   .BindConfiguration(configurationSection)
		   .ExpressValidate(action, validationMode)
		   .ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation and binds configuration using IConfiguration section.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> action,
			IConfiguration configuration,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			services
		   .AddOptions<TOptions>()
		   .Bind(configuration)
		   .ExpressValidate(action, validationMode)
		   .ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation and binds configuration using IConfiguration section with custom binder options.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> action,
			IConfiguration configuration,
			Action<BinderOptions>? configureBinder,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			services
		   .AddOptions<TOptions>()
		   .Bind(configuration, configureBinder)
		   .ExpressValidate(action, validationMode)
		   .ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation and binds configuration section by name with custom binder options.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> action,
			string configurationSection,
			Action<BinderOptions>? configureBinder,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			services
		   .AddOptions<TOptions>()
		   .BindConfiguration(configurationSection, configureBinder)
		   .ExpressValidate(action, validationMode)
		   .ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation using manual configuration (no config binding).
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> validationAction,
			Action<TOptions> configureOptions,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			services
		   .AddOptions<TOptions>()
		   .Configure(configureOptions)
		   .ExpressValidate(validationAction, validationMode)
		   .ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation, binds configuration, and applies post-configuration.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> validationAction,
			string configurationSection,
			Action<TOptions> postConfigureOptions,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			var optionsBuilder = services
		   .AddOptions<TOptions>()
		   .BindConfiguration(configurationSection)
		   .ExpressValidate(validationAction, validationMode);

			services.PostConfigure(postConfigureOptions);

			optionsBuilder.ValidateOnStart();
			return services;
		}

		/// <summary>
		/// Adds options with express validation, binds IConfiguration, and applies post-configuration.
		/// </summary>
		public static IServiceCollection AddOptionsWithExpressValidation<TOptions>(
			this IServiceCollection services,
			Action<ExpressValidatorBuilder<TOptions>> validationAction,
			IConfiguration configuration,
			Action<TOptions> postConfigureOptions,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		where TOptions : class
		{
			var optionsBuilder = services
		   .AddOptions<TOptions>()
		   .Bind(configuration)
		   .ExpressValidate(validationAction, validationMode);

			services.PostConfigure(postConfigureOptions);

			optionsBuilder.ValidateOnStart();
			return services;
		}
	}
}
