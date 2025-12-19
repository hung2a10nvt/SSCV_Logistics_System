# SSCV_Logistics_System

## A real-time C# backend system for logistics that processes vehicle telemetry data and monitors vehicle statuses.

## **Key Feature**
- **Ingest Data API**: Provides endpoints to receive telemetry data (Speed, Temperature, Coordinates) from vehicle-mounted IoT devices. (IRL its meant to be, but without real vehicles/devices we send vehicles' info manually)
- **Real-time Alert System**:
  - Automatically analyzes data to detect violations (Speed > 80km/h or Engine Temperature > 30°C, well, depends on the system).
  - Sends instant alert notifications directly to mobile devices via Telegram Bot.

## **Data Flow Architecture**
1. **API Service**: Receives JSON packets -> Pushes to telemetry_queue.
2. **Telemetry Worker**: Consumes messages from telemetry_queue -> Maps vehicle info -> Saves to Database -> Checks for violations -> Pushes to vehicle_alerts.
3. **Alert Worker**: Consumes messages from vehicle_alerts -> Saves alert history -> Calls Telegram API to send notifications.

## **Installation**
1. **Prerequisites**
   
Docker Desktop installed and running.

2. **Environment Variables Configuration**
  
  In the root dir, create a file named .env

  Copy the following template and fill in your credentials:
  ```bash
  # Database Configuration
  DB_USER=postgres
  DB_PASSWORD=your_password_here
  DB_NAME=logistics_db
  
  # RabbitMQ Configuration
  RABBITMQ_USER=guest
  RABBITMQ_PASS=guest
  
  # Telegram Configuration
  # Chat with @BotFather to get your Token and @userinfobot to get your ChatID
  TELEGRAM_BOT_TOKEN=
  TELEGRAM_CHAT_ID=
  ```
3. **Launch with Docker Compose**
  ```bash
  docker-compose up -d --build
  ```
4. **Essential Service URLs**
- **Swagger**
```bash
http://localhost:5294/swagger
```
- **RabbitMQ**
```bash
http://localhost:15672
```
