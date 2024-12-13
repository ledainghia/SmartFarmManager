using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Repository.Repositories;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Mapper;
using SmartFarmManager.Service.Services;
using SmartFarmManager.Service.Settings;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SmartFarmManager.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                    .AddJsonOptions(x =>
                    {
                        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        x.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    }); // Ngăn không tuần tự hóa vòng lặp

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(config =>
            {
                config.EnableAnnotations();
            });

            var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }
            var jwtSettings = new JwtSettings
            {
                Key = secretKey
            };

            services.Configure<JwtSettings>(options => { options.Key = jwtSettings.Key; });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAutoMapper(typeof(ApplicationMapper));

            services.AddAuthorization();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                }).AddCookie();

            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "F-Driver API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
            });
            services.AddCors(option =>
               option.AddPolicy("CORS", builder =>
                   builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()));
            services.AddInfrastructureServices();



            services.ConfigureDbContext(configuration);


            return services;
        }
        private static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {

            //var connectionString = configuration.GetConnectionString("DbConnection");
            ////var connectionString = $"Data Source={dbServer};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate={dbTrustServerCertificate};MultipleActiveResultSets={dbMultipleActiveResultSets}";

            //services.AddDbContext<SmartFarmContext>(opt =>
            //{
            //    opt.UseSqlServer(connectionString);
            //});

            services.AddDbContext<SmartFarmContext>();

            
            return services;
        }
        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {

            services.AddRepositories();
            services.AddApplicationServices();
            services.AddConfigurations();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Đăng ký các repository
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskTypeRepository, TaskTypeRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IStatusLogRepository, StatusLogRepository>();
            services.AddScoped<ICageRepository, CageRepository>();
            services.AddScoped<ICageStaffRepository, CageStaffRepository>();
            services.AddScoped<IMedicalSymptomRepository, MedicalSymptomRepository>();
            services.AddScoped<IMedicationRepository, MedicationRepository>();
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            services.AddScoped<IFarmingBatchRepository, FarmingBatchRepository>();


            return services;
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký các service logic
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ICageService, CageService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IMedicationService, MedicationService>();
            services.AddScoped<IMedicalSymptomService, MedicalSymptomService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
            services.AddScoped<ITaskTypeService, TaskTypeService>();
            return services;
        }

        private static IServiceCollection AddConfigurations(this IServiceCollection services)
        {
            // Đăng ký các configuration (ví dụ: JWT settings, database settings)
            services.AddScoped<JwtSettings>();

            return services;
        }


    }
}
