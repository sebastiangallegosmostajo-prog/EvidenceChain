using System.Security.Claims;
using System.Text;
using EvidenceChain.Api.Authentication;
using EvidenceChain.Api.ErrorHandling;
using EvidenceChain.Application.Authentication.Login;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Application.Common.Options;
using EvidenceChain.Application.CustodyTransfers.Accept;
using EvidenceChain.Application.CustodyTransfers.Reject;
using EvidenceChain.Application.CustodyTransfers.Request;
using EvidenceChain.Application.Evidences.Chain;
using EvidenceChain.Application.Evidences.Detail;
using EvidenceChain.Application.Evidences.List;
using EvidenceChain.Application.Evidences.VerifyChain;
using EvidenceChain.Infrastructure;
using EvidenceChain.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

const string FrontendCorsPolicy =
    "FrontendCors";

var builder =
    WebApplication.CreateBuilder(args);

// ==========================================
// Configuración de la base de datos
// ==========================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection connection string is missing.");

builder.Services.AddInfrastructure(
    connectionString);

// ==========================================
// Configuración JWT
// ==========================================

var jwtSection =
    builder.Configuration.GetSection(
        JwtOptions.SectionName);

var jwtOptions =
    jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "La configuración JWT no existe.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
    jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException(
        "El secreto Jwt:Key no existe o es demasiado corto.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException(
        "La configuración Jwt:Issuer es obligatoria.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException(
        "La configuración Jwt:Audience es obligatoria.");
}

builder.Services.Configure<JwtOptions>(
    jwtSection);

// ==========================================
// Configuración CORS
// ==========================================

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

if (builder.Environment.IsDevelopment() &&
    allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Debe configurarse al menos un origen CORS en desarrollo.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ==========================================
// Servicios de ASP.NET Core
// ==========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Ingrese únicamente el token JWT."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// ==========================================
// Servicios de aplicación
// ==========================================

builder.Services.Configure<CustodyTransferOptions>(
    builder.Configuration.GetSection(
        CustodyTransferOptions.SectionName));

builder.Services.AddSingleton(
    TimeProvider.System);

builder.Services.AddScoped<LoginHandler>();

builder.Services.AddScoped<
    RequestCustodyTransferHandler>();

builder.Services.AddScoped<
    AcceptCustodyTransferHandler>();

builder.Services.AddScoped<
    RejectCustodyTransferHandler>();

builder.Services.AddScoped<
    GetEvidenceListHandler>();

builder.Services.AddScoped<
    GetEvidenceDetailHandler>();

builder.Services.AddScoped<
    GetEvidenceChainHandler>();

builder.Services.AddScoped<
    VerifyEvidenceChainHandler>();

// ==========================================
// Servicios de seguridad
// ==========================================

builder.Services.AddSingleton<
    IJwtTokenGenerator,
    JwtTokenGenerator>();

builder.Services.AddSingleton<
    IPasswordService,
    AspNetPasswordService>();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.Key)),

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.FromSeconds(30),

                NameClaimType =
                    ClaimTypes.Name,

                RoleClaimType =
                    ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

// ==========================================
// Servicios de infraestructura y errores
// ==========================================

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddScoped<EvidenceDataSeeder>();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    ApiExceptionHandler>();

// ==========================================
// Construcción de la aplicación
// ==========================================

var app = builder.Build();

// ==========================================
// Carga de datos de desarrollo
// ==========================================

if (app.Environment.IsDevelopment())
{
    var seedPassword =
        builder.Configuration[
            "Seed:DefaultPassword"]
        ?? throw new InvalidOperationException(
            "El secreto Seed:DefaultPassword no está configurado.");

    await using var scope =
        app.Services.CreateAsyncScope();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<DatabaseSeeder>();

    await seeder.SeedUsersAsync(
        seedPassword);

    var evidenceSeeder =
        scope.ServiceProvider
            .GetRequiredService<EvidenceDataSeeder>();

    await evidenceSeeder.SeedAsync();
}

// ==========================================
// Pipeline HTTP
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(
    FrontendCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();