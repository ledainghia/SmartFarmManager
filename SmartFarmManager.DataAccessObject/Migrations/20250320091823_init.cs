using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFarmManager.DataAccessObject.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AnimalTe__F87ADD27AE731EF5", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlBoardTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ControlB__8CDFB1CCB09566BA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Diseases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Area = table.Column<double>(type: "float", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MACAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Farms__ED7BBAB9F3B62FC0", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobTypes__E1F462AD8039AD4A", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UsageInstructions = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DoseWeight = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true),
                    DoseQuantity = table.Column<int>(type: "int", nullable: true),
                    PricePerDose = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Medicati__62EC1AFA81A8C124", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MqttConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    BrokerAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QoS = table.Column<int>(type: "int", nullable: false),
                    KeepAlive = table.Column<int>(type: "int", nullable: false),
                    CleanSession = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WillMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UseTls = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MqttConf__065618CFF49F21BB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    NotiTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__54F5A3018831B14F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerUnit = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pricings__EC306B12D14952C7", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE1A640F661E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StageTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Discription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ScheduleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeOn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    TimeOff = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schedule__9C8A5B49CADB8578", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: ""),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: ""),
                    Unit = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: ""),
                    DefaultPinCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SensorTy__B6E7763F2A9179FF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CostPerUser = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CostPerVet = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MonthlyBaseCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Subscrip__755C22B7124150BB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symptoms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SymptomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symptoms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TaskTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PriorityNum = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TaskType__66B23E330DE8C67D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vaccines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    AgeStart = table.Column<int>(type: "int", nullable: true),
                    AgeEnd = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vaccines__45DC6889A12FCD5C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhitelistDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrowthStageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WeightAnimal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AgeStart = table.Column<int>(type: "int", nullable: true),
                    AgeEnd = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SaleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GrowthSt__12B67065BFCFA63B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__GrowthSta__Templ__59063A47",
                        column: x => x.TemplateId,
                        principalTable: "AnimalTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VaccineTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaccineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApplicationAge = table.Column<int>(type: "int", nullable: true),
                    Session = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VaccineT__B9AB66085E77BB5A", x => x.Id);
                    table.ForeignKey(
                        name: "FK__VaccineTe__Templ__5CD6CB2B",
                        column: x => x.TemplateId,
                        principalTable: "AnimalTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StandardPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    DiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RecommendDay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardPrescriptions_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PenCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Area = table.Column<double>(type: "float", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    BoardCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoardStatus = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CameraUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: ""),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    IsSolationCage = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cages__792D9F9AACADDF50", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Farm__CageI__2321213213",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CostingReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportMonth = table.Column<int>(type: "int", nullable: false),
                    ReportYear = table.Column<int>(type: "int", nullable: false),
                    CostType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostingReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostingReport_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectricityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalConsumption = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Electric__0B83AE01DB836F69", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Electrici__FarmI__58D1301D",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FarmCameras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChannelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FarmCame__0EBA770C379AB5BB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FarmCamer__FarmI__69FBBC1F",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FarmConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxCagesPerStaff = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MaxFarmingBatchesPerCage = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    TimeDifferenceInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmConfigs_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodStack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CostPerKg = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CurrentStock = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodStac__E117F10707F5099D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FoodStack__FarmI__08B54D69",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MasterData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CostType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterData_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaterLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstIndexData = table.Column<double>(type: "float", nullable: true),
                    LastIndexData = table.Column<double>(type: "float", nullable: true),
                    TotalConsumption = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WaterLog__C32B73CF24992978", x => x.Id);
                    table.ForeignKey(
                        name: "FK__WaterLogs__FarmI__5CA1C101",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C9404FA6D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Users__RoleId__403A8C7D",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FoodTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StageTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WeightBasedOnBodyMass = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodTemp__58E25FB67BABBFBB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FoodTempl__Stage__60A75C0F",
                        column: x => x.StageTemplateId,
                        principalTable: "GrowthStageTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskDailyTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrowthStageTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Session = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDailyTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskDailyTemplate_GrowthStageTemplates_GrowthStageTemplateId",
                        column: x => x.GrowthStageTemplateId,
                        principalTable: "GrowthStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StandardPrescriptionMedications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Morning = table.Column<int>(type: "int", nullable: false),
                    Noon = table.Column<int>(type: "int", nullable: false),
                    Afternoon = table.Column<int>(type: "int", nullable: false),
                    Evening = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardPrescriptionMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardPrescriptionMedications_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StandardPrescriptionMedications_StandardPrescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "StandardPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlBoards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControlBoardTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControlBoardCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PinCode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    CommandOn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    CommandOff = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ControlB__AB951CC9E119D9D0", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ControlBo__CageI__634EBE90",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ControlBo__Contr__6442E2C9",
                        column: x => x.ControlBoardTypeId,
                        principalTable: "ControlBoardTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ControlDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ControlCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Command = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlDevices_Cages_CageId",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmingBatchs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmingBatchCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CompleteAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedTimeStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CleaningFrequency = table.Column<int>(type: "int", nullable: false),
                    DeadQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FarmingB__CF22FB97B35EFCF4", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FarmingBa__CageI__114A936A",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__FarmingBa__Templ__10566F31",
                        column: x => x.TemplateId,
                        principalTable: "AnimalTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SensorTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PinCode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sensors__D8099BFA33DDEEA4", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Sensors__CageId__7B264821",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Sensors__SensorT__7A3223E8",
                        column: x => x.SensorTypeId,
                        principalTable: "SensorTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CostPerKg = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DateAdded = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StockLog__730D96D8A3211789", x => x.Id);
                    table.ForeignKey(
                        name: "FK__StockLogs__Stack__0C85DE4D",
                        column: x => x.StackId,
                        principalTable: "FoodStack",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CageStaffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffFarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CageStaf__666D484D977ED0B7", x => x.Id);
                    table.ForeignKey(
                        name: "FK__CageStaff__CageI__6A30C649",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__CageStaff__Staff__6B24EA82",
                        column: x => x.StaffFarmId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FarmAdmins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FarmAdmi__78A22BE82F789E8F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FarmAdmin__Admin__70DDC3D8",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__FarmAdmin__FarmI__6FE99F9F",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FarmSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfUsers = table.Column<int>(type: "int", nullable: false),
                    RequiresVet = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FarmSubs__9A2B249D65D4C549", x => x.Id);
                    table.ForeignKey(
                        name: "FK__FarmSubsc__FarmI__49C3F6B7",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__FarmSubsc__PlanI__4AB81AF0",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__FarmSubsc__UserI__4BAC3F29",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffFarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserTempId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequest_Users_StaffFarmId",
                        column: x => x.StaffFarmId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotiTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicalSymptomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E12C83A84F0", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Notificat__NotiT__531856C7",
                        column: x => x.NotiTypeId,
                        principalTable: "NotificationTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__5224328E",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TaskTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PriorityNum = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Session = table.Column<int>(type: "int", nullable: false),
                    IsWarning = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsTreatmentTask = table.Column<bool>(type: "bit", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tasks__7C6949B12CFAA294", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Tasks__AssignedT__7B5B524B",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Tasks__CageId__7A672E12",
                        column: x => x.CageId,
                        principalTable: "Cages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Tasks__TaskTypeI__797309D9",
                        column: x => x.TaskTypeId,
                        principalTable: "TaskTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnimalSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmingBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Total = table.Column<double>(type: "float", nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AnimalSa__1EE3C3FF9307295C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalSales_SaleTypes_SaleTypeId",
                        column: x => x.SaleTypeId,
                        principalTable: "SaleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__AnimalSal__Farmi__160F4887",
                        column: x => x.FarmingBatchId,
                        principalTable: "FarmingBatchs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GrowthStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmingBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WeightAnimal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    WeightAnimalExpect = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    DeadQuantity = table.Column<int>(type: "int", nullable: true),
                    AffectedQuantity = table.Column<int>(type: "int", nullable: true),
                    AgeStart = table.Column<int>(type: "int", nullable: true),
                    FoodType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgeEnd = table.Column<int>(type: "int", nullable: true),
                    SaleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgeStartDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    AgeEndDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedWeightPerSession = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    WeightBasedOnBodyMass = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GrowthSt__03EB7AD8E9B0E8F9", x => x.Id);
                    table.ForeignKey(
                        name: "FK__GrowthSta__Farmi__1AD3FDA4",
                        column: x => x.FarmingBatchId,
                        principalTable: "FarmingBatchs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalSymptoms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FarmingBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Pending"),
                    AffectedQuantity = table.Column<int>(type: "int", nullable: true),
                    IsEmergency = table.Column<bool>(type: "bit", nullable: false),
                    QuantityInCage = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecondReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalS__E39D8C018EEF7572", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalSymptoms_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__MedicalSy__Farmi__3493CFA7",
                        column: x => x.FarmingBatchId,
                        principalTable: "FarmingBatchs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControlBoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StateSmaller = table.Column<bool>(type: "bit", nullable: false),
                    ValueSmaller = table.Column<double>(type: "float", nullable: true),
                    ValueLarger = table.Column<double>(type: "float", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Jobs__056690C2461B319B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Jobs__ControlBoa__03BB8E22",
                        column: x => x.ControlBoardId,
                        principalTable: "ControlBoards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Jobs__JobTypeId__05A3D694",
                        column: x => x.JobTypeId,
                        principalTable: "JobTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Jobs__ScheduleId__02C769E9",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Jobs__SensorId__04AFB25B",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SensorDataLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsWarning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SensorDa__14C88410F7C62D8F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__SensorDat__Senso__09746778",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__55433A6BAFE66D2B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Transacti__Subsc__5165187F",
                        column: x => x.SubscriptionId,
                        principalTable: "FarmSubscriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StatusLo__A1B4D09D3A1C2CC0", x => x.Id);
                    table.ForeignKey(
                        name: "FK__StatusLog__TaskI__02FC7413",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailyFoodUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendedWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ActualWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyFoo__29B197206BAA687E", x => x.Id);
                    table.ForeignKey(
                        name: "FK__DailyFood__Stage__2180FB33",
                        column: x => x.StageId,
                        principalTable: "GrowthStages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EggHarvests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    GrowthStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCollected = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EggCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EggHarvests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EggHarvests_GrowthStages_GrowthStageId",
                        column: x => x.GrowthStageId,
                        principalTable: "GrowthStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskDaily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrowthStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Session = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDaily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskDaily_GrowthStages_GrowthStageId",
                        column: x => x.GrowthStageId,
                        principalTable: "GrowthStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaccineSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    VaccineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    ApplicationAge = table.Column<int>(type: "int", nullable: true),
                    ToltalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Session = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Chua tiêm")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VaccineS__9C8A5B49BF96F02B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__VaccineSc__Stage__2BFE89A6",
                        column: x => x.StageId,
                        principalTable: "GrowthStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__VaccineSc__Vacci__2B0A656D",
                        column: x => x.VaccineId,
                        principalTable: "Vaccines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalSymtomDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    MedicalSymptomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SymptomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSymtomDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalSymtomDetails_MedicalSymptoms_MedicalSymptomId",
                        column: x => x.MedicalSymptomId,
                        principalTable: "MedicalSymptoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalSymtomDetails_Symptoms_SymptomId",
                        column: x => x.SymptomId,
                        principalTable: "Symptoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pictures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DateCaptured = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pictures__8C2866D8353C89AA", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Pictures__Record__395884C4",
                        column: x => x.RecordId,
                        principalTable: "MedicalSymptoms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    MedicalSymtomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescribedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QuantityAnimal = table.Column<int>(type: "int", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaysToTake = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__401308323ACA723E", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Prescript__Recor__40F9A68C",
                        column: x => x.MedicalSymtomId,
                        principalTable: "MedicalSymptoms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorValue = table.Column<double>(type: "float", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobLogs__2B515D3E32FD6CEE", x => x.Id);
                    table.ForeignKey(
                        name: "FK__JobLogs__JobId__0E391C95",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VaccineScheduleLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VaccineS__E2771C19EB9C932E", x => x.Id);
                    table.ForeignKey(
                        name: "FK__VaccineSc__Sched__30C33EC3",
                        column: x => x.ScheduleId,
                        principalTable: "VaccineSchedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HealthLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HealthLo__C872D3274175629B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__HealthLog__Presc__4A8310C6",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionMedications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Morning = table.Column<int>(type: "int", nullable: false),
                    Afternoon = table.Column<int>(type: "int", nullable: false),
                    Evening = table.Column<int>(type: "int", nullable: false),
                    Noon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__CDB4BF945ED62D85", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Prescript__Medic__46B27FE2",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Prescript__Presc__45BE5BA9",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSales_FarmingBatchId",
                table: "AnimalSales",
                column: "FarmingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSales_SaleTypeId",
                table: "AnimalSales",
                column: "SaleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cages_FarmId",
                table: "Cages",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_CageStaffs_CageId",
                table: "CageStaffs",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_CageStaffs_StaffFarmId",
                table: "CageStaffs",
                column: "StaffFarmId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlBoards_CageId",
                table: "ControlBoards",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlBoards_ControlBoardTypeId",
                table: "ControlBoards",
                column: "ControlBoardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlDevices_CageId",
                table: "ControlDevices",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_CostingReport_FarmId",
                table: "CostingReport",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyFoodUsageLogs_StageId",
                table: "DailyFoodUsageLogs",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_EggHarvests_GrowthStageId",
                table: "EggHarvests",
                column: "GrowthStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricityLogs_FarmId",
                table: "ElectricityLogs",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmAdmins_AdminId",
                table: "FarmAdmins",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmAdmins_FarmId",
                table: "FarmAdmins",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmCameras_FarmId",
                table: "FarmCameras",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmConfigs_FarmId",
                table: "FarmConfigs",
                column: "FarmId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmingBatchs_CageId",
                table: "FarmingBatchs",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmingBatchs_TemplateId",
                table: "FarmingBatchs",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmSubscriptions_FarmId",
                table: "FarmSubscriptions",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmSubscriptions_PlanId",
                table: "FarmSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmSubscriptions_UserId",
                table: "FarmSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodStack_FarmId",
                table: "FoodStack",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodTemplates_StageTemplateId",
                table: "FoodTemplates",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthStages_FarmingBatchId",
                table: "GrowthStages",
                column: "FarmingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthStageTemplates_TemplateId",
                table: "GrowthStageTemplates",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthLogs_PrescriptionId",
                table: "HealthLogs",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogs_JobId",
                table: "JobLogs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ControlBoardId",
                table: "Jobs",
                column: "ControlBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobTypeId",
                table: "Jobs",
                column: "JobTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduleId",
                table: "Jobs",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SensorId",
                table: "Jobs",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_StaffFarmId",
                table: "LeaveRequest",
                column: "StaffFarmId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_FarmId",
                table: "MasterData",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSymptoms_DiseaseId",
                table: "MedicalSymptoms",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSymptoms_FarmingBatchId",
                table: "MedicalSymptoms",
                column: "FarmingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSymtomDetails_MedicalSymptomId",
                table: "MedicalSymtomDetails",
                column: "MedicalSymptomId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSymtomDetails_SymptomId",
                table: "MedicalSymtomDetails",
                column: "SymptomId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotiTypeId",
                table: "Notifications",
                column: "NotiTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Notifica__712AE57216695082",
                table: "NotificationTypes",
                column: "NotiTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pictures_RecordId",
                table: "Pictures",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedications_MedicationId",
                table: "PrescriptionMedications",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedications_PrescriptionId",
                table: "PrescriptionMedications",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalSymtomId",
                table: "Prescriptions",
                column: "MedicalSymtomId");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B61609571AADF",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorDataLogs_SensorId",
                table: "SensorDataLogs",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_CageId",
                table: "Sensors",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_SensorTypeId",
                table: "Sensors",
                column: "SensorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardPrescriptionMedications_MedicationId",
                table: "StandardPrescriptionMedications",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardPrescriptionMedications_PrescriptionId",
                table: "StandardPrescriptionMedications",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardPrescriptions_DiseaseId",
                table: "StandardPrescriptions",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusLogs_TaskId",
                table: "StatusLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLogs_StackId",
                table: "StockLogs",
                column: "StackId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDaily_GrowthStageId",
                table: "TaskDaily",
                column: "GrowthStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDailyTemplate_GrowthStageTemplateId",
                table: "TaskDailyTemplate",
                column: "GrowthStageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "Tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CageId",
                table: "Tasks",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskTypeId",
                table: "Tasks",
                column: "TaskTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ__TaskType__3B9D797BA9BD327F",
                table: "TaskTypes",
                column: "TaskTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SubscriptionId",
                table: "Transactions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E450363FD5",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534B1C73AFC",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineScheduleLogs_ScheduleId",
                table: "VaccineScheduleLogs",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineSchedules_StageId",
                table: "VaccineSchedules",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineSchedules_VaccineId",
                table: "VaccineSchedules",
                column: "VaccineId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccineTemplates_TemplateId",
                table: "VaccineTemplates",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterLogs_FarmId",
                table: "WaterLogs",
                column: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalSales");

            migrationBuilder.DropTable(
                name: "CageStaffs");

            migrationBuilder.DropTable(
                name: "ControlDevices");

            migrationBuilder.DropTable(
                name: "CostingReport");

            migrationBuilder.DropTable(
                name: "DailyFoodUsageLogs");

            migrationBuilder.DropTable(
                name: "EggHarvests");

            migrationBuilder.DropTable(
                name: "ElectricityLogs");

            migrationBuilder.DropTable(
                name: "FarmAdmins");

            migrationBuilder.DropTable(
                name: "FarmCameras");

            migrationBuilder.DropTable(
                name: "FarmConfigs");

            migrationBuilder.DropTable(
                name: "FoodTemplates");

            migrationBuilder.DropTable(
                name: "HealthLogs");

            migrationBuilder.DropTable(
                name: "JobLogs");

            migrationBuilder.DropTable(
                name: "LeaveRequest");

            migrationBuilder.DropTable(
                name: "MasterData");

            migrationBuilder.DropTable(
                name: "MedicalSymtomDetails");

            migrationBuilder.DropTable(
                name: "MqttConfigs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Pictures");

            migrationBuilder.DropTable(
                name: "PrescriptionMedications");

            migrationBuilder.DropTable(
                name: "Pricings");

            migrationBuilder.DropTable(
                name: "SensorDataLogs");

            migrationBuilder.DropTable(
                name: "StandardPrescriptionMedications");

            migrationBuilder.DropTable(
                name: "StatusLogs");

            migrationBuilder.DropTable(
                name: "StockLogs");

            migrationBuilder.DropTable(
                name: "TaskDaily");

            migrationBuilder.DropTable(
                name: "TaskDailyTemplate");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "VaccineScheduleLogs");

            migrationBuilder.DropTable(
                name: "VaccineTemplates");

            migrationBuilder.DropTable(
                name: "WaterLogs");

            migrationBuilder.DropTable(
                name: "WhitelistDomains");

            migrationBuilder.DropTable(
                name: "SaleTypes");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Symptoms");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "StandardPrescriptions");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "FoodStack");

            migrationBuilder.DropTable(
                name: "GrowthStageTemplates");

            migrationBuilder.DropTable(
                name: "FarmSubscriptions");

            migrationBuilder.DropTable(
                name: "VaccineSchedules");

            migrationBuilder.DropTable(
                name: "ControlBoards");

            migrationBuilder.DropTable(
                name: "JobTypes");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "MedicalSymptoms");

            migrationBuilder.DropTable(
                name: "TaskTypes");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GrowthStages");

            migrationBuilder.DropTable(
                name: "Vaccines");

            migrationBuilder.DropTable(
                name: "ControlBoardTypes");

            migrationBuilder.DropTable(
                name: "SensorTypes");

            migrationBuilder.DropTable(
                name: "Diseases");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "FarmingBatchs");

            migrationBuilder.DropTable(
                name: "Cages");

            migrationBuilder.DropTable(
                name: "AnimalTemplates");

            migrationBuilder.DropTable(
                name: "Farms");
        }
    }
}
