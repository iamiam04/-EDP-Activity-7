# 📚 Library Management System
### Activity 5 Integration — Visual Studio Community 2022 / 2024

---

## Project Structure

```
LibraryManagementSystem/
├── LibraryManagementSystem.slnx          ← Solution file
└── LibraryManagementSystem/
    ├── LibraryManagementSystem.csproj    ← .NET 8 WinForms project
    ├── Program.cs
    ├── database_setup.sql                ← Run this first in MySQL
    ├── Forms/
    │   ├── FormBuilder.cs               ← Shared UI factory
    │   ├── LoginForm.cs                 ← Activity 5 login (MD5 auth)
    │   ├── MainForm.cs                  ← MDI shell + sidebar nav
    │   ├── BorrowBookForm.cs            ← Transaction 1: Borrow
    │   ├── ReturnBookForm.cs            ← Transaction 2: Return + Fine
    │   ├── ManageBooksForm.cs           ← Transaction 3: Inventory CRUD
    │   ├── ManageUsersForm.cs           ← Activity 5 User Management
    │   └── ReportForm.cs               ← Report viewer + Excel export
    └── Helpers/
        ├── DatabaseHelper.cs            ← MySql connection string
        ├── SessionHelper.cs             ← Logged-in user state
        └── ExcelHelper.cs              ← ClosedXML report generator
```

---

## Quick Setup

### 1 — Database
1. Open **MySQL Workbench** (or any MySQL client).
2. Run `database_setup.sql` — it creates `library_db`, all tables, triggers, and seed data.

### 2 — Connection String
Open `Helpers/DatabaseHelper.cs` and update:
```csharp
private const string Server   = "localhost";
private const string Database = "library_db";
private const string User     = "root";
private const string Password  = "";   // ← your MySQL root password
```

### 3 — Build & Run
1. Open `LibraryManagementSystem.slnx` in **Visual Studio Community**.
2. NuGet packages restore automatically (`ClosedXML` + `MySql.Data`).
3. Press **F5**.

### 4 — Default Login Credentials
| Username    | Password      | Role       |
|-------------|---------------|------------|
| `admin`     | `admin123`    | Admin      |
| `librarian` | `librarian123`| Librarian  |
| `juan`      | `juan123`     | Member     |

---

## Features

### Transactions
| # | Form | Description |
|---|------|-------------|
| 1 | **Borrow a Book** | Select member + book → inserts borrowing; DB trigger decrements `available_copies` |
| 2 | **Return a Book** | Select active loan → sets `return_date`; DB trigger increments copies & computes fine (₱5/day overdue) |
| 3 | **Manage Books** | Add / Edit / Delete books with copy-count management (inventory) |

### Reports (DataGridView + Excel Export)
| Report | Sheet 1 | Sheet 2 |
|--------|---------|---------|
| Borrowings | All loans with fine totals | Bar chart: Fine per borrower |
| Books Inventory | Available vs borrowed copies | Clustered bar: Available vs Borrowed |
| User Activity | Borrowings + fines per user | Clustered bar: Borrowings & Fines |

Every Excel file includes:
- **Header**: Company name, address, report title, generated date/user
- **Logo placeholder**: `[LIBRARY LOGO]` cell (replace with actual image in ExcelHelper)
- **Signature block**: Prepared-by (logged-in user) + Approved-by lines
- **Sheet 2**: Auto-generated chart of the data

### User Management (Activity 5)
- Full CRUD for users (Admin only can Add/Update)
- MD5-hashed passwords (matching Activity 5 schema)
- Role-based access: Admin, Librarian, Member

---

## Adding Your Logo
In `Helpers/ExcelHelper.cs`, replace the logo placeholder cell with:
```csharp
// After WriteHeader(), add:
using var img = System.Drawing.Image.FromFile("path/to/logo.png");
ws.AddPicture(img).MoveTo(ws.Cell(1, 7)).WithSize(120, 60);
```

---

## Dependencies
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) `0.102.2` — Excel generation
- [MySql.Data](https://www.nuget.org/packages/MySql.Data) `8.3.0` — MySQL connector
- .NET 8 Windows (WinForms)
