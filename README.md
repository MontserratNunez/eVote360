# eVote360 Pro

**eVote360 Pro** is an electronic voting web application designed to manage the entire lifecycle of an electoral process. It enables the registration of eligible citizens (voters), and provides management modules for configuring elections, political parties, elective positions, candidates, and political alliances.

## 2. Technologies Used
- **Framework:** ASP.NET Core MVC (.NET 9)
- **Architecture:** Onion Architecture
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server
- **Identity & Security:** Tesseract OCR (for ID card validation), Email Verification Codes, Role-Based Access Control (Admin, Political Leader)
- **Frontend:** HTML5, CSS3, Bootstrap

## 3. Installation Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/MontserratNunez/eVote360.git
   cd eVote360
   ```

2. **Configure Database Connection:**
   Navigate to the web project directory (`eVote360/`) and locate the `appsettings.json` file. Update the `DefaultConnection` string with your SQL Server credentials.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EVote360;Trusted_Connection=true;TrustServerCertificate=true"
   }
   ```

3. **Configure Email Service:**
   In the same `appsettings.json` file, update the `EmailConfiguration` block with your SMTP credentials to enable verification code emails and voting summaries.
   ```json
   "EmailConfiguration": {
     "Host": "smtp.gmail.com",
     "Port": 587,
     "Email": "your_email",
     "Password": "your_app_password"
   }
   ```
   *(Note: If using Gmail, make sure to use an App Password instead of your regular password).*

4. **Apply Database Migrations:**
   Open the Package Manager Console or use the .NET CLI to apply the EF Core migrations and create the database schema.
   ```bash
   dotnet ef database update --project Persistence --startup-project eVote360
   ```

5. **Run the Application:**
   ```bash
   dotnet run --project eVote360
   ```

## 4. Folder Structure
The solution is structured following the principles of the Onion Architecture to ensure separation of concerns:

- **`Domain/`**: The core layer containing enterprise logic, Entities, and Enums. It has no dependencies on other projects.
- **`Application/`**: Contains business logic, Services, Interfaces, DTOs, and ViewModels. It depends only on the Domain layer.
- **`Persistence/`**: The Infrastructure layer for database access. Contains the EF Core `DbContext`, Code First Migrations, Entity Configurations, and generic/specific Repositories.
- **`eVote.Infraestructure.Shared/`**: Shared infrastructure services such as the Email delivery service and Tesseract OCR integration.
- **`eVote360/`**: The Web/Presentation layer. Contains MVC Controllers, Views, Middlewares, and dependency injection setups.

## 5. Key Functionalities

### Admin
The Administrator has full control over the electoral setup and system maintenance. Key features include:
- **Dashboard & Electoral Summary:** View historical election results filtered by year, including participating parties, candidates, and voter turnout.
- **Election Management:** Create, activate, and finalize elections. Modifying critical data is strictly blocked when an election is actively running.
- **Elective Positions:** Manage available public offices (e.g., Mayor, Senator).
- **Citizens Registry:** Maintain the database of eligible voters (Padrón Electoral).
- **Political Parties:** Register and manage political organizations, their acronyms, and logos.
- **User Management:** Create system users and assign roles (`Admin` or `Political Leader`).

### Political Leader
The Political Leader acts on behalf of a specific political party and manages their electoral strategy. Key features include:
- **Candidate Management:** Register citizens as candidates for their party. Candidate affiliation is handled automatically based on the leader's session context to prevent unauthorized assignments.
- **Political Alliances:** Send, accept, or reject alliance requests with other political parties for upcoming elections.
- **Assign Candidates to Positions:** Place active candidates into elective positions for a pending election. Leaders can assign their own candidates or allied candidates (provided an accepted alliance exists and strict assignment rules are met).
