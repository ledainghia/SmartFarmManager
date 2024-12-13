IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AnimalTemplates] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(100) NOT NULL,
    [Species] nvarchar(50) NOT NULL,
    [DefaultCapacity] int NULL,
    [Notes] nvarchar(255) NULL,
    CONSTRAINT [PK__AnimalTe__F87ADD27AE731EF5] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ControlBoardTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(2000) NULL,
    CONSTRAINT [PK__ControlB__8CDFB1CCB09566BA] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Farms] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmCode] nvarchar(50) NULL,
    [Name] nvarchar(255) NOT NULL,
    [Address] nvarchar(255) NULL,
    [PhoneNumber] nvarchar(50) NULL,
    [Email] nvarchar(255) NULL,
    [Area] float NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    [MACAddress] nvarchar(255) NOT NULL,
    CONSTRAINT [PK__Farms__ED7BBAB9F3B62FC0] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [JobTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(2000) NULL,
    CONSTRAINT [PK__JobTypes__E1F462AD8039AD4A] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Medications] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(100) NULL,
    [UsageInstructions] nvarchar(255) NULL,
    [Price] decimal(10,2) NULL,
    [DoseQuantity] int NULL,
    [PricePerDose] decimal(10,2) NULL,
    CONSTRAINT [PK__Medicati__62EC1AFA81A8C124] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [MqttConfigs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Port] int NOT NULL,
    [BrokerAddress] nvarchar(255) NULL,
    [QoS] int NOT NULL,
    [KeepAlive] int NOT NULL,
    [CleanSession] bit NOT NULL,
    [UserName] nvarchar(50) NULL,
    [Password] nvarchar(50) NULL,
    [WillMessage] nvarchar(2000) NULL,
    [UseTls] bit NOT NULL,
    CONSTRAINT [PK__MqttConf__065618CFF49F21BB] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [NotificationTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [NotiTypeName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__Notifica__54F5A3018831B14F] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Pricings] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(max) NOT NULL,
    [PricePerUnit] int NOT NULL,
    [Unit] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK__Pricings__EC306B12D14952C7] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [RoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__Roles__8AFACE1A640F661E] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Schedules] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [ScheduleCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [TimeOn] nvarchar(50) NOT NULL DEFAULT N'',
    [TimeOff] nvarchar(50) NOT NULL DEFAULT N'',
    CONSTRAINT [PK__Schedule__9C8A5B49CADB8578] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SensorTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(255) NOT NULL DEFAULT N'',
    [Description] nvarchar(2000) NULL,
    [FieldName] nvarchar(255) NOT NULL DEFAULT N'',
    [Unit] nvarchar(255) NOT NULL DEFAULT N'',
    [DefaultPinCode] int NOT NULL,
    CONSTRAINT [PK__SensorTy__B6E7763F2A9179FF] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Statuses] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [StatusName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__Statuses__C8EE2063A8C85F92] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SubscriptionPlans] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [PlanName] nvarchar(100) NOT NULL,
    [CostPerUser] decimal(10,2) NOT NULL,
    [CostPerVet] decimal(10,2) NOT NULL,
    [MonthlyBaseCost] decimal(10,2) NOT NULL,
    CONSTRAINT [PK__Subscrip__755C22B7124150BB] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TaskTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TaskTypeName] nvarchar(50) NOT NULL,
    [PriorityNum] int NULL,
    CONSTRAINT [PK__TaskType__66B23E330DE8C67D] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Vaccines] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(100) NULL,
    [Method] nvarchar(50) NULL,
    [AgeStartDate] datetime NULL DEFAULT ((getdate())),
    [AgeEndDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Vaccines__45DC6889A12FCD5C] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [GrowthStageTemplates] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TemplateId] uniqueidentifier NOT NULL,
    [StageName] nvarchar(50) NULL,
    [WeightAnimal] decimal(10,2) NULL,
    [AgeStart] int NULL,
    [AgeEnd] int NULL,
    [Notes] nvarchar(255) NULL,
    CONSTRAINT [PK__GrowthSt__12B67065BFCFA63B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__GrowthSta__Templ__59063A47] FOREIGN KEY ([TemplateId]) REFERENCES [AnimalTemplates] ([Id])
);
GO

