using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Repository.Repositories;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Mapper;
using SmartFarmManager.Service.Services;
using SmartFarmManager.Service.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Quartz;
using Quartz.Spi;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using SmartFarmManager.Service.Configuration;
using SmartFarmManager.Service.MQTT;
using SmartFarmManager.Service.Helpers;
using Quartz.Impl;
using SmartFarmManager.API.HostedServices;
using SmartFarmManager.API.BackgroundJobs.QuartzConfigurations;


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
            //Get config mail form environment
            services.Configure<MailSettings>(options =>
            {
                options.Server = Environment.GetEnvironmentVariable("MailSettings__Server");
                options.Port = int.Parse(Environment.GetEnvironmentVariable("MailSettings__Port") ?? "0");
                options.SenderName = Environment.GetEnvironmentVariable("MailSettings__SenderName");
                options.SenderEmail = Environment.GetEnvironmentVariable("MailSettings__SenderEmail");
                options.UserName = Environment.GetEnvironmentVariable("MailSettings__UserName");
                options.Password = Environment.GetEnvironmentVariable("MailSettings__Password");
            });
            ConfigureFirebaseAdminSDK(configuration);
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
                    // Để SignalR có thể lấy token từ query string
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            // Nếu đây là yêu cầu cho SignalR Hub
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                            {
                                context.Token = accessToken;
                            }

                            return System.Threading.Tasks.Task.CompletedTask;
                        }
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
            services.AddInfrastructureServices(configuration);
            services.AddSignalR();


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
        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {


            services.AddRepositories();
            services.AddApplicationServices();
            services.AddConfigurations();
            services.AddQuartzServices();
            services.AddAppHostedService();
            services.AddMqttClientService(configuration);

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Đăng ký các repository
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskTypeRepository, TaskTypeRepository>();
            services.AddScoped<IStatusLogRepository, StatusLogRepository>();
            services.AddScoped<ICageRepository, CageRepository>();
            services.AddScoped<ICageStaffRepository, CageStaffRepository>();
            services.AddScoped<IMedicalSymptomRepository, MedicalSymptomRepository>();
            services.AddScoped<IMedicationRepository, MedicationRepository>();
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            services.AddScoped<IFarmingBatchRepository, FarmingBatchRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IFarmRepository, FarmRepository>();
            services.AddScoped<IFarmAdminRepository, FarmAdminRepository>();
            services.AddScoped<IAnimalTemplateRepository, AnimalTemplateRepository>();
            services.AddScoped<IGrowthStageTemplateRepository, GrowthStageTemplateRepository>();
            services.AddScoped<ITaskDailyTemplateRepository, TaskDailyTemplateRepository>();
            services.AddScoped<IFoodTemplateRepository, FoodTemplateRepository>();
            services.AddScoped<IVaccineTemplateRepository, VaccineTemplateRepository>();
            services.AddScoped<IGrowthStageRepository, GrowthStageRepository>();
            services.AddScoped<IVaccineRepository, VaccineRepository>();
            services.AddScoped<ITaskDailyRepository, TaskDailyRepository>();
            services.AddScoped<IVaccineScheduleRepository, VaccineScheduleRepository>();
            services.AddScoped<IHealthLogRepository, HealthLogRepository>();
            services.AddScoped<IPictureRepository, PictureRepostory>();
            services.AddScoped<IPrescriptionMedicationRepository, PrescriptionMedicationRepository>();
            services.AddScoped<IDailyFoodUsageLogRepository, DailyFoodUsageLogRepository>();
            services.AddScoped<IVaccineScheduleLogRepository, VaccineScheduleLogRepository>();
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            services.AddScoped<ISaleTypeRepository, SaleTypeRepository>();
            services.AddScoped<IMedicalSymptomDetailRepository, MedicalSymptomDetailRepository>();
            services.AddScoped<ISymptomRepository, SymptomRepository>();
            services.AddScoped<IDiseaseRepositoy, DiseaseRepository>();
            services.AddScoped<IStandardPrescriptionRepository, StandardPrescriptionRepository>();
            services.AddScoped<IFoodStackRepository, FoodStackRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationTypeRepository, NotifitcationTypeRepository>();
            services.AddScoped<IAnimalSalesRepository, AnimalSalesRepository>();
            services.AddScoped<ICostingReportsRepository, CostingReportsRepository>();
            services.AddScoped<IElectricityLogsRepository, ElectricityLogsRepository>();
            services.AddScoped<IMasterDataRepository, MasterDataRepository>();
            services.AddScoped<IWaterLogsRepository, WaterLogsRepository>();
            return services;
        }

        private static void AddAppHostedService(this IServiceCollection services)
        {
            services.AddHostedService<AppHostedService>();
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
            services.AddScoped<IRoleService, RoleService>(); 
            services.AddScoped<IFarmService, FarmService>();
            services.AddScoped<INotificationService,NotificationUserService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<IAnimalTemplateService, AnimalTemplateService>();
            services.AddScoped<IGrowthStageTemplateService,GrowthStageTemplateService>();
            services.AddScoped<ITaskDailyTemplateService, TaskDailyTemplateService>();
            services.AddScoped<IFoodTemplateService, FoodTemplateService>();
            services.AddScoped<IFarmingBatchService, FarmingBatchService>();
            services.AddScoped<IHealthLogService, HealthLogService>();
            services.AddScoped<IDailyFoodUsageLogService, DailyFoodUsageLogService>();
            services.AddScoped<IVaccineScheduleLogService, VaccineScheduleLogService>();
            services.AddScoped<IVaccineTemplateService, VaccineTemplateService>();
            services.AddScoped<IGrowthStageService,GrowthStageService>();
            services.AddScoped<IVaccineService, VaccineService>();
            services.AddScoped<ISymptomService, SymptomService>();
            services.AddScoped<IDiseaseService, DiseaseService>();
            services.AddScoped<IStandardPrescriptionService, StandardPrescriptionService>();
            services.AddScoped<ISaleTypeService, SaleTypeService>();
            services.AddScoped<EmailService>();
            services.AddScoped<ICostingService, CostingService>();


            return services;
        }

        private static IServiceCollection AddConfigurations(this IServiceCollection services)
        {
            // Đăng ký các configuration (ví dụ: JWT settings, database settings)
            services.AddScoped<JwtSettings>();
            services.AddSingleton<JwtSecurityTokenHandler>();
            services.AddSingleton<SystemConfigurationService>();
            return services;
        }
        private static void AddMqttClientService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MqttClientSetting>(configuration.GetSection(MqttClientSetting.Section));

         
            services.AddSingleton<IMqttService, MqttService>();

            
        }


        public static IServiceCollection AddQuartzServices(this IServiceCollection services)
        {
            // Đăng ký Quartz Scheduler
            services.AddQuartz(config =>
            {
                config.UseMicrosoftDependencyInjectionJobFactory();
            });

            // Đăng ký Job Factory
            services.AddSingleton<IJobFactory, ScopedJobFactory>();

            // Đăng ký Scheduler Factory
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            
            // Đăng ký Scheduler vào DI Container
            services.AddSingleton(provider =>
            {
                var scheduler = provider.GetRequiredService<ISchedulerFactory>().GetScheduler().Result;
                scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
                return scheduler;
            });
            services.AddSingleton<IQuartzService, QuartzService>();
            services.AddTransient<SmartFarmManager.Service.Jobs.HelloWorldJob>();
            services.AddTransient<SmartFarmManager.Service.Jobs.GenerateTasksForTomorrowJob>();
            services.AddTransient<SmartFarmManager.Service.Jobs.UpdateTaskStatusesJob>();
            services.AddTransient<SmartFarmManager.Service.Jobs.UpdateEveningTaskStatusesJob>();
            services.AddTransient<SmartFarmManager.Service.Jobs.MedicalSymptomReminderJob>();
            return services;
        }

        private static void ConfigureFirebaseAdminSDK(IConfiguration configuration)
        {
            var firebaseAdminSDK = new Dictionary<string, string>
    {
        { "type", configuration["CLOUDMESSAGE_TYPE"] },
        { "project_id", configuration["CLOUDMESSAGE_PROJECT_ID"] },
        { "private_key_id", configuration["CLOUDMESSAGE_PRIVATE_KEY_ID"] },
        { "private_key", configuration["CLOUDMESSAGE_PRIVATE_KEY"]?.Replace("\\n", "\n") },
        { "client_email", configuration["CLOUDMESSAGE_CLIENT_EMAIL"] },
        { "client_id", configuration["CLOUDMESSAGE_CLIENT_ID"] },
        { "auth_uri", configuration["CLOUDMESSAGE_AUTH_URI"] },
        { "token_uri", configuration["CLOUDMESSAGE_TOKEN_URI"] },
        { "auth_provider_x509_cert_url", configuration["CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL"] },
        { "client_x509_cert_url", configuration["CLOUDMESSAGE_CLIENT_X509_CERT_URL"] },
        { "universe_domain", configuration["CLOUDMESSAGE_UNIVERSE_DOMAIN"] }
    };

            var firebaseAdminSDKJson = JsonConvert.SerializeObject(firebaseAdminSDK);
            var googleCredential = GoogleCredential.FromJson(firebaseAdminSDKJson);

            FirebaseApp.Create(new AppOptions
            {
                Credential = googleCredential,
                ProjectId = configuration["CLOUDMESSAGE_PROJECT_ID"]
            });
        }
    }
}
