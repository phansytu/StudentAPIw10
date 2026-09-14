using Microsoft.OpenApi.Models;
using StudentAPI.Application;
using StudentAPI.Infrastructure;
using StudentAPI.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StudentAPI.Domain.Entities;
using StudentAPI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "StudentAPI",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập trực tiếp chuỗi Access Token của bạn vào bên dưới (không cần gõ thêm chữ 'Bearer')."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    }
);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });

builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API v1");
        c.RoutePrefix = string.Empty; // Mở Swagger ngay tại Root URL (http://localhost:<port>/)
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
//     var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//     var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

//     // Tự động Apply Migration nếu chưa có
//     if (context is AppDbContext dbContext)
//     {
//         await dbContext.Database.MigrateAsync();
//     }


//     var hasAdmin = await context.NguoiDungs.AnyAsync(x => x.Role == "Admin");
//     if (!hasAdmin)
//     {
//         var adminUser = new NguoiDung
//         {
//             Email = "admin@studentapi.com",
//             PasswordHash = passwordHasher.HashPassword("Admin@123456"),
//             HoTen = "System Administrator",
//             Role = "Admin",
//             IsActive = true,
//             CreatedAt = DateTime.UtcNow
//         };

//         context.NguoiDungs.Add(adminUser);
//         await unit.SaveChangesAsync();
//     }
// }
app.Run();