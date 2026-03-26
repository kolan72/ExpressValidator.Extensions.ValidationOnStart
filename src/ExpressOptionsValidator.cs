using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart
{
	public class ExpressOptionsValidator<TOptions> : IValidateOptions<TOptions> where TOptions : class
	{
		private readonly IExpressValidator<TOptions> _expressValidator;
		private readonly string _name;

		private static readonly Func<ValidationFailure, string> _failureFactory
			= static (failure) => $"Validation failed for {typeof(TOptions).Name}.{failure.PropertyName} with the error: {failure.ErrorMessage}";

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

			ArgumentNullException.ThrowIfNull(options);

			var result = _expressValidator.Validate(options);
			if (result.IsValid)
			{
				return ValidateOptionsResult.Success;
			}

			var errors = new List<string>(result.Errors.Count);

			foreach (var failure in result.Errors)
			{
				errors.Add(_failureFactory(failure));
			}

			return ValidateOptionsResult.Fail(errors);
		}
	}
}
