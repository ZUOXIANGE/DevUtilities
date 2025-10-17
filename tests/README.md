# DevUtilities Tests

This directory contains test files and test projects for DevUtilities.

## Directory Structure

- `unit/` - Unit tests for individual components
- `integration/` - Integration tests for complete workflows
- `performance/` - Performance and benchmark tests
- `data/` - Test data files

## Test Framework

The project uses the following testing frameworks:
- **xUnit** - Primary testing framework
- **FluentAssertions** - For more readable assertions
- **Moq** - For mocking dependencies

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Category
```bash
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Performance
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### Unit Tests
- ViewModel logic tests
- Converter tests
- Utility function tests
- Model validation tests

### Integration Tests
- End-to-end workflow tests
- File I/O operations
- Cross-platform compatibility tests

### Performance Tests
- Large file processing benchmarks
- Memory usage tests
- Startup time measurements

## Contributing

When adding new features, please ensure:
1. Unit tests cover the new functionality
2. Integration tests verify the complete workflow
3. Performance tests are added for computationally intensive features
4. All tests pass on all supported platforms