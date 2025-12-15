using Microsoft.Extensions.Options;

namespace ExpressValidator.Extensions.ValidationOnStart.Tests
{
	[TestFixture]
	public class ExpressOptionsValidatorTests
	{
		private class TestOptions
		{
			public string? Name { get; set; }
			public int Age { get; set; }
		}

		[Test]
		public void Should_CreateValidator_WithNameAndConfiguration()
		{
			// Arrange & Act
			var validator = ExpressOptionsValidator<TestOptions>.Create("test",
				_ => { /* configuration */ },
				OnFirstPropertyValidatorFailed.Continue);

			// Assert
			Assert.That(validator, Is.Not.Null);
		}

		[Test]
		public void Should_CreateValidator_UsingStaticCreateMethod()
		{
			// Arrange & Act
			var validator = ExpressOptionsValidator<TestOptions>.Create("test",
				_ => { /* configuration */ });

			// Assert
			Assert.That(validator, Is.Not.Null);
		}

		[Test]
		public void Should_ReturnSkip_WhenNameIsNullAndDoesNotMatchProvidedName()
		{
			// Arrange
			var validator = ExpressOptionsValidator<TestOptions>.Create(null!, (_) => { });
			var options = new TestOptions { Name = "test", Age = 25 };

			// Act
			var result = validator.Validate("different-name", options);

			// Assert
			Assert.That(result, Is.EqualTo(ValidateOptionsResult.Skip));
		}

		[Test]
		public void Should_ThrowArgumentNullException_WhenOptionsIsNull()
		{
			// Arrange
			var validator = ExpressOptionsValidator<TestOptions>.Create("test", (_) => { });

			// Act & Assert
			Assert.That(() => validator.Validate("test", null!),
				Throws.ArgumentNullException.With.Property("ParamName").EqualTo("options"));
		}
	}
}
