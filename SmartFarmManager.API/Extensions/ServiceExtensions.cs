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

            var connectionString = configuration.GetConnectionString("DefaultConnectionString");
            //var connectionString = $"Data Source={dbServer};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate={dbTrustServerCertificate};MultipleActiveResultSets={dbMultipleActiveResultSets}";

            services.AddDbContext<FarmsContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });

            
            return services;
        }
        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<IAlertTypeRepository, AlertTypeRepository>();
            services.AddScoped<IAlertUserRepository, AlertUserRepository>();
            services.AddScoped<ICameraSurveillanceRepository, CameraSurveillanceRepository>();
            services.AddScoped<IDeviceReadingRepository, DeviceReadingRepository>();
            services.AddScoped<IFarmRepository, FarmRepository>();
            services.AddScoped<IFarmStaffAssignmentRepository, FarmStaffAssignmentRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IIoTDeviceRepository, IoTDeviceRepository>();
            services.AddScoped<ILivestockRepository, LivestockRepository>();
            services.AddScoped<ILivestockExpenseRepository, LivestockExpenseRepository>();
            services.AddScoped<ILivestockSaleRepository, LivestockSaleRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRevenueAndProfitReportRepository, RevenueAndProfitReportRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

            // Add services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<IAlertTypeService, AlertTypeService>();
            services.AddScoped<IAlertUserService, AlertUserService>();
            services.AddScoped<ICameraSurveillanceService, CameraSurveillanceService>();
            services.AddScoped<IDeviceReadingService, DeviceReadingService>();
            services.AddScoped<IFarmService, FarmService>();
            services.AddScoped<IFarmStaffAssignmentService, FarmStaffAssignmentService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
            services.AddScoped<IIoTDeviceService, IoTDeviceService>();
            services.AddScoped<ILivestockService, LivestockService>();
            services.AddScoped<ILivestockExpenseService, LivestockExpenseService>();
            services.AddScoped<ILivestockSaleService, LivestockSaleService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRevenueAndProfitReportService, RevenueAndProfitReportService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ITaskHistoryService, TaskHistoryService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            //add unit ofwork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //add settings
            services.AddScoped<JwtSettings>();

            return services;
        }


    }
}
