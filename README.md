# NexHire — Online Job Recruitment System

A full-stack job recruitment platform. Employers post job openings, job seekers upload CVs and apply, and both sides manage contracts and payments directly within the platform.

## Features

**Core functionality:**
- Separate registration and profiles for Employers and Job Seekers
- Job posting (title, description, requirements, location, deadline)
- Job seeker profile with CV upload, skills, and work experience
- Application submission and tracking
- Employer dashboard: view, filter, and manage applications
- Job search with filters (category, location, job type, salary range)
- Application status tracking (Applied to Reviewed to Shortlisted/Rejected)
- Saved jobs list

**Additional features:**
- Real-time messaging and notifications (SignalR)
- Review and rating system
- Portfolio showcase
- Contracts with Stripe payments (including platform fee)
- CV analysis and job recommendations via Gemini AI
- Job alerts (background service)
- Admin panel
- Premium subscription
- Two-factor authentication via email (2FA)
- JWT + refresh token authentication

## Tech Stack

**Backend:** ASP.NET Core Web API (.NET 8), Entity Framework Core, SQL Server, SignalR, JWT Bearer Auth, FluentValidation, AutoMapper, Stripe API, Google Gemini AI

**Frontend:** HTML, CSS, JavaScript (jQuery)

**Architecture:** Clean Architecture, 4 projects (Domain, Application, Infrastructure, API)

## Project Structure

- OnlineJobRecruitmentSystem.API : Controllers, Hubs, Middlewares, wwwroot (frontend)
- OnlineJobRecruitmentSystem.Application : DTOs, Interfaces, Validations, AutoMapper Profiles
- OnlineJobRecruitmentSystem.Domain : Entities, Enums
- OnlineJobRecruitmentSystem.Infrastructure : Services, AppDbContext, Migrations
- OnlineJobRecruitmentSystem.Tests : Unit tests (xUnit + Moq)

## Setup

1. Clone the repo: `git clone https://github.com/khanimb/FinalApp.git`
2. Fill in your own values in `appsettings.json` (connection string, JWT key, Stripe key, Gemini key, Email settings).
3. Create the database: `dotnet ef database update --project OnlineJobRecruitmentSystem.Infrastructure --startup-project OnlineJobRecruitmentSystem.API`
4. Run the project: `dotnet run --project OnlineJobRecruitmentSystem.API`

## API Documentation

Available at `/scalar/v1` in the Development environment.

## Tests

`dotnet test`
