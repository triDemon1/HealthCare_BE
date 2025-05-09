﻿/*
CREATED		4/24/2025
MODIFIED		4/26/2025
PROJECT		
MODEL			
COMPANY		
AUTHOR		
VERSION		
DATABASE		MS SQL 2005 
*/
use master
go

drop database if EXISTS HealthCare
go

create database HealthCare
go

use HealthCare
go

CREATE TABLE [USER]
(
	[USERID] INTEGER IDENTITY NOT NULL,
	[USERNAME] VARCHAR(100) NOT NULL, UNIQUE ([USERNAME]),
	[PASSWORDHASH] VARCHAR(255) NOT NULL,
	[EMAIL] VARCHAR(255) NOT NULL, UNIQUE ([EMAIL]),
	[PHONENUMBER] VARCHAR(20) NULL, UNIQUE ([PHONENUMBER]),
	[CREATEDAT]  DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT]  DATETIME2(3) NULL,
	[ISACTIVE] BIT NOT NULL,
	[ROLEID] INTEGER NOT NULL,
PRIMARY KEY ([USERID])
) 
GO

CREATE TABLE [CUSTOMERS]
(
	[CUSTOMERID] INTEGER IDENTITY NOT NULL,
	[USERID] INTEGER NOT NULL,
	[FIRSTNAME] NVARCHAR(100) NOT NULL,
	[LASTNAME] NVARCHAR(100) NULL,
	[DATEOFBIRTH] DATE NULL,
	[GENDER] BIT NULL,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT] DATETIME2(3) NULL,
PRIMARY KEY ([CUSTOMERID])
) 
GO

CREATE TABLE [ADDRESSES]
(
	[ADDRESSID] INTEGER IDENTITY NOT NULL,
	[CUSTOMERID] INTEGER NOT NULL,
	[COUNTRY] VARCHAR(100) NULL,
	[STREET] VARCHAR(255) NULL,
	[WARD] VARCHAR(100) NULL,
	[DISTRICT] VARCHAR(100) NULL,
	[CITY] VARCHAR(100) NULL,
PRIMARY KEY ([ADDRESSID])
) 
GO

CREATE TABLE [SERVICEGROUPS]
(
	[SERVICEGROUPID] INTEGER IDENTITY NOT NULL,
	[NAME] NVARCHAR(100) NULL,
PRIMARY KEY ([SERVICEGROUPID])
) 
GO

CREATE TABLE [SERVICES]
(
	[SERVICEID] INTEGER IDENTITY NOT NULL,
	[SERVICEGROUPID] INTEGER NOT NULL,
	[TYPEID] INTEGER  NOT NULL,
	[NAME] NVARCHAR(255) NOT NULL,
	[DESCRIPTION] NVARCHAR(MAX) NULL,
	[DURATION] INTEGER NULL,
	[PRICE] DECIMAL(15,2) NULL,
	[ISACTIVE] BIT NULL,
	[CREATEDAT]  DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT]  DATETIME2(3) NULL,
PRIMARY KEY ([SERVICEID])
) 
GO

CREATE TABLE [STAFF]
(
	[STAFFID] INTEGER IDENTITY NOT NULL,
	[USERID] INTEGER NOT NULL,
	[FIRSTNAME] NVARCHAR(100) NOT NULL,
	[LASTNAME] NVARCHAR(100) NOT NULL,
	[PHONENUMBER] VARCHAR(20) NULL, UNIQUE ([PHONENUMBER]),
	[SKILLS] NVARCHAR(MAX) NULL,
	[EXPYEAR] INTEGER NULL,
	[ISAVAILABLE] BIT NULL DEFAULT 1,
	[CREATEAT] DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT] DATETIME2(3) NULL,
PRIMARY KEY ([STAFFID])
) 
GO

CREATE TABLE [SUBJECTS]
(
	[SUBJECTID] INTEGER IDENTITY NOT NULL,
	[CUSTOMERID] INTEGER NOT NULL,
	[TYPEID] INTEGER NOT NULL,
	[NAME] NVARCHAR(150) NULL,
	[DATEOFBIRTH] DATE NULL,
	[GENDER] BIT NULL,
	[MEDICALNOTES] NVARCHAR(MAX) NULL,
	[CREATEDAT]  DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT]  DATETIME2(3) NULL,
	[IMAGEURL] VARCHAR(2048) NULL,
