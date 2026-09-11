# CONTRIBUTING.md

# Contributing to OpenIdConnect SSO

Thank you for considering contributing to this project! We welcome contributions from the community.

## How to Contribute

### 1. Fork and Clone

```bash
git clone https://github.com/yourusername/OpenIdConnectSSO.git
cd OpenIdConnectSSO
```

### 2. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Make Your Changes

- Follow the existing code style
- Write tests for new features
- Update documentation as needed

### 4. Test Your Changes

```bash
dotnet test OpenIdConnectSSO.sln
```

### 5. Commit Your Changes

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
git commit -m "feat: add new authentication method"
git commit -m "fix: resolve token validation issue"
git commit -m "docs: update README with setup instructions"
git commit -m "refactor: improve error handling"
git commit -m "test: add integration tests for login flow"
```

**Commit Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### 6. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then open a Pull Request on GitHub.

## Code Style Guidelines

### C# Conventions

- Use meaningful variable and method names
- Follow PascalCase for classes and methods
- Use camelCase for local variables and parameters
- Keep methods small and focused
- Use async/await for asynchronous operations
- Add XML documentation for public APIs

### Example

```csharp
/// <summary>
/// Validates the authorization request parameters.
/// </summary>
/// <param name="request">The authorization request.</param>
/// <returns>True if valid, otherwise false.</returns>
public bool ValidateAuthorizationRequest(AuthorizationRequest request)
{
    if (request == null)
        return false;
        
    return !string.IsNullOrWhiteSpace(request.ClientId) 
           && request.RedirectUri != null;
}
```

## Testing Requirements

- All new features must include tests
- Maintain or improve code coverage
- Include both unit and integration tests where appropriate

### Running Tests

```bash
# Run all tests
dotnet test OpenIdConnectSSO.sln

# Run with coverage
dotnet test OpenIdConnectSSO.sln --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthenticationFlowTests"
```

## Documentation

- Update README.md for significant changes
- Add XML comments to public methods and classes
- Update API documentation if endpoints change

## Security Guidelines

- Never commit secrets or credentials
- Use environment variables for sensitive configuration
- Follow OWASP security best practices
- Report security vulnerabilities privately

## Questions?

Feel free to open an issue for any questions or discussions.

---

Thank you for contributing! 🎉