CREATE TABLE [VaccineTemplates] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TemplateId] uniqueidentifier NOT NULL,
    [VaccineName] nvarchar(100) NULL,
    [ApplicationMethod] nvarchar(50) NULL,
    [ApplicationAge] int NULL,
    [Session] int NOT NULL,
    CONSTRAINT [PK__VaccineT__B9AB66085E77BB5A] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__VaccineTe__Templ__5CD6CB2B] FOREIGN KEY ([TemplateId]) REFERENCES [AnimalTemplates] ([Id])
);
GO

CREATE TABLE [Cages] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [PenCode] nvarchar(50) NOT NULL DEFAULT N'',
    [FarmId] uniqueidentifier NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [Area] float NOT NULL,
    [Location] nvarchar(255) NULL,
    [Capacity] int NOT NULL,
    [AnimalType] nvarchar(255) NULL,
    [BoardCode] nvarchar(50) NOT NULL,
    [BoardStatus] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    [CameraUrl] nvarchar(255) NOT NULL DEFAULT N'',
    [ChannelId] int NOT NULL,
    CONSTRAINT [PK__Cages__792D9F9AACADDF50] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Farm__CageI__2321213213] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [ElectricityLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [Data] float NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK__Electric__0B83AE01DB836F69] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Electrici__FarmI__58D1301D] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [FarmCameras] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Url] nvarchar(255) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    [ChannelId] int NOT NULL,
    CONSTRAINT [PK__FarmCame__0EBA770C379AB5BB] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FarmCamer__FarmI__69FBBC1F] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [FoodStack] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [NameFood] nvarchar(100) NULL,
    [Quantity] decimal(10,2) NULL,
    [CostPerKg] decimal(10,2) NULL,
    [CurrentStock] decimal(10,2) NULL,
    CONSTRAINT [PK__FoodStac__E117F10707F5099D] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FoodStack__FarmI__08B54D69] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [WaterLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [Data] float NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [FirstIndexData] float NULL,
    [LastIndexData] float NULL,
    CONSTRAINT [PK__WaterLog__C32B73CF24992978] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__WaterLogs__FarmI__5CA1C101] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [Email] nvarchar(100) NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [Address] nvarchar(100) NULL,
    [IsActive] bit NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK__Users__1788CC4C9404FA6D] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Users__RoleId__403A8C7D] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);
GO

CREATE TABLE [FoodTemplates] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [StageTemplateId] uniqueidentifier NOT NULL,
    [FoodName] nvarchar(100) NOT NULL,
    [RecommendedWeightPerDay] decimal(10,2) NULL,
    [Session] int NOT NULL,
    [WeightBasedOnBodyMass] decimal(5,2) NULL,
    CONSTRAINT [PK__FoodTemp__58E25FB67BABBFBB] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FoodTempl__Stage__60A75C0F] FOREIGN KEY ([StageTemplateId]) REFERENCES [GrowthStageTemplates] ([Id])
);
GO

CREATE TABLE [TaskDailyTemplate] (
    [Id] uniqueidentifier NOT NULL,
    [GrowthStageTemplateId] uniqueidentifier NOT NULL,
    [TaskTypeId] uniqueidentifier NULL,
    [TaskName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [Session] int NOT NULL,
    CONSTRAINT [PK_TaskDailyTemplate] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskDailyTemplate_GrowthStageTemplates_GrowthStageTemplateId] FOREIGN KEY ([GrowthStageTemplateId]) REFERENCES [GrowthStageTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ControlBoards] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [CageId] uniqueidentifier NOT NULL,
    [ControlBoardTypeId] uniqueidentifier NOT NULL,
    [ControlBoardCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [PinCode] int NOT NULL,
    [Status] bit NOT NULL,
    [State] bit NOT NULL,
    [CommandOn] nvarchar(50) NOT NULL DEFAULT N'',
    [CommandOff] nvarchar(50) NOT NULL DEFAULT N'',
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK__ControlB__AB951CC9E119D9D0] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__ControlBo__CageI__634EBE90] FOREIGN KEY ([CageId]) REFERENCES [Cages] ([Id]),
    CONSTRAINT [FK__ControlBo__Contr__6442E2C9] FOREIGN KEY ([ControlBoardTypeId]) REFERENCES [ControlBoardTypes] ([Id])
);
GO

