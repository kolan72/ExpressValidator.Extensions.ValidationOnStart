using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart
{
	internal static class OptionsBuilderExtensions
	{
		public static OptionsBuilder<TOptions> ExpressValidate<TOptions>(
			this OptionsBuilder<TOptions> builder, Action<ExpressValidatorBuilder<TOptions>> action,
			OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
			where TOptions : class
		{
			builder.Services.AddSingleton<IValidateOptions<TOptions>>(
				 (_) => ExpressOptionsValidator<TOptions>.Create(
					builder.Name
					, action,
					validationMode));

			return builder;
		}
	}
}
