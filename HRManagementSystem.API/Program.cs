using HRManagementSystem.Application.Services;
using HRManagementSystem.Infrastructure.Persistence;
using HRManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using System.Text;
using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Employees.Validators;

// Hangfire paketleri
using Hangfire;
using Hangfire.MemoryStorage; // Demo/test için, prod'da SQL tavsiye edilir

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// 1?? Servisler
// ----------------------------
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LeaveService>();

// Hangfire job servisini ekle (aþaðýda kodunu bulacaksýn)
builder.Services.AddScoped<LeaveJobService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddDbContext<HRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services
    .AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateEmployeeDtoValidator>());

// ----------------------------
// Hangfire konfigürasyonu
// ----------------------------
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

// ----------------------------
// 2?? Swagger + JWT
// ----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HRManagementSystem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT tokenini 'Bearer {token}' formatýnda giriniz."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ----------------------------
// 3?? JWT Authentication
// ----------------------------
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// ----------------------------
// 4?? CORS
// ----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ----------------------------
// 5?? App build
// ----------------------------
var app = builder.Build();

// ----------------------------
// 6?? Middleware pipeline
// ----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseStaticFiles(); // wwwroot'u dýþarý açar

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard (opsiyonel, istersen kaldýrabilirsin)
app.UseHangfireDashboard(); // /hangfire ile dashboard'a eriþebilirsin

app.MapControllers();

// ----------------------------
// 7?? Hangfire job'u baþlat
// ----------------------------
using (var scope = app.Services.CreateScope())
{
    var jobService = scope.ServiceProvider.GetRequiredService<LeaveJobService>();
    // Her gün saat 03:00'te çalýþacak þekilde ayarlandý
    RecurringJob.AddOrUpdate(
        "AutoUpdateEmployeeStatus",
        () => jobService.AutoUpdateEmployeeStatusAsync(),
            "0 3 * * *" //  (her gece 03:00)
    );
}

app.Run();