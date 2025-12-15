using Microsoft.Extensions.DependencyInjection;

namespace ExpressValidator.Extensions.ValidationOnStart
{
	public static class ServiceCollectionExtensions
	{
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
	}
}
