# FUNewsManagementSystem - PRN232 Assignment 01

**Author:** Dang Minh Hieu  
**Target Platform:** .NET 8.0 (C#) & MS SQL Server  
**Architecture:** 3-Layer Architecture with Master Solution containing BackEnd and FrontEnd Projects

---

## 1. Project Solutions Overview

The system is organized under a single master solution `16_DangMinhHieu_Assignment01.sln` containing **2 dedicated C# Projects**:

```text
16_DangMinhHieu_Assignment1_PRN232/
│
├── 16_DangMinhHieu_Assignment01.sln                         # Master Solution (chứa trực tiếp 2 Project BE & FE)
├── 16_DangMinhHieu_Assignment01.slnLaunch.user              # Cấu hình F5 chạy đồng thời cả BE và FE
├── README.md
├── .gitignore
│
├── 16_DangMinhHieu_Assignment01_BackEnd/                   # BackEnd Project (.NET 8 Web API + OData)
│   ├── 16_DangMinhHieu_Assignment01_BackEnd.csproj          # Project file
│   ├── Program.cs                                          # DI, OData EDM, DbContext configuration
│   ├── appsettings.json                                    # Database Connection String & Admin Account
│   ├── appsettings.Development.json
│   ├── ODataQueryOptionsFilter.cs                          # Swagger OData query options filter
│   ├── Properties/
│   │   └── launchSettings.json                             # Runs at http://localhost:5173
│   ├── Controllers/                                        # API Controllers (Auth, News, Categories, Accounts, Tags, Reports)
│   ├── Entities/                                           # Entity Models (Category, NewsArticle, SystemAccount, Tag, FUNewsManagementDbContext)
│   ├── DTOs/                                               # Data Transfer Objects
│   ├── DAOs/                                               # Thread-safe Singleton DAOs (CategoryDAO, NewsArticleDAO, ...)
│   └── Repositories/                                       # Repository pattern (ICategoryRepository, CategoryRepository, ...)
│
└── 16_DangMinhHieu_Assignment01_FrontEnd/                  # FrontEnd Project (.NET 8 ASP.NET Core MVC Client)
    ├── 16_DangMinhHieu_Assignment01_FrontEnd.csproj         # References BackEnd Project
    ├── Program.cs                                          # MVC Pipeline, Session, Cookie Auth
    ├── appsettings.json                                    # Web API BaseUrl: http://localhost:5173
    ├── appsettings.Development.json
    ├── Properties/
    │   └── launchSettings.json                             # Runs at http://localhost:5076
    ├── Controllers/                                        # MVC Controllers (Account, AdminAccounts, Categories, Home, NewsArticles, Reports)
    ├── Models/                                             # ViewModels (ErrorViewModel)
    ├── Services/                                           # ApiService & IApiService (HTTP calls to BackEnd)
    ├── Views/                                              # Razor Views with Modals & SweetAlert2 confirmation
    └── wwwroot/                                            # Static assets (Bootstrap 5, CSS, JS)
```

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

### Option 1: Visual Studio (Recommended)
1. Open `16_DangMinhHieu_Assignment01.sln` in Visual Studio 2022.
2. The multi-project launch profile (`16_DangMinhHieu_Assignment01.slnLaunch.user`) is pre-configured to start both **BackEnd** and **FrontEnd** automatically.
3. Simply press **F5** or click **Start**.

### Option 2: Terminal / Command Line
1. **Start BackEnd Web API:**
   ```bash
   cd 16_DangMinhHieu_Assignment01_BackEnd
   dotnet run --urls "http://localhost:5173"
   ```
   - API Base URL: `http://localhost:5173`
   - Swagger Documentation: `http://localhost:5173/swagger`
   - OData Metadata Endpoint: `http://localhost:5173/odata/$metadata`

2. **Start FrontEnd Client:**
   Open a second terminal window:
   ```bash
   cd 16_DangMinhHieu_Assignment01_FrontEnd
   dotnet run --urls "http://localhost:5076"
   ```
   - FrontEnd URL: `http://localhost:5076`
