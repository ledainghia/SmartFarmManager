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

CREATE TABLE [AlertTypes] (
    [AlertTypeId] int NOT NULL IDENTITY,
    [AlertTypeName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__AlertTyp__016D41BD47CEE0B1] PRIMARY KEY ([AlertTypeId])
);
GO

CREATE TABLE [Permissions] (
    [PermissionId] int NOT NULL IDENTITY,
    [PermissionName] nvarchar(50) NOT NULL,
    [RoleType] nvarchar(255) NULL,
    CONSTRAINT [PK__Permissi__EFA6FB2F137B7BF6] PRIMARY KEY ([PermissionId])
);
GO

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__Roles__8AFACE1A6F649AA8] PRIMARY KEY ([RoleId])
);
GO

CREATE TABLE [RoleUser] (
    [RoleId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_RoleUser] PRIMARY KEY ([RoleId], [UserId])
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [Email] nvarchar(100) NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [IsActive] bit NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Users__1788CC4C736F545B] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [Farms] (
    [FarmId] int NOT NULL IDENTITY,
    [FarmName] nvarchar(100) NOT NULL,
    [Location] nvarchar(255) NULL,
    [OwnerId] int NOT NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Farms__ED7BBAB981AFFB50] PRIMARY KEY ([FarmId]),
    CONSTRAINT [FK__Farms__OwnerId__4CA06362] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [Notifications] (
    [NotificationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Content] nvarchar(255) NOT NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [IsRead] bit NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK__Notifica__20CF2E12B1571953] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK__Notificat__UserI__10566F31] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [UserPermissions] (
    [UserPermissionId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [GrantedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__UserPerm__A90F88B2DD6A3730] PRIMARY KEY ([UserPermissionId]),
    CONSTRAINT [FK__UserPermi__Permi__48CFD27E] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([PermissionId]),
    CONSTRAINT [FK__UserPermi__UserI__47DBAE45] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [UserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK__UserRole__AF2760AD0C3A26E3] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK__UserRoles__RoleI__412EB0B6] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]),
    CONSTRAINT [FK__UserRoles__UserI__403A8C7D] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [CameraSurveillance] (
    [CameraId] int NOT NULL IDENTITY,
    [FarmId] int NOT NULL,
    [CameraLocation] nvarchar(255) NULL,
    [Status] nvarchar(50) NULL DEFAULT N'Active',
    [InstallDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__CameraSu__F971E0C87134DF29] PRIMARY KEY ([CameraId]),
    CONSTRAINT [FK__CameraSur__FarmI__5DCAEF64] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [FarmStaffAssignments] (
    [AssignmentId] int NOT NULL IDENTITY,
    [FarmId] int NOT NULL,
    [FarmStaffId] int NOT NULL,
    CONSTRAINT [PK__FarmStaf__32499E77E4556627] PRIMARY KEY ([AssignmentId]),
    CONSTRAINT [FK__FarmStaff__FarmI__4F7CD00D] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId]),
    CONSTRAINT [FK__FarmStaff__FarmS__5070F446] FOREIGN KEY ([FarmStaffId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [Inventory] (
    [InventoryId] int NOT NULL IDENTITY,
    [FarmId] int NOT NULL,
    [ItemName] nvarchar(100) NOT NULL,
    [Quantity] int NOT NULL,
    [Unit] nvarchar(50) NULL,
    [CostPerUnit] float NOT NULL,
    [ExpirationDate] datetime NULL,
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Inventor__F5FDE6B3D8FDE6F2] PRIMARY KEY ([InventoryId]),
    CONSTRAINT [FK__Inventory__FarmI__787EE5A0] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [IoTDevices] (
    [DeviceId] int NOT NULL IDENTITY,
    [DeviceType] nvarchar(50) NOT NULL,
    [FarmId] int NOT NULL,
    [Status] nvarchar(50) NULL DEFAULT N'Active',
    [InstallDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__IoTDevic__49E12311DF9DC36E] PRIMARY KEY ([DeviceId]),
    CONSTRAINT [FK__IoTDevice__FarmI__5535A963] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [Livestock] (
    [LivestockId] int NOT NULL IDENTITY,
    [FarmId] int NOT NULL,
    [Species] nvarchar(50) NOT NULL,
    [Quantity] int NOT NULL,
    [GrowthCycle] nvarchar(50) NULL,
    [Expenses] float NULL DEFAULT 0.0E0,
    [Status] nvarchar(50) NULL DEFAULT N'Active',
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Livestoc__8228ABF08B90916C] PRIMARY KEY ([LivestockId]),
    CONSTRAINT [FK__Livestock__FarmI__6D0D32F4] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [RevenueAndProfitReports] (
    [ReportId] int NOT NULL IDENTITY,
    [FarmId] int NOT NULL,
    [StartDate] datetime NOT NULL,
    [EndDate] datetime NOT NULL,
    [TotalRevenue] float NOT NULL,
    [TotalExpenses] float NOT NULL,
    [TotalProfit] AS ([TotalRevenue]-[TotalExpenses]) PERSISTED,
    [ReportGeneratedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__RevenueA__D5BD4805D2AAFD61] PRIMARY KEY ([ReportId]),
    CONSTRAINT [FK__RevenueAn__FarmI__00200768] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [Tasks] (
    [TaskId] int NOT NULL IDENTITY,
    [TaskName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [AssignedToUserId] int NOT NULL,
    [FarmId] int NOT NULL,
    [DueDate] datetime NULL,
    [Status] nvarchar(50) NULL DEFAULT N'Pending',
    [TaskType] nvarchar(50) NOT NULL,
    [CompletedAt] datetime NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Tasks__7C6949B1048346FE] PRIMARY KEY ([TaskId]),
    CONSTRAINT [FK__Tasks__AssignedT__628FA481] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK__Tasks__FarmId__6383C8BA] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [InventoryTransactions] (
    [TransactionId] int NOT NULL IDENTITY,
    [InventoryId] int NOT NULL,
    [TransactionType] nvarchar(50) NOT NULL,
    [Quantity] int NOT NULL,
    [Date] datetime NULL DEFAULT ((getdate())),
    [Purpose] nvarchar(255) NULL,
    CONSTRAINT [PK__Inventor__55433A6B3D631B6C] PRIMARY KEY ([TransactionId]),
    CONSTRAINT [FK__Inventory__Inven__7C4F7684] FOREIGN KEY ([InventoryId]) REFERENCES [Inventory] ([InventoryId])
);
GO

CREATE TABLE [Alerts] (
    [AlertId] int NOT NULL IDENTITY,
    [DeviceId] int NOT NULL,
    [AlertTypeId] int NOT NULL,
    [FarmId] int NOT NULL,
    [Message] nvarchar(255) NULL,
    [AcknowledgedAt] datetime NULL,
    CONSTRAINT [PK__Alerts__EBB16A8D70B82B60] PRIMARY KEY ([AlertId]),
    CONSTRAINT [FK__Alerts__AlertTyp__06CD04F7] FOREIGN KEY ([AlertTypeId]) REFERENCES [AlertTypes] ([AlertTypeId]),
    CONSTRAINT [FK__Alerts__DeviceId__05D8E0BE] FOREIGN KEY ([DeviceId]) REFERENCES [IoTDevices] ([DeviceId]),
    CONSTRAINT [FK__Alerts__FarmId__07C12930] FOREIGN KEY ([FarmId]) REFERENCES [Farms] ([FarmId])
);
GO

CREATE TABLE [DeviceReadings] (
    [ReadingId] int NOT NULL IDENTITY,
    [DeviceId] int NOT NULL,
    [ReadingType] nvarchar(50) NOT NULL,
    [Value] float NOT NULL,
    [Timestamp] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__DeviceRe__C80F9C4ECED516C4] PRIMARY KEY ([ReadingId]),
    CONSTRAINT [FK__DeviceRea__Devic__59063A47] FOREIGN KEY ([DeviceId]) REFERENCES [IoTDevices] ([DeviceId])
);
GO

CREATE TABLE [LivestockExpenses] (
    [ExpenseId] int NOT NULL IDENTITY,
    [LivestockId] int NOT NULL,
    [ExpenseType] nvarchar(50) NOT NULL,
    [Amount] float NOT NULL,
    [ExpenseDate] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Livestoc__1445CFD3B68EF710] PRIMARY KEY ([ExpenseId]),
    CONSTRAINT [FK__Livestock__Lives__70DDC3D8] FOREIGN KEY ([LivestockId]) REFERENCES [Livestock] ([LivestockId])
);
GO

CREATE TABLE [LivestockSales] (
    [SaleId] int NOT NULL IDENTITY,
    [LivestockId] int NOT NULL,
    [SaleDate] datetime NULL DEFAULT ((getdate())),
    [Revenue] float NOT NULL,
    [BuyerInfo] nvarchar(255) NULL,
    CONSTRAINT [PK__Livestoc__1EE3C3FF9E0C9548] PRIMARY KEY ([SaleId]),
    CONSTRAINT [FK__Livestock__Lives__74AE54BC] FOREIGN KEY ([LivestockId]) REFERENCES [Livestock] ([LivestockId])
);
GO

CREATE TABLE [TaskHistory] (
    [TaskHistoryId] int NOT NULL IDENTITY,
    [TaskId] int NOT NULL,
    [StatusBefore] nvarchar(50) NULL,
    [StatusAfter] nvarchar(50) NOT NULL,
    [ChangedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__TaskHist__2F15B73C3FA789A4] PRIMARY KEY ([TaskHistoryId]),
    CONSTRAINT [FK__TaskHisto__TaskI__6754599E] FOREIGN KEY ([TaskId]) REFERENCES [Tasks] ([TaskId])
);
GO

CREATE TABLE [AlertUsers] (
    [AlertUserId] int NOT NULL IDENTITY,
    [AlertId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK__AlertUse__9E868E4382438AE9] PRIMARY KEY ([AlertUserId]),
    CONSTRAINT [FK__AlertUser__Alert__0A9D95DB] FOREIGN KEY ([AlertId]) REFERENCES [Alerts] ([AlertId]),
    CONSTRAINT [FK__AlertUser__UserI__0B91BA14] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE INDEX [IX_Alerts_AlertTypeId] ON [Alerts] ([AlertTypeId]);
GO

CREATE INDEX [IX_Alerts_DeviceId] ON [Alerts] ([DeviceId]);
GO

CREATE INDEX [IX_Alerts_FarmId] ON [Alerts] ([FarmId]);
GO

CREATE UNIQUE INDEX [UQ__AlertTyp__AB2F036A77E854D4] ON [AlertTypes] ([AlertTypeName]);
GO

CREATE INDEX [IX_AlertUsers_AlertId] ON [AlertUsers] ([AlertId]);
GO

CREATE INDEX [IX_AlertUsers_UserId] ON [AlertUsers] ([UserId]);
GO

CREATE INDEX [IX_CameraSurveillance_FarmId] ON [CameraSurveillance] ([FarmId]);
GO

CREATE INDEX [IX_DeviceReadings_DeviceId] ON [DeviceReadings] ([DeviceId]);
GO

CREATE INDEX [IX_Farms_OwnerId] ON [Farms] ([OwnerId]);
GO

CREATE INDEX [IX_FarmStaffAssignments_FarmId] ON [FarmStaffAssignments] ([FarmId]);
GO

CREATE INDEX [IX_FarmStaffAssignments_FarmStaffId] ON [FarmStaffAssignments] ([FarmStaffId]);
GO

CREATE INDEX [IX_Inventory_FarmId] ON [Inventory] ([FarmId]);
GO

CREATE INDEX [IX_InventoryTransactions_InventoryId] ON [InventoryTransactions] ([InventoryId]);
GO

CREATE INDEX [IX_IoTDevices_FarmId] ON [IoTDevices] ([FarmId]);
GO

CREATE INDEX [IX_Livestock_FarmId] ON [Livestock] ([FarmId]);
GO

CREATE INDEX [IX_LivestockExpenses_LivestockId] ON [LivestockExpenses] ([LivestockId]);
GO

CREATE INDEX [IX_LivestockSales_LivestockId] ON [LivestockSales] ([LivestockId]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE UNIQUE INDEX [UQ__Permissi__0FFDA357F2115A31] ON [Permissions] ([PermissionName]);
GO

CREATE INDEX [IX_RevenueAndProfitReports_FarmId] ON [RevenueAndProfitReports] ([FarmId]);
GO

CREATE UNIQUE INDEX [UQ__Roles__8A2B61603525D2EC] ON [Roles] ([RoleName]);
GO

CREATE INDEX [IX_TaskHistory_TaskId] ON [TaskHistory] ([TaskId]);
GO

CREATE INDEX [IX_Tasks_AssignedToUserId] ON [Tasks] ([AssignedToUserId]);
GO

CREATE INDEX [IX_Tasks_FarmId] ON [Tasks] ([FarmId]);
GO

CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
GO

CREATE INDEX [IX_UserPermissions_UserId] ON [UserPermissions] ([UserId]);
GO

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO

CREATE UNIQUE INDEX [UQ__Users__536C85E4FE039EB6] ON [Users] ([Username]);
GO

CREATE UNIQUE INDEX [UQ__Users__A9D1053466D7C567] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241029033643_init', N'8.0.10');
GO

COMMIT;
GO