PRIMARY KEY ([SUBJECTID])
) 
GO

CREATE TABLE [ROLES]
(
	[ROLEID] INTEGER NOT NULL,
	[ROLENAME] NVARCHAR(50) NOT NULL, UNIQUE ([ROLENAME]),
PRIMARY KEY ([ROLEID])
) 
GO

CREATE TABLE [CATEGORIES]
(
	[CATEGORYID] INTEGER IDENTITY NOT NULL,
	[NAME] NVARCHAR(150) NOT NULL,
	[PARENTCATOREGORYID] INTEGER NULL,
	[DESCRIPTION] NVARCHAR(MAX) NULL,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
PRIMARY KEY ([CATEGORYID])
) 
GO

CREATE TABLE [PRODUCTS]
(
	[PRODUCTID] INTEGER IDENTITY NOT NULL,
	[CATEGORYID] INTEGER NOT NULL,
	[NAME] NVARCHAR(255) NOT NULL,
	[DESCRIPTION] NVARCHAR(MAX) NULL,
	[PRICE] DECIMAL(15,2) NOT NULL,
	[STOCKQUANTITY] INTEGER  NOT NULL DEFAULT 0,
	[IMAGEURL] VARCHAR(2048) NULL,
	[SKU] VARCHAR(100) NULL, UNIQUE ([SKU]),
	[ISACTIVE] BIT NOT NULL DEFAULT 1,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT] DATETIME2(3) NULL,
PRIMARY KEY ([PRODUCTID])
) 
GO

CREATE TABLE [BOOKINGS]
(
	[BOOKINGID] INTEGER IDENTITY NOT NULL,
	[ADDRESSID] INTEGER NOT NULL,
	[SUBJECTID] INTEGER NOT NULL,
	[STAFFID] INTEGER NULL,
	[STATUSID] INTEGER NOT NULL,
	[CUSTOMERID] INTEGER NOT NULL,
	[SERVICEID] INTEGER NOT NULL,
	[PRICEATBOOKING] DECIMAL(15,2) NOT NULL,
	[SCHEDULEDSTARTTIME] DATETIME2(3) NOT NULL,
	[SCHEDULEDENDTIME]  DATETIME2(3) NOT NULL,
	[ACTUALSTARTTIME]  DATETIME2(3) NULL,
	[ACTUALENDTIME]  DATETIME2(3) NULL,
	[NOTES] NVARCHAR(MAX) NULL,
	[CREATEDAT]  DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT]  DATETIME2(3) NULL,
PRIMARY KEY ([BOOKINGID])
) 
GO

CREATE TABLE [PAYMENTMETHOD]
(
	[PAYMENTMETHODID] INTEGER IDENTITY NOT NULL,
	[METHODNAME] NVARCHAR(100) NOT NULL, UNIQUE ([METHODNAME]),
PRIMARY KEY ([PAYMENTMETHODID])
) 
GO

CREATE TABLE [PAYMENTSTATUS]
(
	[PAYMENTSTATUSID] INTEGER IDENTITY NOT NULL,
	[STATUSNAME] NVARCHAR(100) NOT NULL, UNIQUE ([STATUSNAME]),
PRIMARY KEY ([PAYMENTSTATUSID])
) 
GO

CREATE TABLE [BOOKINGSTATUS]
(
	[STATUSID] INTEGER IDENTITY NOT NULL,
	[STATUSNAME] NVARCHAR(100) NOT NULL, UNIQUE ([STATUSNAME]),
PRIMARY KEY ([STATUSID])
) 
GO

CREATE TABLE [ORDERS]
(
	[ORDERID] INTEGER IDENTITY NOT NULL,
	[CUSTOMERID] INTEGER NOT NULL,
	[ORDERSTATUSID] INTEGER NOT NULL,
	[ADDRESSID] INTEGER NOT NULL,
	[ORDERDATE] DATETIME2(3) NULL DEFAULT GETDATE(),
	[TOTALAMOUNT] DECIMAL(15,2) NOT NULL DEFAULT 0,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT] DATETIME2(3) NULL,
PRIMARY KEY ([ORDERID])
) 
GO