CREATE TABLE [FarmingBatchs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TemplateId] uniqueidentifier NOT NULL,
    [CageId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL,
    [Species] nvarchar(50) NULL,
    [StartDate] datetime NULL DEFAULT ((getdate())),
    [CompleteAt] datetime2 NULL,
    [Status] nvarchar(max) NULL,
    [CleaningFrequency] int NOT NULL,
    [Quantity] int NULL,
    [FarmId] int NOT NULL,
    CONSTRAINT [PK__FarmingB__CF22FB97B35EFCF4] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FarmingBa__CageI__114A936A] FOREIGN KEY ([CageId]) REFERENCES [Cages] ([Id]),
    CONSTRAINT [FK__FarmingBa__Templ__10566F31] FOREIGN KEY ([TemplateId]) REFERENCES [AnimalTemplates] ([Id])
);
GO

CREATE TABLE [Sensors] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [SensorTypeId] uniqueidentifier NOT NULL,
    [CageId] uniqueidentifier NOT NULL,
    [SensorCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [PinCode] int NOT NULL,
    [Status] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    [NodeId] int NOT NULL,
    CONSTRAINT [PK__Sensors__D8099BFA33DDEEA4] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Sensors__CageId__7B264821] FOREIGN KEY ([CageId]) REFERENCES [Cages] ([Id]),
    CONSTRAINT [FK__Sensors__SensorT__7A3223E8] FOREIGN KEY ([SensorTypeId]) REFERENCES [SensorTypes] ([Id])
);
GO

CREATE TABLE [StockLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [StackId] uniqueidentifier NOT NULL,
    [NameFood] nvarchar(100) NULL,
    [Quantity] decimal(10,2) NULL,
    [CostPerKg] decimal(10,2) NULL,
    [DateAdded] date NULL,
    CONSTRAINT [PK__StockLog__730D96D8A3211789] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__StockLogs__Stack__0C85DE4D] FOREIGN KEY ([StackId]) REFERENCES [FoodStack] ([Id])
);
GO

CREATE TABLE [CageStaffs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [CageId] uniqueidentifier NOT NULL,
    [StaffFarmId] uniqueidentifier NOT NULL,
    [AssignedDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__CageStaf__666D484D977ED0B7] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__CageStaff__CageI__6A30C649] FOREIGN KEY ([CageId]) REFERENCES [Cages] ([Id]),
    CONSTRAINT [FK__CageStaff__Staff__6B24EA82] FOREIGN KEY ([StaffFarmId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [FarmAdmins] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [AdminId] uniqueidentifier NOT NULL,
    [AssignedDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__FarmAdmi__78A22BE82F789E8F] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FarmAdmin__Admin__70DDC3D8] FOREIGN KEY ([AdminId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK__FarmAdmin__FarmI__6FE99F9F] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id])
);
GO

CREATE TABLE [FarmSubscriptions] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmId] uniqueidentifier NOT NULL,
    [PlanId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [NumberOfUsers] int NOT NULL,
    [RequiresVet] bit NULL DEFAULT CAST(0 AS bit),
    [StartDate] datetime NULL DEFAULT ((getdate())),
    [EndDate] datetime NULL,
    [Status] nvarchar(50) NULL,
    [TotalCost] decimal(10,2) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK__FarmSubs__9A2B249D65D4C549] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__FarmSubsc__FarmI__49C3F6B7] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([Id]),
    CONSTRAINT [FK__FarmSubsc__PlanI__4AB81AF0] FOREIGN KEY ([PlanId]) REFERENCES [SubscriptionPlans] ([Id]),
    CONSTRAINT [FK__FarmSubsc__UserI__4BAC3F29] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [NotiTypeId] uniqueidentifier NOT NULL,
    [Content] nvarchar(255) NOT NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [IsRead] bit NULL DEFAULT CAST(0 AS bit),
    [FarmId] int NULL,
    [CageId] int NULL,
    CONSTRAINT [PK__Notifica__20CF2E12C83A84F0] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Notificat__NotiT__531856C7] FOREIGN KEY ([NotiTypeId]) REFERENCES [NotificationTypes] ([Id]),
    CONSTRAINT [FK__Notificat__UserI__5224328E] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Tasks] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TaskTypeId] uniqueidentifier NULL,
    [CageId] uniqueidentifier NOT NULL,
    [AssignedToUserId] uniqueidentifier NOT NULL,
    [CreatedByUserId] uniqueidentifier NOT NULL,
    [TaskName] nvarchar(50) NOT NULL,
    [PriorityNum] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [DueDate] datetime NULL,
    [Status] nvarchar(50) NULL,
    [Session] int NOT NULL,
    [CompletedAt] datetime NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Tasks__7C6949B12CFAA294] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Tasks__AssignedT__7B5B524B] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK__Tasks__CageId__7A672E12] FOREIGN KEY ([CageId]) REFERENCES [Cages] ([Id]),
    CONSTRAINT [FK__Tasks__TaskTypeI__797309D9] FOREIGN KEY ([TaskTypeId]) REFERENCES [TaskTypes] ([Id])
);
GO

