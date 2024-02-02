using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class InitDB_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodySize",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BodyPart = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BodyIndex = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    GuideVideoLink = table.Column<string>(type: "text", nullable: true),
                    MinValidValue = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    MaxValidValue = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodySize", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Fullname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OTPNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OTPTimeLimit = table.Column<DateTime>(type: "datetime", nullable: true),
                    OTPUsed = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    PhoneVerified = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    SecrectKeyLogin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discount",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ConditionPriceMin = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ConditionPriceMax = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ConditionProductMin = table.Column<int>(type: "int", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Fullname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SecrectKeyLogin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastLoginDeviceToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((2))"),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComponentType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentType", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Component__Categ__52793849",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductTemplate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2550)", maxLength: 2550, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: true, defaultValueSql: "((0))"),
                    ThumbnailImage = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    CollectionImage = table.Column<string>(type: "text", nullable: true),
                    UrlPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProductTe__Categ__442B18F2",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Chat__CustomerId__125EB334",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerClient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClientToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK__CustomerC__Custo__2B2A60FE",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaterialCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MaterialTypeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__MaterialC__Mater__7C6F7215",
                        column: x => x.MaterialTypeId,
                        principalTable: "MaterialType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UrlPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Blog__StaffId__22951AFD",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mastery",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mastery", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Mastery__Categor__2E06CDA9",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Mastery__StaffId__2EFAF1E2",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReadTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Notificat__Custo__1CDC41A7",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Notificat__Staff__1DD065E0",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreaterId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiscountId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TotalProduct = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,0)", nullable: true, defaultValueSql: "((0))"),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    DiscountCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AfterDiscountPrice = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    PayDeposit = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    Deposit = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    PaidMoney = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    UnPaidMoney = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    CancelTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Order__CreaterId__62AFA012",
                        column: x => x.CreaterId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Order__CustomerI__61BB7BD9",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Order__DiscountI__63A3C44B",
                        column: x => x.DiscountId,
                        principalTable: "Discount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProfileBody",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileBody", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProfileBo__Custo__33008CF0",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProfileBo__Staff__33F4B129",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Component",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ComponentTypeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProductTemplateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Component", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Component__Compo__5A1A5A11",
                        column: x => x.ComponentTypeId,
                        principalTable: "ComponentType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Component__Produ__5B0E7E4A",
                        column: x => x.ProductTemplateId,
                        principalTable: "ProductTemplate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemplateBodySize",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductTemplateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BodySizeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateBodySize", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TemplateB__BodyS__49E3F248",
                        column: x => x.BodySizeId,
                        principalTable: "BodySize",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__TemplateB__Produ__48EFCE0F",
                        column: x => x.ProductTemplateId,
                        principalTable: "ProductTemplate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemplateStage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductTemplateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TemplateStageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(155)", maxLength: 155, nullable: true),
                    StageNum = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TemplateS__Produ__4CC05EF3",
                        column: x => x.ProductTemplateId,
                        principalTable: "ProductTemplate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__TemplateS__Templ__4DB4832C",
                        column: x => x.TemplateStageId,
                        principalTable: "TemplateStage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReplierId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromCus = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    SendTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    ReadTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ChatHisto__ChatI__162F4418",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ChatHisto__Repli__17236851",
                        column: x => x.ReplierId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Material",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MaterialCategoryId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Material__Materi__004002F9",
                        column: x => x.MaterialCategoryId,
                        principalTable: "MaterialCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    PayTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Payment__OrderId__0E8E2250",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProductTemplateId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    EvidenceImage = table.Column<string>(type: "text", nullable: true),
                    FinishTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Product__OrderId__6B44E613",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Product__Product__6C390A4C",
                        column: x => x.ProductTemplateId,
                        principalTable: "ProductTemplate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BodyAttribute",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProfileBodyId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BodySizeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK__BodyAttri__BodyS__3D7E1B63",
                        column: x => x.BodySizeId,
                        principalTable: "BodySize",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__BodyAttri__Profi__3C89F72A",
                        column: x => x.ProfileBodyId,
                        principalTable: "ProfileBody",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComponentStage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ComponentTypeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TemplateStageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Component__Compo__5649C92D",
                        column: x => x.ComponentTypeId,
                        principalTable: "ComponentType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Component__Templ__573DED66",
                        column: x => x.TemplateStageId,
                        principalTable: "TemplateStage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderMaterial",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MaterialId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValueSql: "((0))"),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK__OrderMate__Mater__08D548FA",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__OrderMate__Order__09C96D33",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductBodySize",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BodySizeId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBodySize", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProductBo__BodyS__2759D01A",
                        column: x => x.BodySizeId,
                        principalTable: "BodySize",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProductBo__Produ__2665ABE1",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductStage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TemplateStageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StageNum = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    TaskIndex = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    FinishTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProductSt__Produ__72E607DB",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProductSt__Staff__70FDBF69",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProductSt__Templ__71F1E3A2",
                        column: x => x.TemplateStageId,
                        principalTable: "TemplateStage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductComponent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ComponentId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProductStageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProductCo__Compo__041093DD",
                        column: x => x.ComponentId,
                        principalTable: "Component",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProductCo__Produ__0504B816",
                        column: x => x.ProductStageId,
                        principalTable: "ProductStage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductComponentMaterial",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductComponentId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MaterialId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComponentMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ProductCo__Mater__32CB82C6",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ProductCo__Produ__31D75E8D",
                        column: x => x.ProductComponentId,
                        principalTable: "ProductComponent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blog_StaffId",
                table: "Blog",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_BodyAttribute_BodySizeId",
                table: "BodyAttribute",
                column: "BodySizeId");

            migrationBuilder.CreateIndex(
                name: "IX_BodyAttribute_ProfileBodyId",
                table: "BodyAttribute",
                column: "ProfileBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CustomerId",
                table: "Chat",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_ChatId",
                table: "ChatHistory",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_ReplierId",
                table: "ChatHistory",
                column: "ReplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Component_ComponentTypeId",
                table: "Component",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Component_ProductTemplateId",
                table: "Component",
                column: "ProductTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentStage_ComponentTypeId",
                table: "ComponentStage",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentStage_TemplateStageId",
                table: "ComponentStage",
                column: "TemplateStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentType_CategoryId",
                table: "ComponentType",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerClient_CustomerId",
                table: "CustomerClient",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Mastery_CategoryId",
                table: "Mastery",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Mastery_StaffId",
                table: "Mastery",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Material_MaterialCategoryId",
                table: "Material",
                column: "MaterialCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialCategory_MaterialTypeId",
                table: "MaterialCategory",
                column: "MaterialTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CustomerId",
                table: "Notification",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_StaffId",
                table: "Notification",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreaterId",
                table: "Order",
                column: "CreaterId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_DiscountId",
                table: "Order",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterial_MaterialId",
                table: "OrderMaterial",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterial_OrderId",
                table: "OrderMaterial",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                table: "Payment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_OrderId",
                table: "Product",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductTemplateId",
                table: "Product",
                column: "ProductTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBodySize_BodySizeId",
                table: "ProductBodySize",
                column: "BodySizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBodySize_ProductId",
                table: "ProductBodySize",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponent_ComponentId",
                table: "ProductComponent",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponent_ProductStageId",
                table: "ProductComponent",
                column: "ProductStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponentMaterial_MaterialId",
                table: "ProductComponentMaterial",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponentMaterial_ProductComponentId",
                table: "ProductComponentMaterial",
                column: "ProductComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStage_ProductId",
                table: "ProductStage",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStage_StaffId",
                table: "ProductStage",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStage_TemplateStageId",
                table: "ProductStage",
                column: "TemplateStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTemplate_CategoryId",
                table: "ProductTemplate",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileBody_CustomerId",
                table: "ProfileBody",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileBody_StaffId",
                table: "ProfileBody",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateBodySize_BodySizeId",
                table: "TemplateBodySize",
                column: "BodySizeId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateBodySize_ProductTemplateId",
                table: "TemplateBodySize",
                column: "ProductTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateStage_ProductTemplateId",
                table: "TemplateStage",
                column: "ProductTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateStage_TemplateStageId",
                table: "TemplateStage",
                column: "TemplateStageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "BodyAttribute");

            migrationBuilder.DropTable(
                name: "ChatHistory");

            migrationBuilder.DropTable(
                name: "ComponentStage");

            migrationBuilder.DropTable(
                name: "CustomerClient");

            migrationBuilder.DropTable(
                name: "Mastery");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "OrderMaterial");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "ProductBodySize");

            migrationBuilder.DropTable(
                name: "ProductComponentMaterial");

            migrationBuilder.DropTable(
                name: "TemplateBodySize");

            migrationBuilder.DropTable(
                name: "ProfileBody");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "Material");

            migrationBuilder.DropTable(
                name: "ProductComponent");

            migrationBuilder.DropTable(
                name: "BodySize");

            migrationBuilder.DropTable(
                name: "MaterialCategory");

            migrationBuilder.DropTable(
                name: "Component");

            migrationBuilder.DropTable(
                name: "ProductStage");

            migrationBuilder.DropTable(
                name: "MaterialType");

            migrationBuilder.DropTable(
                name: "ComponentType");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "TemplateStage");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "ProductTemplate");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Discount");

            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
