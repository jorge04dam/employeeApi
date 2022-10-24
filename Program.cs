using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EmployeeDb>(opt => opt.UseInMemoryDatabase("EmployeesList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//cors
builder.Services.AddCors(option=>option.AddPolicy("AllowWebapp",
                        builder=>builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors("AllowWebapp");

app.MapGet("/", () => "Welcome to employees API");

app.MapGet("/employees", async (EmployeeDb db) =>
    await db.Employees.ToListAsync());

// app.MapGet("/employees/available", async (EmployeeDb db) =>
//     await db.Employees.Where(t => t.Stock).ToListAsync());

app.MapGet("/employee/{id}", async (int id, EmployeeDb db) =>
    await db.Employees.FindAsync(id)
        is Employee employee
            ? Results.Ok(employee)
            : Results.NotFound());

app.MapPost("/employee", async (Employee employee, EmployeeDb db) =>
{
    db.Employees.Add(employee);
    await db.SaveChangesAsync();

    return Results.Created($"/employee/{employee.id}", employee);
});

app.MapPut("/employee/{id}", async (int id, Employee inputEmployee, EmployeeDb db) =>
{
    var employee = await db.Employees.FindAsync(id);

    if (employee is null) return Results.NotFound();

    employee.employeenumber = inputEmployee.employeenumber;
    employee.fullname = inputEmployee.fullname;
    employee.dateofbirth = inputEmployee.dateofbirth;
    employee.numbercellphone = inputEmployee.numbercellphone;
    employee.status = inputEmployee.status;
   

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPut("/employee/patch/{id}", async (int id, Employee inputEmployee, EmployeeDb db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee is null) return Results.NotFound();
    employee.status = inputEmployee.status;
    await db.SaveChangesAsync();
    return Results.NoContent();
});



app.MapDelete("/employee/{id}", async (int id, EmployeeDb db) =>
{
       if (await db.Employees.FindAsync(id) is Employee employee)
    {
        db.Employees.Remove(employee);
        await db.SaveChangesAsync();
        return Results.Ok(employee);
    }
    return Results.NotFound();
});

app.Run();

class Employee
{
    public int id { get; set; }
    public int employeenumber { get; set; }

    public string? fullname { get; set; }

    public string? dateofbirth { get; set; }
    
    public long numbercellphone { get; set; }

    public String? status { get; set; }
}

class EmployeeDb : DbContext
{
    public EmployeeDb(DbContextOptions<EmployeeDb> options)
        : base(options) { } 

    public DbSet<Employee> Employees => Set<Employee>();
}