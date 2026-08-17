# KPI Productivity Tracking System

A web-based KPI and productivity tracking application developed with ASP.NET Core MVC.

The system allows employees and managers to define performance targets, record completed activities, monitor KPI progress, and manage notifications through role-based workflows.

## Features

- User authentication and session-based authorization
- Employee and manager roles
- KPI target creation and management
- Assignment of targets to employees
- Completed activity tracking
- Automatic KPI progress calculation
- Employee productivity monitoring
- Manager dashboard
- Notification system
- Performance reporting

## Technologies

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- HTML
- CSS
- JavaScript
- Git & GitHub

## Architecture

The application follows the Model-View-Controller (MVC) architecture.

- **Models** represent application and database entities.
- **Views** provide the user interface.
- **Controllers** manage application logic and user requests.
- **Services** contain reusable business logic.
- **Data** manages database access through Entity Framework Core.

## Database

The application uses SQLite with Entity Framework Core.

The main entities include:

- **Kisiler** — stores employees, managers, roles, and manager relationships.
- **Yapilacaklar** — stores predefined activity types.
- **KisiYapacaklari** — stores KPI targets assigned to users.
- **Tamamlanan** — stores completed activities associated with KPI targets.
- **Bildirimler** — stores user notifications and their read/unread status.

The database structure separates the main components of the KPI system while maintaining relationships between employees, targets, completed activities, and notifications.

## Project Purpose

This project was developed during my Software Engineering internship to apply software engineering concepts to a real-world employee productivity scenario.

The main objective was to design a system that enables organizations to define measurable employee targets, track completed work, and evaluate KPI progress through a centralized web application.

The project provided practical experience with:

- MVC application architecture
- Relational database design
- Entity Framework Core
- Role-based application workflows
- Backend development with C#
- CRUD operations
- Session management
- Git version control

## Future Improvements

Possible future improvements include:

- Advanced analytics and visualization
- AI-assisted performance insights
- Automated KPI recommendations
- Exportable performance reports
- REST API support
- Improved automated testing

## Author

**Ceyda Ece Han**

Software Engineering  
Izmir University of Economics