# Heimevernet

Heimevernet is a learning project built with ASP.NET Core MVC, .NET Aspire, and MariaDB. The solution currently contains the basic web application and the local development infrastructure that will support future features.

## What you need

Install these tools before starting:

- [.NET SDK 10.0.100](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- An editor or IDE, such as Visual Studio or Visual Studio Code

Docker Desktop must be running because Aspire starts MariaDB as a container.

You can check the .NET version with:

```powershell
dotnet --version
```

## Solution architecture

The solution is split into four projects:

| Project | Responsibility |
| --- | --- |
| `Heimevernet.Web` | The ASP.NET Core MVC website. It contains controllers, views, models, CSS, JavaScript, and static libraries. |
| `Heimevernet.Aspire.AppHost` | The local application orchestrator. It starts the website and MariaDB, connects them, and provides the Aspire dashboard. |
| `Heimevernet.Aspire.ServiceDefaults` | Shared service configuration for logging, health checks, service discovery, resilience, and OpenTelemetry. |
| `Heimevernet.Web.UnitTests` | Automated tests for the web project, using xUnit v3. |

The solution file is `Heimevernet.slnx`.

### How the parts connect

```text
Your browser
     |
     v
Heimevernet.Web (ASP.NET Core MVC)
     |
     | connection string supplied by Aspire
     v
MariaDB container

Heimevernet.Aspire.AppHost
     |
     +-- starts Heimevernet.Web
     +-- starts MariaDB
     +-- creates the heimevernetdb database
     +-- shows status in the Aspire dashboard
```

The AppHost is not the website itself. It is the program that describes the local application: which services exist, which containers should run, and which services depend on each other.

## How a request works

1. You open the website URL in a browser.
2. ASP.NET Core receives the request.
3. MVC routing selects a controller and action. For example, `/Home/Index` calls `HomeController.Index()`.
4. The controller returns a Razor view from `Heimevernet.Web/Views`.
5. The layout and static files provide the shared page structure and styling.

The current MVC application is a scaffold with Home, Privacy, and Error pages. The database connection is prepared by Aspire, but the application does not yet contain database models, migrations, or data-access code.

## Start the complete application

Run these commands from the repository root:

```powershell
dotnet restore
dotnet run --project .\Heimevernet.Aspire\Heimevernet.Aspire.AppHost\Heimevernet.Aspire.AppHost.csproj
```

The AppHost will:

1. Start the MariaDB container.
2. Create the `heimevernetdb` database if it does not exist.
3. Start the ASP.NET Core web project.
4. Supply the database connection to the web project through Aspire service references.
5. Start the Aspire dashboard.

The terminal prints the local URLs for the web application and dashboard. Open the dashboard URL to see service status, logs, traces, and health checks.

Stop the application with `Ctrl+C`. MariaDB uses a persistent container lifetime, so the container can remain available between runs. Docker Desktop can be used to inspect or stop it.

## Run only the web project

You can run the MVC project without Aspire:

```powershell
dotnet run --project .\Heimevernet.Web\Heimevernet.Web.csproj
```

This is useful when working only on views, controllers, or CSS. It does not start MariaDB or the Aspire dashboard, so database-dependent features will not work unless their connection is configured separately.

## Run the tests

Run all tests with:

```powershell
dotnet test .\Heimevernet.Web.UnitTests\Heimevernet.Web.UnitTests.csproj
```

Build the complete solution with:

```powershell
dotnet build .\Heimevernet.slnx
```

## Where to make changes
- Clone the project and open it in your IDE.
- Connect it to your own GitHub repository (optional).
- Add or update page actions in `Heimevernet.Web/Controllers`.
- Add page-specific data models in `Heimevernet.Web/Models`.
- Add Razor pages in `Heimevernet.Web/Views`.
- Add styling in `Heimevernet.Web/wwwroot/css`.
- Add browser JavaScript in `Heimevernet.Web/wwwroot/js`.
- Configure local services in `Heimevernet.Aspire/Heimevernet.Aspire.AppHost/AppHost.cs`.
- Add database resource behavior in `Heimevernet.Aspire/Heimevernet.Aspire.AppHost/MariaDb`.
- Add automated tests in `Heimevernet.Web.UnitTests`.

## Useful beginner concepts

### ASP.NET Core MVC

MVC means Model-View-Controller:

- **Model** represents data.
- **View** generates the HTML shown in the browser.
- **Controller** receives requests and decides what response to return.

### .NET Aspire

Aspire helps run several related services together during development. Instead of starting the web application and database manually, the AppHost describes the whole local system in code.

### Dependency injection

Dependency injection is how ASP.NET Core supplies services to controllers and other classes. Services are registered during application startup and then requested where they are needed, rather than being created manually in every class.

### Automated tests

Tests execute code automatically and check expected behavior. A good test should make an assertion about a specific result; a test with no assertion can pass without proving that the application works.

## Development workflow

Work on a feature branch rather than directly on `main`:

```powershell
git switch -c {developer-name}/{feature-name}
```

Make small changes, run the relevant tests, and build the solution before sharing the branch.
