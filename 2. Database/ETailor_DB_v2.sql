Use Master;
Drop database ETailor_DB

Create database ETailor_DB
Go

Use ETailor_DB
Go

Create Table [Role] (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(55),
	IsActive [bit] NULL DEFAULT 0
);

Create Table Skill (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(55),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
);

Create Table Staff ( 
	Id nvarchar(30) Primary Key,
	RoleId nvarchar(30) FOREIGN KEY REFERENCES [Role](Id),
	Avatar text,
	[Fullname] nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Email nvarchar(255) null,
	Username nvarchar(255),
	[Password] nvarchar(255),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
);

Create Table SkillOfStaff (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(55),
	SkillId nvarchar(30) FOREIGN KEY REFERENCES Skill(Id),
	StaffId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
);

Create Table Customer ( 
	Id nvarchar(30) Primary Key,
	RoleId nvarchar(30) FOREIGN KEY REFERENCES [Role](Id),
	Avatar text,
	[Fullname] nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Email nvarchar(255) null,
	Username nvarchar(255),
	[Password] nvarchar(255),
	OTPNumber nvarchar(10), 
	OTPTimeLimit datetime NULL,
	OTPUsed [bit] NULL DEFAULT 0,
	PhoneVerified [bit] NULL DEFAULT 0,
	EmailVerified [bit] NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
);


Create Table ProfileBody (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	StaffId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	[Name] nvarchar(100),
	IsLocked [bit] NULL DEFAULT 0,
	IsByStaff [bit] NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table BodySize (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	[Image] text null, 
	GuideVideoLink text null,
	MinValidValue float,
	MaxValidValue float,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table BodyAttribute (
	Id nvarchar(30) Primary Key,
	ProfileBodyId nvarchar(30) FOREIGN KEY REFERENCES ProfileBody(Id),
	BodySizeId nvarchar (30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] float,
	Measure nvarchar (10),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Category (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0,
)

Create Table [Catalog] (
	Id nvarchar(30) Primary Key,
	CategoryId nvarchar(30) FOREIGN KEY REFERENCES Category(Id),
	[Name] nvarchar(100),
	[Description] nvarchar (255),
	Price float,
	[Image] text,
	UrlPath nvarchar(255),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table CatalogBodySize (
	Id nvarchar(30) Primary Key,
	CatalogId nvarchar(30) FOREIGN KEY REFERENCES [Catalog](Id),
	BodySizeId nvarchar(30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] float,
	Measure nvarchar(10),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table CatalogStage (
	Id nvarchar(30) Primary Key,
	CatalogId nvarchar(30) FOREIGN KEY REFERENCES [Catalog](Id),
	CatalogStageId nvarchar(30) FOREIGN KEY REFERENCES CatalogStage(Id),
	[Name] nvarchar(100),
	StageNum int,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Discount (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(255),
	Code nvarchar(30),
	StartDate datetime Null,
	EndDate datetime Null, 
	DiscountPercent float null,
	DiscountPrice nvarchar(30) null,
	ConditionPriceMin float null,
	ConditionPriceMax float null,
	ConditionProductMin int null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table [Order] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	StaffId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	DiscountId nvarchar(30) FOREIGN KEY REFERENCES Discount(Id) null,
	TotalProduct int,
	TotalPrice float,
	DiscountPrice float null,
	DiscountCode nvarchar(30) null,
	AfterDiscountPrice float null,
	PayDeposit [bit] NULL DEFAULT 0,
	Deposit float null,
	PaidMoney float null,
	UnPaidMoney float null,
	IsApproved [bit] NULL DEFAULT 0,
	ApproveTime datetime Null,
	IsOrdered [bit] NULL DEFAULT 0,
	IsCanceled [bit] NULL DEFAULT 0,
	CancelTime datetime Null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)


Create Table Product (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	CatalogId nvarchar(30) FOREIGN KEY REFERENCES [Catalog](Id),
	[Name] nvarchar(100),
	[Status] nvarchar(30),
	StatusPercent float,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0,
)

Create Table ProductStage (
	Id nvarchar(30) Primary Key,
	CatalogId nvarchar(30) FOREIGN KEY REFERENCES [Catalog](Id),
	CatalogStageId nvarchar(30) FOREIGN KEY REFERENCES CatalogStage(Id),
	ProductId nvarchar(30) FOREIGN KEY REFERENCES [Product](Id),
	[Name] nvarchar(100),
	StageNum int,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table SkillForStage (
	Id nvarchar(30) Primary Key,
	SkillId nvarchar(30) FOREIGN KEY REFERENCES Skill(Id),
	ProductStageId nvarchar(30) FOREIGN KEY REFERENCES ProductStage(Id),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table ComponentType (
	Id nvarchar(30) Primary Key,
	CategoryId nvarchar(30) FOREIGN KEY REFERENCES Category(Id),
	CatalogStageId nvarchar(30) FOREIGN KEY REFERENCES CatalogStage(Id),
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Component (
	Id nvarchar(30) Primary Key,
	ComponentTypeId nvarchar(30) FOREIGN KEY REFERENCES ComponentType(Id),
	CatalogId nvarchar(30) FOREIGN KEY REFERENCES [Catalog](Id),
	[Name] nvarchar(100),
	[image] text,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table MaterialType (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table MaterialCategory (
	Id nvarchar(30) Primary Key,
	MaterialTypeId nvarchar(30) FOREIGN KEY REFERENCES MaterialType(Id),
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Material (
	Id nvarchar(30) Primary Key,
	MaterialCategoryId nvarchar(30) FOREIGN KEY REFERENCES MaterialCategory(Id),
	[Name] nvarchar(100),
	Measure nvarchar(10),
	[Image] text null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table ProductComponent (
	Id nvarchar(30) Primary Key,
	ComponentId nvarchar(30) FOREIGN KEY REFERENCES Component(Id),
	ProductStageId nvarchar(30) FOREIGN KEY REFERENCES ProductStage(Id),
	MaterialId nvarchar(30) FOREIGN KEY REFERENCES Material(Id),
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table OrderMaterial (
	Id nvarchar(30) Primary Key,
	MaterialId nvarchar(30) FOREIGN KEY REFERENCES Material(Id),
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	Approver nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	[Name] nvarchar(100),
	[Image] text null,
	IsApproved [bit] NULL DEFAULT 0,
	ApproveTime datetime Null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table [Transaction] (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	[Platform] nvarchar(50),
	Amount float,
	Currency nvarchar(30),
	TransactionTime datetime Null, 
	CreatedTime datetime Null,
	[Status] nvarchar(30),
	IsSuccess [bit] NULL DEFAULT 0,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Chat (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	CreatedTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table ChatHistory (
	Id nvarchar(30) Primary Key,
	ChatId nvarchar(30) FOREIGN KEY REFERENCES Chat(Id),
	StaffReply nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	[Message] nvarchar(255),
	FromCus [bit] NULL DEFAULT 1,
	SendTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	ReadTime datetime Null,
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)

Create Table [Notification] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	Title nvarchar(255),
	Content nvarchar(Max),
	SendTime datetime Null,
	ReadTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	IsActive [bit] NULL DEFAULT 0
)

Create Table Blog (
	Id nvarchar(30) Primary Key,
	Title nvarchar(255),
	UrlPath nvarchar(255),
	Content nvarchar(max),
	[Image] text null, 
	CreatedTime datetime Null,
	Creater nvarchar(30) FOREIGN KEY REFERENCES [User](Id),
	LastestUpdatedTime datetime Null, 
	InactiveTime datetime Null,
	IsActive [bit] NULL DEFAULT 0
)
