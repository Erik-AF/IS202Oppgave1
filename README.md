# IS202Oppgave1

# CrisisSystem

CrisisSystem is an ASP.NET Core MVC web application developed as part of the IS-200 course at the University of Agder.

The application is intended to support coordination between public actors and resource providers during crises by providing an overview of needs, available resources, and their geographical locations.

## Project Requirements

The application is developed according to the requirements of the assignment.

The project shall include:

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
git clone <repository-url>
cd <repository-name>
```

### Running the application

Add instructions here for how the application should be started.

For example:

```bash
docker compose up --build
```

> Update this section when the final Docker configuration has been created.

---

## System Architecture

The application follows the ASP.NET Core MVC architecture.

### Model

Models represent the data and domain concepts used by the application.

### View

Views are responsible for presenting information to the user.

### Controller

Controllers receive HTTP requests, process user actions, and determine which views or responses should be returned.

### ViewModel

ViewModels contain the data required by specific views and are used to transfer data between controllers and views.

### Architecture Overview

```text
User
  |
  v
Controller
  |
  v
ViewModel / Model
  |
  v
View
  |
  v
User
```

> Expand this section as the architecture of the application develops.

---

## Project Structure

```text
CrisisSystem/
├── Controllers/
├── Models/
├── ViewModels/
├── Views/
├── wwwroot/
├── Dockerfile
├── README.md
└── ...
```

> Update this structure as the project develops.

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
- Use `/` between the prefix and branch name.
- Use lowercase letters.
- Use `-` to separate words.
- Be short and descriptive.

Example:

```text
feat/login-page
```

or:

```text
refactor/login-page-refactor
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

Testing scenarios and their results should be documented here.

### Test Scenario Template

| ID | Scenario | Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|
| T01 | Example test | 1. ... 2. ... | Expected behaviour | Actual behaviour | Pass/Fail |

### Test Results

Add completed test scenarios here throughout development.

Example:

| ID | Scenario | Expected Result | Actual Result | Status |
|---|---|---|---|---|
| T01 | Submit resource form | Resource information is displayed on the result page | Resource displayed correctly | Pass |

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

This project is developed by Group 7 as part of IS-200.

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

**Course:** IS-200 – Systemanalyse og systemutvikling  
**Institution:** University of Agder  
**Project:** CrisisSystem
