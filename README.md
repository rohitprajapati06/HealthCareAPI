# 🏥 Smart Healthcare System

> An enterprise-grade **Healthcare Management System** built with **ASP.NET Core Web API**, following **Clean Architecture**, **CQRS**, and **SOLID Principles**, integrated with modern Azure cloud services.

---

## 📖 Overview

Smart Healthcare System is a scalable healthcare management platform designed to simplify interactions between **Patients**, **Doctors**, **Hospitals**, and **Administrators**.

The project demonstrates enterprise backend development using **ASP.NET Core**, **Entity Framework Core**, **MediatR**, **JWT Authentication**, **Azure Services**, and modern software architecture patterns.

This project is being built as a real-world production-ready application and serves as a comprehensive portfolio project for .NET and Azure development.

---

# ✨ Features

## 🔐 Authentication & Authorization

- JWT Authentication
- Refresh Token
- ASP.NET Core Identity
- Role-Based Authorization
- Forgot Password
- Reset Password
- Secure Password Hashing

---

## 🏥 Hospital Management

- Multi-Hospital Support
- Hospital CRUD
- Hospital-wise Dashboard
- Hospital Search
- Hospital Administration

---

## 👨‍⚕️ Doctor Module

- Doctor Registration
- Doctor Approval / Rejection
- Doctor Profile Management
- Availability Slot Management
- Hospital-wise Availability
- Consultation Fee Management

---

## 👨‍👩‍👧 Patient Module

- Patient Registration
- Medical History
- Appointment Booking
- View Prescriptions
- View Medical Records

---

## 📅 Appointment System

- Book Appointment
- Cancel Appointment
- Complete Appointment
- Slot-Based Booking
- Prevent Double Booking
- Appointment Status Workflow

---

## 💊 Prescription Module

- Create Prescription
- View Prescription
- Link Prescription with Appointment
- Digital Prescription Management

---

## 📂 Medical Records

- Upload Medical Reports
- Update Medical Records
- Delete Medical Records
- Patient Medical History
- File Storage Abstraction

---

## 📊 Dashboard

### Patient Dashboard

- Upcoming Appointments
- Medical History
- Prescriptions

### Doctor Dashboard

- Today's Appointments
- Patient Statistics
- Schedule Overview

### Hospital Dashboard

- Revenue
- Appointment Analytics
- Doctor Performance

### Super Admin Dashboard

- System Analytics
- Hospital Monitoring
- User Statistics

---

# 🏗️ Architecture

The project follows **Clean Architecture**.

```
Presentation (API)
        │
        ▼
Application (CQRS + MediatR)
        │
        ▼
Domain
        ▲
        │
Infrastructure
        │
Persistence
```

### Design Principles

- Clean Architecture
- CQRS
- SOLID Principles
- Dependency Injection
- Repository Pattern
- Domain Driven Design (DDD Concepts)

---

# 🛠️ Technology Stack

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- JWT Authentication

### Libraries

- MediatR
- AutoMapper
- FluentValidation
- Serilog

### Testing

- xUnit
- Moq
- FluentAssertions

---

# ☁️ Azure Services

This project is designed to use the following Azure services:

| Azure Service | Purpose |
|---------------|---------|
| Azure App Service | API Hosting |
| Azure API Management (APIM) | API Gateway, Rate Limiting, Versioning |
| Azure SQL Database | Cloud Database |
| Azure Blob Storage | Medical Reports & Documents |
| Azure Service Bus | Event-Driven Communication |
| Azure Functions | Background Jobs |
| Azure Cache for Redis | Performance & Caching |
| Azure Key Vault | Secrets Management |
| Azure Application Insights | Monitoring & Telemetry |
| Azure OpenAI | AI Symptom Checker |

---

# 🔄 Request Flow

```
Client

↓

API Controller

↓

MediatR

↓

Command / Query

↓

Handler

↓

Entity Framework Core

↓

SQL Server

↓

Response
```

---

# 📂 Solution Structure

```
SmartHealthcareSystem

│

├── SmartHealthcare.API

├── SmartHealthcare.Application

├── SmartHealthcare.Domain

├── SmartHealthcare.Infrastructure

├── SmartHealthcare.Persistence

├── SmartHealthcare.Shared

│

└── SmartHealthcare.UnitTests
```

---

# 🔒 Security

- JWT Authentication
- Refresh Tokens
- Role-Based Authorization
- Global Exception Handling
- File Upload Validation
- Secure Password Storage

---

# 📈 Logging

Implemented using **Serilog**

- Request Logging
- Exception Logging
- Rolling Log Files
- Structured Logging

---

# 🧪 Testing

### Unit Testing

- xUnit
- Moq
- FluentAssertions

### Integration Testing

- WebApplicationFactory
- In-Memory Database

---

# 🚀 CI/CD (Planned)

GitHub Actions

```
Push

↓

Restore

↓

Build

↓

Unit Tests

↓

Integration Tests

↓

Publish

↓

Deploy to Azure
```

---

# 🧠 Future Enhancements

- AI Symptom Checker (Azure OpenAI)
- Email Notifications
- SMS Notifications
- Billing & Payments
- Reviews & Ratings
- Report Generation
- Background Processing
- Redis Caching
- Event-Driven Architecture
- Docker Support
- Kubernetes Deployment (Future)
- API Versioning
- Multi-Tenant Support

---

# 📌 Current Progress

✅ Authentication

✅ Hospital Module

✅ Doctor Module

✅ Availability Slots

✅ Appointment Module

✅ Prescription Module

✅ Medical Records

✅ Dashboard APIs

✅ File Upload

✅ Global Exception Handling

✅ Serilog Logging

🟡 Unit Testing (In Progress)

🟡 Azure Integration (Planned)

🟡 Docker (Planned)

🟡 CI/CD (Planned)

---

# 🎯 Project Goals

- Build a production-ready healthcare backend.
- Demonstrate enterprise software architecture.
- Gain hands-on experience with Azure cloud services.
- Implement modern DevOps practices.
- Showcase scalable and maintainable backend development.

---

# 👨‍💻 Author

**Rohit Prajapati**

Backend Developer | ASP.NET Core | Azure | Clean Architecture | CQRS

---

## ⭐ If you found this project useful, consider giving it a star!
