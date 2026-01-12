# One Global Devices API

A RESTful API for managing devices built with .NET 10.0, SQL Server, and Docker.

## 📋 Project Overview

One Global Devices API is a CRUD (Create, Read, Update, Delete) application designed to manage device records. The API provides endpoints for creating, reading, updating, and deleting device information, including device name, brand, and state.

### Tech Stack

- **.NET 10.0** - Modern web framework
- **SQL Server 2022** - Relational database
- **Docker & Docker Compose** - Containerization
- **xUnit** - Unit testing framework
- **Coverlet** - Code coverage tool
- **Moq & FluentAssertions** - Testing utilities

### Architecture

The project follows Clean Architecture principles with the following layers:

- **Application Layer** - Controllers and DTOs
- **Domain Layer** - Entities, Services, and Repository interfaces
- **Infrastructure Layer** - SQL Server implementations and database connections

## 🚀 API Endpoints

Base URL when running with Docker: `http://localhost:5000`

### Devices Controller

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| **POST** | `/Devices` | Create a new device | `DeviceCreateRequestDTO` | `201 Created` with device details |
| **GET** | `/Devices` | Get all devices | - | `200 OK` with array of devices |
| **GET** | `/Devices/{id}` | Get device by ID | - | `200 OK` with device details |
| **GET** | `/Devices/byBrand?brand={brand}` | Get devices by brand | - | `200 OK` with array of devices |
| **GET** | `/Devices/byState?state={state}` | Get devices by state | - | `200 OK` with array of devices |
| **PUT** | `/Devices/{id}` | Fully update a device | `DeviceFullyUpdateRequestDTO` | `200 OK` with updated device |
| **PATCH** | `/Devices/{id}` | Partially update a device | `DevicePartiallyUpdateRequestDTO` | `200 OK` with updated device |
| **DELETE** | `/Devices/{id}` | Delete a device | - | `204 No Content` |

### Request/Response Examples

#### Create Device (POST /Devices)
```json
{
  "name": "iPhone 15 Pro",
  "brand": "Apple"
}
```

#### Fully Update Device (PUT /Devices/{id})
```json
{
  "newName": "iPhone 15 Pro Max",
  "newBrand": "Apple",
  "newState": "Available"
}
```

#### Partially Update Device (PATCH /Devices/{id})
```json
{
  "newName": "iPhone 15 Pro Max"
}
```

#### Device Response
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "iPhone 15 Pro",
  "brand": "Apple",
  "state": "Available",
  "creationTime": "2026-01-11T10:30:00Z"
}
```

### Device States

- `Available`
- `Inactive`
- `InUse`

## 🐳 Running with Docker Compose

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed
- At least 4GB of RAM available for containers

### Quick Start

1. **Clone the repository**
```bash
git clone <repository-url>
cd one-global-devices-api
```

2. **Navigate to the project directory**
```bash
cd src/one-global-devices-api
```

3. **Start the services**
```bash
docker-compose up -d
```

This will:
- Start SQL Server 2022 on port `1433`
- Build and start the API on port `5000`
- Create a dedicated network for the services
- Set up health checks for SQL Server

4. **Access the API**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

5. **Stop the services**
```bash
docker-compose down
```

### Docker Compose Configuration

The [docker-compose.yml](src/one-global-devices-api/docker-compose.yml) defines two services:

**SQL Server Container:**
- Image: `mcr.microsoft.com/mssql/server:2022-latest`
- Port: `1433:1433`
- SA Password: Set via environment variable (see docker-compose.yml)
- Health check: Ensures database is ready before starting API
- Volume: `sqlserver_data` for data persistence

**API Container:**
- Built from [Dockerfile](src/one-global-devices-api/Dockerfile)
- Port: `5000:8080`
- Environment: Development
- Depends on SQL Server health check

### Connection String

When running with Docker Compose, the API uses:
```
Server=devicesapi-sqlserver;Database=oneglobal;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;MultipleActiveResultSets=true
```

For local development (without Docker):
```
Server=localhost;Database=oneglobal;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Update in [appsettings.json](src/one-global-devices-api/appsettings.json) as needed.

## 🗄️ Database Scripts

### Location

Database scripts are located in the [databases/](databases/) folder.

### Setup Script

**[00-create-database.sql](databases/00-create-database.sql)**

This script creates the `oneglobal` database with the following:

1. **Database Creation**
   - Database name: `oneglobal`
   - Compatibility level: 160 (SQL Server 2022)
   - Full recovery model

2. **Devices Table Schema**
```sql
CREATE TABLE [dbo].[devices](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Brand] [nvarchar](100) NOT NULL,
    [State] [nvarchar](20) NOT NULL,
    [CreationTime] [datetimeoffset] NOT NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED ([Id] ASC)
)
```

### Running Database Scripts

**Option 1: Docker Container**
```bash
# Copy script to container
docker cp databases/00-create-database.sql devicesapi-sqlserver:/tmp/

# Execute inside container
docker exec -it devicesapi-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P <YourPassword> -i /tmp/00-create-database.sql
```

**Option 2: SQL Server Management Studio (SSMS)**
1. Connect to `localhost,1433` with SA credentials (User: sa, Password: configured in docker-compose.yml)
2. Open [00-create-database.sql](databases/00-create-database.sql)
3. Execute the script (F5)

**Option 3: Azure Data Studio**
1. Connect to the server
2. Open the script file
3. Run the script

**Option 4: Command Line (sqlcmd)**
```bash
sqlcmd -S localhost -U sa -P <YourPassword> -i databases/00-create-database.sql
```

## 🧪 Running Tests

### Unit Tests

Run all tests:
```bash
dotnet test
```

### Code Coverage

Use the PowerShell script to run tests with coverage reports:

```powershell
.\run-tests-with-coverage.ps1
```

This will:
1. Execute all tests with code coverage collection
2. Generate HTML coverage reports in `CoverageReport/`
3. Display coverage summary in the console
4. Automatically open the HTML report in your browser

**Coverage Reports Location:**
- HTML Report: `CoverageReport/index.html`
- Test Results: `TestResults/`

### Test Project

Tests are located in [tests/one-global-devices-api-tests/](tests/one-global-devices-api-tests/) and use:
- xUnit for test framework
- Moq for mocking
- FluentAssertions for readable assertions
- Coverlet for coverage collection

## 📁 Project Structure

```
one-global-devices-api/
├── src/
│   └── one-global-devices-api/
│       ├── Application/          # Controllers and DTOs
│       ├── Domain/                # Entities, Services, Repositories
│       ├── Infra.SQLServer/       # Database implementations
│       ├── Program.cs             # Application entry point
│       ├── appsettings.json       # Configuration
│       ├── Dockerfile             # Container image definition
│       └── docker-compose.yml     # Multi-container orchestration
├── tests/
│   └── one-global-devices-api-tests/  # Unit tests
├── databases/
│   └── 00-create-database.sql     # Database setup script
└── run-tests-with-coverage.ps1    # Test coverage script
```

## 🛠️ Development

### Prerequisites for Local Development

- .NET 9.0 SDK
- SQL Server 2022 (or Docker)
- Visual Studio 2022 or VS Code

### Build

```bash
dotnet build
```

### Run Locally (without Docker)

1. Start SQL Server
2. Update connection string in [appsettings.json](src/one-global-devices-api/appsettings.json)
3. Run the database script
4. Start the application:

```bash
cd src/one-global-devices-api
dotnet run
```

## 📝 License

This project is licensed under the MIT License.

## 👤 Author

William B. Santos
