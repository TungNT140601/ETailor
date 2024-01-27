--Use Master;
--Drop database ETailor_DB

--Create database ETailor_DB
--Go

Use ETailor_DB
Go

Create Table Staff ( 
	Id nvarchar(30) Primary Key,
	Avatar text,
	Fullname nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Username nvarchar(255),
	[Password] nvarchar(255),
	SecrectKeyLogin nvarchar(20) null,
	LastLoginDeviceToken NVARCHAR(255) NULL,
	[Role] int null default 2,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
);

Create Table Customer ( 
	Id nvarchar(30) Primary Key,
	Avatar text null,
	Fullname nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Email nvarchar(255) null,
	Gender INT NULL DEFAULT 0,
	Username nvarchar(255) null,
	[Password] nvarchar(255) null,
	OTPNumber nvarchar(10) null, 
	OTPTimeLimit datetime NULL,
	OTPUsed [bit] NULL DEFAULT 0,
	PhoneVerified [bit] NULL DEFAULT 0,
	EmailVerified [bit] NULL DEFAULT 0,
	SecrectKeyLogin nvarchar(20) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
);

Create Table ProfileBody (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	StaffId nvarchar(30) null FOREIGN KEY REFERENCES Staff(Id),
	[Name] nvarchar(100),
	IsLocked [bit] NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table BodySize (
	Id nvarchar(30) Primary Key,
	BodyPart nvarchar(50) null,
	BodyIndex int null default 0,
	[Name] nvarchar(100) null,
	[Image] text null, 
	GuideVideoLink text null,
	MinValidValue decimal null,
	MaxValidValue decimal null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table BodyAttribute (
	Id nvarchar(30) Primary Key,
	ProfileBodyId nvarchar(30) FOREIGN KEY REFERENCES ProfileBody(Id),
	BodySizeId nvarchar (30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] decimal null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table Category (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1,
)

CREATE TABLE Mastery(
	Id nvarchar(30) Primary Key,
	CategoryId nvarchar(30) null FOREIGN KEY REFERENCES Category(Id),
	StaffId nvarchar(30) null FOREIGN KEY REFERENCES Staff(Id),
)

Create Table ProductTemplate (
	Id nvarchar(30) Primary Key,
	CategoryId nvarchar(30) FOREIGN KEY REFERENCES Category(Id) NOT NULL,
	[Name] nvarchar(255) null,
	[Description] nvarchar(2550) null,
	Price decimal null default 0,
	ThumbnailImage text null,
	[Image] text null,
	CollectionImage TEXT NULL,
	UrlPath nvarchar(255) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table TemplateBodySize (
	Id nvarchar(30) Primary Key,
	ProductTemplateId nvarchar(30) FOREIGN KEY REFERENCES ProductTemplate(Id),
	BodySizeId nvarchar(30) FOREIGN KEY REFERENCES BodySize(Id)
)

Create Table TemplateStage (
	Id nvarchar(30) Primary Key,
	ProductTemplateId nvarchar(30) FOREIGN KEY REFERENCES ProductTemplate(Id),
	TemplateStageId nvarchar(30) null FOREIGN KEY REFERENCES TemplateStage(Id),
	[Name] nvarchar(155) null,
	StageNum int null default 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table ComponentType (
	Id nvarchar(30) Primary Key,
	CategoryId nvarchar(30) FOREIGN KEY REFERENCES Category(Id),
	[Name] nvarchar(100) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table ComponentStage (
	Id nvarchar(30) Primary Key,
	ComponentTypeId nvarchar(30) FOREIGN KEY REFERENCES ComponentType(Id),
	TemplateStageId nvarchar(30) FOREIGN KEY REFERENCES TemplateStage(Id),
)

Create Table Component (
	Id nvarchar(30) Primary Key,
	ComponentTypeId nvarchar(30) FOREIGN KEY REFERENCES ComponentType(Id),
	ProductTemplateId nvarchar(30) FOREIGN KEY REFERENCES ProductTemplate(Id),
	[Name] nvarchar(100),
	[Image] text,
	CreatedTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)


Create Table Discount (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(255) null,
	Code nvarchar(30) null,
	StartDate datetime Null,
	EndDate datetime Null, 
	DiscountPercent decimal null,
	DiscountPrice decimal null,
	ConditionPriceMin decimal null,
	ConditionPriceMax decimal null,
	ConditionProductMin int null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table [Order] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	CreaterId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	DiscountId nvarchar(30) null FOREIGN KEY REFERENCES Discount(Id),
	TotalProduct int null default 0,
	TotalPrice decimal null default 0,
	DiscountPrice decimal null,
	DiscountCode nvarchar(30) null,
	AfterDiscountPrice decimal null,
	PayDeposit [bit] NULL DEFAULT 0,
	Deposit decimal null,
	PaidMoney decimal null,
	UnPaidMoney decimal null,
	[Status] int default 1,
	CancelTime datetime Null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)


Create Table Product (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	ProductTemplateId nvarchar(30) FOREIGN KEY REFERENCES ProductTemplate(Id),
	[Name] nvarchar(255) null,
	Note nvarchar(255) null,
	[Status] int null default 1,
	EvidenceImage TEXT NULL,
	FinishTime DATETIME NULL,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1,
)

Create Table ProductStage (
	Id nvarchar(30) Primary Key,
	StaffId nvarchar(30) FOREIGN KEY REFERENCES [Staff](Id),
	TemplateStageId nvarchar(30) FOREIGN KEY REFERENCES TemplateStage(Id),
	ProductId nvarchar(30) FOREIGN KEY REFERENCES [Product](Id),
	StageNum INT NULL DEFAULT 0,
	TaskIndex INT NULL DEFAULT 0,
	StartTime datetime Null,
	FinishTime datetime Null,
	Deadline datetime NULL,
	[Status] INT NULL DEFAULT 0,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1,
)


Create Table MaterialType (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	Unit nvarchar(10),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table MaterialCategory (
	Id nvarchar(30) Primary Key,
	MaterialTypeId nvarchar(30) FOREIGN KEY REFERENCES MaterialType(Id),
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table Material (
	Id nvarchar(30) Primary Key,
	MaterialCategoryId nvarchar(30) FOREIGN KEY REFERENCES MaterialCategory(Id),
	[Name] nvarchar(100),
	[Image] text null,
	Quantity decimal null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table ProductComponent (
	Id nvarchar(30) Primary Key,
	ComponentId nvarchar(30) FOREIGN KEY REFERENCES Component(Id),
	ProductStageId nvarchar(30) FOREIGN KEY REFERENCES ProductStage(Id),
	[Name] nvarchar(100),
	[Image] text null,
	LastestUpdatedTime datetime Null
)

CREATE TABLE ProductComponentMaterial (
	Id nvarchar(30) Primary Key,
	ProductComponentId NVARCHAR(30) FOREIGN KEY REFERENCES dbo.ProductComponent(Id),
	MaterialId NVARCHAR(30) FOREIGN KEY REFERENCES dbo.Material(Id),
	Quantity DECIMAL NULL 
)

Create Table OrderMaterial (
	Id nvarchar(30) Primary Key,
	MaterialId nvarchar(30) FOREIGN KEY REFERENCES Material(Id),
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	[Image] text null,
	[Value] DECIMAL(18,3) NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table Payment (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	[Platform] nvarchar(50),
	Amount decimal,
	PayTime datetime Null, 
	CreatedTime datetime Null,
	[Status] int null default 0
)

Create Table Chat (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	CreatedTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table ChatHistory (
	Id nvarchar(30) Primary Key,
	ChatId nvarchar(30) FOREIGN KEY REFERENCES Chat(Id),
	ReplierId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	[Message] nvarchar(500),
	FromCus [bit] NULL DEFAULT 1,
	SendTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	ReadTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table [Notification] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id) null,
	StaffId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	Title nvarchar(255) null,
	Content nvarchar(Max) null,
	SendTime datetime Null,
	ReadTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	IsActive [bit] NULL DEFAULT 1
)

Create Table Blog (
	Id nvarchar(30) Primary Key,
	Title nvarchar(255) null,
	UrlPath nvarchar(255) null,
	Content nvarchar(max) null,
	CreatedTime datetime Null,
	StaffId nvarchar(30) null FOREIGN KEY REFERENCES [Staff](Id),
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table ProductBodySize (
	Id nvarchar(30) Primary Key,
	ProductId nvarchar(30) FOREIGN KEY REFERENCES Product(Id),
	BodySizeId nvarchar(30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] decimal null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 1
)

Create Table CustomerClient (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	ClientToken nvarchar(255),
	LastLogin datetime null,
	IpAddress nvarchar(30)
)