CREATE TABLE [ORDERSTATUS]
(
	[ORDERSTATUSID] INTEGER IDENTITY NOT NULL,
	[STATUSNAME] NVARCHAR(100) NOT NULL, UNIQUE ([STATUSNAME]),
PRIMARY KEY ([ORDERSTATUSID])
) 
GO

CREATE TABLE [ORDERDETAILS]
(
	[ORDERDETAILID] INTEGER IDENTITY NOT NULL,
	[PRODUCTID] INTEGER NOT NULL,
	[ORDERID] INTEGER NOT NULL,
	[QUANTITY] INTEGER NOT NULL,
	[PRICEATPURCHASE] DECIMAL(15,2) NOT NULL,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
PRIMARY KEY ([ORDERDETAILID])
) 
GO

CREATE TABLE [SUBJECTTYPES]
(
	[TYPEID] INTEGER IDENTITY NOT NULL,
	[SUBJECTNAME] NVARCHAR(100) NOT NULL, UNIQUE ([SUBJECTNAME]),
PRIMARY KEY ([TYPEID])
) 
GO

CREATE TABLE [PAYMENTS]
(
	[PAYMENTID] INTEGER IDENTITY NOT NULL,
	[AMOUNT] DECIMAL(15,2) NOT NULL,
	[PAYMENTDATE] DATETIME2(3) NULL DEFAULT GETDATE(),
	[TRANSACTIONID] INTEGER NOT NULL,
	[TRANSACTIONID_PaymentGateway] VARCHAR(255) NULL,
	[CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
	[UPDATEDAT] DATETIME2(3) NULL,
PRIMARY KEY ([PAYMENTID])
) 
GO

CREATE TABLE [TRANSACTIONS]
(
    [TransactionID] INTEGER IDENTITY NOT NULL,
    [CustomerID] INTEGER NOT NULL,
    [TransactionDate] DATETIME2(3) NULL DEFAULT GETDATE(),
    [TotalAmount] DECIMAL(15,2) NOT NULL,
    [PaymentMethodID] INTEGER NOT NULL,
    [PaymentStatusID] INTEGER NOT NULL,
    [TransactionID_PaymentGateway] VARCHAR(255) NULL,
    [CREATEDAT] DATETIME2(3) NULL DEFAULT GETDATE(),
    [UPDATEDAT] DATETIME2(3) NULL,
    PRIMARY KEY ([TransactionID]),
    FOREIGN KEY ([CustomerID]) REFERENCES [CUSTOMERS] ([CUSTOMERID]),
    FOREIGN KEY ([PaymentMethodID]) REFERENCES [PAYMENTMETHOD] ([PAYMENTMETHODID]),
    FOREIGN KEY ([PaymentStatusID]) REFERENCES [PAYMENTSTATUS] ([PAYMENTSTATUSID])
)
GO

CREATE TABLE [TRANSACTION_ITEMS]
(
    [TransactionItemID] INTEGER IDENTITY NOT NULL,
    [TransactionID] INTEGER NOT NULL,
    [OrderID] INTEGER NULL,
    [BookingID] INTEGER NULL,
    [ItemAmount] DECIMAL(15,2) NOT NULL,
    PRIMARY KEY ([TransactionItemID]),
    FOREIGN KEY ([TransactionID]) REFERENCES [TRANSACTIONS] ([TransactionID]),
    FOREIGN KEY ([OrderID]) REFERENCES [ORDERS] ([ORDERID]),
    FOREIGN KEY ([BookingID]) REFERENCES [BOOKINGS] ([BOOKINGID]),
    -- Thêm ràng buộc để đảm bảo chỉ có một trong OrderID hoặc BookingID có giá trị
    CONSTRAINT [CHK_TRANSACTION_ITEM_SOURCE] CHECK (([OrderID] IS NOT NULL AND [BookingID] IS NULL) OR ([OrderID] IS NULL AND [BookingID] IS NOT NULL))
)
GO


CREATE TABLE [dbo].[RefreshTokens]
(
    [Id] INT IDENTITY(1,1) NOT NULL, -- Khóa chính tự tăng
    [Token] VARCHAR(256) NOT NULL, -- Chuỗi Refresh Token
    [CreatedAt] DATETIME2(3) NOT NULL, -- Thời gian tạo token
    [ExpiresAt] DATETIME2(3) NOT NULL, -- Thời gian hết hạn của token
    [RevokedAt] DATETIME2(3) NULL, -- Thời gian token bị thu hồi (NULL nếu chưa thu hồi)
    [CreatedByIp] VARCHAR(45) NULL, -- Địa chỉ IP khi tạo token (cho IPv4 hoặc IPv6)
    [RevokedByIp] VARCHAR(45) NULL, -- Địa chỉ IP khi thu hồi token (NULL nếu chưa thu hồi)
    [ReplacedByToken] VARCHAR(256) NULL, -- Token được thay thế bởi token nào (NULL nếu đây là token cuối cùng)
    [UserId] INT NOT NULL, -- Khóa ngoại liên kết với bảng USER

    -- Định nghĩa khóa chính
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

SELECT name
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('[SERVICES]')
  AND referenced_object_id = OBJECT_ID('[SUBJECTTYPES]');

ALTER TABLE [dbo].[RefreshTokens] WITH CHECK ADD CONSTRAINT [FK_RefreshTokens_User_UserId]
FOREIGN KEY ([UserId])
REFERENCES [dbo].[USER] ([USERID])
ON DELETE NO ACTION; -- Hoặc ON DELETE CASCADE nếu bạn muốn xóa token khi user bị xóa
GO
ALTER TABLE STAFF
ADD CONSTRAINT UQ_STAFF_USERID UNIQUE (USERID);
ALTER TABLE [SERVICES] DROP CONSTRAINT FK__SERVICES__SUBJEC__3C34F16F; -- Thay thế bằng tên ràng buộc thực tế bạn muốn xóa
GO
ALTER TABLE [SERVICES] ADD [SUBJECTTYPEID] INTEGER NOT NULL; -- Thêm cột SUBJECTTYPEID
GO
ALTER TABLE [SERVICES] ADD FOREIGN KEY ([SUBJECTTYPEID]) REFERENCES [SUBJECTTYPES] ([TYPEID]) ON UPDATE NO ACTION ON DELETE NO ACTION ;
GO
ALTER TABLE [PAYMENTS] ADD FOREIGN KEY ([TransactionID]) REFERENCES [TRANSACTIONS] ([TransactionID]); -- Thêm khóa ngoại liên kết đến TRANSACTIONS
GO
ALTER TABLE [STAFF] ADD  FOREIGN KEY([USERID]) REFERENCES [USER] ([USERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [CUSTOMERS] ADD  FOREIGN KEY([USERID]) REFERENCES [USER] ([USERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ADDRESSES] ADD  FOREIGN KEY([CUSTOMERID]) REFERENCES [CUSTOMERS] ([CUSTOMERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [SUBJECTS] ADD  FOREIGN KEY([CUSTOMERID]) REFERENCES [CUSTOMERS] ([CUSTOMERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ORDERS] ADD  FOREIGN KEY([CUSTOMERID]) REFERENCES [CUSTOMERS] ([CUSTOMERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([CUSTOMERID]) REFERENCES [CUSTOMERS] ([CUSTOMERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ORDERS] ADD  FOREIGN KEY([ADDRESSID]) REFERENCES [ADDRESSES] ([ADDRESSID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([ADDRESSID]) REFERENCES [ADDRESSES] ([ADDRESSID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [SERVICES] ADD  FOREIGN KEY([SERVICEGROUPID]) REFERENCES [SERVICEGROUPS] ([SERVICEGROUPID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([SERVICEID]) REFERENCES [SERVICES] ([SERVICEID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([STAFFID]) REFERENCES [STAFF] ([STAFFID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([SUBJECTID]) REFERENCES [SUBJECTS] ([SUBJECTID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [USER] ADD  FOREIGN KEY([ROLEID]) REFERENCES [ROLES] ([ROLEID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [PRODUCTS] ADD  FOREIGN KEY([CATEGORYID]) REFERENCES [CATEGORIES] ([CATEGORYID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [CATEGORIES] ADD  FOREIGN KEY([CATEGORYID]) REFERENCES [CATEGORIES] ([CATEGORYID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ORDERDETAILS] ADD  FOREIGN KEY([PRODUCTID]) REFERENCES [PRODUCTS] ([PRODUCTID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD  FOREIGN KEY([STATUSID]) REFERENCES [BOOKINGSTATUS] ([STATUSID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ORDERDETAILS] ADD  FOREIGN KEY([ORDERID]) REFERENCES [ORDERS] ([ORDERID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [ORDERS] ADD  FOREIGN KEY([ORDERSTATUSID]) REFERENCES [ORDERSTATUS] ([ORDERSTATUSID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [SUBJECTS] ADD  FOREIGN KEY([TYPEID]) REFERENCES [SUBJECTTYPES] ([TYPEID])  ON UPDATE NO ACTION ON DELETE NO ACTION 
GO
ALTER TABLE [BOOKINGS] ADD [PaymentStatusID] INTEGER NULL;
GO
ALTER TABLE [BOOKINGS] ADD FOREIGN KEY ([PaymentStatusID]) REFERENCES [PAYMENTSTATUS] ([PAYMENTSTATUSID]);
GO
ALTER TABLE [CUSTOMERS]
ADD CONSTRAINT UQ_CUSTOMERS_USERID  UNIQUE ([USERID]);

ALTER TABLE ADDRESSES
DROP CONSTRAINT FK__ADDRESSES__CUSTO__160F4887;

-- B2: Thêm lại constraint mới với CASCADE
ALTER TABLE ADDRESSES
ADD CONSTRAINT FK_Addresses_Customers
FOREIGN KEY (CustomerId) 
REFERENCES Customers(CustomerId) 
ON DELETE CASCADE;



--TRIGGER
CREATE TRIGGER trg_UpdateTransactionPaymentStatus
ON BOOKINGS
AFTER UPDATE
AS
BEGIN
    IF UPDATE(PaymentStatusID)
    BEGIN
        UPDATE T
        SET T.PaymentStatusID = I.PaymentStatusID
        FROM TRANSACTIONS T
        INNER JOIN TRANSACTION_ITEMS TI ON T.TransactionID = TI.TransactionID
        INNER JOIN INSERTED I ON TI.BookingID = I.BOOKINGID;
    END
END;
GO



SELECT * FROM [BOOKINGSTATUS]











-- thêm dữ liệu cho bảng roles
INSERT INTO [ROLES] ([ROLEID], [ROLENAME]) VALUES
(1, N'Admin'),
(2, N'Customer'),
(3, N'Staff');
GO
-- Thêm dữ liệu mẫu vào bảng SUBJECTTYPES
SELECT * FROM [SUBJECTTYPES]
PRINT 'Inserting data into SUBJECTTYPES...';
INSERT INTO [SUBJECTTYPES] ([SUBJECTNAME]) VALUES
(N'Elderly'),
(N'Child'),
(N'Pets');
GO

-- Thêm dữ liệu mẫu vào bảng BOOKINGSTATUS
PRINT 'Inserting data into BOOKINGSTATUS...';
INSERT INTO [BOOKINGSTATUS] ([STATUSNAME]) VALUES
(N'Pending'),
(N'Confirmed'),
(N'Completed'),
(N'Cancelled');
GO

SELECT * FROM [ORDERSTATUS]
-- Thêm dữ liệu mẫu vào bảng ORDERSTATUS
PRINT 'Inserting data into ORDERSTATUS...';
INSERT INTO [ORDERSTATUS] ([STATUSNAME]) VALUES
(N'Pending'),
(N'Processing'),
(N'Shipped'),
(N'Delivered'),
(N'Cancelled');
GO

SELECT * FROM [PAYMENTMETHOD]
-- Thêm dữ liệu mẫu vào bảng PAYMENTMETHOD
PRINT 'Inserting data into PAYMENTMETHOD...';
INSERT INTO [PAYMENTMETHOD] ([METHODNAME]) VALUES
(N'Credit Card'),
(N'Bank Transfer'),
(N'Cash'),
(N'E-wallet');
GO

SELECT * FROM [PAYMENTSTATUS]
-- Thêm dữ liệu mẫu vào bảng PAYMENTSTATUS
PRINT 'Inserting data into PAYMENTSTATUS...';
INSERT INTO [PAYMENTSTATUS] ([STATUSNAME]) VALUES
(N'Pending'),
(N'Completed'),
(N'Failed'),
(N'Refunded');
GO
INSERT INTO [PAYMENTSTATUS] ([STATUSNAME]) VALUES (N'Cancelled')
SELECT * FROM [USER]
-- Thêm dữ liệu mẫu vào bảng USER (Liên kết với ROLES)
PRINT 'Inserting data into USER...';
INSERT INTO [USER] ([USERNAME], [PASSWORDHASH], [EMAIL], [PHONENUMBER], [ISACTIVE], [ROLEID]) VALUES
('tri', '123', 'dinhtri205@gmail.com', '0123456789', 1, 2), -- Role: Customer
('hung', '1234', 'hungbui@example.com', '0123456788', 1, 2), -- Role: Customer
('staff1', 'hashed_password_staff1', 'staff1@example.com', '0903333333', 1, 3), -- Role: Staff
('admin1', 'hashed_password_admin1', 'admin1@example.com', '0904444444', 1, 1); -- Role: Admin
GO
-- Thêm dữ liệu mẫu vào bảng CUSTOMERS (Liên kết với USER)
PRINT 'Inserting data into CUSTOMERS...';
INSERT INTO [CUSTOMERS] ([USERID], [FIRSTNAME], [LASTNAME], [DATEOFBIRTH], [GENDER], [CREATEDAT]) VALUES
(5, N'Vũ', N'Đình Trí', '2003-05-20', 1, GETDATE()), -- USERID 1 is customer1
(6, N'Bùi', N'Quang Hưng', '2003-08-01', 0, GETDATE()),-- USERID 2 is customer2
(9, N'Trí', N'Messi', '2003-08-01', 1, GETDATE());
GO
INSERT INTO [CUSTOMERS] ([USERID], [FIRSTNAME], [LASTNAME], [DATEOFBIRTH], [GENDER], [CREATEDAT]) VALUES (9, N'Trí', N'Messi', '2003-08-01', 1, GETDATE())
-- Thêm dữ liệu mẫu vào bảng ADDRESSES (Liên kết với CUSTOMERS)
PRINT 'Inserting data into ADDRESSES...';
INSERT INTO [ADDRESSES] ([CUSTOMERID], [COUNTRY], [STREET], [WARD], [DISTRICT], [CITY]) VALUES
(16, N'Vietnam', N'Đường 70', N'Phường Tây Tựu', N'Quận Bắc Từ Liêm', N'Hà Nội'), -- CUSTOMERID 1
(17, N'Vietnam', N'Đường B', N'Phường Tây Tựu', N'Quận Bắc Từ Liêm', N'Hà Nội'); -- CUSTOMERID 2
GO
-- Thêm dữ liệu mẫu vào bảng SERVICEGROUPS (Loại dịch vụ chung)
PRINT 'Updating data for SERVICEGROUPS...';
INSERT INTO [SERVICEGROUPS] ([NAME]) VALUES
(N'Chăm sóc cơ bản'), -- Có thể áp dụng cho cả 3 đối tượng
(N'Y tế và sức khỏe'), -- Áp dụng cho Người già, Trẻ em, Thú cưng
(N'Chăm sóc trẻ sơ sinh'),
(N'Chăm sóc trẻ lớn'),
(N'Chó'),
(N'Mèo')
GO

-- Thêm dữ liệu mẫu vào bảng CATEGORIES (Phân loại sản phẩm/dịch vụ theo đối tượng và loại)
PRINT 'Updating data for CATEGORIES...';
-- Xóa dữ liệu cũ nếu cần
-- DELETE FROM [CATEGORIES];
INSERT INTO [CATEGORIES] ([NAME], [PARENTCATOREGORYID], [DESCRIPTION], [CREATEDAT]) VALUES
(N'Sản phẩm cho Người Già', NULL, N'Các sản phẩm dành cho người già', GETDATE()), -- CATEGORYID 1 (Ví dụ)
(N'Sản phẩm cho Trẻ Em', NULL, N'Các sản phẩm dành cho trẻ em', GETDATE()),    -- CATEGORYID 2 (Ví dụ)
(N'Sản phẩm cho Thú Cưng', NULL, N'Các sản phẩm dành cho thú cưng', GETDATE()); -- CATEGORYID 3 (Ví dụ)
GO
PRINT 'Inserting data into SERVICES...';
 DELETE FROM [SERVICES]; -- Xóa dữ liệu cũ nếu cần
INSERT INTO [SERVICES] ([SUBJECTTYPEID], [SERVICEGROUPID], [NAME], [DESCRIPTION], [DURATION], [PRICE], [ISACTIVE], [CREATEDAT]) VALUES
-- Dịch vụ cho Thú Cưng (SUBJECTTYPEID = 3)
(3, 19, N'Tắm và sấy khô cho chó', N'Dịch vụ tắm và sấy khô cơ bản', 60, 200000.00, 1, GETDATE()), -- SERVICEID 1 (Ví dụ), Subject: Thú Cưng, Group: Làm đẹp & Vệ sinh
(3, 19, N'Huấn luyện cơ bản', N'Huấn luyện định kỳ', 45, 350000.00, 1, GETDATE()), -- SERVICEID 2 (Ví dụ), Subject: Thú Cưng, Group: Chăm sóc sức khỏe
(3, 19, N'Chăm sóc tại nhà', N'Hướng dẫn chăm sóc tại nhà', 65, 950000.00, 1, GETDATE()), -- SERVICEID 2 (Ví dụ), Subject: Thú Cưng, Group: Chăm sóc sức khỏe
(3, 20, N'Vệ sinh khay cát', N'Cách vệ sinh khay cát', 75, 750000.00, 1, GETDATE()), -- SERVICEID 2 (Ví dụ), Subject: Thú Cưng, Group: Chăm sóc sức khỏe
(3, 20, N'Chải lông', N'Chải lông', 35, 250000.00, 1, GETDATE()), -- SERVICEID 2 (Ví dụ), Subject: Thú Cưng, Group: Chăm sóc sức khỏe
(3, 20, N'Kiểm tra sức khỏe', N'Kiểm tra sức khỏe định kỳ', 45, 550000.00, 1, GETDATE()), -- SERVICEID 2 (Ví dụ), Subject: Thú Cưng, Group: Chăm sóc sức khỏe

-- Dịch vụ cho Người Già (SUBJECTTYPEID = 1)
(1, 15, N'Tắm rửa (Người Già)', N'abc', 120, 500000.00, 1, GETDATE()), -- SERVICEID 3 (Ví dụ), Subject: Người Già, Group: Hỗ trợ sinh hoạt
(1, 15, N'Vệ Sinh Cá Nhân (Người Già)', N'Y tá đến kiểm tra sức khỏe', 60, 400000.00, 1, GETDATE()), -- SERVICEID 4 (Ví dụ), Subject: Người Già, Group: Chăm sóc sức khỏe
(1, 15, N'Cho ăn (Người Già)', N'abc', 120, 500000.00, 1, GETDATE()),
(1, 16, N'The dõi thuốc (Người Già)', N'ac', 120, 500000.00, 1, GETDATE()),
(1, 16, N'Tập thể dục (Người Già)', N'kkkk', 120, 500000.00, 1, GETDATE()),
(1, 16, N'Kiểm tra định kỳ (Người Già)', N'123', 120, 500000.00, 1, GETDATE()),


-- Dịch vụ cho Trẻ Em (SUBJECTTYPEID = 2)
(2, 17, N'Tắm cho bé', N'Trông nom và chăm sóc trẻ', 180, 600000.00, 1, GETDATE()), -- SERVICEID 5 (Ví dụ), Subject: Trẻ Em, Group: Hỗ trợ sinh hoạt

(2, 17, N'Cho bé bú', N'hhhhh', 120, 500000.00, 1, GETDATE()),
(2, 17, N'Thay tã', N'Hỗ hhhh', 120, 500000.00, 1, GETDATE()),
(2, 17, N'Ngủ đúng giờ', N'11111', 120, 500000.00, 1, GETDATE()),
(2, 18, N'Chuẩn bị bữa ăn', N'11223123', 120, 500000.00, 1, GETDATE()),
(2, 18, N'Hỗ trợ học tập', N'223232y', 120, 500000.00, 1, GETDATE()),
(2, 18, N'Đưa đón', N'bbbbb', 120, 500000.00, 1, GETDATE())
GO

INSERT INTO [PRODUCTS] ([CATEGORYID], [NAME], [DESCRIPTION], [PRICE], [STOCKQUANTITY], [IMAGEURL], [SKU], [ISACTIVE], [CREATEDAT]) VALUES
(3, N'Thức ăn cho mèo loại A', N'Thức ăn dinh dưỡng cao', 70000.00, 500, 'url_to_image_dog_food', 'SKU-DOG-123', 1, GETDATE()), -- Liên kết với 'Sản phẩm cho Thú Cưng'
(3, N'Xích chó', N'Xích inox', 90000.00, 100, 'url_to_image_cat_litter', 'SKU-CAT-123', 1, GETDATE()), -- Liên kết với 'Sản phẩm cho Thú Cưng'
(1, N'Sửa Ensure', N'Sữa bổ sung canxi', 600000.00, 250, 'url_to_image_old_milk', 'SKU-OLD-321', 1, GETDATE()), -- Liên kết với 'Sản phẩm cho Người Già'
(2, N'Đồ chơi cho trẻ', N'Bỉm siêu thấm', 350000.00, 350, 'url_to_image_baby_diaper', 'SKU-BABY-258', 1, GETDATE()); -- Liên kết với 'Sản phẩm cho Trẻ Em'
GO

-- chưa động đến
-- Thêm dữ liệu mẫu vào bảng STAFF (Liên kết với USER)
PRINT 'Inserting data into STAFF...';
-- DELETE FROM [STAFF]; -- Xóa dữ liệu cũ nếu cần
INSERT INTO [STAFF] ([USERID], [FIRSTNAME], [LASTNAME], [PHONENUMBER], [SKILLS], [EXPYEAR], [ISAVAILABLE], [CREATEAT]) VALUES
(3, N'Phạm', N'Văn C', '0903333333', N'Grooming, Healthcare', 5, 1, GETDATE()); -- STAFFID 1, USERID 3 is staff1
GO

-- Thêm dữ liệu mẫu vào bảng SUBJECTS (Liên kết với CUSTOMERS, SUBJECTTYPES)
PRINT 'Inserting data into SUBJECTS...';

INSERT INTO [SUBJECTS] ([CUSTOMERID], [TYPEID], [NAME], [DATEOFBIRTH], [GENDER], [MEDICALNOTES], [CREATEDAT], [IMAGEURL]) VALUES
(8, 3, N'Lucky (Chó)', '2018-08-01', 1, N'Không có vấn đề sức khỏe', GETDATE(), 'url_to_lucky_image'), -- SUBJECTID 1, CUSTOMERID 1, TYPEID: Thú Cưng
(9, 3, N'Mimi (Mèo)', '2020-03-10', 0, N'Dị ứng nhẹ', GETDATE(), 'url_to_mimi_image'), -- SUBJECTID 2, CUSTOMERID 2, TYPEID: Thú Cưng
(8, 1, N'Bà Mai', '1950-01-20', 0, N'Tiểu đường', GETDATE(), 'url_to_ba_mai_image'), -- SUBJECTID 3, CUSTOMERID 1, TYPEID: Người Già
(9, 2, N'Bé An', '2022-07-05', 0, N'Sức khỏe tốt', GETDATE(), 'url_to_be_an_image'); -- SUBJECTID 4, CUSTOMERID 2, TYPEID: Trẻ Em
GO
DELETE FROM [SUBJECTS]
SELECT * FROM [SERVICES]
SELECT * FROM [SUBJECTS]
SELECT * FROM [RefreshTokens]
SELECT * FROM BOOKINGS
SELECT * FROM STAFF
SELECT * FROM [USER]
SELECT * FROM PRODUCTS
SELECT * FROM PAYMENTMETHOD
	UPDATE PAYMENTSTATUS
	SET STATUSNAME = 'MoMo'
	WHERE  PAYMENTMETHODID = '2'

SELECT * FROM [TRANSACTIONS]
SELECT * FROM PAYMENTSTATUS
SELECT * FROM [TRANSACTION_ITEMS]





















SET QUOTED_IDENTIFIER ON
GO


SET QUOTED_IDENTIFIER OFF
GO


