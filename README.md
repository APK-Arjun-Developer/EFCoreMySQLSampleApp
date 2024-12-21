# Architecting .NET Core Applications: Integrating Entity Framework with MySQL Database

## Synopsis
This guide walks you through setting up a .NET Core application integrated with Entity Framework (EF) and MySQL. We'll create a multi-project architecture with CRUD operations for Employee module.

---

## Prerequisites
- **Visual Studio 2022** or later.
- **.NET Core 8.0 SDK**.
- **MySQL Server**.

## Project Structure
We will organize our application into the following projects:
- **Sample.API**: Hosts the Web API controllers.
- **Sample.Application**: Contains business logic.
- **Sample.Context**: Manages the database context and configurations.
- **Sample.Model**: Houses models and DTOs.

---

## Step 1: Create a New Solution
1. Open Visual Studio and create a new **New Solution** named `SampleSolution`.
2. Add the following projects:
   - `Sample.API`: ASP.NET Core Web API project.
   - `Sample.Application`: Class Library.
   - `Sample.Context`: Class Library.
   - `Sample.Model`: Class Library.
3. Add Project reference:
   - `Sample.API`: Application, Context, Model
   - `Sample.Application`: Context, Model
   - `Sample.Context`: Model

---

## Step 2: Packages to Install
- `Swashbuckle.AspNetCore` - API
- `Pomelo.EntityFrameworkCore.MySql` - API, Context
- `Microsoft.EntityFrameworkCore` - Context, Application
- `Microsoft.EntityFrameworkCore.Design` - Context
- `Expressmapper` - API
## Step 3: Set Up Models
### Sample.Model

### Employee.cs
```csharp
namespace Sample.Model
{
    public class Employee
    {
        public string Name { get; set; } = string.Empty;
    }
}

```

### EmployeeEntity.cs
```csharp
namespace Sample.Model
{
    public class EmployeeEntity
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
```

## Step 4: Configure Database Context

### Sample.Context

### ApplicationDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Sample.Model;

namespace Sample.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<EmployeeEntity> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeEntity>().HasKey(e => e.EmployeeId);
        }
    }
}
```

## Step 5: Create Interface

### Sample.Context

### IEmployeeService.cs
```csharp
using Sample.Model;

namespace Sample.Context
{
    public interface IEmployeeService
    {
        Task<List<EmployeeEntity>> GetEmployees();
        Task<EmployeeEntity?> GetEmployee(int id);
        Task<bool> CreateEmployee(EmployeeEntity employee);
        Task<bool> UpdateEmployee(EmployeeEntity employee);
        Task<bool> DeleteEmployee(int id);
    }
}
```

## Step 6: Create Business Logic

### Sample.Application

### EmployeeService.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Sample.Model;

namespace Sample.Context
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeEntity>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<EmployeeEntity?> GetEmployee(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<bool> CreateEmployee(EmployeeEntity employee)
        {
            try
            {
                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmployee(EmployeeEntity employee)
        {
            try
            {
                var existingEmployee = await _context.Employees.FindAsync(employee.EmployeeId);
                if (existingEmployee == null) return false;

                existingEmployee.Name = employee.Name;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null) return false;

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

## Step 7: Create API Endpoints

### Sample.API

### EmployeeController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Sample.Context;
using Sample.Model;

namespace Sample.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetEmployees();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployee(id);
            if (employee == null) return NotFound($"Employee with ID {id} not found.");
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employeeModel)
        {
            if (employeeModel == null) return BadRequest("Employee data is required.");

            var employee = ExpressMapper.Mapper.Map<Employee, EmployeeEntity>(employeeModel);
            var success = await _employeeService.CreateEmployee(employee);
            if (!success) return StatusCode(500, "Error creating employee.");

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employeeModel)
        {
            if (id <= 0) return BadRequest("Invalid employee ID.");

            if (employeeModel == null) return BadRequest("Invalid employee data.");

            var employee = ExpressMapper.Mapper.Map<Employee, EmployeeEntity>(employeeModel);
            employee.EmployeeId = id;
            var success = await _employeeService.UpdateEmployee(employee);
            if (!success) return NotFound($"Employee with ID {id} not found.");

            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var success = await _employeeService.DeleteEmployee(id);
            if (!success) return NotFound($"Employee with ID {id} not found.");

            return NoContent();
        }
    }
}
```

### Program.cs
```csharp

using Microsoft.EntityFrameworkCore;
using Sample.Context;

namespace Sample.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql("Server=localhost;Database=SampleDB;User=root;Password=;", new MySqlServerVersion(new Version(8, 0, 31))));

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            var app = builder.Build();
                
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
```

## Step 8: Migrations and Database Update

```bash
dotnet ef migrations add InitialMigration --project Sample.Context --startup-project Sample.API
dotnet ef database update --project Sample.Context --startup-project Sample.API
```

## Step 9: Folder Structure

```plaintext
SampleSolution/
├── Sample.API/
│   ├── Controllers/
│   │   └── EmployeeController.cs
│   ├── Program.cs
│   └── appsettings.json
├── Sample.Application/
│   └── EmployeeService.cs
├── Sample.Context/
│   └── ApplicationDbContext.cs
│   └── IEmployeeService.cs
├── Sample.Model/
│   ├── EmployeeEntity.cs
│   └── Employee.cs
```

## Conclusion
This article demonstrated how to structure a .NET Core application with EF Core and MySQL integration using a clean architecture approach. You can expand this architecture for other modules or databases with minimal changes. Reference for [Git Repository](https://github.com/APK-Arjun-Developer/EFCoreMySQLSampleApp)