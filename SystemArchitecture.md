# System Architecture – CrisisSystem

## Overview

CrisisSystem is an ASP.NET Core MVC web application developed by Group 7.

The application follows the **Model-View-Controller (MVC)** architectural pattern and uses **.NET Aspire** for orchestration and **Docker** for containerization.

The system supports coordination between public actors and resource providers during crises by providing functionality for registering needs, managing resources and displaying geographical information.

## Architecture Diagram

The following diagram illustrates the application architecture.

```text
Browser
   |
   | HTTP (GET / POST) and fetch (JSON)
   v
+--------------------------------------------+
| Oppgave202 (Docker container)              |
|                                            |
| Controllers --> Models / ViewModels        |
|      |                    |                |
|      v                    v                |
| Views (Razor)      In-memory data          |
| + Bootstrap        (temporary, until       |
| + Leaflet map       MariaDB is added)      |
+--------------------------------------------+
   ^
   | Built from 202Oppgave/Dockerfile
   | Started and monitored by
   |
IS202Oppgave11.AppHost (.NET Aspire)
```

The AppHost builds the web application from `202Oppgave/Dockerfile` and runs it as a Docker container named `Oppgave202`.

The application is accessible at:

```text
http://localhost:5027
```

Port `5027` on the host is mapped to port `8080` inside the Docker container.

---

## Architecture Components

### Controllers

Controllers receive HTTP requests from the browser, process user input and determine which views or data should be returned.

| Controller | Responsibility |
|---|---|
| `HomeController` | Handles the front page, privacy page and error page |
| `ResourceController` | Handles resource registration, input validation and confirmation |
| `NeedController` | Handles registration of needs, input validation and confirmation |
| `KartController` | Handles geographical map functionality and JSON data for registered map points |
| `InformationController` | Handles about, help and contact pages, including the contact form |

### Models and ViewModels

Models and ViewModels are used to represent and transfer data between controllers and views.

| Class | Responsibility |
|---|---|
| `ResourceViewModel` | Handles resource registration data, including resource information and geographical location |
| `RegistrerPunktViewModel` | Handles geographical coordinates, category, type and description |
| `KartPunktViewModel` | Represents registered map points |
| `NeedViewModel` | Handles registration of needs, including validation and priority |
| `KontaktFormModel` | Handles input from the contact form |
| `ErrorViewModel` | Contains the request ID displayed on the error page |

### Views

The application uses Razor Views (`.cshtml`) to render dynamic HTML pages.

The user interface uses:

- **HTML** for page structure.
- **CSS** for styling.
- **Bootstrap** for responsive layouts.
- **components.css** for custom component styling.
- **Leaflet** for interactive maps and geographical visualization.

The interface is designed to adapt to both desktop and mobile devices.

---

## Data Storage

The application currently uses temporary in-memory storage for registered map points.

### Current Implementation

- Registered map points are stored in a static list inside `KartController`.
- Registered needs are validated and displayed on a confirmation page but are not permanently stored.
- Map points are made available through a JSON endpoint.
- Data stored in memory is not persistent across application restarts.

### Planned Database Integration

MariaDB is planned for persistent data storage.

The database integration is intended to replace temporary in-memory storage and allow registered information to be retained between application restarts.

**Note:** MariaDB is not yet connected in the architecture described here.

---

## Request Flows

### 1. Resource Registration

The resource registration functionality allows resource providers to register available resources, including their details and geographical location.

```text
1. GET /Resource/Create
   |
   v
ResourceController.Create()
   |
   v
Display resource registration form
   |
   v
User enters resource information
   |
   v
2. POST /Resource/Create
   |
   v
ResourceController.Create(model)
   |
   v
Validate submitted data
   |
   +-- Invalid --> Return form with validation messages
   |
   +-- Valid --> Display confirmation page
```

The resource registration follows the same structure as the need registration functionality, using a controller, view model and Razor view to handle user input and validation.

### 2. Need Registration

The need registration functionality allows public actors to submit information about resources or assistance they require.

```text
1. GET /Need/Create
   |
   v
NeedController.Create()
   |
   v
Display need registration form
   |
   v
User enters need information
   |
   v
2. POST /Need/Create
   |
   v
NeedController.Create(model)
   |
   v
Validate submitted data
   |
   +-- Invalid --> Return form with validation messages
   |
   +-- Valid --> Display confirmation page
```

The need registration uses a controller, view model and Razor view to handle user input and validation.

Currently, registered needs are displayed on a confirmation page but are not permanently stored.

### 3. Contact Form

The contact functionality allows users to submit information through a contact form.

```text
1. GET /Information/Kontakt
   |
   v
InformationController.Kontakt()
   |
   v
Display contact form
   |
   v
User fills in required fields
   |
   v
2. POST /Information/Kontakt
   |
   v
InformationController.Kontakt(model)
   |
   v
Validate submitted information
   |
   +-- Invalid --> Display validation messages
   |
   +-- Valid --> Display KontaktBekreftelse view
```

---

## Containerization and Orchestration

CrisisSystem uses Docker and .NET Aspire to run and manage the application.

### Docker

The Docker configuration is defined in:

```text
202Oppgave/Dockerfile
```

Docker provides a containerized environment for running the web application.

### .NET Aspire

The Aspire AppHost project is responsible for building, starting and monitoring the application container.

```text
IS202Oppgave11.AppHost/
```

The application can be started using:

```bash
dotnet run --project IS202Oppgave11.AppHost
```

Alternatively, the AppHost project can be started directly from JetBrains Rider.

The Aspire dashboard provides access to the application endpoint.

---

## Project Structure

```text
IS202Oppgave1/
├── 202Oppgave/
│   ├── Controllers/
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   └── Dockerfile
│
├── IS202Oppgave11.AppHost/
│
├── IS202Oppgave11.ServiceDefaults/
│
├── docs/
│   └── SYSTEM_ARCHITECTURE.md
│
├── README.md
└── Testscenarioer.md
```

### Directory Responsibilities

| Directory | Responsibility |
|---|---|
| `202Oppgave/` | Main ASP.NET Core MVC application |
| `Controllers/` | Handles HTTP requests and application logic |
| `Models/` | Defines application data models |
| `ViewModels/` | Transfers data between controllers and views |
| `Views/` | Contains Razor pages |
| `wwwroot/` | Contains static files such as CSS and JavaScript |
| `IS202Oppgave11.AppHost/` | Handles .NET Aspire orchestration |
| `IS202Oppgave11.ServiceDefaults/` | Contains shared Aspire configuration |




[Back to README](README.md)
