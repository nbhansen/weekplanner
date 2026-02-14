using GirafAPI.Authorization;
using GirafAPI.Clients;
using GirafAPI.Configuration;
using GirafAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace GirafAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var connectionString = configuration.GetConnectionString("DbConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DbConnection is required.");
            }

            services.AddDbContext<GirafDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings configuration is missing.");
            }

            // Allow env var override so all backends share the same secret
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? jwtSettings.SecretKey;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JwtSettings:SecretKey or JWT_SECRET env var is required.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !env.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });

            return services;
        }

        public static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthorizationHandler, JwtOrgRoleHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OrganizationMember", policy =>
                    policy.Requirements.Add(new OrgMemberRequirement()));
                options.AddPolicy("OrganizationAdmin", policy =>
                    policy.Requirements.Add(new OrgAdminRequirement()));
                options.AddPolicy("OrganizationOwner", policy =>
                    policy.Requirements.Add(new OrgOwnerRequirement()));
                options.AddPolicy("OwnData", policy =>
                    policy.Requirements.Add(new OwnDataRequirement()));
            });

            return services;
        }

        public static IServiceCollection ConfigureCoreClient(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("GirafCore:BaseUrl")
                ?? throw new InvalidOperationException("GirafCore:BaseUrl configuration is required.");

            services.AddHttpClient<ICoreClient, GirafCoreClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }

        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}
