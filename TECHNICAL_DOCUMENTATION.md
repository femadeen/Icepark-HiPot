# Hipot Technical Documentation

## 1. High-Level Architecture

The Hipot application is a .NET MAUI Blazor Hybrid application. This architecture allows for a cross-platform native client application that uses Blazor for its user interface. The application is designed to be a modern replacement for a legacy Windows Forms application, `Pekasuz3`.

The core of the application is divided into two main projects:

*   **Hipot**: The main .NET MAUI project that contains the UI (Blazor components) and the application's entry point.
*   **Hipot.Data**: A class library that contains the application's data models, services, and database context.

The application follows a modular, service-oriented architecture with a clear separation of concerns. Dependency injection is used extensively to manage the application's services.

## 2. Project Structure

### 2.1. Hipot

This is the main project of the application. It contains the following key directories:

*   **`Components`**: This directory contains the Blazor components that make up the application's UI.
    *   **`Layout`**: Contains the main layout and navigation menu for the application.
    *   **`Pages`**: Contains the main pages of the application, such as `Index.razor` and `Login.razor`.
    *   **`Shared`**: Contains reusable UI components, such as `InputPanel.razor`, `MessageBoxPanel.razor`, and `PassFailPanel.razor`.
*   **`Data`**: Contains the `AppState.cs` file, which is used to manage the application's global state.
*   **`Platforms`**: Contains platform-specific code for Android, iOS, MacCatalyst, Tizen, and Windows.
*   **`Resources`**: Contains the application's resources, such as icons, fonts, images, and raw assets.
    *   **`tscript`**: Contains the XML test scripts that define the test sequences.
*   **`wwwroot`**: Contains the root HTML page (`index.html`) for the Blazor application and static assets like CSS and images.

### 2.2. Hipot.Data

This project contains the application's data layer and business logic.

*   **`Core`**: This directory contains the core data-related classes.
    *   **`Context`**: Contains the `HipotDbContext` for interacting with the SQLite database.
    *   **`DTOs`**: Contains Data Transfer Objects used to transfer data between layers.
    *   **`Models`**: Contains the application's data models.
    *   **`Repositories`**: Contains the repository interfaces and implementations for accessing the database.
    *   **`Services`**: Contains the service interfaces and implementations for the application's business logic.

## 3. File Descriptions

### 3.1. Hipot Project

*   **`MauiProgram.cs`**: The entry point of the application. It initializes the MAUI application, configures services for dependency injection, and sets up the database.
*   **`App.xaml.cs`**: The main application class. It creates the main window and sets up global exception handlers.
*   **`MainPage.xaml`**: The main page of the application, which hosts the `BlazorWebView`.
*   **`Components/Pages/Index.razor`**: The main UI of the application. It displays the test channels, handles user interactions, and orchestrates the test sequence.
*   **`Components/ChannelView.razor`**: A component that displays the state of a single test channel and allows the user to interact with it.
*   **`Data/AppState.cs`**: A singleton service that holds the global state of the application, such as the current user and any error messages.

### 3.2. Hipot.Data Project

*   **`Core/Context/HipotDbContext.cs`**: The Entity Framework Core database context for the application. It defines the database schema and provides an API for querying and saving data.
*   **`Core/Services/SequenceService.cs`**: This service is responsible for executing the test sequences defined in the XML test scripts.
*   **`Core/Services/SerialPortService.cs`**: This service manages the communication with the Hipot tester hardware via serial ports.
*   **`Core/Services/XmlConfigService.cs`**: This service reads the application's configuration from XML files.
*   **`Core/Services/XmlTestScriptService.cs`**: This service reads and parses the XML test scripts.

## 4. Functionalities

*   **User Authentication**: The application requires users to log in before they can access the main testing interface.
*   **Multi-Channel Testing**: The application supports testing on multiple channels simultaneously. Each channel is displayed in its own tab.
*   **Test Sequence Execution**: The application executes test sequences defined in XML files. The progress of each test is displayed in the UI.
*   **Hardware Communication**: The application communicates with the Hipot tester hardware via serial ports to control the tests and read the results.
*   **Data Logging**: The application logs test results and other data to an SQLite database.
*   **Dynamic UI**: The UI is updated in real-time to reflect the status of the tests. The tabs for each channel are color-coded to indicate the test status (e.g., "TESTING", "PASS", "FAIL").
*   **Interactive UI Panels**: The application uses modal panels to interact with the user, such as prompting for input, displaying messages, and showing the final pass/fail status.

## 5. Application Flow

1.  **Application Startup**: The application is launched, and the `MauiProgram.cs` file initializes the MAUI application and its services. The database is migrated to the latest version.
2.  **Login**: The user is presented with a login screen. Upon successful login, the user's information is stored in the `AppState` service, and the user is navigated to the main testing interface.
3.  **Channel Initialization**: The `Index.razor` component initializes the test channels based on the configuration files. It opens the serial ports and prepares each channel for testing.
4.  **Test Execution**:
    *   The user enters a serial number for a unit under test (UUT) and clicks the "Start" button.
    *   The `ChannelView.razor` component calls the `StartTest` method, which retrieves the appropriate test script based on the serial number.
    *   The `SequenceService` is then used to execute the test sequence defined in the script.
    *   During the test, the `SequenceService` sends commands to the Hipot tester via the `SerialPortService` and receives the results.
    *   The UI is updated in real-time with the test progress, logs, and status.
5.  **Test Completion**:
    *   Once the test is complete, the final result ("PASS" or "FAIL") is displayed to the user.
    *   The test results are logged to the database.
6.  **Application Exit**: The user can exit the application through a menu option. A confirmation prompt is displayed before the application closes.
