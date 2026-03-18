University Project – Developed as a web-based question-and-answer platform inspired by Stack Overflow.

# Features

  User registration, authentication, and profile management
  Post, edit, and delete questions
  Voting system for questions and answers
  Mark best answers and track responses
  Asynchronous email notifications for question updates and best answer selection
  Role-based permissions (User, Admin)

# Architecture & Technologies

  Frontend / Backend: C#, ASP.NET Core
  Storage: Azure Blob Storage for user, question, answer, and notification data
  Cloud / Services: Azure, NotificationService, Background Services
  APIs: ASP.NET Core Web APIs for all backend operations

# How it Works

  Users can post questions, provide answers, and vote on others’ contributions
  Admins have role-based access for moderation
  Asynchronous notifications are sent using Azure queues whenever a question is updated or a best answer is selected
  All data is stored in Azure Blob Storage
  
# How to Run the Project

  Open the solution (.sln) file in Visual Studio
  Restore NuGet packages if prompted
  Ensure Azure services are configured correctly (Blob Storage connection string, queues, NotificationService)
  Press F5 or click Start to run the application
  The web app will open in your default browser
  
# Purpose

  The project was designed to demonstrate full-stack web development, cloud integration, asynchronous processing, and role-based access control within a university assignment context.

Natasa Jevremov
