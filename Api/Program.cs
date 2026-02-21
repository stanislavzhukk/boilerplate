using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Data.Context;
using Data.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Add Identity services
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Apply pending migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        //var userManager = services.GetRequiredService<UserManager<User>>();

        await dbContext.Database.MigrateAsync();

        //var seeder = new SeedData(roleManager, userManager, dbContext);
        //await seeder.InitializeAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"error during migration: {ex.Message} | {ex.InnerException}");
    }
}

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
