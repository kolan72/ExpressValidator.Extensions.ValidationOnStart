using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart
{
	public class ExpressOptionsValidator<TOptions> : IValidateOptions<TOptions> where TOptions : class
	{
		private readonly IExpressValidator<TOptions> _expressValidator;
		private readonly string _name;

		private static readonly Func<string, ValidationFailure, string> _failureFactory
			= static (string type, ValidationFailure failure) => $"Validation failed for {type}.{failure.PropertyName} with the error: {failure.ErrorMessage}";

		private ExpressOptionsValidator(string name, Action<ExpressValidatorBuilder<TOptions>> configure, OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		{
			var eb = new ExpressValidatorBuilder<TOptions>(validationMode);
			configure(eb);
			_name = name;
			_expressValidator = eb.Build();
		}

		public static ExpressOptionsValidator<TOptions> Create(string name, Action<ExpressValidatorBuilder<TOptions>> configure, OnFirstPropertyValidatorFailed validationMode = OnFirstPropertyValidatorFailed.Continue)
		{
			return new ExpressOptionsValidator<TOptions>(name, configure, validationMode);
		}

		public ValidateOptionsResult Validate(string? name, TOptions options)
		{
			if (_name is null && _name != name)
			{
				return ValidateOptionsResult.Skip;
			}

			if (options is null)
				throw new ArgumentNullException(nameof(options));

			var result = _expressValidator.Validate(options);
			if (result.IsValid)
			{
				return ValidateOptionsResult.Success;
			}

			var type = options.GetType().Name;
			var errors = new List<string>();

			foreach (var failure in result.Errors)
			{
				errors.Add(_failureFactory(type, failure));
			}

			return ValidateOptionsResult.Fail(errors);
		}
	}
}
