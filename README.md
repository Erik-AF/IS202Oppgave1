
# CrisisSystem

CrisisSystem is an ASP.NET Core MVC web application developed as part of the IS-20X courses at the University of Agder.

The application is intended to support coordination between public actors and resource providers during crises by providing an overview of needs, available resources, and their geographical locations.

---

## AI Usage

In this project we have used the language models Claude and ChatGPT as support during development. We used them to get explanations of concepts in ASP.NET Core MVC, HTML and CSS. The usage of AI also contributed with navigating Rider and VS Code, for troubleshooting, and to get suggestions for code and text. All suggestions were reviewed, adapted and tested by the group before being used. 

### Example Prompts

The prompts were originally written in Norwegian and are translated to English here.

**Emma**
- "Explain the code line by line so I can write it myself and understand it"
- "Can I write this code using the C# syntax I learned in IS-110?"
- "What is usually on an about page on a website?"
- "Why is InformationController.cs showing red?"
- "When I press send on the contact form I get no confirmation, what is wrong with KontaktBekreftelse.cshtml?"

**Sarah**
- 

**Erik**
- 

**Sindre**
- 

**Marius**
- 

**Vetle**
-


---

## Project Requirements

The application is developed according to the requirements of the assignment.

- ASP.NET Core MVC
- Controller, ViewModel, and View
- Responsive web pages
- Dynamic content retrieved from the web server
- GET and POST requests
- Forms for receiving user input
- Displaying submitted data on another web page
- A map that allows geographical data to be selected and displayed on another web page
- Docker
- GitHub documentation
- Code documentation

---

## Getting Started

### Prerequisites

Before running the project, make sure the following software is installed:

- Git
- Docker
- .NET SDK

### Clone the repository

```bash
git clone https://github.com/Erik-AF/IS202Oppgave1
cd IS202Oppgave1
```

### Running the application

1. Make sure Docker Desktop is running.
2. Start the AppHost project from the root folder:

   ```bash
   dotnet run --project IS202Oppgave11.AppHost
   ```

   Alternatively, open the solution in Rider and run `IS202Oppgave11.AppHost`.
3. Open the web application at `http://localhost:5027`, or click the endpoint for `Oppgave202` in the Aspire dashboard.

---

## System Architecture

The application follows the ASP.NET Core MVC pattern and is orchestrated with .NET Aspire. The web application runs in a Docker container that is started by the Aspire AppHost.

The AppHost builds the web application from `202Oppgave/Dockerfile` and runs it as a container named `Oppgave202`. The application is available at `http://localhost:5027`, which is mapped to port 8080 inside the container.

### Overview

```text
Browser
   |
   |  HTTP (GET / POST)  and  fetch (JSON)
   v
+--------------------------------------------+
|  Oppgave202 (Docker container)             |
|                                            |
|  Controllers  -->  Models / ViewModels     |
|      |                    |                |
|      v                    v                |
|  Views (Razor)      In-memory data         |
|  + Bootstrap        (temporary, until      |
|  + Leaflet map       MariaDB is added)     |
+--------------------------------------------+
   ^
   |  built from 202Oppgave/Dockerfile,
   |  started and monitored by
   |
IS202Oppgave11.AppHost (.NET Aspire)
```

### Components

**Controllers** receive requests from the browser and decide what to return.

| Controller | Responsibility |
|---|---|
| `HomeController` | Front page, privacy page and error page |
| `KartController` | Registering points on the map, the map overview, and a JSON endpoint with all points |
| `NeedController` | Registering needs from public actors, with validation and a confirmation page |
| `InformationController` | About, help and contact pages, including the contact form |

**Models and ViewModels** hold the data sent between controllers and views.

| Class | Used for |
|---|---|
| `RegistrerPunktViewModel` | Input from the map registration form (coordinates, category, type, description) |
| `KartPunktViewModel` | A registered point shown on the map. Also the agreed data format for when the database is added |
| `NeedViewModel` | Input when a public actor registers a need, with validation and priority |
| `KontaktFormModel` | Input from the contact form |
| `ErrorViewModel` | Request ID shown on the error page |

**Views** are Razor pages (`.cshtml`) styled with Bootstrap and `components.css`, so they adapt to both mobile and desktop.

**Leaflet** displays the maps and lets the user select a location.

**Data storage:** registered map points are currently stored in a static list in `KartController`. Registered needs are validated and shown on a confirmation page, but not stored yet. Both will be stored in MariaDB.

### Request Flow: Map Registration

