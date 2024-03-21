using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Repository;
using Etailor.API.WebAPI;
using Hangfire;
using Hangfire.MemoryStorage;
using Etailor.API.Ultity.MiddleWare;
using SixLabors.ImageSharp;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var time = DateTime.UtcNow.AddHours(7);
builder.Services.AddCors();
// Add services to the container.

builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ETailor API", Version = $"v1.00.{time.ToString("yy.MM.dd.HH.mm.ss")}" });

                    // Configure Swagger to use JWT authentication
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
                }
);

builder.Services.AddSignalR();

builder.Services.AddSerilog();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<ETailor_DBContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("ETailor_DB")));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogService, BlogService>();

builder.Services.AddScoped<IProfileBodyRepository, ProfileBodyRepository>();
builder.Services.AddScoped<IProfileBodyService, ProfileBodyService>();

builder.Services.AddScoped<IBodySizeRepository, BodySizeRepository>();
builder.Services.AddScoped<IBodySizeService, BodySizeService>();

builder.Services.AddScoped<IBodyAttributeRepository, BodyAttributeRepository>();
builder.Services.AddScoped<IBodyAttributeService, BodyAttributeService>();

builder.Services.AddScoped<ITemplateBodySizeRepository, TemplateBodySizeRepository>();
builder.Services.AddScoped<ITemplateBodySizeService, TemplateBodySizeService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IProductStageRepository, ProductStageRepository>();
builder.Services.AddScoped<IProductStageService, ProductStageService>();

builder.Services.AddScoped<IProductComponentRepository, ProductComponentRepository>();
builder.Services.AddScoped<IProductComponentService, ProductComponentService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IComponentService, ComponentService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductTemplateRepository, ProductTemplateRepository>();
builder.Services.AddScoped<IProductTemplateService, ProductTemplateService>();

builder.Services.AddScoped<IProductBodySizeRepository, ProductBodySizeRepository>();
builder.Services.AddScoped<IProductBodySizeService, ProductBodySizeService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddScoped<ICustomerClientRepository, CustomerClientRepository>();


builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IDiscountService, DiscountService>();

builder.Services.AddScoped<IMaterialTypeRepository, MaterialTypeRepository>();
builder.Services.AddScoped<IMaterialTypeService, MaterialTypeService>();

builder.Services.AddScoped<IMaterialCategoryRepository, MaterialCategoryRepository>();
builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();

builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IMaterialService, MaterialService>();

builder.Services.AddScoped<IMasteryRepository, MasteryRepository>();
//builder.Services.AddScoped<IMaterialTypeService, MaterialTypeService>();

builder.Services.AddScoped<ITemplateStateRepository, TemplateStageRepository>();
builder.Services.AddScoped<ITemplateStageService, TemplateStageService>();

builder.Services.AddScoped<IComponentStageRepository, ComponentStageRepository>();
//builder.Services.AddScoped<ITemplateStageService, TemplateStageService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IOrderMaterialRepository, OrderMaterialRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddScoped<IChatListRepository, ChatListRepository>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IBackgroundService, Etailor.API.Service.Service.BackgroundService>();

builder.Services.AddSingleton<ISignalRService, SignalRService>();


builder.Services.AddHostedService<Etailor.API.Service.Service.HostedService>();


var credentials = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), AppValue.FIREBASE_KEY));

FirebaseApp.Create(new AppOptions { Credential = credentials });

var app = builder.Build();

// Configure Serilog logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("./wwwroot/Log/Check/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

app.UseAuthorization();
app.UseAuthentication();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"ETailor API v1.00.{time.ToString("yy.MM.dd.HH.mm.ss")}");
});

var MyAllowSpecificOrigins = builder.Configuration.GetSection("MyAllowSpecificOrigins").Get<string[]>();

app.UseCors(option =>
{
    option
    .WithOrigins(MyAllowSpecificOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
});

app.UseHangfireDashboard();

app.UseSerilogRequestLogging(); // Optionally add request logging

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SignalRHub>("/chatHub");
    endpoints.MapControllers();
});

app.Run();
