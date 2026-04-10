# Contributing to EasyTool

Thank you for your interest in contributing to EasyTool! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- Visual Studio 2022 / JetBrains Rider / VS Code with C# extension
- Git

### Development Setup

1. Fork the repository
2. Clone your fork locally
   ```bash
   git clone https://github.com/YOUR_USERNAME/easytool.git
   ```
3. Open `EasyTool.sln` in your IDE
4. Build the solution to verify everything works
   ```bash
   dotnet build
   ```
5. Run the tests
   ```bash
   dotnet test
   ```

## Development Guidelines

### Code Style

- Follow the project's `.editorconfig` settings
- Use 4 spaces for indentation (no tabs)
- Use PascalCase for public members, camelCase for private fields
- Use `_camelCase` for private fields
- Add XML documentation comments to all public APIs

### Project Structure

```
EasyTool.Core/
├── BusinessCategory/     # Business validation utilities
├── CodeCategory/         # Encoding/encryption utilities
├── TextCategory/         # Text processing utilities
├── CollectionsCategory/  # Collection utilities
├── DateTimeCategory/     # Date/time utilities
├── IdentifierCategory/   # ID generators
├── IOCategory/           # File operation utilities
├── MathCategory/         # Math utilities
├── NetCategory/          # Network utilities
├── SecurityCategory/     # Security tools
└── ToolCategory/         # General utilities
```

### Coding Standards

1. **Thread Safety**: Utility classes that may be used concurrently must be thread-safe. Use `lock` or concurrent collections.
2. **Null Safety**: All public API parameters must have null checks. Use nullable reference types.
3. **Exception Handling**: Catch specific exceptions, never catch bare `Exception` without re-throwing. Use `throw;` to preserve stack traces.
4. **Performance**: Cache compiled regex patterns as `static readonly` fields with `RegexOptions.Compiled`.
5. **Naming**: Follow consistent naming patterns. Utility classes should end with `Util`. Extension classes should end with `Extension`.

### Adding a New Utility

1. Create the utility class in the appropriate category folder
2. Add XML documentation to the class and all public methods
3. Add corresponding unit tests in `EasyTool.UnitTests/`
4. Update the README if the utility is significant

### Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add new utility for XXX
fix: resolve issue with XXX
docs: update documentation for XXX
test: add tests for XXX
refactor: improve XXX performance
```

## Pull Request Process

1. Create a feature branch from `dev` or `main`
   ```bash
   git checkout -b feat/your-feature-name
   ```
2. Make your changes and commit them
3. Add tests for your changes
4. Ensure all tests pass
   ```bash
   dotnet test
   ```
5. Push your branch and create a Pull Request

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] XML documentation added for public APIs
- [ ] Unit tests added/updated
- [ ] All tests pass
- [ ] No breaking changes (or clearly documented)

## Reporting Issues

When reporting issues, please use the provided issue templates and include:

- Clear description of the issue
- Minimal reproduction steps
- Expected vs actual behavior
- .NET version and OS information

## License

By contributing to EasyTool, you agree that your contributions will be licensed under the MIT License.