CREATE TABLE [AnimalSales] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmingBatchId] uniqueidentifier NOT NULL,
    [SaleDate] datetime NULL DEFAULT ((getdate())),
    [Revenue] float NOT NULL,
    [BuyerInfo] nvarchar(255) NULL,
    CONSTRAINT [PK__AnimalSa__1EE3C3FF9307295C] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__AnimalSal__Farmi__160F4887] FOREIGN KEY ([FarmingBatchId]) REFERENCES [FarmingBatchs] ([Id])
);
GO

CREATE TABLE [GrowthStages] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmingBatchId] uniqueidentifier NOT NULL,
    [Name] nvarchar(50) NULL,
    [WeightAnimal] decimal(10,2) NULL,
    [Quantity] int NULL,
    [AgeStartDate] datetime NULL DEFAULT ((getdate())),
    [AgeEndDate] datetime NULL DEFAULT ((getdate())),
    [RecommendedWeightPerDay] decimal(10,2) NULL,
    [WeightBasedOnBodyMass] decimal(5,2) NULL,
    CONSTRAINT [PK__GrowthSt__03EB7AD8E9B0E8F9] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__GrowthSta__Farmi__1AD3FDA4] FOREIGN KEY ([FarmingBatchId]) REFERENCES [FarmingBatchs] ([Id])
);
GO

CREATE TABLE [MedicalSymptoms] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [FarmingBatchId] uniqueidentifier NOT NULL,
    [Symptoms] nvarchar(200) NULL,
    [Diagnosis] nvarchar(100) NULL,
    [Treatment] nvarchar(100) NULL,
    [Status] nvarchar(50) NULL DEFAULT N'Ðang di?u tr?',
    [AffectedQuantity] int NULL,
    [Notes] nvarchar(255) NULL,
    CONSTRAINT [PK__MedicalS__E39D8C018EEF7572] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__MedicalSy__Farmi__3493CFA7] FOREIGN KEY ([FarmingBatchId]) REFERENCES [FarmingBatchs] ([Id])
);
GO

CREATE TABLE [Jobs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [ScheduleId] uniqueidentifier NOT NULL,
    [ControlBoardId] uniqueidentifier NOT NULL,
    [SensorId] uniqueidentifier NOT NULL,
    [JobTypeId] uniqueidentifier NOT NULL,
    [JobCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [StateSmaller] bit NOT NULL,
    [ValueSmaller] float NULL,
    [ValueLarger] float NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK__Jobs__056690C2461B319B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Jobs__ControlBoa__03BB8E22] FOREIGN KEY ([ControlBoardId]) REFERENCES [ControlBoards] ([Id]),
    CONSTRAINT [FK__Jobs__JobTypeId__05A3D694] FOREIGN KEY ([JobTypeId]) REFERENCES [JobTypes] ([Id]),
    CONSTRAINT [FK__Jobs__ScheduleId__02C769E9] FOREIGN KEY ([ScheduleId]) REFERENCES [Schedules] ([Id]),
    CONSTRAINT [FK__Jobs__SensorId__04AFB25B] FOREIGN KEY ([SensorId]) REFERENCES [Sensors] ([Id])
);
GO

