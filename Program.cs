using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TodoDb"));

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Seed some data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Todos.AddRange(
        new Todo { Title = "Learn ASP.NET", IsComplete = false },
        new Todo { Title = "Deploy to Render", IsComplete = false }
    );
    db.SaveChanges();
}

// Endpoints
app.MapGet("/", () => "Hello! Visit /swagger for API documentation");

app.MapGet("/api/todos", async (AppDbContext db) =>
    await db.Todos.ToListAsync());

app.MapGet("/api/todos/{id}", async (int id, AppDbContext db) =>
    await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());

app.MapPost("/api/todos", async (Todo todo, AppDbContext db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{todo.Id}", todo);
});

app.MapPut("/api/todos/{id}", async (int id, Todo inputTodo, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    
    todo.Title = inputTodo.Title;
    todo.IsComplete = inputTodo.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/todos/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

// Models
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Todo> Todos => Set<Todo>();
}
