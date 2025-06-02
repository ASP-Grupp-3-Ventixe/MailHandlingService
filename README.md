# MailHandlingService (ASP.NET Core)

MailHandlingService is a .NET-based microservice for managing emails, attachments, and labels. 
It provides a comprehensive API to create, read, search, move, and delete emails, as well as manage labels and attachments. 
The project is organized into separate layers for domain, application, and infrastructure, following clean architecture principles.

## Features
- **/api/emails** (POST): Create a new email
- **/api/emails** (GET): List emails (with filters)
- **/api/emails/{id}** (GET): Get a specific email
- **/api/emails/{id}** (DELETE): Soft-delete (move to trash)
- **/api/emails/{emailId}/permanent** (DELETE): Permanently delete an email
- **/api/emails/trash/empty** (DELETE): Empty the trash

_Not yet implemented:_
- **/api/emails/{id}/read** (PUT): Mark as read
- **/api/emails/{id}/unread** (PUT): Mark as unread
- **/api/emails/{id}/reply** (POST): Reply to an email
- **/api/emails/{id}/forward** (POST): Forward an email
- **/api/emails/{id}/folder** (PUT): Move email to folder
- **/api/emails/unread-count** (GET): Get unread email count
- **/api/emails/starred** (GET): List starred emails
- **/api/emails/{id}/star** (POST): Star an email
- **/api/emails/{id}/star** (DELETE): Remove star from email

## Environment Variables (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "SqlServer": "..."
  },
  "Jwt": {
    "Issuer": "https://localhost:7111",
    "Audience": "Ventixe",
    "SecretKey": "..."
  }
}
```

## Example Requests

### Create Email
POST `/api/emails`
```json
{
  "from": "user@example.com",
  "to": ["recipient@example.com"],
  "subject": "Hello",
  "body": "This is a test email.",
  "attachments": []
}
```
**Response:**
```json
{
  "succeeded": true,
  "emailId": "...",
  "message": "Email created successfully."
}
```

### List Emails
GET `/api/emails?folder=Inbox&unread=true`
**Response:**
```json
[
  {
    "emailId": "...",
    "from": "user@example.com",
    "to": ["recipient@example.com"],
    "subject": "Hello",
    "body": "This is a test email.",
    "isRead": false,
    "labels": ["Work"]
  }
]
```

### Delete Email
DELETE `/api/emails/{id}`
**Response:**
```json
{
  "succeeded": true,
  "message": "Email moved to trash."
}
```


<img width="1192" alt="Screenshot 2025-06-01 at 22 32 36" src="https://github.com/user-attachments/assets/a9e28117-e665-4a51-b919-bcbeba02c7b2" /> 


<img width="1224" alt="Screenshot 2025-06-01 at 22 41 40" src="https://github.com/user-attachments/assets/e13eb693-2c89-4fef-938c-661899b379b0" />

## Important Notes
- JWT authentication is required for all endpoints (`[Authorize]` attribute).
- The database is managed via Entity Framework Core (`MailDbContext`).
- The API is organized according to Domain-Driven Design (DDD) and clean architecture.