CREATE TABLE [SensorDataLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [SensorId] uniqueidentifier NOT NULL,
    [Data] decimal(18,1) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [IsWarning] bit NOT NULL,
    CONSTRAINT [PK__SensorDa__14C88410F7C62D8F] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__SensorDat__Senso__09746778] FOREIGN KEY ([SensorId]) REFERENCES [Sensors] ([Id])
);
GO

CREATE TABLE [Transactions] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [SubscriptionId] uniqueidentifier NOT NULL,
    [TransactionDate] datetime NULL DEFAULT ((getdate())),
    [Amount] decimal(10,2) NOT NULL,
    [Status] nvarchar(50) NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK__Transact__55433A6BAFE66D2B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Transacti__Subsc__5165187F] FOREIGN KEY ([SubscriptionId]) REFERENCES [FarmSubscriptions] ([Id])
);
GO

CREATE TABLE [StatusLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [TaskId] uniqueidentifier NOT NULL,
    [StatusId] uniqueidentifier NOT NULL,
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__StatusLo__A1B4D09D3A1C2CC0] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__StatusLog__Statu__03F0984C] FOREIGN KEY ([StatusId]) REFERENCES [Statuses] ([Id]),
    CONSTRAINT [FK__StatusLog__TaskI__02FC7413] FOREIGN KEY ([TaskId]) REFERENCES [Tasks] ([Id])
);
GO

CREATE TABLE [DailyFoodUsageLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [StageId] uniqueidentifier NOT NULL,
    [RecommendedWeight] decimal(10,2) NULL,
    [ActualWeight] decimal(10,2) NULL,
    [Notes] nvarchar(255) NULL,
    [LogTime] datetime NULL DEFAULT ((getdate())),
    [Photo] nvarchar(255) NULL,
    [TaskId] int NULL,
    CONSTRAINT [PK__DailyFoo__29B197206BAA687E] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__DailyFood__Stage__2180FB33] FOREIGN KEY ([StageId]) REFERENCES [GrowthStages] ([Id])
);
GO

CREATE TABLE [TaskDaily] (
    [Id] uniqueidentifier NOT NULL,
    [GrowthStageId] uniqueidentifier NOT NULL,
    [TaskTypeId] uniqueidentifier NULL,
    [TaskName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [Session] int NOT NULL,
    [StartAt] datetime2 NULL,
    [EndAt] datetime2 NULL,
    CONSTRAINT [PK_TaskDaily] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskDaily_GrowthStages_GrowthStageId] FOREIGN KEY ([GrowthStageId]) REFERENCES [GrowthStages] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VaccineSchedules] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [VaccineId] uniqueidentifier NOT NULL,
    [StageId] uniqueidentifier NOT NULL,
    [Date] date NULL,
    [Quantity] int NULL,
    [Status] nvarchar(50) NULL DEFAULT N'Chua tiêm',
    CONSTRAINT [PK__VaccineS__9C8A5B49BF96F02B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__VaccineSc__Stage__2BFE89A6] FOREIGN KEY ([StageId]) REFERENCES [GrowthStages] ([Id]),
    CONSTRAINT [FK__VaccineSc__Vacci__2B0A656D] FOREIGN KEY ([VaccineId]) REFERENCES [Vaccines] ([Id])
);
GO

CREATE TABLE [Pictures] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [RecordId] uniqueidentifier NOT NULL,
    [Image] nvarchar(255) NULL,
    [DateCaptured] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Pictures__8C2866D8353C89AA] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Pictures__Record__395884C4] FOREIGN KEY ([RecordId]) REFERENCES [MedicalSymptoms] ([Id])
);
GO

CREATE TABLE [Prescriptions] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [RecordId] uniqueidentifier NOT NULL,
    [PrescribedDate] date NULL DEFAULT ((getdate())),
    [CaseType] nvarchar(50) NULL,
    [Notes] nvarchar(255) NULL,
    [QuantityAnimal] int NOT NULL,
    [Status] nvarchar(max) NULL,
    [Price] decimal(10,2) NULL,
    CONSTRAINT [PK__Prescrip__401308323ACA723E] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Prescript__Recor__40F9A68C] FOREIGN KEY ([RecordId]) REFERENCES [MedicalSymptoms] ([Id])
);
GO

