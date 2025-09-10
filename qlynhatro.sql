-- =========================================================
-- TẠO CƠ SỞ DỮ LIỆU VÀ SỬ DỤNG
-- =========================================================
CREATE DATABASE QLyNhaTro;
GO
USE QLyNhaTro;
GO

-- =========================================================
-- PHÂN QUYỀN VÀ NGƯỜI DÙNG
-- =========================================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE -- Admin, Owner, Tenant
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

-- =========================================================
-- DANH MỤC NHÀ TRỌ / PHÒNG
-- =========================================================
CREATE TABLE Motels (
    MotelId INT IDENTITY(1,1) PRIMARY KEY,
    MotelName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    OwnerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId), -- Role = Owner
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

-- Bảng lookup trạng thái phòng
CREATE TABLE RoomStatuses (
    RoomStatusId INT PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE -- Vacant, Occupied, Maintenance
);
INSERT INTO RoomStatuses VALUES (1, N'Vacant'), (2, N'Occupied'), (3, N'Maintenance');

CREATE TABLE Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    MotelId INT NOT NULL FOREIGN KEY REFERENCES Motels(MotelId),
    RoomName NVARCHAR(100) NOT NULL,
    RentalPrice DECIMAL(18,2) NOT NULL,
    Area DECIMAL(10,2) NULL,
    RoomStatusId INT NOT NULL FOREIGN KEY REFERENCES RoomStatuses(RoomStatusId) DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Room UNIQUE(MotelId, RoomName)
);

-- =========================================================
-- HỢP ĐỒNG
-- =========================================================
CREATE TABLE Contracts (
    ContractId INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL FOREIGN KEY REFERENCES Rooms(RoomId),
    TenantId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId), -- Role = Tenant
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    DepositAmount DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Active', -- Active, Expired, Canceled
    Notes NVARCHAR(500) NULL,
    FileContractPath NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Contract_Dates CHECK (StartDate <= EndDate)
);
CREATE INDEX IX_Contract_Room ON Contracts(RoomId);
CREATE INDEX IX_Contract_Tenant ON Contracts(TenantId);

-- =========================================================
-- QUẢN LÝ HÓA ĐƠN & THANH TOÁN CHI TIẾT
-- =========================================================

-- Bảng lưu các loại tiện ích (phí điện, nước, internet,...)
CREATE TABLE Utilities (
    UtilityId INT IDENTITY(1,1) PRIMARY KEY,
    UtilityName NVARCHAR(100) NOT NULL, -- "Electricity", "Water", "Internet",...
    Unit NVARCHAR(50) NOT NULL -- "kWh", "m3", "month",...
);

-- Bảng lưu chỉ số tiêu thụ tiện ích hàng tháng của từng phòng
CREATE TABLE UtilityReadings (
    ReadingId INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL FOREIGN KEY REFERENCES Rooms(RoomId),
    UtilityId INT NOT NULL FOREIGN KEY REFERENCES Utilities(UtilityId),
    ReadingValue DECIMAL(18,2) NOT NULL,
    ReadingDate DATE NOT NULL,
    CONSTRAINT UQ_UtilityReadings UNIQUE (RoomId, UtilityId, ReadingDate)
);

-- Bảng hóa đơn tổng hợp
CREATE TABLE Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    ContractId INT NOT NULL FOREIGN KEY REFERENCES Contracts(ContractId),
    BillingPeriod CHAR(7) NOT NULL, -- 'YYYY-MM'
    DueDate DATE NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Unpaid', -- Paid, Overdue, Unpaid
    Notes NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Invoice UNIQUE (ContractId, BillingPeriod)
);

-- Bảng chi tiết từng khoản mục trong hóa đơn
CREATE TABLE InvoiceDetails (
    InvoiceDetailId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    Description NVARCHAR(255) NOT NULL, -- e.g., "Rental fee", "Electricity (200 kWh)",...
    Amount DECIMAL(18,2) NOT NULL,
    UtilityReadingId INT NULL FOREIGN KEY REFERENCES UtilityReadings(ReadingId)
);

-- Bảng lưu thông tin thanh toán
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Method NVARCHAR(50) NULL, -- Cash, Bank Transfer, ...
    Status NVARCHAR(50) NOT NULL DEFAULT N'Success' -- Success, Failed, Refunded
);
CREATE INDEX IX_Payment_Invoice ON Payments(InvoiceId);

-- =========================================================
-- THÔNG BÁO VÀ HỖ TRỢ
-- =========================================================
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    SenderUserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ReceiverUserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    RelatedMotelId INT NULL FOREIGN KEY REFERENCES Motels(MotelId),
    RelatedRoomId INT NULL FOREIGN KEY REFERENCES Rooms(RoomId),
    RelatedContractId INT NULL FOREIGN KEY REFERENCES Contracts(ContractId),
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL DEFAULT N'General', -- OwnerToTenant, TenantToOwner, System
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ReadAt DATETIME2 NULL
);
CREATE INDEX IX_Notification_Receiver ON Notifications(ReceiverUserId, IsRead);

-- Báo cáo sự cố / hỏng hóc
CREATE TABLE Incidents (
    IncidentId INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL FOREIGN KEY REFERENCES Rooms(RoomId),
    TenantId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT N'Normal', -- Low, Normal, High, Urgent
    Status NVARCHAR(30) NOT NULL DEFAULT N'Reported', -- Reported, In Progress, Resolved, Canceled
    AttachedImagePath NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ResolvedAt DATETIME2 NULL
);
CREATE INDEX IX_Incident_Room ON Incidents(RoomId);
CREATE INDEX IX_Incident_Tenant ON Incidents(TenantId);
CREATE INDEX IX_Incident_Status ON Incidents(Status);

-- =========================================================
-- LOGS (Audit Trail)
-- =========================================================
CREATE TABLE Logs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL FOREIGN KEY REFERENCES Users(UserId),
    Action NVARCHAR(200) NOT NULL,
    TableName NVARCHAR(100) NULL,
    RecordId INT NULL,
    Detail NVARCHAR(MAX) NULL,
    IPAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_Logs_UserTime ON Logs(UserId, CreatedAt);