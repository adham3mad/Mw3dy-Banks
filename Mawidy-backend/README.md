# Mw3dy - Premium Booking System

Mw3dy is a premium, full-stack, production-ready appointment scheduling web application built on ASP.NET Core MVC. It features rich interactive animations, a custom Egyptian weekend calendar, and a 100% database-driven bilingual localization architecture.

---

## Key Features

- **Full-Stack MVC Architecture**: Implemented with ASP.NET Core MVC and Entity Framework Core, moving away from decoupled static SPAs to a unified, secure, server-rendered structure.
- **100% Database-Driven Bilingual Localization**:
  - Dynamic content like **Services** and **Branches** is queried directly from SQL Server.
  - Supports model-level translation fields (e.g., `NameEn` / `NameAr`, `AddressEn` / `AddressAr`) that adapt to the active user culture (`ar`/`en`) on the fly.
  - Static labels are managed centrally in `translations.json` to keep the application modular and easily customizable.
- **Tailored Weekend Calendar**:
  - Calendar automatically recognizes the Middle Eastern/Egyptian weekend (**Friday and Saturday** as holidays).
  - Sunday is treated as a standard working day, allowing seamless local scheduling.
- **Premium UI & Interactive UX**:
  - Built with modern Tailwind CSS glassmorphism, gradient accents, and responsive card-based steps.
  - Animated transitions (`animate-fade-in`), card lifting, and button micro-interactions.
  - Smooth loading overlay with backdrop blur upon booking confirmation.
  - Sonner-style toast notifications and Canvas-Confetti celebrations for successful bookings.

---

## Technology Stack

- **Backend**: `.NET 9`, `ASP.NET Core MVC`, `Entity Framework Core`
- **Database**: `Microsoft SQL Server`
- **Frontend**: `Tailwind CSS`, `Vanilla JavaScript`, `Lucide Icons`, `Canvas-Confetti`

---

## Project Structure

```bash
├── Controllers/
│   ├── BookingController.cs     # Booking wizard API and pages
│   ├── DashboardController.cs   # User dashboard and cancellation handler
│   └── HomeController.cs        # Main landing and language controller
├── Data/
│   └── AppDbContext.cs          # Database schema and seed data (bilingual)
├── Models/
│   ├── Appointment.cs           # Booking record schema
│   ├── Branch.cs                # Bilingual branch details
│   ├── Service.cs               # Dynamic service card metadata
│   └── User.cs                  # Default client model
├── Services/
│   └── LocalizationService.cs   # JSON-based localization engine
├── Views/
│   ├── Booking/
│   │   └── Index.cshtml         # Responsive booking wizard steps
│   ├── Dashboard/
│   │   └── Index.cshtml         # List of upcoming/past bookings with cancellation
│   ├── Home/
│   │   └── Index.cshtml         # Premium landing page
│   ├── Shared/
│   │   └── _Layout.cshtml       # Standard bilingual shell layout
│   └── _ViewImports.cshtml      # Global razor directives
├── wwwroot/                     # Compiled styles, assets, and scripts
└── translations.json            # Static UI translations (Arabic & English)
```

---

## Getting Started

### Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB or Docker SQL Instance)

### Configuration
Update the database connection string in your `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Mw3dyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Installation & Run
1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Build the application:
   ```bash
   dotnet build
   ```
3. Run the project:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to: `http://localhost:5281`

---

## Industry Flexibility (Hospitals & Telecom)

The core architecture of **Mw3dy** is completely domain-agnostic:
- **Services & Branches** are seeded directly into database tables rather than hardcoded in the codebase.
- **Icons** are loaded dynamically from Lucide names saved in the database (e.g. `heart`, `phone`, `activity`).
- To adapt this system for **hospitals** or **telecom retail stores**, simply modify the database seed data in `AppDbContext.cs` and update the brand names inside `translations.json`.

---

## License
This project is licensed under the MIT License.
