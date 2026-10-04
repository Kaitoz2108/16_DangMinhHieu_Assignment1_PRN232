# FUNewsManagementSystem - PRN232 Assignment 01

**Author:** Dang Minh Hieu  
**Target Platform:** .NET 8.0 (C#) & MS SQL Server  
**Architecture:** 3-Layer Architecture with Separate BackEnd and FrontEnd Solutions

---

## 1. Project Solutions Overview

### A. BackEnd Solution: `16_DangMinhHieu_Assignment01_BackEnd.sln`
- **`BusinessObjects`**: Entity Models (`Category`, `NewsArticle`, `SystemAccount`, `Tag`) and `FUNewsManagementDbContext` scaffolded from MS SQL Server with entity mapping intact (preserving `CategoryDesciption`).
- **`DataAccessObjects`**: Thread-safe Singleton Pattern with lock (`CategoryDAO.Instance`, `NewsArticleDAO.Instance`, `SystemAccountDAO.Instance`, `TagDAO.Instance`).
- **`Repositories`**: Repository Interfaces and Implementation classes encapsulating DAO methods (`ICategoryRepository`, `INewsArticleRepository`, `ISystemAccountRepository`, `ITagRepository`).
- **`FUNewsManagementAPI`**: ASP.NET Core Web API with OData routing (`odata` prefix), `$filter`, `$select`, `$orderby`, `$expand`, `$count`, `$top`, JWT Bearer Authentication, and Swagger.

### B. FrontEnd Solution: `16_DangMinhHieu_Assignment01_FrontEnd.sln`
- **`FUNewsManagementClient`**: ASP.NET Core MVC interacting with the Web API via `HttpClient`, Bootstrap 5 modals, SweetAlert2 confirmation dialogs, and real-time form validation.

---

## 2. Default Credentials & Role Matrix

| Role | Email | Password | Auth Source | Capabilities |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` | `appsettings.json` | - Account Management (CRUD + Search using Modals)<br>- Delete Restriction: Cannot delete account with created articles (400 Bad Request)<br>- Report & Statistics between StartDate and EndDate ordered by CreatedDate desc |
| **Staff** (Role 1) | `IsabellaDavid@FUNewsManagement.org` | `@1` | SQL Database | - Category Management (CRUD + Modal + Delete Restriction)<br>- News Article Management & Tags (CRUD + Modal)<br>- Profile Management<br>- My History: View articles created by own AccountID |
| **Lecturer** (Role 2) | `EmmaWilliam@FUNewsManagement.org` | `@1` | SQL Database | - Read active articles and manage personal profile |
| **Guest** | *No login needed* | - | - | - Browse active published news articles, search, view article details |

---

## 3. How to Run

### Option 1: Terminal / Command Line
1. **Start BackEnd Web API:**
   ```bash
   dotnet run --project FUNewsManagementAPI/FUNewsManagementAPI.csproj --urls "http://localhost:5173"
   ```
2. **Start FrontEnd Client:**
   ```bash
   dotnet run --project FUNewsManagementClient/FUNewsManagementClient.csproj --urls "http://localhost:5076"
   ```
3. Open browser at: `http://localhost:5076`

### Option 2: Visual Studio
1. Open `16_DangMinhHieu_Assignment01_BackEnd.sln` -> Set `FUNewsManagementAPI` as Startup Project -> Run (F5).
2. Open `16_DangMinhHieu_Assignment01_FrontEnd.sln` -> Set `FUNewsManagementClient` as Startup Project -> Run (F5).
