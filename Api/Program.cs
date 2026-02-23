using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Data.Context;
using Data.Models;
using Services.Interfaces;
using Services.Services;
using Data.Interfaces;
using Data.Repositories;
using Data.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Add Identity services
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


//jwt 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured"))
    );
});

builder.Services.AddAuthorization();

//Add repositories
builder.Services.AddScoped<IModel1Repository, Model1Repository>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

//Add services
builder.Services.AddScoped<IModel1Service, Model1Service>();
builder.Services.AddScoped<IAuthService, AuthService>();



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
        var userManager = services.GetRequiredService<UserManager<User>>();
        if(dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }

        await RolesSeeder.SeedRoles(services);

        if (userManager.Users.Count() < 2)
        {
            await UsersSeeder.SeedUsers(services);
        }
        if (!dbContext.Model1s.Any())
        {
            await ModelsSeeder.SeedModels(services);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"error during migration/seeds: {ex.Message} | {ex.InnerException}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
