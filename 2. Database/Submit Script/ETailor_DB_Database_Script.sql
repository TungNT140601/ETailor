USE [master]
GO
/****** Object:  Database [ETailor_DB]    Script Date: 5/30/2024 3:55:14 PM ******/
CREATE DATABASE [ETailor_DB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ETailor_DB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ETailor_DB.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ETailor_DB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ETailor_DB_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ETailor_DB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ETailor_DB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ETailor_DB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ETailor_DB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ETailor_DB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ETailor_DB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ETailor_DB] SET ARITHABORT OFF 
GO
ALTER DATABASE [ETailor_DB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [ETailor_DB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ETailor_DB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ETailor_DB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ETailor_DB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ETailor_DB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ETailor_DB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ETailor_DB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ETailor_DB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ETailor_DB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ETailor_DB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ETailor_DB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ETailor_DB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ETailor_DB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ETailor_DB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ETailor_DB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ETailor_DB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ETailor_DB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ETailor_DB] SET  MULTI_USER 
GO
ALTER DATABASE [ETailor_DB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ETailor_DB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ETailor_DB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ETailor_DB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ETailor_DB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ETailor_DB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [ETailor_DB] SET QUERY_STORE = ON
GO
ALTER DATABASE [ETailor_DB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ETailor_DB]
GO
/****** Object:  UserDefinedTableType [dbo].[MaterialStageType]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE TYPE [dbo].[MaterialStageType] AS TABLE(
	[MaterialId] [nvarchar](30) NULL,
	[Value] [decimal](18, 3) NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[CalculateNumOfDateToFinish]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[CalculateNumOfDateToFinish](
    @ProductId NVARCHAR(30)
)
RETURNS INT
BEGIN
    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderCreateTime DATETIME;
    DECLARE @ProductTemplateId NVARCHAR(30);
    DECLARE @CategoryId NVARCHAR(30);
    DECLARE @AveDateForComplete INT;
    DECLARE @TotalNotFinishProduct INT;
    DECLARE @TotalStaffHasMastery INT;
    DECLARE @ProductCreateTime DATETIME;
    DECLARE @TotalDaysNeeded FLOAT = 0;
    DECLARE @AveDate FLOAT;
    DECLARE @EffectiveStaff INT;
    DECLARE @ProductCount INT;
    DECLARE @StaffMakerId NVARCHAR(30);

    SELECT @OrderId = O.Id, @ProductTemplateId = P.ProductTemplateId, @OrderCreateTime = O.CreatedTime, @ProductCreateTime = P.CreatedTime, @StaffMakerId = StaffMakerId
    FROM Product P INNER JOIN [Order] O ON (P.OrderId = O.Id AND P.IsActive = 1 AND P.Status > 0 AND P.Status < 5 AND O.Status > 0 AND O.Status < 8)
    WHERE P.Id = @ProductId;

    IF @OrderId IS NULL
        RETURN -1;
    --/'Không tìm thấy sản phẩm'

    IF NOT EXISTS( SELECT 1
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0)
        RETURN -2; --/'Không tìm thấy hóa đơn'
    ELSE
    SELECT @OrderCreateTime = CreatedTime
    FROM [Order]
    WHERE Id= @OrderId

    SELECT @CategoryId = CategoryId, @AveDateForComplete = AveDateForComplete
    FROM ProductTemplate
    WHERE Id = @ProductTemplateId

    IF @CategoryId IS NULL
        RETURN -3;
    --/'Không tìm thấy bản mẫu'

    SELECT @TotalStaffHasMastery = COUNT(*)
    FROM Mastery
    WHERE CategoryId = @CategoryId AND StaffId IN (
        SELECT Id
        FROM Staff
        WHERE IsActive = 1
    );

    SELECT @TotalNotFinishProduct = COUNT(*)
    FROM Product P INNER JOIN [Order] O ON P.OrderId = O.Id
    WHERE P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND P.ProductTemplateId IN (
            SELECT Id
        FROM [dbo].[ProductTemplate]
        WHERE CategoryId = @CategoryId
        )
        AND ((O.[Status] > 0 AND O.[Status] < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)


    IF @TotalStaffHasMastery IS NULL OR @TotalStaffHasMastery = 0
    BEGIN
        IF @StaffMakerId IS NULL
        RETURN -4;
    END;
    --/'Không có staff phù hợp'

    DECLARE CategoryCursor CURSOR FOR
        SELECT PT.CategoryId
    FROM ProductTemplate PT INNER JOIN Product P ON PT.Id = P.ProductTemplateId
        INNER JOIN [Order] O ON P.OrderId = O.Id
    WHERE ((P.Status > 0 AND P.Status < 5
        AND P.IsActive = 1 AND P.CreatedTime < @ProductCreateTime) OR P.Id = @ProductId)
        AND ((O.Status > 0 AND O.Status < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)
    GROUP BY PT.CategoryId;

    OPEN CategoryCursor;

    FETCH NEXT FROM CategoryCursor INTO @CategoryId;

    WHILE @@FETCH_STATUS = 0
        BEGIN
        
        DECLARE DataProductCursor CURSOR FOR
            SELECT PT.AveDateForComplete, COUNT(*) AS ProductCount
            FROM Product P INNER JOIN ProductTemplate PT ON P.ProductTemplateId = PT.Id
                INNER JOIN [Order] O ON P.OrderId = O.Id
            WHERE PT.CategoryId = @CategoryId
                AND ((P.Status > 0 AND P.Status < 5
                AND P.IsActive = 1 AND P.CreatedTime < @ProductCreateTime) OR P.Id = @ProductId)
                AND ((O.Status > 0 AND O.Status < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)
            GROUP BY PT.AveDateForComplete
        

        OPEN DataProductCursor;
        FETCH NEXT FROM DataProductCursor INTO  @AveDate,@ProductCount

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Calculate effective staff for the current category
            SELECT @EffectiveStaff = COUNT(DISTINCT M.StaffId)
            FROM Mastery M
                JOIN Staff S ON M.StaffId = S.Id
            WHERE M.CategoryId = @CategoryId AND S.IsActive = 1;

            -- Accumulate total days needed, adjusted for effective staff
            IF(@ProductCount > @EffectiveStaff)
            SET @TotalDaysNeeded += @AveDate * (@ProductCount / @EffectiveStaff + 1);
            -- SET @TotalDaysNeeded += (@AveDate / NULLIF(@EffectiveStaff, 0));
            ELSE
            SET @TotalDaysNeeded += @AveDate;
            
        FETCH NEXT FROM DataProductCursor INTO  @AveDate,@ProductCount
        END;
        CLOSE DataProductCursor;
        DEALLOCATE DataProductCursor;

        FETCH NEXT FROM CategoryCursor INTO @CategoryId;
    END;

    CLOSE CategoryCursor;
    DEALLOCATE CategoryCursor;

    RETURN CEILING(@TotalDaysNeeded);
END
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Blog]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Blog](
	[Id] [nvarchar](30) NOT NULL,
	[Title] [nvarchar](255) NULL,
	[UrlPath] [nvarchar](255) NULL,
	[Content] [nvarchar](max) NULL,
	[CreatedTime] [datetime] NULL,
	[StaffId] [nvarchar](30) NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Hastag] [nvarchar](255) NULL,
	[Thumbnail] [text] NULL,
 CONSTRAINT [PK_Blog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BodyAttribute]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BodyAttribute](
	[Id] [nvarchar](30) NOT NULL,
	[ProfileBodyId] [nvarchar](30) NULL,
	[BodySizeId] [nvarchar](30) NULL,
	[Value] [decimal](18, 3) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_BodyAttribute] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BodySize]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BodySize](
	[Id] [nvarchar](30) NOT NULL,
	[BodyPart] [nvarchar](50) NULL,
	[BodyIndex] [int] NULL,
	[Name] [nvarchar](100) NULL,
	[Image] [text] NULL,
	[GuideVideoLink] [text] NULL,
	[MinValidValue] [decimal](18, 3) NULL,
	[MaxValidValue] [decimal](18, 3) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_BodySize] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[Id] [nvarchar](30) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Chat]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chat](
	[Id] [nvarchar](30) NOT NULL,
	[OrderId] [nvarchar](30) NULL,
	[CreatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Chat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChatList]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatList](
	[Id] [nvarchar](30) NOT NULL,
	[ChatId] [nvarchar](30) NULL,
	[ReplierId] [nvarchar](30) NULL,
	[Message] [nvarchar](500) NULL,
	[FromCus] [bit] NULL,
	[SendTime] [datetime] NULL,
	[IsRead] [bit] NULL,
	[ReadTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Images] [text] NULL,
 CONSTRAINT [PK_ChatList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Component]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Component](
	[Id] [nvarchar](30) NOT NULL,
	[ComponentTypeId] [nvarchar](30) NULL,
	[ProductTemplateId] [nvarchar](30) NULL,
	[Name] [nvarchar](100) NULL,
	[Image] [text] NULL,
	[CreatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Default] [bit] NULL,
	[Index] [int] NULL,
 CONSTRAINT [PK_Component] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ComponentStage]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ComponentStage](
	[Id] [nvarchar](30) NOT NULL,
	[ComponentTypeId] [nvarchar](30) NULL,
	[TemplateStageId] [nvarchar](30) NULL,
 CONSTRAINT [PK_ComponentStage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ComponentType]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ComponentType](
	[Id] [nvarchar](30) NOT NULL,
	[CategoryId] [nvarchar](30) NULL,
	[Name] [nvarchar](100) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ComponentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [nvarchar](30) NOT NULL,
	[Avatar] [text] NULL,
	[Fullname] [nvarchar](100) NULL,
	[Address] [nvarchar](max) NULL,
	[Phone] [nvarchar](10) NULL,
	[Email] [nvarchar](255) NULL,
	[Gender] [int] NULL,
	[Username] [nvarchar](255) NULL,
	[Password] [nvarchar](max) NULL,
	[OTPNumber] [nvarchar](10) NULL,
	[OTPTimeLimit] [datetime] NULL,
	[OTPUsed] [bit] NULL,
	[PhoneVerified] [bit] NULL,
	[EmailVerified] [bit] NULL,
	[SecrectKeyLogin] [nvarchar](20) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Discount]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Discount](
	[Id] [nvarchar](30) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Code] [nvarchar](30) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[DiscountPercent] [real] NULL,
	[DiscountPrice] [decimal](18, 3) NULL,
	[ConditionPriceMin] [decimal](18, 3) NULL,
	[ConditionPriceMax] [decimal](18, 3) NULL,
	[ConditionProductMin] [int] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Discount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mastery]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mastery](
	[Id] [nvarchar](30) NOT NULL,
	[CategoryId] [nvarchar](30) NULL,
	[StaffId] [nvarchar](30) NULL,
 CONSTRAINT [PK_Mastery] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Material]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Material](
	[Id] [nvarchar](30) NOT NULL,
	[MaterialCategoryId] [nvarchar](30) NULL,
	[Name] [nvarchar](100) NULL,
	[Image] [text] NULL,
	[Quantity] [decimal](18, 3) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Material] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MaterialCategory]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaterialCategory](
	[Id] [nvarchar](30) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[PricePerUnit] [decimal](38, 3) NULL,
 CONSTRAINT [PK_MaterialCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[Id] [nvarchar](30) NOT NULL,
	[CustomerId] [nvarchar](30) NULL,
	[StaffId] [nvarchar](30) NULL,
	[Title] [nvarchar](255) NULL,
	[Content] [nvarchar](max) NULL,
	[SendTime] [datetime] NULL,
	[ReadTime] [datetime] NULL,
	[IsRead] [bit] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[Id] [nvarchar](30) NOT NULL,
	[CustomerId] [nvarchar](30) NULL,
	[CreaterId] [nvarchar](30) NULL,
	[DiscountId] [nvarchar](30) NULL,
	[TotalProduct] [int] NULL,
	[TotalPrice] [decimal](18, 3) NULL,
	[DiscountPrice] [decimal](18, 3) NULL,
	[DiscountCode] [nvarchar](30) NULL,
	[AfterDiscountPrice] [decimal](18, 3) NULL,
	[PayDeposit] [bit] NULL,
	[Deposit] [decimal](18, 3) NULL,
	[PaidMoney] [decimal](18, 3) NULL,
	[UnPaidMoney] [decimal](18, 3) NULL,
	[Status] [int] NULL,
	[CancelTime] [datetime] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[ApproveTime] [datetime] NULL,
	[CusAddress] [nvarchar](255) NULL,
	[CusEmail] [nvarchar](255) NULL,
	[CusName] [nvarchar](100) NULL,
	[CusPhone] [nvarchar](10) NULL,
	[FinishTime] [datetime2](7) NULL,
	[PlannedTime] [datetime2](7) NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderMaterial]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderMaterial](
	[Id] [nvarchar](30) NOT NULL,
	[MaterialId] [nvarchar](30) NULL,
	[OrderId] [nvarchar](30) NULL,
	[Image] [text] NULL,
	[Value] [decimal](18, 3) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[IsCusMaterial] [bit] NULL,
	[ValueUsed] [decimal](18, 3) NULL,
 CONSTRAINT [PK_OrderMaterial] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[Id] [nvarchar](30) NOT NULL,
	[OrderId] [nvarchar](30) NULL,
	[Platform] [nvarchar](50) NULL,
	[Amount] [decimal](18, 3) NULL,
	[PayTime] [datetime] NULL,
	[CreatedTime] [datetime] NULL,
	[Status] [int] NULL,
	[PayType] [int] NULL,
	[AmountAfterRefund] [decimal](18, 3) NULL,
	[PaymentRefundId] [nvarchar](30) NULL,
	[StaffCreateId] [nvarchar](30) NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[Id] [nvarchar](30) NOT NULL,
	[OrderId] [nvarchar](30) NULL,
	[ProductTemplateId] [nvarchar](30) NULL,
	[Name] [nvarchar](255) NULL,
	[Note] [nvarchar](255) NULL,
	[Status] [int] NULL,
	[EvidenceImage] [text] NULL,
	[FinishTime] [datetime] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Price] [decimal](18, 3) NULL,
	[SaveOrderComponents] [nvarchar](max) NULL,
	[FabricMaterialId] [nvarchar](30) NULL,
	[ReferenceProfileBodyId] [nvarchar](30) NULL,
	[Index] [int] NULL,
	[StaffMakerId] [nvarchar](30) NULL,
	[PlannedTime] [datetime2](7) NULL,
 CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductBodySize]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductBodySize](
	[Id] [nvarchar](30) NOT NULL,
	[ProductId] [nvarchar](30) NULL,
	[BodySizeId] [nvarchar](30) NULL,
	[Value] [decimal](18, 3) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ProductBodySize] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductComponent]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductComponent](
	[Id] [nvarchar](30) NOT NULL,
	[ComponentId] [nvarchar](30) NULL,
	[ProductStageId] [nvarchar](30) NULL,
	[Name] [nvarchar](100) NULL,
	[Image] [text] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[Note] [nvarchar](2550) NULL,
	[NoteImage] [text] NULL,
 CONSTRAINT [PK_ProductComponent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductStage]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductStage](
	[Id] [nvarchar](30) NOT NULL,
	[StaffId] [nvarchar](30) NULL,
	[TemplateStageId] [nvarchar](30) NULL,
	[ProductId] [nvarchar](30) NULL,
	[StageNum] [int] NULL,
	[TaskIndex] [int] NULL,
	[StartTime] [datetime] NULL,
	[FinishTime] [datetime] NULL,
	[Deadline] [datetime] NULL,
	[Status] [int] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[EvidenceImage] [text] NULL,
	[StageName] [nvarchar](255) NULL,
 CONSTRAINT [PK_ProductStage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductStageMaterial]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductStageMaterial](
	[Id] [nvarchar](30) NOT NULL,
	[ProductStageId] [nvarchar](30) NULL,
	[MaterialId] [nvarchar](30) NULL,
	[Quantity] [decimal](18, 3) NULL,
 CONSTRAINT [PK_ProductStageMaterial] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductTemplate]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTemplate](
	[Id] [nvarchar](30) NOT NULL,
	[CategoryId] [nvarchar](30) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [nvarchar](2550) NULL,
	[Price] [decimal](18, 3) NULL,
	[ThumbnailImage] [text] NULL,
	[Image] [text] NULL,
	[CollectionImage] [text] NULL,
	[UrlPath] [nvarchar](255) NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[AveDateForComplete] [int] NULL,
	[Gender] [int] NULL,
 CONSTRAINT [PK_ProductTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfileBody]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfileBody](
	[Id] [nvarchar](30) NOT NULL,
	[CustomerId] [nvarchar](30) NULL,
	[StaffId] [nvarchar](30) NULL,
	[Name] [nvarchar](100) NULL,
	[IsLocked] [bit] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ProfileBody] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Staff]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Staff](
	[Id] [nvarchar](30) NOT NULL,
	[Avatar] [text] NULL,
	[Fullname] [nvarchar](100) NULL,
	[Address] [nvarchar](255) NULL,
	[Phone] [nvarchar](10) NULL,
	[Username] [nvarchar](255) NULL,
	[Password] [nvarchar](255) NULL,
	[SecrectKeyLogin] [nvarchar](20) NULL,
	[LastLoginDeviceToken] [nvarchar](255) NULL,
	[Role] [int] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Staff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TemplateBodySize]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateBodySize](
	[Id] [nvarchar](30) NOT NULL,
	[ProductTemplateId] [nvarchar](30) NULL,
	[BodySizeId] [nvarchar](30) NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_TemplateBodySize] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TemplateStage]    Script Date: 5/30/2024 3:55:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateStage](
	[Id] [nvarchar](30) NOT NULL,
	[ProductTemplateId] [nvarchar](30) NULL,
	[TemplateStageId] [nvarchar](30) NULL,
	[Name] [nvarchar](255) NULL,
	[StageNum] [int] NULL,
	[CreatedTime] [datetime] NULL,
	[LastestUpdatedTime] [datetime] NULL,
	[InactiveTime] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_TemplateStage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Blog_StaffId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Blog_StaffId] ON [dbo].[Blog]
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BodyAttribute_BodySizeId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_BodyAttribute_BodySizeId] ON [dbo].[BodyAttribute]
(
	[BodySizeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BodyAttribute_ProfileBodyId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_BodyAttribute_ProfileBodyId] ON [dbo].[BodyAttribute]
(
	[ProfileBodyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Chat_OrderId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Chat_OrderId] ON [dbo].[Chat]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ChatList_ChatId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ChatList_ChatId] ON [dbo].[ChatList]
(
	[ChatId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ChatList_ReplierId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ChatList_ReplierId] ON [dbo].[ChatList]
(
	[ReplierId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Component_ComponentTypeId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Component_ComponentTypeId] ON [dbo].[Component]
(
	[ComponentTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Component_ProductTemplateId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Component_ProductTemplateId] ON [dbo].[Component]
(
	[ProductTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ComponentStage_ComponentTypeId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ComponentStage_ComponentTypeId] ON [dbo].[ComponentStage]
(
	[ComponentTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ComponentStage_TemplateStageId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ComponentStage_TemplateStageId] ON [dbo].[ComponentStage]
(
	[TemplateStageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ComponentType_CategoryId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ComponentType_CategoryId] ON [dbo].[ComponentType]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mastery_CategoryId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Mastery_CategoryId] ON [dbo].[Mastery]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mastery_StaffId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Mastery_StaffId] ON [dbo].[Mastery]
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Material_MaterialCategoryId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Material_MaterialCategoryId] ON [dbo].[Material]
(
	[MaterialCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Notification_CustomerId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Notification_CustomerId] ON [dbo].[Notification]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Notification_StaffId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Notification_StaffId] ON [dbo].[Notification]
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Order_CreaterId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_CreaterId] ON [dbo].[Order]
(
	[CreaterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Order_CustomerId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_CustomerId] ON [dbo].[Order]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Order_DiscountId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_DiscountId] ON [dbo].[Order]
(
	[DiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_OrderMaterial_MaterialId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderMaterial_MaterialId] ON [dbo].[OrderMaterial]
(
	[MaterialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_OrderMaterial_OrderId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderMaterial_OrderId] ON [dbo].[OrderMaterial]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Payment_OrderId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_OrderId] ON [dbo].[Payment]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Payment_PaymentRefundId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PaymentRefundId] ON [dbo].[Payment]
(
	[PaymentRefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Payment_StaffCreateId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_StaffCreateId] ON [dbo].[Payment]
(
	[StaffCreateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Product_FabricMaterialId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_FabricMaterialId] ON [dbo].[Product]
(
	[FabricMaterialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Product_OrderId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_OrderId] ON [dbo].[Product]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Product_ProductTemplateId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_ProductTemplateId] ON [dbo].[Product]
(
	[ProductTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Product_ReferenceProfileBodyId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_ReferenceProfileBodyId] ON [dbo].[Product]
(
	[ReferenceProfileBodyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Product_StaffMakerId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_StaffMakerId] ON [dbo].[Product]
(
	[StaffMakerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductBodySize_BodySizeId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductBodySize_BodySizeId] ON [dbo].[ProductBodySize]
(
	[BodySizeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductBodySize_ProductId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductBodySize_ProductId] ON [dbo].[ProductBodySize]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductComponent_ComponentId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductComponent_ComponentId] ON [dbo].[ProductComponent]
(
	[ComponentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductComponent_ProductStageId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductComponent_ProductStageId] ON [dbo].[ProductComponent]
(
	[ProductStageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductStage_ProductId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductStage_ProductId] ON [dbo].[ProductStage]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductStage_StaffId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductStage_StaffId] ON [dbo].[ProductStage]
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductStage_TemplateStageId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductStage_TemplateStageId] ON [dbo].[ProductStage]
(
	[TemplateStageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductStageMaterial_MaterialId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductStageMaterial_MaterialId] ON [dbo].[ProductStageMaterial]
(
	[MaterialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductStageMaterial_ProductStageId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductStageMaterial_ProductStageId] ON [dbo].[ProductStageMaterial]
(
	[ProductStageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProductTemplate_CategoryId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductTemplate_CategoryId] ON [dbo].[ProductTemplate]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProfileBody_CustomerId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfileBody_CustomerId] ON [dbo].[ProfileBody]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProfileBody_StaffId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfileBody_StaffId] ON [dbo].[ProfileBody]
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TemplateBodySize_BodySizeId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_TemplateBodySize_BodySizeId] ON [dbo].[TemplateBodySize]
(
	[BodySizeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TemplateBodySize_ProductTemplateId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_TemplateBodySize_ProductTemplateId] ON [dbo].[TemplateBodySize]
(
	[ProductTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TemplateStage_ProductTemplateId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_TemplateStage_ProductTemplateId] ON [dbo].[TemplateStage]
(
	[ProductTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TemplateStage_TemplateStageId]    Script Date: 5/30/2024 3:55:15 PM ******/
CREATE NONCLUSTERED INDEX [IX_TemplateStage_TemplateStageId] ON [dbo].[TemplateStage]
(
	[TemplateStageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Blog] ADD  CONSTRAINT [DF__Blog__IsActive__17786E0C]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[BodyAttribute] ADD  CONSTRAINT [DF__BodyAttri__IsAct__5669C4BE]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[BodySize] ADD  CONSTRAINT [DF__BodySize__BodyIn__6D823440]  DEFAULT ((0)) FOR [BodyIndex]
GO
ALTER TABLE [dbo].[BodySize] ADD  CONSTRAINT [DF__BodySize__IsActi__6E765879]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Category] ADD  CONSTRAINT [DF__Category__IsActi__7152C524]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Chat] ADD  CONSTRAINT [DF__Chat__IsActive__0CFADF99]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ChatList] ADD  CONSTRAINT [DF__ChatList__FromCu__422DC1E7]  DEFAULT ((1)) FOR [FromCus]
GO
ALTER TABLE [dbo].[ChatList] ADD  CONSTRAINT [DF__ChatList__IsRead__4321E620]  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[ChatList] ADD  CONSTRAINT [DF__ChatList__IsActi__44160A59]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Component] ADD  CONSTRAINT [DF__Component__IsAct__3414ACBA]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Component] ADD  CONSTRAINT [DF__Component__Defau__77CAB889]  DEFAULT ((0)) FOR [Default]
GO
ALTER TABLE [dbo].[ComponentType] ADD  CONSTRAINT [DF__Component__IsAct__04659998]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF__Customer__Gender__742F31CF]  DEFAULT ((0)) FOR [Gender]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF__Customer__OTPUse__75235608]  DEFAULT ((0)) FOR [OTPUsed]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF__Customer__PhoneV__76177A41]  DEFAULT ((0)) FOR [PhoneVerified]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF__Customer__EmailV__770B9E7A]  DEFAULT ((0)) FOR [EmailVerified]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF__Customer__IsActi__77FFC2B3]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Discount] ADD  CONSTRAINT [DF__Discount__IsActi__7ADC2F5E]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Material] ADD  CONSTRAINT [DF__Material__IsActi__490FC9A0]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[MaterialCategory] ADD  CONSTRAINT [DF__MaterialC__IsAct__13A7DD28]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF__Notificat__IsRea__1F198FD4]  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF__Notificat__IsAct__200DB40D]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF__Order__TotalProd__24D2692A]  DEFAULT ((0)) FOR [TotalProduct]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF__Order__TotalPric__10615C29]  DEFAULT ((0)) FOR [TotalPrice]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF__Order__PayDeposi__26BAB19C]  DEFAULT ((0)) FOR [PayDeposit]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF__Order__Status__27AED5D5]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF__Order__IsActive__28A2FA0E]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[OrderMaterial] ADD  CONSTRAINT [DF__OrderMate__Value__5EFF0ABF]  DEFAULT ((0)) FOR [Value]
GO
ALTER TABLE [dbo].[OrderMaterial] ADD  CONSTRAINT [DF__OrderMate__IsAct__5FF32EF8]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[OrderMaterial] ADD  CONSTRAINT [DF__OrderMate__IsCus__1CFC3D38]  DEFAULT ((0)) FOR [IsCusMaterial]
GO
ALTER TABLE [dbo].[OrderMaterial] ADD  CONSTRAINT [DF__OrderMate__Value__4AC307E8]  DEFAULT ((0)) FOR [ValueUsed]
GO
ALTER TABLE [dbo].[Payment] ADD  CONSTRAINT [DF__Payment__Status__4CE05A84]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Payment] ADD  CONSTRAINT [DF__Payment__PayType__1B13F4C6]  DEFAULT ((0)) FOR [PayType]
GO
ALTER TABLE [dbo].[Product] ADD  CONSTRAINT [DF__Product__Status__50B0EB68]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Product] ADD  CONSTRAINT [DF__Product__IsActiv__51A50FA1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Product] ADD  CONSTRAINT [DF__Product__Price__0F6D37F0]  DEFAULT ((0)) FOR [Price]
GO
ALTER TABLE [dbo].[ProductBodySize] ADD  CONSTRAINT [DF__ProductBo__IsAct__64B7E415]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductStage] ADD  CONSTRAINT [DF__ProductSt__Stage__697C9932]  DEFAULT ((0)) FOR [StageNum]
GO
ALTER TABLE [dbo].[ProductStage] ADD  CONSTRAINT [DF__ProductSt__TaskI__6A70BD6B]  DEFAULT ((0)) FOR [TaskIndex]
GO
ALTER TABLE [dbo].[ProductStage] ADD  CONSTRAINT [DF__ProductSt__Statu__6B64E1A4]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[ProductStage] ADD  CONSTRAINT [DF__ProductSt__IsAct__6C5905DD]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductTemplate] ADD  CONSTRAINT [DF__ProductTe__Price__0E7913B7]  DEFAULT ((0)) FOR [Price]
GO
ALTER TABLE [dbo].[ProductTemplate] ADD  CONSTRAINT [DF__ProductTe__IsAct__092A4EB5]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductTemplate] ADD  CONSTRAINT [DF__ProductTe__Gende__06D7F1EF]  DEFAULT ((-1)) FOR [Gender]
GO
ALTER TABLE [dbo].[ProfileBody] ADD  CONSTRAINT [DF__ProfileBo__IsLoc__2E5BD364]  DEFAULT ((0)) FOR [IsLocked]
GO
ALTER TABLE [dbo].[ProfileBody] ADD  CONSTRAINT [DF__ProfileBo__IsAct__2F4FF79D]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Staff] ADD  CONSTRAINT [DF__Staff__Role__009508B4]  DEFAULT ((2)) FOR [Role]
GO
ALTER TABLE [dbo].[Staff] ADD  CONSTRAINT [DF__Staff__IsActive__01892CED]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[TemplateBodySize] ADD  CONSTRAINT [DF__TemplateB__IsAct__070CFC19]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[TemplateStage] ADD  CONSTRAINT [DF__TemplateS__Stage__3CA9F2BB]  DEFAULT ((0)) FOR [StageNum]
GO
ALTER TABLE [dbo].[TemplateStage] ADD  CONSTRAINT [DF__TemplateS__IsAct__3D9E16F4]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Blog]  WITH CHECK ADD  CONSTRAINT [FK__Blog__StaffId__22951AFD] FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Blog] CHECK CONSTRAINT [FK__Blog__StaffId__22951AFD]
GO
ALTER TABLE [dbo].[BodyAttribute]  WITH CHECK ADD  CONSTRAINT [FK__BodyAttri__BodyS__3D7E1B63] FOREIGN KEY([BodySizeId])
REFERENCES [dbo].[BodySize] ([Id])
GO
ALTER TABLE [dbo].[BodyAttribute] CHECK CONSTRAINT [FK__BodyAttri__BodyS__3D7E1B63]
GO
ALTER TABLE [dbo].[BodyAttribute]  WITH CHECK ADD  CONSTRAINT [FK__BodyAttri__Profi__3C89F72A] FOREIGN KEY([ProfileBodyId])
REFERENCES [dbo].[ProfileBody] ([Id])
GO
ALTER TABLE [dbo].[BodyAttribute] CHECK CONSTRAINT [FK__BodyAttri__Profi__3C89F72A]
GO
ALTER TABLE [dbo].[Chat]  WITH CHECK ADD  CONSTRAINT [FK__Chat__OrderId__125EB334] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id])
GO
ALTER TABLE [dbo].[Chat] CHECK CONSTRAINT [FK__Chat__OrderId__125EB334]
GO
ALTER TABLE [dbo].[ChatList]  WITH CHECK ADD  CONSTRAINT [FK__ChatHisto__ChatI__162F4418] FOREIGN KEY([ChatId])
REFERENCES [dbo].[Chat] ([Id])
GO
ALTER TABLE [dbo].[ChatList] CHECK CONSTRAINT [FK__ChatHisto__ChatI__162F4418]
GO
ALTER TABLE [dbo].[ChatList]  WITH CHECK ADD  CONSTRAINT [FK__ChatHisto__Repli__17236851] FOREIGN KEY([ReplierId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[ChatList] CHECK CONSTRAINT [FK__ChatHisto__Repli__17236851]
GO
ALTER TABLE [dbo].[Component]  WITH CHECK ADD  CONSTRAINT [FK__Component__Compo__5A1A5A11] FOREIGN KEY([ComponentTypeId])
REFERENCES [dbo].[ComponentType] ([Id])
GO
ALTER TABLE [dbo].[Component] CHECK CONSTRAINT [FK__Component__Compo__5A1A5A11]
GO
ALTER TABLE [dbo].[Component]  WITH CHECK ADD  CONSTRAINT [FK__Component__Produ__5B0E7E4A] FOREIGN KEY([ProductTemplateId])
REFERENCES [dbo].[ProductTemplate] ([Id])
GO
ALTER TABLE [dbo].[Component] CHECK CONSTRAINT [FK__Component__Produ__5B0E7E4A]
GO
ALTER TABLE [dbo].[ComponentStage]  WITH CHECK ADD  CONSTRAINT [FK__Component__Compo__5649C92D] FOREIGN KEY([ComponentTypeId])
REFERENCES [dbo].[ComponentType] ([Id])
GO
ALTER TABLE [dbo].[ComponentStage] CHECK CONSTRAINT [FK__Component__Compo__5649C92D]
GO
ALTER TABLE [dbo].[ComponentStage]  WITH CHECK ADD  CONSTRAINT [FK__Component__Templ__573DED66] FOREIGN KEY([TemplateStageId])
REFERENCES [dbo].[TemplateStage] ([Id])
GO
ALTER TABLE [dbo].[ComponentStage] CHECK CONSTRAINT [FK__Component__Templ__573DED66]
GO
ALTER TABLE [dbo].[ComponentType]  WITH CHECK ADD  CONSTRAINT [FK__Component__Categ__52793849] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[ComponentType] CHECK CONSTRAINT [FK__Component__Categ__52793849]
GO
ALTER TABLE [dbo].[Mastery]  WITH CHECK ADD  CONSTRAINT [FK__Mastery__Categor__2E06CDA9] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Mastery] CHECK CONSTRAINT [FK__Mastery__Categor__2E06CDA9]
GO
ALTER TABLE [dbo].[Mastery]  WITH CHECK ADD  CONSTRAINT [FK__Mastery__StaffId__2EFAF1E2] FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Mastery] CHECK CONSTRAINT [FK__Mastery__StaffId__2EFAF1E2]
GO
ALTER TABLE [dbo].[Material]  WITH CHECK ADD  CONSTRAINT [FK__Material__Materi__004002F9] FOREIGN KEY([MaterialCategoryId])
REFERENCES [dbo].[MaterialCategory] ([Id])
GO
ALTER TABLE [dbo].[Material] CHECK CONSTRAINT [FK__Material__Materi__004002F9]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK__Notificat__Custo__1CDC41A7] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK__Notificat__Custo__1CDC41A7]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK__Notificat__Staff__1DD065E0] FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK__Notificat__Staff__1DD065E0]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK__Order__CreaterId__62AFA012] FOREIGN KEY([CreaterId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK__Order__CreaterId__62AFA012]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK__Order__CustomerI__61BB7BD9] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK__Order__CustomerI__61BB7BD9]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK__Order__DiscountI__63A3C44B] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discount] ([Id])
GO
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK__Order__DiscountI__63A3C44B]
GO
ALTER TABLE [dbo].[OrderMaterial]  WITH CHECK ADD  CONSTRAINT [FK__OrderMate__Mater__08D548FA] FOREIGN KEY([MaterialId])
REFERENCES [dbo].[Material] ([Id])
GO
ALTER TABLE [dbo].[OrderMaterial] CHECK CONSTRAINT [FK__OrderMate__Mater__08D548FA]
GO
ALTER TABLE [dbo].[OrderMaterial]  WITH CHECK ADD  CONSTRAINT [FK__OrderMate__Order__09C96D33] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id])
GO
ALTER TABLE [dbo].[OrderMaterial] CHECK CONSTRAINT [FK__OrderMate__Order__09C96D33]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK__Payment__OrderId__0E8E2250] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK__Payment__OrderId__0E8E2250]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK__Payment__PaymentRefund__1A25A48] FOREIGN KEY([PaymentRefundId])
REFERENCES [dbo].[Payment] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK__Payment__PaymentRefund__1A25A48]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK__Payment__Staff_Create__1A25A48] FOREIGN KEY([StaffCreateId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK__Payment__Staff_Create__1A25A48]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__Fabric_Material__6B44E613] FOREIGN KEY([FabricMaterialId])
REFERENCES [dbo].[Material] ([Id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__Fabric_Material__6B44E613]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__OrderId__6B44E613] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__OrderId__6B44E613]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__Product__6C390A4C] FOREIGN KEY([ProductTemplateId])
REFERENCES [dbo].[ProductTemplate] ([Id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__Product__6C390A4C]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__ProfileBody__6B44E613] FOREIGN KEY([ReferenceProfileBodyId])
REFERENCES [dbo].[ProfileBody] ([Id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__ProfileBody__6B44E613]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__Staff_Maker__6B44E613] FOREIGN KEY([StaffMakerId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__Staff_Maker__6B44E613]
GO
ALTER TABLE [dbo].[ProductBodySize]  WITH CHECK ADD  CONSTRAINT [FK__ProductBo__BodyS__2759D01A] FOREIGN KEY([BodySizeId])
REFERENCES [dbo].[BodySize] ([Id])
GO
ALTER TABLE [dbo].[ProductBodySize] CHECK CONSTRAINT [FK__ProductBo__BodyS__2759D01A]
GO
ALTER TABLE [dbo].[ProductBodySize]  WITH CHECK ADD  CONSTRAINT [FK__ProductBo__Produ__2665ABE1] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[ProductBodySize] CHECK CONSTRAINT [FK__ProductBo__Produ__2665ABE1]
GO
ALTER TABLE [dbo].[ProductComponent]  WITH CHECK ADD  CONSTRAINT [FK__ProductCo__Compo__041093DD] FOREIGN KEY([ComponentId])
REFERENCES [dbo].[Component] ([Id])
GO
ALTER TABLE [dbo].[ProductComponent] CHECK CONSTRAINT [FK__ProductCo__Compo__041093DD]
GO
ALTER TABLE [dbo].[ProductComponent]  WITH CHECK ADD  CONSTRAINT [FK__ProductCo__Produ__0504B816] FOREIGN KEY([ProductStageId])
REFERENCES [dbo].[ProductStage] ([Id])
GO
ALTER TABLE [dbo].[ProductComponent] CHECK CONSTRAINT [FK__ProductCo__Produ__0504B816]
GO
ALTER TABLE [dbo].[ProductStage]  WITH CHECK ADD  CONSTRAINT [FK__ProductSt__Produ__72E607DB] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[ProductStage] CHECK CONSTRAINT [FK__ProductSt__Produ__72E607DB]
GO
ALTER TABLE [dbo].[ProductStage]  WITH CHECK ADD  CONSTRAINT [FK__ProductSt__Staff__70FDBF69] FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[ProductStage] CHECK CONSTRAINT [FK__ProductSt__Staff__70FDBF69]
GO
ALTER TABLE [dbo].[ProductStage]  WITH CHECK ADD  CONSTRAINT [FK__ProductSt__Templ__71F1E3A2] FOREIGN KEY([TemplateStageId])
REFERENCES [dbo].[TemplateStage] ([Id])
GO
ALTER TABLE [dbo].[ProductStage] CHECK CONSTRAINT [FK__ProductSt__Templ__71F1E3A2]
GO
ALTER TABLE [dbo].[ProductStageMaterial]  WITH CHECK ADD  CONSTRAINT [FK__ProductCo__Mater__32CB82C6] FOREIGN KEY([MaterialId])
REFERENCES [dbo].[Material] ([Id])
GO
ALTER TABLE [dbo].[ProductStageMaterial] CHECK CONSTRAINT [FK__ProductCo__Mater__32CB82C6]
GO
ALTER TABLE [dbo].[ProductStageMaterial]  WITH CHECK ADD  CONSTRAINT [FK__ProductStage__Materail__31D75E8D] FOREIGN KEY([ProductStageId])
REFERENCES [dbo].[ProductStage] ([Id])
GO
ALTER TABLE [dbo].[ProductStageMaterial] CHECK CONSTRAINT [FK__ProductStage__Materail__31D75E8D]
GO
ALTER TABLE [dbo].[ProductTemplate]  WITH CHECK ADD  CONSTRAINT [FK__ProductTe__Categ__442B18F2] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[ProductTemplate] CHECK CONSTRAINT [FK__ProductTe__Categ__442B18F2]
GO
ALTER TABLE [dbo].[ProfileBody]  WITH CHECK ADD  CONSTRAINT [FK__ProfileBo__Custo__33008CF0] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[ProfileBody] CHECK CONSTRAINT [FK__ProfileBo__Custo__33008CF0]
GO
ALTER TABLE [dbo].[ProfileBody]  WITH CHECK ADD  CONSTRAINT [FK__ProfileBo__Staff__33F4B129] FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staff] ([Id])
GO
ALTER TABLE [dbo].[ProfileBody] CHECK CONSTRAINT [FK__ProfileBo__Staff__33F4B129]
GO
ALTER TABLE [dbo].[TemplateBodySize]  WITH CHECK ADD  CONSTRAINT [FK__TemplateB__BodyS__49E3F248] FOREIGN KEY([BodySizeId])
REFERENCES [dbo].[BodySize] ([Id])
GO
ALTER TABLE [dbo].[TemplateBodySize] CHECK CONSTRAINT [FK__TemplateB__BodyS__49E3F248]
GO
ALTER TABLE [dbo].[TemplateBodySize]  WITH CHECK ADD  CONSTRAINT [FK__TemplateB__Produ__48EFCE0F] FOREIGN KEY([ProductTemplateId])
REFERENCES [dbo].[ProductTemplate] ([Id])
GO
ALTER TABLE [dbo].[TemplateBodySize] CHECK CONSTRAINT [FK__TemplateB__Produ__48EFCE0F]
GO
ALTER TABLE [dbo].[TemplateStage]  WITH CHECK ADD  CONSTRAINT [FK__TemplateS__Produ__4CC05EF3] FOREIGN KEY([ProductTemplateId])
REFERENCES [dbo].[ProductTemplate] ([Id])
GO
ALTER TABLE [dbo].[TemplateStage] CHECK CONSTRAINT [FK__TemplateS__Produ__4CC05EF3]
GO
ALTER TABLE [dbo].[TemplateStage]  WITH CHECK ADD  CONSTRAINT [FK__TemplateS__Templ__4DB4832C] FOREIGN KEY([TemplateStageId])
REFERENCES [dbo].[TemplateStage] ([Id])
GO
ALTER TABLE [dbo].[TemplateStage] CHECK CONSTRAINT [FK__TemplateS__Templ__4DB4832C]
GO
/****** Object:  StoredProcedure [dbo].[AddProduct]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[AddProduct]
    @OrderId NVARCHAR(30),
    @ProductId NVARCHAR(30),
    @ProductTemplateId NVARCHAR(30) NULL,
    @FabricMaterialId NVARCHAR(30) NULL,
    @MaterialValue DECIMAL(18,3),
    @IsCusMaterial BIT,
    @ProductName NVARCHAR(500) NULL,
    @SaveOrderComponents NVARCHAR(MAX) NULL,
    @Note NVARCHAR(255) NULL,
    @ProfileBodyId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @ProducPrice DECIMAL(18, 0) = 0;
    DECLARE @MaterialPrice DECIMAL(18, 0);
    DECLARE @MaterialCategoryPricePerUnit DECIMAL(18, 2);
    DECLARE @CustomerId NVARCHAR(50);
    DECLARE @StaffMakeProfileBody NVARCHAR(50);
    DECLARE @ProductBodySize TABLE (
        BodySizeId NVARCHAR(30),
        [Value] DECIMAL(18,0)
    );
    DECLARE @OrderPlannedTime DATETIME;

    IF @FabricMaterialId IS NULL
        THROW 50000, N'Vui lòng chọn loại vải chính cho sản phẩm', 1;

    SELECT @OrderStatus = [Status], @CustomerId = CustomerId, @OrderPlannedTime = PlannedTime
    FROM [Order]
    WHERE Id = @orderId AND IsActive = 0;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus > 2
        THROW 50000, N'Đơn hàng đã vào giai đoạn thực hiện. Không thể thêm sản phẩm', 1;

    SELECT @ProducPrice = [Price],
        @ProductName = CASE WHEN @ProductName IS NULL THEN [Name] ELSE @ProductName END
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF @ProducPrice IS NULL
    BEGIN
        RAISERROR('Không tìm thấy bản mẫu asas', 16, 1);
        RETURN;
    END
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    SELECT @ProducPrice = [Price]
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF NOT EXISTS(SELECT 1
    FROM [Material]
    WHERE Id = @FabricMaterialId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nguyên liệu', 1;
    ELSE
    BEGIN
        SELECT @MaterialPrice = MC.PricePerUnit
        FROM MaterialCategory MC INNER JOIN Material M ON MC.Id = M.MaterialCategoryId

        IF EXISTS(SELECT 1
        FROM OrderMaterial
        WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId)
        BEGIN
            UPDATE OrderMaterial SET [Value] = [Value] + @MaterialValue WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId
        END;
        ELSE
        BEGIN
            INSERT INTO [dbo].[OrderMaterial]
                ([Id]
                ,[MaterialId]
                ,[OrderId]
                ,[Image]
                ,[Value]
                ,[CreatedTime]
                ,[LastestUpdatedTime]
                ,[InactiveTime]
                ,[IsActive]
                ,[IsCusMaterial]
                ,[ValueUsed])
            VALUES
                (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
                    @FabricMaterialId,
                    @OrderId,
                    NULL,
                    @MaterialValue,
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    NULL,
                    1,
                    @IsCusMaterial,
                    0)
        END;
    END;

    IF(@IsCusMaterial = 0)
    SET @ProducPrice = @ProducPrice + ROUND((@MaterialPrice * @MaterialValue), 2);
    ELSE
    SET @ProducPrice = @ProducPrice

    IF NOT EXISTS(SELECT 1
    FROM ProfileBody
    WHERE Id = @ProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy hồ sơ số đo của khách', 1;
    ELSE
    BEGIN
        SELECT @StaffMakeProfileBody = StaffId
        FROM ProfileBody
        WHERE Id = @ProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1
        IF(@StaffMakeProfileBody IS NULL)
        THROW 50000, N'Hồ sơ số đo này được đo bởi khách. Vui lòng kiểm tra và cập nhật lại hồ sơ số đo', 1;
        ELSE
        BEGIN
            INSERT INTO @ProductBodySize
                (BodySizeId,[Value])
            SELECT BS.Id AS BodySizeId, BA.[Value]
            FROM TemplateBodySize TBS LEFT JOIN BodySize BS ON (TBS.BodySizeId = BS.Id AND TBS.IsActive = 1)
                LEFT JOIN BodyAttribute BA ON (BS.Id = BA.BodySizeId AND BA.IsActive = 1)
                LEFT JOIN ProfileBody PB ON (BA.ProfileBodyId = PB.Id AND PB.IsActive = 1)
            WHERE PB.Id = @ProfileBodyId AND TBS.ProductTemplateId = @ProductTemplateId

            IF EXISTS(SELECT 1
            FROM @ProductBodySize
            WHERE [Value] IS NULL OR [Value] = 0)
            THROW 50000, N'Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung', 1;
        END;
    END;

    INSERT INTO [dbo].[Product]
        ([Id]
        ,[OrderId]
        ,[ProductTemplateId]
        ,[Name]
        ,[Note]
        ,[Status]
        ,[EvidenceImage]
        ,[FinishTime]
        ,[CreatedTime]
        ,[LastestUpdatedTime]
        ,[InactiveTime]
        ,[IsActive]
        ,[Price]
        ,[SaveOrderComponents]
        ,[FabricMaterialId]
        ,[ReferenceProfileBodyId]
        ,[Index]
        ,[StaffMakerId]
        ,[PlannedTime])
    VALUES
        (@ProductId,
            @OrderId,
            @ProductTemplateId,
            @ProductName,
            @Note,
            1,
            NULL,
            NULL,
            DATEADD(HOUR, 7, GETUTCDATE()),
            DATEADD(HOUR, 7, GETUTCDATE()),
            NULL,
            1,
            @ProducPrice,
            @SaveOrderComponents,
            @FabricMaterialId,
            @ProfileBodyId,
            NULL,
            NULL,
            NULL)

    UPDATE ProductBodySize SET IsActive = 0 WHERE ProductId = @ProductId;

    DECLARE @BodySizeId NVARCHAR(30);
    DECLARE @Value DECIMAL(18,0);

    DECLARE ProductBodySizeCursor CURSOR FOR
    SELECT BodySizeId, [Value]
    FROM @ProductBodySize;

    OPEN ProductBodySizeCursor;
    FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @ProductBodySizeId NVARCHAR(30) = CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0);
        IF NOT EXISTS(SELECT 1
        FROM ProductBodySize
        WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId)
        BEGIN
            INSERT INTO [dbo].[ProductBodySize]
                ([Id]
                ,[ProductId]
                ,[BodySizeId]
                ,[Value]
                ,[CreatedTime]
                ,[LastestUpdatedTime]
                ,[InactiveTime]
                ,[IsActive])
            VALUES
                (@ProductBodySizeId,
                    @ProductId,
                    @BodySizeId,
                    @Value,
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    NULL,
                    1)
        END;
        ELSE
        BEGIN
            UPDATE ProductBodySize SET IsActive = 1, [Value] = @Value WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId;
        END;

        FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;
    END

    CLOSE ProductBodySizeCursor;
    DEALLOCATE ProductBodySizeCursor;

    --EXECUTE [dbo].[CheckNumOfDateToFinish];

    DECLARE @DayToFinish INT = dbo.CalculateNumOfDateToFinish(@ProductId);
     
    DECLARE @DateToFinish DATETIME;

    IF @DayToFinish > 0
    BEGIN
    SET @DateToFinish = DATEADD(DAY,@DayToFinish,DATEADD(HOUR, 7, GETUTCDATE()));
    
    UPDATE Product SET PlannedTime = @DateToFinish WHERE Id = @ProductId;
    IF @OrderPlannedTime IS NULL OR @OrderPlannedTime < @DateToFinish
    UPDATE [Order] SET PlannedTime = @DateToFinish WHERE Id = @OrderId;
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[AutoAssignTaskForStaff]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AutoAssignTaskForStaff]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ProductId NVARCHAR(30);
    DECLARE @CategoryId NVARCHAR(30);
    DECLARE @StaffMakerId NVARCHAR(30);
    DECLARE @IndexTask INT;

    DECLARE NotAssignProducts CURSOR FOR
    SELECT P.Id, PT.CategoryId
    FROM Product P INNER JOIN [Order] O ON O.Id = P.OrderId
        INNER JOIN ProductTemplate PT ON P.ProductTemplateId = PT.Id
    WHERE P.IsActive = 1 AND O.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND O.[Status] = 3 AND P.StaffMakerId IS NULL

    OPEN NotAssignProducts;
    FETCH NEXT FROM NotAssignProducts INTO @ProductId, @CategoryId;

    WHILE @@FETCH_STATUS = 0
    BEGIN

        SET @IndexTask = 0;
        SET @StaffMakerId = NULL;

        SELECT TOP 1 @StaffMakerId = S.Id, @IndexTask = P.[Index]
            FROM Staff S INNER JOIN (
            SELECT TOP 1 S.Id, COUNT(P.Id) AS NumOfTask
            FROM Staff S INNER JOIN Mastery M ON (M.StaffId = S.Id AND M.CategoryId = @CategoryId)
            LEFT JOIN Product P ON (P.StaffMakerId = S.Id AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5)
            LEFT JOIN [Order] O ON (O.Id = P.OrderId AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status < 8)
            WHERE S.IsActive = 1
            GROUP BY S.Id
            ORDER BY NumOfTask ASC
        ) AS ST ON S.Id = ST.Id
        INNER JOIN Product P ON P.StaffMakerId = S.Id
        INNER JOIN [Order] O ON O.Id = P.OrderId
        WHERE S.IsActive = 1
        AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] <= 5
        AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status <= 8
        ORDER BY P.[Index] DESC

        IF @StaffMakerId IS NOT NULL
        BEGIN
            IF @IndexTask IS NULL
            SET @IndexTask = 1;
            ELSE
            SET @IndexTask = @IndexTask + 1;

            UPDATE Product SET
            StaffMakerId = @StaffMakerId,
            [Index] = @IndexTask
            WHERE Id = @ProductId;

            UPDATE ProductStage SET
            StaffId = @StaffMakerId,
            TaskIndex = StageNum
            WHERE ProductId = @ProductId AND IsActive = 1 AND [Status] > 0 AND [Status] < 4;
        END;
        ELSE
        BEGIN
            SELECT TOP 1 @IndexTask = P.[Index]
            FROM Product P
            INNER JOIN [Order] O ON O.Id = P.OrderId
            WHERE  P.IsActive = 1 AND P.[Status] > 0
            AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status < 8
            AND P.StaffMakerId IS NULL
            ORDER BY P.[Index] DESC

            IF @IndexTask IS NULL
            SET @IndexTask = 1;
            ELSE
            SET @IndexTask = @IndexTask + 1;

            UPDATE Product SET
            StaffMakerId = NULL,
            [Index] = @IndexTask
            WHERE Id = @ProductId;
        END;

    FETCH NEXT FROM NotAssignProducts INTO @ProductId, @CategoryId;
    END;

    CLOSE NotAssignProducts;
    DEALLOCATE NotAssignProducts;

    SELECT 1 AS ReturnValue;
END
GO
/****** Object:  StoredProcedure [dbo].[CancelOrder]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CancelOrder]
    @OrderId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;

    SELECT @OrderStatus = [Status] FROM [Order] WHERE Id = @OrderId;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn đã bị hủy', 1;
    ELSE
    BEGIN
        UPDATE [Order] SET [Status] = 0, [CancelTime] = DATEADD(HOUR, 7, GETUTCDATE()) WHERE Id = @OrderId;

        UPDATE [Product] SET [Status] = 0 WHERE OrderId = @OrderId AND IsActive = 1;

        UPDATE [ProductStage] SET [Status] = 0 WHERE ProductId IN (
            SELECT Id
            FROM Product
            WHERE OrderId = @OrderId AND IsActive = 1
        );
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[CheckNumOfDateToFinish]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CheckNumOfDateToFinish]
AS
BEGIN
    DECLARE @ProductId NVARCHAR(30);
    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderPlannedTime DATETIME;
    DECLARE @ProductCreatedTime DATETIME;
    DECLARE @ProductPlannedTime DATETIME;
    DECLARE @DateNeed INT;

    DECLARE ProductCursor CURSOR FOR
    SELECT P.Id, P.OrderId, P.CreatedTime
    FROM Product P INNER JOIN [Order] O ON O.Id = P.OrderId
    WHERE P.IsActive = 1 AND O.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND O.[Status] > 0 AND O.[Status] < 8


    OPEN ProductCursor;
    FETCH NEXT FROM ProductCursor INTO @ProductId, @OrderId,@ProductCreatedTime;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @OrderPlannedTime = PlannedTime
        FROM [Order]
        WHERE Id = @OrderId

        SET @DateNeed = dbo.CalculateNumOfDateToFinish(@ProductId);

        IF @DateNeed > 0
        BEGIN
            SET @ProductPlannedTime = DATEADD(DAY,@DateNeed,@ProductCreatedTime);

            UPDATE Product SET PlannedTime = @ProductPlannedTime WHERE Id = @ProductId;
            IF @OrderPlannedTime IS NULL OR @OrderPlannedTime < @ProductPlannedTime
        UPDATE [Order] SET PlannedTime = @ProductPlannedTime WHERE Id = @OrderId;
        END;

        FETCH NEXT FROM ProductCursor INTO @ProductId, @OrderId,@ProductCreatedTime;
    END

    CLOSE ProductCursor;
    DEALLOCATE ProductCursor;
END
GO
/****** Object:  StoredProcedure [dbo].[CheckOrderDiscount]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CheckOrderDiscount]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM [Order] WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
        
    DECLARE @TotalProducts INT,
            @TotalPrice DECIMAL(18,2),
            @DiscountPrice DECIMAL(18,2) = 0,
            @AfterDiscountPrice DECIMAL(18,2) = 0,
            @PaidMoney DECIMAL(18,2),
            @UnPaidMoney DECIMAL(18,2),
            @DiscountId NVARCHAR(50),
            @DiscountCode NVARCHAR(50),
            @DiscountPercent DECIMAL(5,2),
            @ConditionProductMin INT,
            @ConditionPriceMin DECIMAL(18,2),
            @ConditionPriceMax DECIMAL(18,2),
            @CalculatedDiscountPrice DECIMAL(18,2) = 0;
    -- Calculate total products and total price
    SELECT
        @TotalProducts = COUNT(*),
        @TotalPrice = SUM(p.Price)
    FROM Product p
    WHERE p.OrderId = @OrderId
    AND p.IsActive = 1
    AND p.Status != 0;

    -- Retrieve applicable discount if any
    SELECT
        @DiscountId = d.Id,
        @DiscountCode = d.Code,
        @DiscountPrice = d.DiscountPrice,
        @DiscountPercent = d.DiscountPercent,
        @ConditionProductMin = d.ConditionProductMin,
        @ConditionPriceMin = d.ConditionPriceMin,
        @ConditionPriceMax = d.ConditionPriceMax
    FROM Discount d
    INNER JOIN [Order] o ON o.DiscountId = d.Id
    WHERE o.Id = @OrderId
    AND d.IsActive = 1;

    -- Apply discount logic
    IF @DiscountId IS NOT NULL
    BEGIN
        IF @TotalProducts >= @ConditionProductMin OR
        (@TotalPrice >= @ConditionPriceMin)
        BEGIN
            IF @DiscountPrice IS NOT NULL AND @DiscountPrice > 0
                SET @CalculatedDiscountPrice = @DiscountPrice;
            ELSE IF @DiscountPercent IS NOT NULL AND @DiscountPercent > 0
                SET @CalculatedDiscountPrice = @TotalPrice * @DiscountPercent / 100;

            -- Check if calculated discount price is greater than condition max price
            IF @CalculatedDiscountPrice > @ConditionPriceMax
                SET @CalculatedDiscountPrice = @ConditionPriceMax;

            SET @AfterDiscountPrice = @TotalPrice - @CalculatedDiscountPrice;
        END
        ELSE
            SET @AfterDiscountPrice = @TotalPrice;
    END
    ELSE
    BEGIN
        SET @AfterDiscountPrice = @TotalPrice;
        SET @DiscountCode = '';
        SET @CalculatedDiscountPrice = 0;
    END

    -- Calculate paid money
    SELECT @PaidMoney = ISNULL(SUM(Amount), 0)
    FROM Payment
    WHERE OrderId = @OrderId
    AND Status = 0;

    -- Calculate unpaid money
    IF(@AfterDiscountPrice IS NOT NULL)
    SET @UnPaidMoney = @AfterDiscountPrice - @PaidMoney;
    ELSE
    SET @UnPaidMoney = @TotalPrice - @PaidMoney;

    -- Update the order
    UPDATE [Order]
    SET TotalProduct = @TotalProducts,
        TotalPrice = @TotalPrice,
        DiscountCode = @DiscountCode,
        DiscountPrice = ISNULL(@CalculatedDiscountPrice, 0),
        AfterDiscountPrice = @AfterDiscountPrice,
        PaidMoney = @PaidMoney,
        UnPaidMoney = @UnPaidMoney
    WHERE Id = @OrderId;

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue; -- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[CheckOrderPaid]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CheckOrderPaid]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    DECLARE @TotalProducts INT,
            @TotalPrice DECIMAL(18,2),
            @DiscountPrice DECIMAL(18,2) = 0,
            @AfterDiscountPrice DECIMAL(18,2) = 0,
            @PaidMoney DECIMAL(18,2),
            @UnPaidMoney DECIMAL(18,2),
            @DiscountCode NVARCHAR(30),
            @DiscountId NVARCHAR(50);
    -- Calculate total products and total price
    SELECT
        @TotalProducts = COUNT(*),
        @TotalPrice = SUM(p.Price)
    FROM Product p
    WHERE p.OrderId = @OrderId
        AND p.IsActive = 1
        AND p.Status != 0;

    SELECT @AfterDiscountPrice = AfterDiscountPrice, @DiscountPrice = DiscountPrice, @DiscountCode = DiscountCode, @DiscountId = DiscountId
    FROM [Order]
    WHERE Id = @OrderId

    -- Apply discount logic
    IF @DiscountId IS NULL
    BEGIN
        SET @AfterDiscountPrice = @TotalPrice;
        SET @DiscountPrice = 0;
        SET @DiscountCode = '';
    END;

    -- Calculate paid money
    SELECT @PaidMoney = ISNULL(SUM(Amount), 0)
    FROM Payment
    WHERE OrderId = @OrderId
        AND Status = 0;

    -- Calculate unpaid money
    IF(@AfterDiscountPrice IS NOT NULL)
    SET @UnPaidMoney = @AfterDiscountPrice - @PaidMoney;
    ELSE
    SET @UnPaidMoney = @TotalPrice - @PaidMoney;

    -- Update the order
    UPDATE [Order]
    SET TotalProduct = @TotalProducts,
        TotalPrice = @TotalPrice,
        DiscountCode = @DiscountCode,
        DiscountId = @DiscountId,
        DiscountPrice = ISNULL(@DiscountPrice, 0),
        AfterDiscountPrice = @AfterDiscountPrice,
        PaidMoney = @PaidMoney,
        UnPaidMoney = @UnPaidMoney
    WHERE Id = @OrderId;

    IF @PaidMoney > 0
    UPDATE [Order]
    SET IsActive = 1
    WHERE Id = @OrderId;

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[CreateManagerNotification]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateManagerNotification]
    @Title NVARCHAR(255),
    @Content NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ManagerId NVARCHAR(30);

    DECLARE ManagerCursor CURSOR FOR
    SELECT Id
    FROM Staff
    WHERE IsActive = 1 AND [Role] = 1

    OPEN ManagerCursor;
    FETCH NEXT FROM ManagerCursor INTO @ManagerId;

    WHILE @@FETCH_STATUS = 0
    BEGIN

    INSERT INTO [dbo].[Notification]
            ([Id]
            ,[CustomerId]
            ,[StaffId]
            ,[Title]
            ,[Content]
            ,[SendTime]
            ,[ReadTime]
            ,[IsRead]
            ,[IsActive])
        VALUES
            (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
            NULL,
            @ManagerId,
            @Title,
            @Content,
            DATEADD(HOUR, 7, GETUTCDATE()),
            NULL,
            0,
            1)

        FETCH NEXT FROM ManagerCursor INTO @ManagerId;
    END

    CLOSE ManagerCursor;
    DEALLOCATE ManagerCursor;
	
    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END
GO
/****** Object:  StoredProcedure [dbo].[CustomerRegis]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerRegis]
    @Username NVARCHAR(255) NULL,
    @Email NVARCHAR(255) NULL,
    @Fullname NVARCHAR(255) NULL,
    @Phone NVARCHAR(10) NULL,
    @Address NVARCHAR(MAX) NULL,
    @Password NVARCHAR(MAX) NULL,
    @Avatar TEXT NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CustomerId NVARCHAR(30);

    IF @Username IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Username] LIKE @Username AND IsActive = 1)
    SELECT -1 AS ReturnValue; -- dupplicate Username

    IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NOT NULL AND IsActive = 1)
    SELECT -2 AS ReturnValue; -- dupplicate Email

    IF @Phone IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Phone] LIKE @Phone AND [Password] IS NOT NULL AND IsActive = 1)
    SELECT -3 AS ReturnValue; -- dupplicate Phone

    IF @Email IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NULL AND EmailVerified = 1 AND IsActive = 1)
    SELECT -4 AS ReturnValue; -- Email not verify
    ELSE
    SELECT @CustomerId = Id FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NULL AND EmailVerified = 1 AND IsActive = 1

    -- Update rows in table '[Customer]' in schema '[dbo]'
    UPDATE [dbo].[Customer]
    SET
        [Username] = @Username,
        [Password] = @Password,
        [IsActive] = 1,
        [CreatedTime] = DATEADD(HOUR, 7, GETUTCDATE()),
        [LastestUpdatedTime] = DATEADD(HOUR, 7, GETUTCDATE())
    WHERE Id = @CustomerId

    IF(@Phone IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Phone] = @Phone
    WHERE Id = @CustomerId

    IF(@Fullname IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Fullname] = @Fullname
    WHERE Id = @CustomerId
    
    IF(@Address IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Address] = @Address
    WHERE Id = @CustomerId

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[DeleteProduct]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[DeleteProduct]
    @ProductId NVARCHAR(30),
    @OrderId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @OrderPaidMoney DECIMAL(18,3);
    DECLARE @OrderDeposit DECIMAL(18,3);
    DECLARE @FabricMaterialId NVARCHAR(30);

    IF NOT EXISTS (SELECT 1
    FROM Product
    WHERE Id = @ProductId AND IsActive = 1 AND [Status] > 0)
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    SELECT @FabricMaterialId = FabricMaterialId
    FROM Product
    WHERE Id = @ProductId

    SELECT @OrderStatus = [Status], @OrderPaidMoney = PaidMoney, @OrderDeposit = Deposit
    FROM [Order]
    WHERE Id = @OrderId

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus >= 2
    BEGIN
        IF @OrderStatus = 0
        THROW 50000, N'Đơn hàng đã hủy. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 2
        THROW 50000, N'Đơn hàng đã duyệt. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 3
        THROW 50000, N'Đơn hàng chuẩn bị vào giai đoạn thực hiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 4
        THROW 50000, N'Đơn hàng đang trong giai đoạn thực hiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 5
        THROW 50000, N'Đơn hàng đã vào giai đoạn hoàn thiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 6
        THROW 50000, N'Đơn hàng đang chờ khách hàng kiểm thử. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 7
        THROW 50000, N'Đơn hàng đã bàn giao cho khách. Không thể xóa sản phẩm', 1;
        ELSE
        THROW 50000, N'Đơn hàng đã hủy. Không thể xóa sản phẩm', 1;
    END;
    ELSE IF (@OrderPaidMoney IS NOT NULL AND @OrderPaidMoney > 0) OR (@OrderDeposit IS NOT NULL AND @OrderDeposit > 0)
        THROW 50000, N'Đơn hàng đã thanh toán. Không thể xóa sản phẩm', 1;
    ELSE
    BEGIN
        UPDATE Product
        SET
        LastestUpdatedTime = DATEADD(HOUR, 7, GETUTCDATE()),
        IsActive = 0,
        InactiveTime = DATEADD(HOUR, 7, GETUTCDATE())
        WHERE Id = @ProductId

        IF NOT EXISTS (SELECT 1
        FROM Product
        WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND FabricMaterialId = @FabricMaterialId AND IsActive = 1 AND [Status] > 0)
        BEGIN
            DELETE FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId AND IsActive = 1 AND IsCusMaterial = 0;
        END;
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[FinishTask]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FinishTask]
    @ProductId NVARCHAR(30),
    @ProductStageId NVARCHAR(30),
    @StaffId NVARCHAR(30),
    @EvidenceImage NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @ProductStatus INT;
    DECLARE @ProductIndex INT;
    DECLARE @ProductStageStatus INT;
    DECLARE @ProductStageNum INT;
    DECLARE @ReturnValue INT = 1;

    SELECT @OrderId = OrderId, @ProductStatus = [Status], @ProductIndex = [Index]
    FROM Product
    WHERE Id = @ProductId AND StaffMakerId = @StaffId AND IsActive = 1;

    IF @OrderId IS NULL OR @ProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    IF @ProductStatus = 0
        THROW 50000, N'Sản phẩm bị hủy', 1;
    ELSE IF @ProductStatus = 1
        THROW 50000, N'Sản phẩm đang chờ', 1;
    ELSE IF @ProductStatus = 5
        THROW 50000, N'Sản phẩm đã hoàn thành', 1;

    SELECT @OrderStatus = [Status]
    FROM [Order]
    WHERE Id = @OrderId AND IsActive = 1;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn bị hủy', 1;
    ELSE IF @OrderStatus = 1
        THROW 50000, N'Hóa đơn chưa được duyệt', 1;
    ELSE IF @OrderStatus = 2
        THROW 50000, N'Hóa đơn chưa được bắt đầu', 1;
    ELSE IF @OrderStatus = 5
        THROW 50000, N'Hóa đơn đã hoàn thiện', 1;
    ELSE IF @OrderStatus = 6
        THROW 50000, N'Hóa đơn đang chờ khách kiểm duyệt', 1;
    ELSE IF @OrderStatus = 8
        THROW 50000, N'Hóa đơn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM Product P INNER JOIN [Order] O ON P.OrderId = O.Id 
        WHERE P.StaffMakerId = @StaffId AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND P.[Index] IS NOT NULL AND P.[Index] < @ProductIndex AND O.[Status] > 0 AND O.IsActive = 1)
        THROW 50000, N'Nhiệm vụ trước chưa hoàn thành. Vui lòng thực hiện theo trình tự', 1;

    SELECT @ProductStageStatus = [Status], @ProductStageNum = StageNum
    FROM ProductStage
    WHERE Id = @ProductStageId AND IsActive = 1;

    IF @ProductStageStatus IS NULL
        THROW 50000, N'Không tìm thấy nhiệm vụ', 1;
    ELSE IF @ProductStageStatus = 0
        THROW 50000, N'Công đoạn bị hủy', 1;
    ELSE IF @ProductStageStatus = 1
        THROW 50000, N'Công đoạn chưa được bắt đầu', 1;
    ELSE IF @ProductStageStatus = 3
        THROW 50000, N'Công đoạn đang tạm dừng', 1;
    ELSE IF @ProductStageStatus = 4 AND @ProductStatus NOT LIKE 4
        THROW 50000, N'Công đoạn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 1 AND [Status] < 4 AND IsActive = 1)
        THROW 50000, N'Có công đoạn đang chờ hoặc đang thực hiện.', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 0 AND [Status] < 4 AND IsActive = 1 AND StageNum < @ProductStageNum)
        THROW 50000, N'Công đoạn trước chưa hoàn thành.', 1;

    IF (SELECT TOP 1 Id FROM ProductStage WHERE ProductId = @ProductId AND IsActive = 1 ORDER BY StageNum DESC) = @ProductStageId
    BEGIN
        UPDATE Product SET 
        [Status] = 5,
        LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE()),
        FinishTime = DATEADD(HOUR,7,GETUTCDATE())
        WHERE Id = @ProductId;

        SET @ReturnValue = 2; -- Notify hoàn thành sản phẩm
        

        IF NOT EXISTS (SELECT 1 FROM Product WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND [Status] > 0 AND [Status] < 5 AND IsActive = 1)
        BEGIN
            IF @OrderStatus NOT LIKE 7 -- Đơn không bị từ chối
            BEGIN
                UPDATE [Order] SET
                [Status] = 5,
                LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE()),
                FinishTime = DATEADD(HOUR,7,GETUTCDATE())
                WHERE Id = @OrderId;

                SET @ReturnValue = 3; -- Notify hoàn thành đơn
            END;
            ELSE
            BEGIN
                UPDATE [Order] SET
                [Status] = 6,
                LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE()),
                FinishTime = DATEADD(HOUR,7,GETUTCDATE())
                WHERE Id = @OrderId;

                SET @ReturnValue = 4; -- Notify hoàn thành đơn bị từ chối
            END;
        END;
    END;

    UPDATE ProductStage SET
    [Status] = 4,
    StaffId = @StaffId,
    FinishTime = DATEADD(HOUR,7,GETUTCDATE()),
    EvidenceImage = @EvidenceImage
    WHERE Id = @ProductStageId;

    SELECT @ReturnValue AS ReturnValue;
END;
GO
/****** Object:  StoredProcedure [dbo].[GetActiveOrders]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetActiveOrders]
AS
BEGIN
    SELECT [Id],
    [CustomerId],
    [CreaterId],
    [DiscountId],
    [TotalProduct],
    [TotalPrice],
    [DiscountPrice],
    [DiscountCode],
    [AfterDiscountPrice],
    [PayDeposit],
    [Deposit],
    [PaidMoney],
    [UnPaidMoney],
    [Status],
    [CancelTime],
    [CreatedTime],
    [LastestUpdatedTime],
    [InactiveTime],
    [IsActive],
    [ApproveTime],
    [CusAddress],
    [CusEmail],
    [CusName],
    [CusPhone],
    [FinishTime],
    [PlannedTime]
    FROM [Order]
    WHERE IsActive = 1
    Order BY CreatedTime DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[GetActiveOrdersProducts]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetActiveOrdersProducts]
    @OrderIds NVARCHAR(MAX)
AS
BEGIN
    SELECT [Id],
    [OrderId],
    [ProductTemplateId],
    [Name],
    [Note],
    [Status],
    [EvidenceImage],
    [FinishTime],
    [CreatedTime],
    [LastestUpdatedTime],
    [InactiveTime],
    [IsActive],
    [Price],
    [SaveOrderComponents],
    [FabricMaterialId],
    [ReferenceProfileBodyId],
    [Index],
    [StaffMakerId],
    [PlannedTime]
    FROM [Product]
    WHERE IsActive = 1 
    AND [Status] > 0 
    AND OrderId IN (SELECT value FROM STRING_SPLIT(@OrderIds, ','))
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderChat]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderChat]
    @OrderId NVARCHAR(30)
AS

BEGIN
    SELECT TOP 1 
    [C].[Id],
    [C].[OrderId],
    [C].[CreatedTime],
    [C].[InactiveTime],
    [C].[IsActive]
    FROM Chat C INNER JOIN [Order] O ON C.OrderId = O.Id
    WHERE O.Id = @OrderId
    AND O.[Status] >= 0
    AND O.[Status] <= 8
    AND O.IsActive = 1
    AND C.IsActive = 1
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderChatList]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderChatList]
    @OrderId NVARCHAR(30),
    @Role INT
AS

BEGIN
IF(@Role = 3)
BEGIN
    UPDATE ChatList
    SET IsRead = 1
    FROM ChatList CL
    INNER JOIN (
        SELECT C.Id
        FROM  [Chat] C
        INNER JOIN [dbo].[Order] O ON C.OrderId = O.Id
        WHERE O.Id = @OrderId
        AND O.[Status] >= 0 AND O.[Status] <= 8
        AND O.IsActive = 1
        AND C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.FromCus = 1;
END
ELSE
BEGIN
    UPDATE ChatList
    SET IsRead = 1
    FROM ChatList CL
    INNER JOIN (
        SELECT C.Id
        FROM  [Chat] C
        INNER JOIN [dbo].[Order] O ON C.OrderId = O.Id
        WHERE O.Id = @OrderId
        AND O.[Status] >= 0 AND O.[Status] <= 8
        AND O.IsActive = 1
        AND C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.FromCus = 0;
END
    SELECT [CL].[Id],
    [CL].[ChatId],
    [CL].[ReplierId],
    [CL].[Message],
    [CL].[FromCus],
    [CL].[SendTime],
    [CL].[IsRead],
    [CL].[ReadTime],
    [CL].[InactiveTime],
    [CL].[IsActive],
    [CL].[Images]
    FROM [ChatList] CL INNER JOIN (
        SELECT TOP 1 C.Id
        FROM Chat C INNER JOIN (
            SELECT TOP 1 Id FROM [dbo].[Order]
            WHERE Id = @OrderId AND [Status] >= 0 AND [Status] <= 8 AND IsActive = 1
        ) AS O ON C.OrderId = O.Id
        WHERE C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.IsActive = 1
    ORDER BY CL.SendTime ASC
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderDashboard]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderDashboard]
    @StartDate DATETIME
AS
BEGIN
Select 
    [Status],
    COUNT(Id) As Total,
    SUM(CASE 
            WHEN AfterDiscountPrice IS NOT NULL THEN AfterDiscountPrice 
            ELSE TotalPrice 
        END) AS TotalPrice
FROM 
    [Order]
WHERE 
    IsActive = 1 
    AND MONTH(CreatedTime) = MONTH(@StartDate) 
    AND YEAR(CreatedTime) = YEAR(@StartDate)
GROUP BY 
    [Status];
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderMaterials]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderMaterials]
    @OrderId NVARCHAR(30)
AS
BEGIN
SELECT 
[OM].[Id],
[OM].[MaterialId],
[OM].[OrderId],
[OM].[Image],
[OM].[Value],
[OM].[CreatedTime],
[OM].[LastestUpdatedTime],
[OM].[InactiveTime],
[OM].[IsActive],
[OM].[IsCusMaterial],
[OM].[ValueUsed]
FROM OrderMaterial OM INNER JOIN (
    SELECT Id
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0
) AS O ON OM.OrderId = O.Id
WHERE IsActive = 1
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderMaterialsCategoryData]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderMaterialsCategoryData]
    @OrderId NVARCHAR(30)
AS
BEGIN
SELECT 
[MC].[Id],
[MC].[Name],
[MC].[CreatedTime],
[MC].[LastestUpdatedTime],
[MC].[InactiveTime],
[MC].[IsActive],
[MC].[PricePerUnit]
FROM MaterialCategory MC INNER JOIN (
    SELECT
    [M].[MaterialCategoryId]
    FROM Material M INNER JOIN (
        SELECT 
        [OM].[MaterialId]
        FROM OrderMaterial OM INNER JOIN (
            SELECT Id
            FROM [Order]
            WHERE Id = @OrderId AND [Status] > 0
        ) AS O ON OM.OrderId = O.Id
        WHERE IsActive = 1
    ) OM ON M.Id = OM.MaterialId
) AS M ON MC.Id = M.MaterialCategoryId
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderMaterialsData]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderMaterialsData]
    @OrderId NVARCHAR(30)
AS
BEGIN
SELECT 
[M].[Id],
[M].[MaterialCategoryId],
[M].[Name],
[M].[Image],
[M].[Quantity],
[M].[CreatedTime],
[M].[LastestUpdatedTime],
[M].[InactiveTime],
[M].[IsActive]
FROM Material M INNER JOIN (
    SELECT 
    [OM].[MaterialId]
    FROM OrderMaterial OM INNER JOIN (
        SELECT Id
        FROM [Order]
        WHERE Id = @OrderId AND [Status] > 0
    ) AS O ON OM.OrderId = O.Id
    WHERE IsActive = 1
) OM ON M.Id = OM.MaterialId
END;
GO
/****** Object:  StoredProcedure [dbo].[GetStaffWithTotalTask]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetStaffWithTotalTask]
AS
BEGIN
SELECT s.Id,s.Avatar,s.Fullname,s.Address,s.Phone,s.Role,st.TotalTask
From Staff s INNER JOIN 
(SELECT S.Id, COUNT(P.Id) as TotalTask
FROM Staff S
LEFT JOIN Product P ON (S.Id = P.StaffMakerId AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5)
LEFT JOIN [Order] O ON (P.OrderId = O.Id AND O.IsActive = 1 AND O.[Status] > 0 AND O.[Status] < 8)
WHERE S.IsActive = 1
GROUP BY S.Id) as st ON s.Id = st.Id
ORDER BY (st.TotalTask)
END;
GO
/****** Object:  StoredProcedure [dbo].[GetSuitableDiscoutForOrder]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[GetSuitableDiscoutForOrder]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM [Order] WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    DECLARE @TotalPrice DECIMAL(18,0) = 0;
    DECLARE @TotalProduct INT = 0;
    SELECT TOP 1 @TotalPrice = TotalPrice, @TotalProduct = TotalProduct
    FROM [Order] 
    WHERE Id = @OrderId AND [Status] > 0

    SELECT 
    [Id],
    [Name],
    [Code],
    [StartDate],
    [EndDate],
    [DiscountPercent],
    [DiscountPrice],
    [ConditionPriceMin],
    [ConditionPriceMax],
    [ConditionProductMin],
    [CreatedTime],
    [LastestUpdatedTime],
    [InactiveTime],
    [IsActive]
    FROM Discount
    WHERE IsActive = 1
    AND StartDate <= DATEADD(HOUR, 7, GETUTCDATE())
    AND EndDate >= DATEADD(HOUR, 7, GETUTCDATE())
    AND (ConditionPriceMin <= @TotalPrice OR ConditionProductMin <= @TotalProduct)
END;
GO
/****** Object:  StoredProcedure [dbo].[GetTemplateComponents]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTemplateComponents]
                    @ProductTemplateId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    IF NOT EXISTS(
        SELECT 1
        FROM ProductTemplate
        WHERE Id = @ProductTemplateId AND IsActive = 1
    )
        THROW 50000, N'Không tìm thấy bản mẫu', 1;

    SELECT CT.*
    FROM Component CT INNER JOIN ProductTemplate PT ON (CT.ProductTemplateId = PT.Id AND CT.IsActive = 1 AND PT.Id = @ProductTemplateId)
END;
GO
/****** Object:  StoredProcedure [dbo].[GetTemplateComponentTypes]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTemplateComponentTypes]
                    @ProductTemplateId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    IF NOT EXISTS(
        SELECT 1
        FROM ProductTemplate
        WHERE Id = @ProductTemplateId AND IsActive = 1
    )
        THROW 50000, N'Không tìm thấy bản mẫu', 1;

    SELECT CT.*
    FROM ComponentType CT INNER JOIN ProductTemplate PT ON (CT.CategoryId = PT.CategoryId AND CT.IsActive = 1 AND PT.Id = @ProductTemplateId)
END;
GO
/****** Object:  StoredProcedure [dbo].[GetTemplateDashboard]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTemplateDashboard]
    @Date DATETIME
AS
BEGIN
SELECT [PT].[Id],
[PT].[Name],
[PT].[ThumbnailImage],
B.Total
FROM ProductTemplate PT LEFT JOIN (
    SELECT P.ProductTemplateId, COUNT(P.Id) AS Total
    FROM Product P INNER JOIN (
        Select 
            Id
        FROM 
            [Order]
        WHERE 
            IsActive = 1 
            AND [Status] = 8
            AND MONTH(CreatedTime) = MONTH(@Date) 
            AND YEAR(CreatedTime) = YEAR(@Date)
    ) AS A ON P.OrderId = A.Id AND P.IsActive = 1
    GROUP BY P.ProductTemplateId
) AS B ON PT.Id = B.ProductTemplateId
WHERE PT.IsActive = 1 OR PT.Id IN (
    SELECT P.ProductTemplateId
    FROM Product P INNER JOIN (
        Select 
            Id
        FROM 
            [Order]
        WHERE 
            IsActive = 1 
            AND [Status] = 8
            AND MONTH(CreatedTime) = MONTH(@Date) 
            AND YEAR(CreatedTime) = YEAR(@Date)
    ) AS A ON P.OrderId = A.Id AND P.IsActive = 1
    GROUP BY P.ProductTemplateId
)
END;
GO
/****** Object:  StoredProcedure [dbo].[GetTotalFabricMaterialCommonUsed]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTotalFabricMaterialCommonUsed]
    @StartDate DATETIME
AS
BEGIN
    SELECT M.Id,M.Name,M.[Image],PO.TotalProducts, 0 AS TotalOrders, M.Quantity
    FROM Material M LEFT JOIN (
        SELECT P.FabricMaterialId, COUNT(P.Id) AS TotalProducts
        FROM Product P INNER JOIN (
            SELECT Id
            FROM [Order]
            WHERE [Status] = 8
            AND IsActive = 1
            AND MONTH(CreatedTime) = MONTH(@StartDate)
            AND YEAR(CreatedTime) = YEAR(@StartDate)
        ) AS O ON P.OrderId = O.Id
        WHERE P.[Status] > 0
        AND P.IsActive = 1
        GROUP BY P.FabricMaterialId
    ) AS PO ON M.Id = PO.FabricMaterialId
    WHERE M.IsActive = 1
END;
GO
/****** Object:  StoredProcedure [dbo].[InsertChatList]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertChatList]
    @ChatId NVARCHAR(30),
    @Message NVARCHAR(500) = NULL,
    @Images TEXT = NULL,
    @ReplierId NVARCHAR(30) = NULL,
    @CustomerId NVARCHAR(30) = NULL,
    @ReturnValue INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FromCustomer BIT = CASE WHEN @CustomerId IS NOT NULL AND LEN(@CustomerId) > 0 THEN 1 ELSE 0 END
    
    IF(@Message IS NOT NULL)
    BEGIN
        DECLARE @Id1 NVARCHAR(30) = (SELECT LEFT(CAST(NEWID() AS NVARCHAR(36)), 30))
        INSERT INTO ChatList (Id, ChatId, ReplierId, [Message], FromCus, SendTime, IsRead, ReadTime, InactiveTime, IsActive, Images)
        VALUES (@Id1, @ChatId, @ReplierId, @Message, @FromCustomer, DATEADD(HOUR, 7, GETUTCDATE()), 0, NULL, NULL, 1, NULL)
    END

    IF(@Images IS NOT NULL)
    BEGIN
        DECLARE @Id2 NVARCHAR(30) = (SELECT LEFT(CAST(NEWID() AS NVARCHAR(36)), 30))
        INSERT INTO ChatList (Id, ChatId, ReplierId, [Message], FromCus, SendTime, IsRead, ReadTime, InactiveTime, IsActive, Images)
        VALUES (@Id2, @ChatId, @ReplierId, NULL, @FromCustomer, DATEADD(HOUR, 7, GETUTCDATE()), 0, NULL, NULL, 1, @Images)
    END
    SET @ReturnValue = 1;
    RETURN @ReturnValue
END
GO
/****** Object:  StoredProcedure [dbo].[ReadAllNotification]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create the stored procedure in the specified schema
CREATE PROCEDURE [dbo].[ReadAllNotification]
    @UserId NVARCHAR(30),
    @Role INT 
AS
BEGIN
    SET NOCOUNT ON;

    IF @Role = 3
    UPDATE [Notification] SET IsRead = 1, ReadTime = DATEADD(HOUR, 7, GETUTCDATE()) WHERE CustomerId = @UserId AND IsRead = 0;
    ELSE
    UPDATE [Notification] SET IsRead = 1, ReadTime = DATEADD(HOUR, 7, GETUTCDATE()) WHERE StaffId = @UserId AND IsRead = 0;

    SELECT 1 AS ReturnValue;
END;
GO
/****** Object:  StoredProcedure [dbo].[SetMaterialForTask]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetMaterialForTask]
    @TaskId NVARCHAR(30) NULL,
    @StageId NVARCHAR(30) NULL,
    @StageMaterials dbo.MaterialStageType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- DECLARE @Errmsg NVARCHAR(MAX) = '';

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @TaskStatus INT;
    DECLARE @TaskStageStatus INT;
    DECLARE @InsertTable TABLE (
        Id NVARCHAR(30) NULL,
        StageId NVARCHAR(30) NULL,
        MaterialId NVARCHAR(30) NULL,
        Quantity DECIMAL (18,3) NULL
    );

    IF NOT EXISTS(SELECT 1
    FROM Product
    WHERE Id = @TaskId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nhiệm vụ', 1;
    ELSE 
    BEGIN
        SELECT @OrderId = OrderId, @TaskStatus = [Status]
        FROM Product
        WHERE Id = @TaskId AND IsActive = 1;
        IF @TaskStatus = 0
            THROW 50000, N'Nhiệm vụ bị hủy', 1;
        ELSE IF @TaskStatus = 5
            THROW 50000, N'Nhiệm vụ đã hoàn thành', 1;

    END;

    SELECT @OrderStatus = [Status]
    FROM [Order]
    WHERE Id = @OrderId AND IsActive = 1;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn bị hủy', 1;
    ELSE IF @OrderStatus = 1
        THROW 50000, N'Hóa đơn chưa được xác nhận', 1;
    ELSE IF @OrderStatus = 5
        THROW 50000, N'Các sản phẩm của hóa đơn đã xong', 1;
    ELSE IF @OrderStatus = 6
        THROW 50000, N'Hóa đơn đang chờ khách hàng kiểm thử', 1;
    ELSE IF @OrderStatus = 8
        THROW 50000, N'Hóa đơn đã hoàn thành', 1;

    SELECT @TaskStageStatus = [Status]
    FROM ProductStage
    WHERE Id = @StageId AND IsActive = 1;

    IF @TaskStageStatus IS NULL
        THROW 50000, N'Không tìm thấy quy trình của nhiệm vụ', 1;
    ELSE IF @TaskStageStatus = 0
        THROW 50000, N'Quy trình bị hủy', 1;
    ELSE IF @TaskStageStatus = 5
        THROW 50000, N'Quy trình đã hoàn thành', 1;

    IF NOT EXISTS (SELECT 1
    FROM @StageMaterials)
        THROW 50000, N'Không có nguyên phụ liệu', 1;

    DECLARE @MaterialId NVARCHAR(30);
    DECLARE @Value DECIMAL (18,3);
    DECLARE @ExistProductStageMaterialId NVARCHAR(30);

    DECLARE StageMaterialsCursor CURSOR FOR
    SELECT MaterialId, [Value]
    FROM @StageMaterials;

    OPEN StageMaterialsCursor;
    FETCH NEXT FROM StageMaterialsCursor INTO @MaterialId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (SELECT COUNT(*)
        FROM @StageMaterials
        WHERE MaterialId = @MaterialId) > 1
            THROW 50000, N'Nguyên phụ liệu bị trùng', 1;
        IF @Value IS NULL OR @Value < 0
            THROW 50000, N'Số lượng nguyên phụ liệu không phù hợp', 1;

        DECLARE @MaterialName NVARCHAR(100);
        DECLARE @StockValue DECIMAL (18,3);
        DECLARE @ValueRecieve DECIMAL (18,3);
        DECLARE @ValueUsed DECIMAL (18,3);
        DECLARE @OldValue DECIMAL (18,3);

        SET @ExistProductStageMaterialId = NULL;

        SELECT @StockValue = [Quantity], @MaterialName = [Name]
        FROM Material
        WHERE Id = @MaterialId AND IsActive = 1;

        SELECT @ExistProductStageMaterialId = Id, @OldValue = Quantity
        FROM ProductStageMaterial
        WHERE MaterialId = @MaterialId AND ProductStageId = @StageId;

        IF @MaterialName IS NULL
            THROW 50000, N'Không tìm thấy nguyên phụ liệu', 1;
        ELSE
        BEGIN
            DECLARE @ErrorMsg NVARCHAR(MAX);
            
            IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1)
            BEGIN
            SET @ErrorMsg  = CAST(N'Số lượng ' + @MaterialName + N' trong kho không đủ' AS NVARCHAR(MAX));
            IF (@StockValue = 0 OR (@StockValue + ISNULL(@OldValue, 0)) < @Value)
                THROW 50000, @ErrorMsg, 1;
            END;
            ELSE
            BEGIN
            SET @ErrorMsg  = CAST(N'Số lượng ' + @MaterialName + N' của khách không đủ' AS NVARCHAR(MAX));
            SELECT @ValueRecieve = [Value], @ValueUsed = [ValueUsed] FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1;
            IF (@ValueRecieve = 0 OR (@ValueUsed - ISNULL(@OldValue, 0) + @Value) > @ValueRecieve)
                THROW 50000, @ErrorMsg, 1;
            END;
        END;


        INSERT INTO @InsertTable
            (Id, StageId, MaterialId, Quantity)
        VALUES
            (ISNULL(@ExistProductStageMaterialId, NULL), @StageId, @MaterialId, @Value)
        
        -- SET @Errmsg = @Errmsg + 'SetupData: ' + @MaterialId + ', Id :' + ISNULL(@ExistProductStageMaterialId, 'Null') + ' ';

        FETCH NEXT FROM StageMaterialsCursor INTO @MaterialId, @Value;
    END;
    CLOSE StageMaterialsCursor;
    DEALLOCATE StageMaterialsCursor;


    DECLARE @ProductStageMaterialIdExist NVARCHAR(30);
    DECLARE CheckMaterial CURSOR FOR
    SELECT Id, MaterialId, Quantity
    FROM ProductStageMaterial
    WHERE ProductStageId = @StageId AND MaterialId NOT IN (
        SELECT MaterialId
        FROM @InsertTable
    )

    OPEN CheckMaterial;
    FETCH NEXT FROM CheckMaterial INTO @ProductStageMaterialIdExist, @MaterialId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        UPDATE Material SET Quantity = ISNULL(Quantity, 0) + ISNULL(@Value, 0)
        WHERE Id = @MaterialId

        IF @ProductStageMaterialIdExist IS NOT NULL
        DELETE ProductStageMaterial WHERE Id = @ProductStageMaterialIdExist

        FETCH NEXT FROM CheckMaterial INTO @ProductStageMaterialIdExist, @MaterialId, @Value;
    END;
    CLOSE CheckMaterial;
    DEALLOCATE CheckMaterial;

    DECLARE @ProductStageMaterialId NVARCHAR(30);
    DECLARE UpdateMaterial CURSOR FOR
    SELECT Id, MaterialId, Quantity
    FROM @InsertTable

    OPEN UpdateMaterial;
    FETCH NEXT FROM UpdateMaterial INTO @ProductStageMaterialId, @MaterialId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN

        IF @ProductStageMaterialId IS NULL
        BEGIN
            INSERT INTO ProductStageMaterial
                (Id, ProductStageId, MaterialId, Quantity)
            VALUES
                (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
                    @StageId,
                    @MaterialId,
                    @Value);

            IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsActive = 1 AND IsCusMaterial = 1)
            UPDATE Material SET Quantity = Quantity - @Value WHERE Id = @MaterialId;
            ELSE
            UPDATE OrderMaterial SET ValueUsed = ValueUsed + @Value WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsActive = 1 AND IsCusMaterial = 1;
        END;
        ELSE
        BEGIN
            SELECT @OldValue = Quantity
            FROM ProductStageMaterial
            WHERE Id = @ProductStageMaterialId;

            IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsActive = 1 AND IsCusMaterial = 1)
            UPDATE Material SET Quantity = Quantity + ISNULL(@OldValue, 0) - @Value WHERE Id = @MaterialId;
            ELSE
            UPDATE OrderMaterial SET ValueUsed = ValueUsed - ISNULL(@OldValue, 0) + @Value WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsActive = 1 AND IsCusMaterial = 1;

            UPDATE ProductStageMaterial
            SET Quantity = @Value
            WHERE Id = @ProductStageMaterialId;
        END;
        FETCH NEXT FROM UpdateMaterial INTO @ProductStageMaterialId, @MaterialId, @Value;
    END;
    CLOSE UpdateMaterial;
    DEALLOCATE UpdateMaterial;

    SELECT 1 AS ReturnValue;
END;
GO
/****** Object:  StoredProcedure [dbo].[StartTask]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[StartTask]
    @ProductId NVARCHAR(30),
    @ProductStageId NVARCHAR(30),
    @StaffId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @ProductStatus INT;
    DECLARE @ProductIndex INT;
    DECLARE @ProductStageStatus INT;
    DECLARE @ProductStageNum INT;

    SELECT @OrderId = OrderId, @ProductStatus = [Status], @ProductIndex = [Index]
    FROM Product
    WHERE Id = @ProductId AND StaffMakerId = @StaffId AND IsActive = 1;

    IF @OrderId IS NULL OR @ProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    IF @ProductStatus = 0
        THROW 50000, N'Sản phẩm bị hủy', 1;
    ELSE IF @ProductStatus = 5
        THROW 50000, N'Sản phẩm đã hoàn thành', 1;

    SELECT @OrderStatus = [Status]
    FROM [Order]
    WHERE Id = @OrderId AND IsActive = 1;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn bị hủy', 1;
    ELSE IF @OrderStatus = 1
        THROW 50000, N'Hóa đơn chưa được duyệt', 1;
    ELSE IF @OrderStatus = 2
        THROW 50000, N'Hóa đơn chưa được bắt đầu', 1;
    ELSE IF @OrderStatus = 5
        THROW 50000, N'Hóa đơn đã hoàn thiện', 1;
    ELSE IF @OrderStatus = 6
        THROW 50000, N'Hóa đơn đang chờ khách kiểm duyệt', 1;
    ELSE IF @OrderStatus = 8
        THROW 50000, N'Hóa đơn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM Product P INNER JOIN [Order] O ON P.OrderId = O.Id 
        WHERE P.StaffMakerId = @StaffId AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND P.[Index] IS NOT NULL AND P.[Index] < @ProductIndex AND O.[Status] > 0 AND O.IsActive = 1)
        THROW 50000, N'Nhiệm vụ trước chưa hoàn thành. Vui lòng thực hiện theo trình tự', 1;

    SELECT @ProductStageStatus = [Status], @ProductStageNum = StageNum
    FROM ProductStage
    WHERE Id = @ProductStageId AND IsActive = 1;

    IF @ProductStageStatus IS NULL
        THROW 50000, N'Không tìm thấy nhiệm vụ', 1;
    ELSE IF @ProductStageStatus = 0
        THROW 50000, N'Công đoạn bị hủy', 1;
    ELSE IF @ProductStageStatus = 2
        THROW 50000, N'Công đoạn đang thực hiện', 1;
    ELSE IF @ProductStageStatus = 4 AND @ProductStatus NOT LIKE 4
        THROW 50000, N'Công đoạn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 1 AND [Status] < 4 AND IsActive = 1)
        THROW 50000, N'Có công đoạn đang chờ hoặc đang thực hiện. Vui lòng hoàn thành trước khi bắt đầu công đoạn này', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 0 AND [Status] < 4 AND IsActive = 1 AND StageNum < @ProductStageNum)
        THROW 50000, N'Công đoạn trước chưa hoàn thành. Vui lòng thực hiện theo trình tự', 1;

    IF NOT EXISTS (SELECT 1 FROM ProductStageMaterial WHERE ProductStageId = @ProductStageId)
        THROW 50000, N'Công đoạn chưa được phân nguyên phụ liệu. Vui lòng liên hệ quản lý để phân nguyên liệu cho công đoạn.', 1;

    UPDATE Product SET 
    [Status] = 2,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @ProductId;

    IF @OrderStatus = 7
    UPDATE [Order] SET
    [Status] = 7,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @OrderId;
    ELSE
    UPDATE [Order] SET
    [Status] = 4,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @OrderId;

    UPDATE ProductStage SET
    [Status] = 2,
    StaffId = @StaffId,
    StartTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @ProductStageId;


    SELECT 1 AS ReturnValue;
END;
GO
/****** Object:  StoredProcedure [dbo].[UpdateProduct]    Script Date: 5/30/2024 3:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[UpdateProduct]
    @OrderId NVARCHAR(30),
    @ProductId NVARCHAR(30),
    @ProductTemplateId NVARCHAR(30) NULL,
    @NewFabricMaterialId NVARCHAR(30) NULL,
    @MaterialValue DECIMAL(18,3),
    @IsCusMaterial BIT,
    @NewProductName NVARCHAR(500) NULL,
    @NewSaveOrderComponents NVARCHAR(MAX) NULL,
    @NewNote NVARCHAR(255) NULL,
    @NewProfileBodyId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @ProducPrice DECIMAL(18, 0) = 0;
    DECLARE @MaterialPrice DECIMAL(18, 0);
    DECLARE @MaterialCategoryPricePerUnit DECIMAL(18, 2);
    DECLARE @CustomerId NVARCHAR(50);
    DECLARE @StaffMakeProfileBody NVARCHAR(50);
    DECLARE @OldProductTemplateId NVARCHAR(30);
    DECLARE @OldProfileBodyId NVARCHAR(30);
    DECLARE @OldProductName NVARCHAR(500);
    DECLARE @OldFabricMaterialId NVARCHAR(30);
    DECLARE @OldProductStatus INT;
    DECLARE @ProductBodySize TABLE (
        BodySizeId NVARCHAR(30),
        [Value] DECIMAL(18,0)
    );
    DECLARE @OrderPlannedTime DATETIME;

    IF @NewFabricMaterialId IS NULL
        THROW 50000, N'Vui lòng chọn loại vải chính cho sản phẩm', 1;

    SELECT @OrderStatus = [Status], @CustomerId = CustomerId, @OrderPlannedTime = PlannedTime
    FROM [Order]
    WHERE Id = @orderId AND IsActive = 0;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus >= 2
        THROW 50000, N'Đơn hàng đã duyệt. Không thể chỉnh sửa', 1;

    SELECT @OldFabricMaterialId = FabricMaterialId, @OldProductName = [Name], @OldProductStatus = [Status], @OldProfileBodyId = ReferenceProfileBodyId, @OldProductTemplateId = ProductTemplateId
    FROM Product
    WHERE Id = @ProductId AND [Status] > 0 AND IsActive = 1;

    IF @OldProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;
    IF @OldProductStatus > 1
        THROW 50000, N'Sản phẩm trong giai đoạn thực hiện. Không thể chỉnh sửa', 1;
    IF @OldProductTemplateId NOT LIKE @ProductTemplateId
        THROW 50000, N'Không thể thay đổi bản mẫu của sản phẩm', 1;

    SELECT @ProducPrice = [Price],
        @NewProductName = CASE WHEN @NewProductName IS NULL THEN [Name] ELSE @NewProductName END
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF @ProducPrice IS NULL
    BEGIN
        RAISERROR('Không tìm thấy bản mẫu', 16, 1);
        RETURN;
    END
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    SELECT @ProducPrice = [Price]
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF NOT EXISTS(SELECT 1
    FROM [Material]
    WHERE Id = @NewFabricMaterialId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nguyên liệu', 1;
    ELSE
    BEGIN
        SELECT @MaterialPrice = MC.PricePerUnit
        FROM MaterialCategory MC INNER JOIN Material M ON MC.Id = M.MaterialCategoryId

        IF @NewFabricMaterialId NOT LIKE @OldFabricMaterialId
        BEGIN
            IF NOT EXISTS (SELECT 1
            FROM Product
            WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND FabricMaterialId = @OldFabricMaterialId AND [Status] > 0 AND IsActive = 1)
            BEGIN
                IF NOT EXISTS ( SELECT 1
                FROM OrderMaterial
                WHERE OrderId = @OrderId AND MaterialId = @OldFabricMaterialId AND IsActive = 1 AND IsCusMaterial = 1)
                DELETE OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @OldFabricMaterialId;
            END;

            IF NOT EXISTS (SELECT 1
            FROM Product
            WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND FabricMaterialId = @NewFabricMaterialId AND [Status] > 0 AND IsActive = 1)
                BEGIN
                IF NOT EXISTS ( SELECT 1
                FROM OrderMaterial
                WHERE OrderId = @OrderId AND MaterialId = @NewFabricMaterialId AND IsActive = 1 AND IsCusMaterial = 1)
                INSERT INTO [dbo].[OrderMaterial]
                    ([Id]
                    ,[MaterialId]
                    ,[OrderId]
                    ,[Image]
                    ,[Value]
                    ,[CreatedTime]
                    ,[LastestUpdatedTime]
                    ,[InactiveTime]
                    ,[IsActive]
                    ,[IsCusMaterial]
                    ,[ValueUsed])
                VALUES
                    (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
                        @NewFabricMaterialId,
                        @OrderId,
                        NULL,
                        @MaterialValue,
                        DATEADD(HOUR, 7, GETUTCDATE()),
                        DATEADD(HOUR, 7, GETUTCDATE()),
                        NULL,
                        1,
                        @IsCusMaterial,
                        0)
            END;
        END;
    END;

    IF(@IsCusMaterial = 0)
    SET @ProducPrice = @ProducPrice + ROUND((@MaterialPrice * @MaterialValue), 2);
    ELSE
    SET @ProducPrice = @ProducPrice

    IF NOT EXISTS(SELECT 1
    FROM ProfileBody
    WHERE Id = @NewProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy hồ sơ số đo của khách', 1;
    ELSE
    BEGIN
        SELECT @StaffMakeProfileBody = StaffId
        FROM ProfileBody
        WHERE Id = @NewProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1
        IF(@StaffMakeProfileBody IS NULL)
        THROW 50000, N'Hồ sơ số đo này được đo bởi khách. Vui lòng kiểm tra và cập nhật lại hồ sơ số đo', 1;
        ELSE
        BEGIN
            INSERT INTO @ProductBodySize
                (BodySizeId,[Value])
            SELECT BS.Id AS BodySizeId, BA.[Value]
            FROM TemplateBodySize TBS LEFT JOIN BodySize BS ON (TBS.BodySizeId = BS.Id AND TBS.IsActive = 1)
                LEFT JOIN BodyAttribute BA ON (BS.Id = BA.BodySizeId AND BA.IsActive = 1)
                LEFT JOIN ProfileBody PB ON (BA.ProfileBodyId = PB.Id AND PB.IsActive = 1)
            WHERE PB.Id = @NewProfileBodyId AND TBS.ProductTemplateId = @ProductTemplateId

            IF EXISTS(SELECT 1
            FROM @ProductBodySize
            WHERE [Value] IS NULL OR [Value] = 0)
            THROW 50000, N'Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung', 1;
        END;
    END;

    UPDATE [dbo].[Product]
    SET [Name] = @NewProductName
        ,[Note] = @NewNote
        ,[Status] = 1
        ,[LastestUpdatedTime] = DATEADD(HOUR, 7, GETUTCDATE())
        ,[IsActive] = 1
        ,[Price] = @ProducPrice
        ,[SaveOrderComponents] = @NewSaveOrderComponents
        ,[FabricMaterialId] = @NewFabricMaterialId
        ,[ReferenceProfileBodyId] = @NewProfileBodyId
    WHERE Id = @ProductId;

    UPDATE ProductBodySize SET IsActive = 0 WHERE ProductId = @ProductId;

    DECLARE @BodySizeId NVARCHAR(30);
    DECLARE @Value DECIMAL(18,0);

    DECLARE ProductBodySizeCursor CURSOR FOR
    SELECT BodySizeId, [Value]
    FROM @ProductBodySize;

    OPEN ProductBodySizeCursor;
    FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @ProductBodySizeId NVARCHAR(30) = CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0);
        IF NOT EXISTS(SELECT 1
        FROM ProductBodySize
        WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId)
        BEGIN
            INSERT INTO [dbo].[ProductBodySize]
                ([Id]
                ,[ProductId]
                ,[BodySizeId]
                ,[Value]
                ,[CreatedTime]
                ,[LastestUpdatedTime]
                ,[InactiveTime]
                ,[IsActive])
            VALUES
                (@ProductBodySizeId,
                    @ProductId,
                    @BodySizeId,
                    @Value,
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    NULL,
                    1)
        END;
        ELSE
        BEGIN
            UPDATE ProductBodySize SET IsActive = 1, [Value] = @Value WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId;
        END;

        FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;
    END

    CLOSE ProductBodySizeCursor;
    DEALLOCATE ProductBodySizeCursor;

    EXECUTE [dbo].[CheckNumOfDateToFinish];

    DECLARE @DayToFinish INT = dbo.CalculateNumOfDateToFinish(@ProductId);
    DECLARE @DateToFinish DATETIME;

    IF @DayToFinish > 0
    BEGIN
        SET @DateToFinish = DATEADD(DAY,@DayToFinish,DATEADD(HOUR, 7, GETUTCDATE()));

        UPDATE Product SET PlannedTime = @DateToFinish WHERE Id = @ProductId;
        IF @OrderPlannedTime IS NULL OR @OrderPlannedTime < @DateToFinish
    UPDATE [Order] SET PlannedTime = @DateToFinish WHERE Id = @OrderId;
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
USE [master]
GO
ALTER DATABASE [ETailor_DB] SET  READ_WRITE 
GO
