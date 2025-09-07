
```mermaid
graph TD
    subgraph User Interaction
        User([<img src='https://www.svgrepo.com/show/499824/user.svg' width='40' /><br>User])
    end

    subgraph UI Layer (.NET MAUI Blazor)
        direction LR
        Login[Login.razor]
        Index[Index.razor<br>(Tabs for Channels)]
        ChannelView[ChannelView.razor<br>(Test Control & Display)]
        subgraph Shared Components
            direction TB
            InputPanel[InputPanel.razor]
            MessageBox[MessageBoxPanel.razor]
            PassFail[PassFailPanel.razor]
        end
    end

    subgraph Application Logic / Services
        direction TB
        AppState[AppState<br>(Global State)]
        SequenceService[SequenceService<br>(Test Execution)]
        SerialPortService[SerialPortService<br>(Hardware I/O)]
        XmlServices[XML Services<br>(Config & Scripts)]
        DataService[DataService<br>(In-Memory Data)]
        DbRepo[Repositories<br>(DB Access)]
    end

    subgraph Data & Hardware
        direction LR
        subgraph Data Layer
            direction TB
            SQLite[HipotDbContext<br>(SQLite Database)]
            XML[XML Files<br>(Test Scripts, Config)]
        end
        subgraph External
            direction TB
            HipotTester[<img src='https://www.svgrepo.com/show/493390/circuit.svg' width='40' /><br>Hipot Tester]
        end
    end

    %% Flows
    User -- "1. Login" --> Login
    Login -- "2. Authenticates &<br>Updates AppState" --> AppState
    User -- "3. Interacts with" --> Index
    Index -- "Loads on startup" --> XmlServices
    Index -- "Selects Channel" --> ChannelView

    ChannelView -- "4. Start/Abort Test" --> SequenceService
    ChannelView -- "Displays data from" --> DataService
    ChannelView -- "Shows/Hides" --> Shared Components

    SequenceService -- "5. Gets Test Script" --> XmlServices
    XmlServices -- "Reads from" --> XML
    SequenceService -- "6. Sends Commands" --> SerialPortService
    SerialPortService -- "7. Communicates with" --> HipotTester
    HipotTester -- "8. Sends Results" --> SerialPortService
    SerialPortService -- "9. Forwards to" --> SequenceService
    SequenceService -- "10. Updates" --> DataService
    SequenceService -- "11. Logs Results via" --> DbRepo
    DbRepo -- "Writes to" --> SQLite

    %% Style Definitions
    classDef ui fill:#D6EAF8,stroke:#3498DB,stroke-width:2px;
    classDef services fill:#D1F2EB,stroke:#1ABC9C,stroke-width:2px;
    classDef data fill:#FDEDEC,stroke:#E74C3C,stroke-width:2px;
    classDef hardware fill:#FDEBD0,stroke:#F39C12,stroke-width:2px;

    class Login,Index,ChannelView,Shared Components ui;
    class AppState,SequenceService,SerialPortService,XmlServices,DataService,DbRepo services;
    class SQLite,XML data;
    class HipotTester hardware;
```

### How to Use This File

1.  **Install a Mermaid Editor:**
    *   **VS Code:** Install the "Markdown Preview Mermaid Support" or "Mermaid Editor" extension.
    *   **Online:** Go to a website like `mermaid.live`.

2.  **Generate the Diagram:**
    *   Copy the Mermaid code (the part inside the ` ```mermaid ... ``` ` block).
    *   Paste it into the editor.
    *   The diagram will be rendered automatically.

3.  **Export to PDF:**
    *   Most editors will have an "Export" or "Save As" option. Choose PDF as the format.

This diagram provides a visual representation of the Hipot application's architecture, showing the relationships between the UI, services, data layer, and external hardware, along with the primary application flows.