CREATE TABLE [JobLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [JobId] uniqueidentifier NOT NULL,
    [SensorValue] float NOT NULL,
    [Command] nvarchar(255) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK__JobLogs__2B515D3E32FD6CEE] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__JobLogs__JobId__0E391C95] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id])
);
GO

CREATE TABLE [VaccineScheduleLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [ScheduleId] uniqueidentifier NOT NULL,
    [Date] date NULL,
    [Notes] nvarchar(255) NULL,
    [Photo] nvarchar(255) NULL,
    [TaskId] int NULL,
    CONSTRAINT [PK__VaccineS__E2771C19EB9C932E] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__VaccineSc__Sched__30C33EC3] FOREIGN KEY ([ScheduleId]) REFERENCES [VaccineSchedules] ([Id])
);
GO

CREATE TABLE [HealthLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [PrescriptionId] uniqueidentifier NOT NULL,
    [Date] date NULL,
    [Notes] nvarchar(255) NULL,
    [Photo] nvarchar(255) NULL,
    [TaskId] int NULL,
    CONSTRAINT [PK__HealthLo__C872D3274175629B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__HealthLog__Presc__4A8310C6] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id])
);
GO

CREATE TABLE [PrescriptionMedications] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [PrescriptionId] uniqueidentifier NOT NULL,
    [MedicationId] uniqueidentifier NOT NULL,
    [Dosage] int NULL,
    [Frequency] int NULL,
    [Duration] int NULL,
    CONSTRAINT [PK__Prescrip__CDB4BF945ED62D85] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Prescript__Medic__46B27FE2] FOREIGN KEY ([MedicationId]) REFERENCES [Medications] ([Id]),
    CONSTRAINT [FK__Prescript__Presc__45BE5BA9] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id])
);
GO

CREATE INDEX [IX_AnimalSales_FarmingBatchId] ON [AnimalSales] ([FarmingBatchId]);
GO

CREATE INDEX [IX_Cages_FarmId] ON [Cages] ([FarmId]);
GO

CREATE INDEX [IX_CageStaffs_CageId] ON [CageStaffs] ([CageId]);
GO

CREATE INDEX [IX_CageStaffs_StaffFarmId] ON [CageStaffs] ([StaffFarmId]);
GO

CREATE INDEX [IX_ControlBoards_CageId] ON [ControlBoards] ([CageId]);
GO

CREATE INDEX [IX_ControlBoards_ControlBoardTypeId] ON [ControlBoards] ([ControlBoardTypeId]);
GO

CREATE INDEX [IX_DailyFoodUsageLogs_StageId] ON [DailyFoodUsageLogs] ([StageId]);
GO

CREATE INDEX [IX_ElectricityLogs_FarmId] ON [ElectricityLogs] ([FarmId]);
GO

CREATE INDEX [IX_FarmAdmins_AdminId] ON [FarmAdmins] ([AdminId]);
GO

CREATE INDEX [IX_FarmAdmins_FarmId] ON [FarmAdmins] ([FarmId]);
GO

CREATE INDEX [IX_FarmCameras_FarmId] ON [FarmCameras] ([FarmId]);
GO

CREATE INDEX [IX_FarmingBatchs_CageId] ON [FarmingBatchs] ([CageId]);
GO

CREATE INDEX [IX_FarmingBatchs_TemplateId] ON [FarmingBatchs] ([TemplateId]);
GO

CREATE INDEX [IX_FarmSubscriptions_FarmId] ON [FarmSubscriptions] ([FarmId]);
GO

CREATE INDEX [IX_FarmSubscriptions_PlanId] ON [FarmSubscriptions] ([PlanId]);
GO

CREATE INDEX [IX_FarmSubscriptions_UserId] ON [FarmSubscriptions] ([UserId]);
GO

CREATE INDEX [IX_FoodStack_FarmId] ON [FoodStack] ([FarmId]);
GO

CREATE INDEX [IX_FoodTemplates_StageTemplateId] ON [FoodTemplates] ([StageTemplateId]);
GO

CREATE INDEX [IX_GrowthStages_FarmingBatchId] ON [GrowthStages] ([FarmingBatchId]);
GO

CREATE INDEX [IX_GrowthStageTemplates_TemplateId] ON [GrowthStageTemplates] ([TemplateId]);
GO

