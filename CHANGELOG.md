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
