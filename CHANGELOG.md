## 0.0.3

- BREAKING CHANGE: Make `ExpressOptionsValidator<TOptions>` internal and sealed.
- Pre-allocate error list capacity with `result.Errors.Count` to avoid resizing in `ExpressOptionsValidator<TOptions>`.
- Simplify `ValidateOptionsResult.Fail` failure factory by removing one parameter from the factory function.
- Fix incorrect version number in `ExpressValidator.Extensions.ValidationOnStart.csproj`.
- Update to ExpressValidator 0.15.0.
- Update Microsoft nuget packages.
- Update libraries used in tests.
- Package 0.0.3.
- Add CHANGELOG.md.


## 0.0.4

- Add overloads for `AddOptionsWithExpressValidation` method. Support 6 additional binding scenarios:
  - Bind with `IConfiguration`
  - Bind with custom `BinderOptions`
  - Manual configuration (no binding)
  - Post-configuration support
- Add exception handling in `ExpressOptionsValidator.Validate` and test for `WithAsyncValidation` in `ExpressOptionsValidator.Create`.  
- Remove unnecessary usage in ExpressOptionsValidator.cs.
- Update to ExpressValidator 0.20.0.
- Add AdvancedSamples sample project.
- Update Microsoft NuGet packages used.
- Update Microsoft NuGet packages used by tests.
- Target net9.0 in ExpressValidator.Extensions.ValidationOnStart.Tests and update NUnit packages.
- Update coverlet.collector package for ExpressValidator.Extensions.ValidationOnStart.Tests.