CREATE INDEX [IX_HealthLogs_PrescriptionId] ON [HealthLogs] ([PrescriptionId]);
GO

CREATE INDEX [IX_JobLogs_JobId] ON [JobLogs] ([JobId]);
GO

CREATE INDEX [IX_Jobs_ControlBoardId] ON [Jobs] ([ControlBoardId]);
GO

CREATE INDEX [IX_Jobs_JobTypeId] ON [Jobs] ([JobTypeId]);
GO

CREATE INDEX [IX_Jobs_ScheduleId] ON [Jobs] ([ScheduleId]);
GO

CREATE INDEX [IX_Jobs_SensorId] ON [Jobs] ([SensorId]);
GO

CREATE INDEX [IX_MedicalSymptoms_FarmingBatchId] ON [MedicalSymptoms] ([FarmingBatchId]);
GO

CREATE INDEX [IX_Notifications_NotiTypeId] ON [Notifications] ([NotiTypeId]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE UNIQUE INDEX [UQ__Notifica__712AE57216695082] ON [NotificationTypes] ([NotiTypeName]);
GO

CREATE INDEX [IX_Pictures_RecordId] ON [Pictures] ([RecordId]);
GO

CREATE INDEX [IX_PrescriptionMedications_MedicationId] ON [PrescriptionMedications] ([MedicationId]);
GO

CREATE INDEX [IX_PrescriptionMedications_PrescriptionId] ON [PrescriptionMedications] ([PrescriptionId]);
GO

CREATE INDEX [IX_Prescriptions_RecordId] ON [Prescriptions] ([RecordId]);
GO

CREATE UNIQUE INDEX [UQ__Roles__8A2B61609571AADF] ON [Roles] ([RoleName]);
GO

CREATE INDEX [IX_SensorDataLogs_SensorId] ON [SensorDataLogs] ([SensorId]);
GO

CREATE INDEX [IX_Sensors_CageId] ON [Sensors] ([CageId]);
GO

CREATE INDEX [IX_Sensors_SensorTypeId] ON [Sensors] ([SensorTypeId]);
GO

CREATE INDEX [IX_StatusLogs_StatusId] ON [StatusLogs] ([StatusId]);
GO

CREATE INDEX [IX_StatusLogs_TaskId] ON [StatusLogs] ([TaskId]);
GO

CREATE INDEX [IX_StockLogs_StackId] ON [StockLogs] ([StackId]);
GO

CREATE INDEX [IX_TaskDaily_GrowthStageId] ON [TaskDaily] ([GrowthStageId]);
GO

CREATE INDEX [IX_TaskDailyTemplate_GrowthStageTemplateId] ON [TaskDailyTemplate] ([GrowthStageTemplateId]);
GO

CREATE INDEX [IX_Tasks_AssignedToUserId] ON [Tasks] ([AssignedToUserId]);
GO

CREATE INDEX [IX_Tasks_CageId] ON [Tasks] ([CageId]);
GO

CREATE INDEX [IX_Tasks_TaskTypeId] ON [Tasks] ([TaskTypeId]);
GO

CREATE UNIQUE INDEX [UQ__TaskType__3B9D797BA9BD327F] ON [TaskTypes] ([TaskTypeName]);
GO

CREATE INDEX [IX_Transactions_SubscriptionId] ON [Transactions] ([SubscriptionId]);
GO

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
GO

CREATE UNIQUE INDEX [UQ__Users__536C85E450363FD5] ON [Users] ([Username]);
GO

CREATE UNIQUE INDEX [UQ__Users__A9D10534B1C73AFC] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE INDEX [IX_VaccineScheduleLogs_ScheduleId] ON [VaccineScheduleLogs] ([ScheduleId]);
GO

CREATE INDEX [IX_VaccineSchedules_StageId] ON [VaccineSchedules] ([StageId]);
GO

CREATE INDEX [IX_VaccineSchedules_VaccineId] ON [VaccineSchedules] ([VaccineId]);
GO

CREATE INDEX [IX_VaccineTemplates_TemplateId] ON [VaccineTemplates] ([TemplateId]);
GO

CREATE INDEX [IX_WaterLogs_FarmId] ON [WaterLogs] ([FarmId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241213133138_init', N'8.0.10');
GO

COMMIT;
GO

