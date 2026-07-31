# Samples Overview

This document provides an overview of both sample projects demonstrating the `ExpressValidator.Extensions.ValidationOnStart` library.

## Available Samples

### 1. Basic Sample (`samples/Intro/`)
**Location:** `d:\MyProjects\ExpressValidator.Extensions.ValidationOnStart\samples\Intro\`

**Demonstrates:**
- Basic usage of `AddOptionsWithExpressValidation` with configuration section names
- Simple validation rules using FluentValidation
- Fail-fast validation mode (`OnFirstPropertyValidatorFailed.Break`)
- Error handling for `OptionsValidationException` and `AggregateException`

---

### 2. Advanced Sample (`samples/AdvancedSamples/`)
**Location:** `d:\MyProjects\ExpressValidator.Extensions.ValidationOnStart\samples\AdvancedSamples\`

**Demonstrates:**
All 6 new overloads added to `AddOptionsWithExpressValidation`:

1. **IConfiguration Binding** - Nested configuration paths
2. **Custom Binder Options** - Strict binding with `ErrorOnUnknownConfiguration`
3. **Manual Configuration** - Environment-based programmatic setup
4. **Post-Configuration (Section)** - Computed values from config
5. **Post-Configuration (IConfiguration)** - Complex transformations
6. **Multiple Validation Rules** - Complex scenarios

**Use this when:**
- You need advanced configuration binding
- You want to compute derived values after binding
- You need strict configuration validation
- You want programmatic configuration
- You're looking for production-ready patterns

---

## Quick Comparison

| Feature | Basic Sample | Advanced Sample |
|---------|--------------|-----------------|
| **Configuration Binding** | ✓ Section name | ✓ Section + IConfiguration + Manual |
| **Post-Configuration** | ✗ | ✓ Computed values |
| **Custom Binder Options** | ✗ | ✓ Strict validation |
| **Manual Config** | ✗ | ✓ Programmatic setup |
| **Nested Paths** | ✗ | ✓ Deep configuration |
| **Real-World Patterns** | Basic | ✓ Production-ready |
| **API Endpoints** | 1 | 7 interactive endpoints |
| **Options Classes** | 2 | 6 different scenarios |

---

## Sample Scenarios by Use Case

### Scenario: Connection String Building
**Sample:** Advanced Sample - Example 1  
**Pattern:** Post-configuration with section binding  
**Code:** `DatabaseOptions` with computed `ConnectionString`

### Scenario: Nested Configuration
**Sample:** Advanced Sample - Example 2  
**Pattern:** IConfiguration section binding  
**Code:** `ApiOptions` from `App:Api` path

### Scenario: Environment-Based Config
**Sample:** Advanced Sample - Example 3  
**Pattern:** Manual configuration  
**Code:** `FeatureFlagOptions` with `IsDevelopment()` logic

### Scenario: Path Normalization
**Sample:** Advanced Sample - Example 4  
**Pattern:** Post-configuration with transformations  
**Code:** `FileStorageOptions` with computed `FullPath`

### Scenario: Strict Security Settings
**Sample:** Advanced Sample - Example 5  
**Pattern:** Custom binder options  
**Code:** `SecurityOptions` with `ErrorOnUnknownConfiguration`

### Scenario: Derived Constraints
**Sample:** Advanced Sample - Example 6  
**Pattern:** IConfiguration + Post-configuration  
**Code:** `CacheOptions` with `EffectiveMaxTtl`

---

## Configuration Files

### Basic Sample
**File:** `samples/Intro/appsettings.json`

```json
{
  "MyOptions1": {
    "Option1": 9,
    "Option2": 19
  },
  "MyOptions2": {
    "Option3": 29,
    "Option4": 39
  }
}
```

### Advanced Sample
**File:** `samples/AdvancedSamples/appsettings.json`

```json
{
  "Database": { ... },
  "App": {
    "Api": { ... }
  },
  "FileStorage": { ... },
  "Security": { ... },
  "Cache": { ... }
}
```

---

## Testing the Samples

### Basic Sample - Expected Behavior
With the default configuration, the basic sample **will fail validation** on startup because:
- `Option1 = 9` but validation requires `> 10`
- `Option2 = 19` but validation requires `> 20`

This demonstrates the fail-fast validation.

**To fix:** Update `appsettings.json`:
```json
{
  "MyOptions1": {
    "Option1": 11,
    "Option2": 21
  },
  "MyOptions2": {
    "Option3": 31,
    "Option4": 41
  }
}
```

### Advanced Sample - Expected Behavior
With the default configuration, the advanced sample **will succeed** and start a web server.

---

## Learning Path

### Step 1: Run Basic Sample
Start here to understand:
- How `AddOptionsWithExpressValidation` works
- How validation failures are reported
- Basic error handling patterns

### Step 2: Fix Basic Sample
Modify `appsettings.json` to make validation pass and observe:
- Successful startup
- Options being injected into endpoints

### Step 3: Run Advanced Sample
Explore the 6 different examples to see:
- All the new overload capabilities
- Post-configuration in action
- Computed values and transformations

### Step 4: Experiment
Try modifying `appsettings.json` in Advanced Sample to:
- Trigger validation failures
- See strict binding errors
- Test different configuration values

## Tips & Best Practices

### From Basic Sample
1. Always handle both `OptionsValidationException` and `AggregateException`
2. Use `OnFirstPropertyValidatorFailed.Break` for performance when appropriate
3. Log validation failures clearly for debugging

### From Advanced Sample
1. Use post-configuration for computed values
2. Use `IConfiguration` sections for nested paths
3. Use manual configuration for environment-specific logic
4. Enable `ErrorOnUnknownConfiguration` for security-critical settings
5. Validate computed values, not just bound values
6. Combine patterns as needed (e.g., binding + post-config)

---
