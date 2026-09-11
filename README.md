# OpenIdConnect SSO - Single Sign-On Implementation

A production-ready Single Sign-On (SSO) implementation using **OpenID Connect** protocol with **OpenIddict** in **.NET 9**.

## 🎯 Project Overview

This project demonstrates a complete SSO solution with:
- **Auth Server**: Centralized authentication server using OpenIddict
- **Client Application**: Relying party application that consumes the SSO service
- **Shared Common Library**: Reusable components and extensions

## 🏗️ Architecture

```
┌─────────────────┐         ┌─────────────────┐
│   Auth Server   │◄───────►│  Client App     │
│  (OpenIddict)   │  OIDC   │  (Relying Party)│
└─────────────────┘         └─────────────────┘
        ▲
        │
┌─────────────────┐
│  SQL Server     │
│  (Database)     │
└─────────────────┘
```

## 🚀 Features

- ✅ **OpenID Connect Protocol** - Industry-standard authentication protocol
- ✅ **OAuth 2.0** - Authorization framework
- ✅ **Single Sign-On (SSO)** - One login, multiple applications
- ✅ **Token-based Authentication** - JWT tokens for secure API access
- ✅ **Identity Management** - ASP.NET Core Identity integration
- ✅ **Entity Framework Core** - Code-first database approach
- ✅ **Comprehensive Testing** - Unit and integration tests with xUnit
- ✅ **CI/CD Pipeline** - Automated build and test with GitHub Actions
- ✅ **Security Best Practices** - HTTPS, secure cookies, proper error handling

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB or full instance)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🛠️ Setup & Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/OpenIdConnectSSO.git
cd OpenIdConnectSSO
```

### 2. Configure Database Connection

Update the connection string in `appsettings.json` for both AuthServer and Client:

**OpenIdConnectSSO.AuthServer/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OpenIdConnectSSO_Auth;Trusted_Connection=True;"
  }
}
```

**OpenIdConnectSSO.Client/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OpenIdConnectSSO_Client;Trusted_Connection=True;"
  }
}
```

### 3. Apply Database Migrations

```bash
# Auth Server Database
cd OpenIdConnectSSO.AuthServer
dotnet ef database update

# Client Database
cd ../OpenIdConnectSSO.Client
dotnet ef database update
```

### 4. Build and Run

#### Option A: Run from Visual Studio
1. Open `OpenIdConnectSSO.sln` in Visual Studio
2. Set multiple startup projects (AuthServer and Client)
3. Press F5 to run

#### Option B: Run from Command Line

```bash
# Terminal 1 - Auth Server
cd OpenIdConnectSSO.AuthServer
dotnet run

# Terminal 2 - Client Application
cd OpenIdConnectSSO.Client
dotnet run
```

The applications will be available at:
- **Auth Server**: `https://localhost:7001` (or port specified in launchSettings.json)
- **Client App**: `https://localhost:7002` (or port specified in launchSettings.json)

## 🧪 Running Tests

```bash
dotnet test OpenIdConnectSSO.sln --verbosity normal
```

### Test Coverage
- Authorization request validation
- Token request validation
- Authentication flow
- Login flow
- Error handling
- OpenID Connect configuration
- Auth server integration

## 🔐 Security Features

- **Password Policy**: Configurable password requirements
- **Cookie Security**: HttpOnly, Secure, SameSite attributes
- **HTTPS Enforcement**: Automatic HTTPS redirection
- **Token Validation**: Comprehensive token validation
- **Error Handling**: Secure error messages without leaking sensitive information

## 📦 Project Structure

```
OpenIdConnectSSO/
├── OpenIdConnectSSO.AuthServer/      # Authentication Server
│   ├── Controllers/                   # MVC Controllers
│   ├── Data/                          # DbContext
│   ├── Migrations/                    # EF Core Migrations
│   ├── Services/                      # Business Logic
│   ├── ViewModels/                    # View Models
│   └── Views/                         # Razor Views
├── OpenIdConnectSSO.Client/           # Client Application
│   ├── Controllers/                   # MVC Controllers
│   ├── Data/                          # DbContext
│   ├── Migrations/                    # EF Core Migrations
│   ├── Services/                      # Business Logic
│   └── ViewModels/                    # View Models
├── OpenIdConnectSSO.Common/           # Shared Library
│   └── PathExtensions.cs              # Utility Extensions
├── OpenIdConnectSSO.Client.Tests/     # Test Project
│   └── *.cs                           # Test Classes
└── .github/workflows/                 # CI/CD Pipeline
```

## 🔄 CI/CD Pipeline

GitHub Actions workflow automatically:
- Builds the solution on every push and pull request
- Runs all tests
- Reports build status

## 🎨 Technologies Used

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 9.0 | Framework |
| ASP.NET Core | 9.0 | Web Framework |
| OpenIddict | 7.5.0 | OIDC/OAuth2 Server |
| Entity Framework Core | 9.0.15 | ORM |
| SQL Server | - | Database |
| xUnit | 2.9.3 | Testing Framework |
| Bootstrap | 5.x | UI Framework |

## 📝 API Endpoints

### Auth Server

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/connect/authorize` | GET | Authorization endpoint |
| `/connect/token` | POST | Token endpoint |
| `/connect/userinfo` | GET | User info endpoint |
| `/account/login` | GET/POST | Login page |
| `/connect/logout` | GET/POST | Logout endpoint |

### Client Application

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Home page |
| `/account/login` | GET | Initiate login |
| `/account/logout` | GET | Initiate logout |
| `/home/secure` | GET | Protected resource |

## 🐛 Troubleshooting

### Common Issues

**Database Connection Error:**
```
Ensure SQL Server is running and connection string is correct
```

**Certificate Errors:**
```bash
# Remove old development certificates
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

**Migration Errors:**
```bash
# Remove last migration and recreate
dotnet ef migrations remove
dotnet ef migrations add NewMigrationName
dotnet ef database update
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 👨‍💻 Author

**Your Name**
- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your Profile](https://linkedin.com/in/yourprofile)

## 🙏 Acknowledgments

- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [OpenID Connect Protocol](https://openid.net/connect/)

---

<div align="center">

**If you find this project helpful, please give it a ⭐!**

[Report Issue](https://github.com/yourusername/OpenIdConnectSSO/issues) • [Request Feature](https://github.com/yourusername/OpenIdConnectSSO/issues)

</div>