```text
1. GET  /Kart           -> KartController.Index()        -> form with map
2. User clicks the map  -> coordinates are filled into the form
3. POST /Kart           -> KartController.Index(model)   -> validates that a point is selected
4a. No point selected   -> same view with error message
4b. Point selected      -> point is saved, redirect to /Kart/Oversikt
5. GET  /Kart/Oversikt  -> page loads, JavaScript fetches /Kart/Data
6. GET  /Kart/Data      -> KartController.Data()         -> returns all points as JSON
7. Leaflet draws the points on the map
```

The redirect after POST (Post-Redirect-Get) prevents the form from being submitted twice if the user reloads the page.

### Request Flow: Register Need

```text
1. GET  /Need/Create -> NeedController.Create()      -> empty form
2. POST /Need/Create -> NeedController.Create(model) -> validates input
3a. Invalid input    -> same view with validation messages
3b. Valid input      -> Confirmation view shows the submitted need
```

### Request Flow: Contact Form

```text
1. GET  /Information/Kontakt -> InformationController.Kontakt()      -> empty form
2. POST /Information/Kontakt -> InformationController.Kontakt(model) -> validates input
3a. Missing fields           -> same view with error message
3b. All fields filled in     -> KontaktBekreftelse view shows the submitted data
```

<!-- TODO: Add MariaDB to the overview and components when the database is connected -->

---

## Project Structure

```text
IS202Oppgave1/
├── 202Oppgave/                      # The web application (ASP.NET Core MVC)
│   ├── Controllers/
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   └── Dockerfile
├── IS202Oppgave11.AppHost/          # .NET Aspire orchestration
├── IS202Oppgave11.ServiceDefaults/  # Shared Aspire configuration
├── README.md
└── Testscenarioer.md
```

---

## Git Workflow

Development should be performed on separate branches rather than directly on `main`.

The general workflow is:

```text
Create branch
     ↓
Make changes
     ↓
Commit changes
     ↓
Push branch
     ↓
Create Pull Request
     ↓
Review
     ↓
Merge into main
```

### Branch Naming Convention

Branch names should:

- Use one of the prefixes listed below.
- Use `_` between the prefix and branch name.
- Use lowercase letters.
- Use `-` to separate words.
- Be short and descriptive.

Example:

```text
feat_login-page
```

or:

```text
refactor_login-page-refactor
```

---

## Commit Messages & Pull Request Names

Commit messages and Pull Request names should use the following prefixes.

| Prefix | Usage |
|---|---|
| `feat:` | A new feature |
| `visual:` | Changes that only affect frontend visuals or text (HTML, CSS) |
| `fix:` | A bug fix |
| `docs:` | Documentation-only changes |
| `style:` | Changes that do not affect the meaning of the code, such as formatting or whitespace |
| `refactor:` | A code change that neither fixes a bug nor adds a feature |
| `perf:` | A code change that improves performance |
| `test:` | Adding missing tests or correcting existing tests |
| `build:` | Changes affecting the build system or external dependencies, such as Docker or libraries |
| `chore:` | Other changes that do not modify source or test files |
| `database:` | Changes to data transfer objects, repository, and SQL |
| `revert:` | Reverts a previous commit |
| `security:` | Changes related to application security |

### Examples

```text
feat: added login page
```

```text
visual: improved resource form layout
```

```text
fix: fixed resource form validation
```

```text
docs: updated docker instructions
```

```text
build: added docker configuration
```

---

## Testing

All test scenarios and results are documented in [Testscenarioer.md](Testscenarioer.md).

Each group member writes and runs the scenarios for their own functionality using the shared template. Emma collects the results, verifies fixed issues and fills in the summary before submission.

Failed tests are registered as GitHub Issues and linked in the test document.


### Test Results

<!-- HUSK: LEGG TIL HER NÅR ALLE ER FERDIG. -->

---

## Code Documentation

Code should be documented where necessary to explain functionality that is not immediately clear from the code itself.

Comments should explain **why** something is done rather than unnecessarily describing obvious code.

Example:

```csharp
// Validate the selected coordinates before creating the resource.
if (model.Latitude == null || model.Longitude == null)
{
    // ...
}
```

---

## Contributors

This project is developed by Group 7 as part of IS-20X.

| Name | GitHub |
|---|---|
| Erik Forberg | |
| Emma Opoku | |
| Sarah Brooks | |
| Sindre Solberg | |
| Marius Solberg | |
| Vetle Hvål-Jenssen | |

---

## Course Information

**Courses:**
- IS-200 – Systemanalyse og systemutvikling
- IS-201 – Datamodellering og databasesystemer
- IS-202 – Programmeringsprosjekt

**Institution:** University of Agder  
**Project:** CrisisSystem
