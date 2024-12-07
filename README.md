
# Webhook-Based Timer Service

## Overview
The **Webhook-Based Timer Service** is a robust solution for scheduling and triggering timers. It sends webhooks when a timer expires and handles scenarios such as missed timers during server downtime. Built with extensibility and scalability in mind, this service is ideal for applications requiring precise event scheduling.

## Assumptions

- **Minimum Time Interval:** The project operates on a minimum time interval of **1 second**. The timer counter checks the time left in intervals of 1 second.
- **Counter Runner:** The system constantly runs a background task that updates the timer status every second to ensure it is checking and updating the time left for each active timer.

---

## Features
- **Timer Management**: Create, update, and manage timers with custom expiration logic.
- **Webhook Integration**: Sends HTTP webhooks to specified endpoints when a timer expires.
- **Recovery Mechanism**: Ensures missed timers during server downtime are handled and webhooks are sent.
- **Scalability**: Designed for high-performance environments with efficient database operations.
- **Extensibility**: Modular architecture for easy integration with other services.

---

## Requirements
- **Framework**: .NET 8
- **Visual Studio 2022** is required for building and running the project. Ensure you have Visual Studio 2022.
- **Database**: Azure Cosmos DB (NoSQL)
- **Dependencies**:
  - Newtonsoft.Json
  - Azure.Cosmos SDK
  - NUnit (for unit testing)

---

## Getting Started

### Clone the Repository
```bash
git clone https://github.com/Rahulbukhar/Webhook-Based-Timer.git
cd Webhook-Based-Timer
```

### Configuration
1. **Database Connection**:
   - Configure your Cosmos DB connection string in the `appsettings.json` file.
   - Example:
     ```json
     {
       "CosmosDb": {
         "COSMOSDB_ACCOUNT_ENDPOINT": "<your-endpoint>",
         "COSMOSDB_ACCOUNT_KEY": "<your-key>",
         "COSMOSDB_DATABASE_NAME": "TimersDB"
       }
     }
     ```

2. **Webhooks**:
   - Ensure your webhook URLs are accessible and capable of handling HTTP POST requests.

### Build and Run
```
Build solution using VS
Hit a Play Button to run Server locally  
```

### Running Tests
Run all unit tests using the NUnit framework:
```
Hit Run unit tests on VS 
```

---

## Usage

### Create a Timer
Send a POST request to the `/api/timers` endpoint with the following payload:
```json
{
  "id": "unique-timer-id",
  "expiryTime": "2024-12-07T12:00:00Z",
  "webhookUrl": "https://example.com/webhook"
}
```

### Retrieve Timers
- **GET `/api/timers/{id}`**: Retrieve a specific timer by ID.
- **GET `/api/timers`**: Retrieve all active timers.

---

## Architecture

### Core Components
1. **TimerBackgroundService**:
   - Continuously monitors and processes timers in the background.
2. **WebhookService**:
   - Handles sending webhooks to configured URLs.
3. **TimerRepository**:
   - Interfaces with Cosmos DB for CRUD operations on timers.

### Recovery Mechanism
- Ensures missed timers during server downtime are processed and webhooks are sent upon startup.

---

## Folder Structure
```plaintext
Webhook-Based-Timer/
│
├── src/                      # Source code
│   ├── Webhook.Based.Timer/  # Core project
│   └── NoSqlDataAccess/      # Data access layer
│
├── test/                     # Unit tests
│   ├── Webhook.Based.Timer.Tests/
│   └── NoSqlDataAccess.Tests/
│
├── README.md                 # Project documentation
└── appsettings.json          # Configuration file
```

---

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request. Ensure all code changes are covered with appropriate tests.

