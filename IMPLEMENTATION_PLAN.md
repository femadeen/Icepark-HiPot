# Implementation Plan: Authentication with Roles

This plan outlines the steps to create a user authentication system where each user is associated with a project and has a specific role. The data will be persisted in a SQL Server database using Entity Framework Core.

### 1. Define Database Models

-   **Location:** `src/Hipot.Data/Core/Models/`
-   **Actions:**
    -   Create a `Role` enum with values like `Admin`, `Operator`, and `Viewer`.
    -   Create a `User` entity with properties:
        -   `int Id` (Primary Key)
        -   `string Username`
        -   `string PasswordHash`
        -   `Role Role`
        -   `int ProjectId` (Foreign Key)
        -   `Project Project` (Navigation Property)
    -   Create a `Project` entity with properties:
        -   `int Id` (Primary Key)
        -   `string Name`
        -   `ICollection<User> Users` (Navigation Property)

### 2. Update Database Context

-   **File:** `src/Hipot.Data/Core/Context/HipotDbContext.cs`
-   **Actions:**
    -   Add `DbSet<User> Users { get; set; }` and `DbSet<Project> Projects { get; set; }`.
    -   In the `OnModelCreating` method, configure the one-to-many relationship between `Project` and `User`.

### 3. Database Migration

-   **Actions:**
    -   Use the `dotnet ef migrations add` command to create a new migration for the `User` and `Project` entities.
    -   Use the `dotnet ef database update` command to apply the migration to the SQL Server database.

### 4. Create Data Transfer Objects (DTOs)

-   **Location:** `src/Hipot.Data/Core/DTOs/`
-   **Actions:**
    -   Create a `UserDto` to transfer user data (e.g., `Id`, `Username`, `Role`, `ProjectId`) between the backend and the UI, excluding sensitive information like `PasswordHash`.

### 5. Develop Services

-   **Location:** `src/Hipot.Data/Core/Services/`
-   **Actions:**
    -   Create an `IAuthenticationService` interface in the `Interfaces` sub-directory with methods like:
        -   `Task<UserDto> LoginAsync(string username, string password);`
        -   `Task LogoutAsync();`
        -   `Task<UserDto> GetCurrentUserAsync();`
    -   Create an `AuthenticationService` class in the `Implementations` sub-directory that implements the interface. This service will handle:
        -   Password hashing and verification.
        -   Validating user credentials against the database.
        -   Managing user state.

### 6. Update UI Components

-   **Actions:**
    -   **Login Page (`src/Hipot/Components/Pages/Login.razor`):**
        -   Inject `IAuthenticationService`.
        -   Bind UI input fields to properties for username and password.
        -   Call the `LoginAsync` method on a button click.
    -   **Application State (`src/Hipot/Data/AppState.cs`):**
        -   Add a property to hold the current `UserDto`.
        -   Include methods to update and clear the user state upon login and logout.
    -   **Route Protection (`src/Hipot/Components/Routes.razor` or `src/Hipot/Components/Layout/MainLayout.razor`):**
        -   Inject `AppState`.
        -   Use conditional logic to check if a user is authenticated.
        -   Use Blazor's `<AuthorizeRouteView>` or custom logic to redirect unauthenticated users to the login page.
        -   Show/hide UI elements based on the user's role from `AppState`.

### 7. Dependency Injection

-   **File:** `src/Hipot/MauiProgram.cs`
-   **Actions:**
    -   Register the `IAuthenticationService` and its implementation (`AuthenticationService`).
    -   Register the `AppState` as a singleton.