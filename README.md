# Alpha Project Management System

Alpha is a comprehensive project management system designed to facilitate team collaboration, task tracking, and project oversight. The application provides a centralized platform for managing projects, tasks, time entries, and team members.

## Core Features

### Project Management
- Create and manage projects with detailed information
- Track project status, timelines, and budgets
- View project details with comprehensive dashboards
- Assign team members to specific projects

### Task Management
- Create, edit, and delete tasks within projects
- Assign tasks to team members
- Track task completion status and priorities
- Generate task summaries and progress reports

### Team Collaboration
- Request to join specific projects
- Approve or reject project join requests
- Add or remove team members from projects
- View all team members assigned to a project

### Time Tracking
- Log time entries for project tasks
- Track time spent on specific projects
- Generate time reports for billing and productivity analysis

### Notifications
- Receive notifications for project updates
- Get alerts for new task assignments
- Stay informed about project join requests

## Technical Implementation

The system is built using:
- ASP.NET Core MVC for the web interface
- Entity Framework Core for data access
- SignalR for real-time notifications and comments
- Bootstrap for responsive design
- JavaScript for enhanced user interactions

## Project Structure

- **Business Layer**: Contains services, interfaces, and DTOs for business logic
- **Data Layer**: Handles data access and repository implementations
- **Domain Layer**: Defines the core domain models and entities
- **MVC Layer**: Implements the user interface and controllers

## User Roles

- **Admin**: Full system access with user management capabilities
- **Project Manager**: Create and manage projects, approve team member requests
- **Team Member**: Work on assigned tasks and projects, request to join projects

## AI Disclaimer

Parts of this codebase were developed with assistance from AI tools:

- **GitHub Copilot**: Used to suggest code implementations and refactoring options
- **Claude**: Used to generate documentation, fix bugs, and improve code quality

While AI tools were utilized during development, all code has been reviewed and validated by human developers to ensure quality, security, and proper functionality. The final implementation decisions and architectural choices were made by the development team.

## License

[Include license information here]
