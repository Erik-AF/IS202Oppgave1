# CrisisSystem — Group 7

CrisisSystem is an ASP.NET Core MVC web application developed by **Group 7** at the University of Agder. It supports coordination between public actors and resource providers during crises by showing needs, available resources and geographical locations.

## Group 7 — Team Members

| Name | GitHub 
|---|---|---|
| Erik Forberg | Erik-AF
| Emma Opoku | emma-opoku
| Sarah Brooks | BrooksSarah
| Sindre Solberg | sindrejsolberg 
| Marius Solberg | Marius-solberg
| Vetle Hvål-Jenssen | VetleUIA 



## Project Goal

The goal is to make crisis coordination easier by allowing public actors to register needs, resource providers to register available resources, and users to view relevant locations on a map. The project uses ASP.NET Core MVC and is run using .NET Aspire and Docker.

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

## Prerequisites & Getting Started

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

## Default / Example Users

No default user accounts or login credentials are documented in the current project README. Add confirmed demo accounts here if authentication is implemented. Do **not** commit real passwords or secrets.

| User type | Example use | User Name | Password |
|---|---|---|
| Public actor | Register a crisis-related need | publicactor@example.com |!RedSideraptors1|
| Resource provider | Register an available resource | provider@example.com | !RiverScuttle1 |
| Admin | Can do All | admin@example.com |!blueSideGromp1 |
## Test Data — Available Resources

A proposed sample dataset is provided in [Testdata — Available Resources](Testscenarioer.md) for development and manual testing. These are **example records**, not confirmed preloaded resources in the running application. The current README describes temporary in-memory map-point storage; database persistence is planned.

## System Architecture

See [System Architecture](SystemArchitecture.md) for the architecture overview, components and request flows.

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

## GitHub Rules

<details>
<summary>Show Git workflow, branch naming and commit/PR conventions</summary>

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

</details>

## AI Usage

<details>
<summary>Show AI tools, usage and example prompts</summary>

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
- We are working on a project for Kartverket and Heimevernet as part of the IT and Information Systems programme this year. Help me and explain each step, including possible solution approaches where relevant. I will paste the assignment description, the project case description, and the division of responsibilities. Wait until I write “ready” before responding.
- Provide a specific breakdown of the proposed code solution.
- Chat has refactored the plan. How should areas 4 and 5 be divided between Sarah and Emma without using partial/shared ownership?


**Erik**
- "How does the AppHost.cs builder work and give insight to how i would use a docker container as a builder?"


**Sindre**
- "Explain Vetle's code step by step so I understand how it works in depth."
- "Check my code for mistakes and duplicate sections."
- "Help me troubleshoot why the application will not start."
- "Explain how i can adapt the NEED code to the group's existing solution better."
- "Explain what changes i can make to the need registration page so its consistent with the resource page."

**Marius**
- "Help me identify why the map is not appearing by reviewing my code and screenshots"
- "Help me move the 'back' button below the map on the resource details page"
- "Explain how to commit, push, and merge my changes into main using Rider and Github"
- "Help me add my existing Leaflet map to the resource registration form"

**Vetle**
- "help me understand my part of the assignment by seperating everything into spesific tasks"
- "Which classes should I create for the Resource module"
- "Which methods should I use to register a resource"
- "What's the difference between a controller and a view model, and which one should my form data go through"
- "Why am I getting 'Cannot resolve symbol' for ResourceViewModel in ResourceController.cs"
- "How do I resolve a merge conflict in _Layout.cshtml between my branch and main"

</details>
