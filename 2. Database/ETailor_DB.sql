Create database ETailor_DB
Go

Use ETailor_DB
Go

Create Table [Role] (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(55),
	[IsDelete] [bit] NULL DEFAULT 0
);

Create Table Staff ( 
	Id nvarchar(30) Primary Key,
	RoleId nvarchar(30) FOREIGN KEY REFERENCES [Role](Id),
	Avatar text,
	[Fullname] nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Username nvarchar(255),
	[Password] nvarchar(255),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
);

Create Table Customer ( 
	Id nvarchar(30) Primary Key,
	Avatar text,
	[Fullname] nvarchar(100) null,
	[Address] nvarchar(255) null,
	Phone nvarchar(10) null,
	Email nvarchar(255) null,
	Username nvarchar(255),
	[Password] nvarchar(255),
	PhoneVerified [bit] NULL DEFAULT 0,
	EmailVerified [bit] NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
);

Create Table ProfileBodyAttribute (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	[Name] nvarchar(100),
	IsLocked [bit] NULL DEFAULT 0,
	IsByStaff [bit] NULL DEFAULT 0,
	MakerId nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table BodySize (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	[Image] text null, 
	GuideVideoLink text null,
	MinValidValue decimal(18,3),
	MaxValidValue decimal(18,3),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table BodyAttribute (
	Id nvarchar(30) Primary Key,
	ProfileBodyAttributeId nvarchar(30) FOREIGN KEY REFERENCES [ProfileBodyAttribute](Id),
	BodySizeId nvarchar (30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] decimal(18,3),
	Measure nvarchar (10),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ProductCategory (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(255),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0,
)

Create Table Product (
	Id nvarchar(30) Primary Key,
	ProductCategoryId nvarchar(30) FOREIGN KEY REFERENCES ProductCategory(Id),
	[Name] nvarchar(255),
	[Description] nvarchar(max),
	Price decimal(18,3),
	[Image] text,
	UrlPath nvarchar(255),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0,
	IsCustomize [bit] NULL DEFAULT 0
)

Create Table ProductBodySize (
	Id nvarchar(30) Primary Key,
	ProductId nvarchar(30) FOREIGN KEY REFERENCES Product(Id),
	BodySizeId nvarchar(30) FOREIGN KEY REFERENCES BodySize(Id),
	[Value] decimal(18,3) null,
	Measure nvarchar(10) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ProductStage (
	Id nvarchar(30) Primary Key,
	ProductId nvarchar(30) FOREIGN KEY REFERENCES Product(Id),
	[Name] nvarchar(100),
	MakerId  nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	StageNum int,
	StageProcess decimal(18,3) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ProductStep (
	Id nvarchar(30) Primary Key,
	ProductStageId nvarchar(30) FOREIGN KEY REFERENCES ProductStage(Id),
	[Name] nvarchar(100),
	IsFinish [bit] NULL DEFAULT 0,
	FinishedTime datetime null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ProductComponent (
	Id nvarchar(30) Primary Key,
	ProductId nvarchar(30) FOREIGN KEY REFERENCES Product(Id),
	ProductStepId nvarchar (30) FOREIGN KEY REFERENCES ProductStep(Id),
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ComponentStyle (
	Id nvarchar(30) Primary Key,
	ProductComponentId nvarchar(30) FOREIGN KEY REFERENCES ProductComponent(Id),
	[Name] nvarchar(100),
	[Image] text null, 
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table MaterialType (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table MaterialCategory (
	Id nvarchar(30) Primary Key,
	MaterialTypeId nvarchar(30) FOREIGN KEY REFERENCES MaterialType(Id),
	[Name] nvarchar(100),
	Handled [bit] NULL DEFAULT 0,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table Material (
	Id nvarchar(30) Primary Key,
	MaterialCategoryId nvarchar(30) FOREIGN KEY REFERENCES MaterialCategory(Id),
	[Name] nvarchar(100),
	Measure nvarchar(10),
	[Image] text null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table Discount (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(255),
	Code nvarchar(30),
	StartDate datetime Null,
	EndDate datetime Null, 
	DiscountPercent decimal(18,3) null,
	DiscountPrice nvarchar(30) null,
	ConditionPriceMin decimal(18,3) null,
	ConditionPriceMax decimal(18,3) null,
	ConditionProductMin int null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table [Order] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	TotalProduct int,
	TotalPrice decimal(18,3),
	DiscountId nvarchar(30) FOREIGN KEY REFERENCES Discount(Id) null,
	DiscountPrice decimal(18,3) null,
	DiscountCode nvarchar(30) null,
	AfterDiscountPrice decimal(18,3) null,
	PayDeposit [bit] NULL DEFAULT 0, 
	Deposit decimal(18,3) null,
	PaidMoney decimal(18,3) null,
	UnPaidMoney decimal(18,3) null,
	IsApproved [bit] NULL DEFAULT 0,
	ApproveTime datetime Null,
	Approver nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null,
	BuyTime datetime null,
	IsBuy [bit] NULL DEFAULT 0,
	CancelTime datetime Null,
	IsCancel [bit] NULL DEFAULT 0,
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table OrderMaterial (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	MaterialCategoryId nvarchar(30) FOREIGN KEY REFERENCES MaterialCategory(Id),
	[Name] nvarchar(100),
	Height decimal(18,3) null,
	Width decimal(18,3) null, 
	[Image] text null,
	InProcess [bit] NULL DEFAULT 0,
	IsApproved [bit] NULL DEFAULT 0,
	ApproveTime datetime Null,
	Approver nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table MaterialForComponent (
	Id nvarchar(30) Primary Key,
	MaterialId nvarchar(30) FOREIGN KEY REFERENCES Material(Id) null,
	OrderMaterialId nvarchar(30) FOREIGN KEY REFERENCES OrderMaterial(Id) null,
	ProductComponentId nvarchar(30) FOREIGN KEY REFERENCES ProductComponent(Id) null,
	Height decimal(18,3) null,
	Width decimal(18,3) null, 
	Quantity int null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table OrderDetail (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	ProductId nvarchar(30) FOREIGN KEY REFERENCES Product(Id),
	ProfileBodyAttributeId nvarchar(30) FOREIGN KEY REFERENCES ProfileBodyAttribute(Id),
	[Status] nvarchar(30) null,
	StatusPercent decimal(18,3) null,
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null,
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table TransactionType (
	Id nvarchar(30) Primary Key,
	[Name] nvarchar(100),
	CreatedTime datetime Null,
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table [Transaction] (
	Id nvarchar(30) Primary Key,
	OrderId nvarchar(30) FOREIGN KEY REFERENCES [Order](Id),
	TransactionTypeId nvarchar(30) FOREIGN KEY REFERENCES TransactionType(Id),
	[Platform] nvarchar(50),
	Amount decimal(18,3),
	Currency nvarchar(30),
	TransactionTime datetime Null, 
	CreatedTime datetime Null,
	[Status] nvarchar(30),
	IsSuccess [bit] NULL DEFAULT 0,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table Chat (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	CreatedTime datetime Null,
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table ChatHistory (
	Id nvarchar(30) Primary Key,
	ChatId nvarchar(30) FOREIGN KEY REFERENCES Chat(Id),
	[Message] nvarchar(255),
	FromCus [bit] NULL DEFAULT 1,
	StaffReply nvarchar(30) FOREIGN KEY REFERENCES Staff(Id) null,
	SendTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	ReadTime datetime Null,
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table [Notification] (
	Id nvarchar(30) Primary Key,
	CustomerId nvarchar(30) FOREIGN KEY REFERENCES Customer(Id),
	Title nvarchar(255),
	Content nvarchar(Max),
	SendTime datetime Null,
	ReadTime datetime Null,
	IsRead [bit] NULL DEFAULT 0,
	[IsDelete] [bit] NULL DEFAULT 0
)

Create Table Blog (
	Id nvarchar(30) Primary Key,
	Title nvarchar(255),
	UrlPath nvarchar(255),
	Content nvarchar(max),
	[Image] text null, 
	CreatedTime datetime Null,
	Creater nvarchar(30) FOREIGN KEY REFERENCES Staff(Id),
	LastestUpdatedTime datetime Null, 
	DeletedTime datetime Null,
	[IsDelete] [bit] NULL DEFAULT 0
)
