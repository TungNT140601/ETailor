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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
// Add services to the container.

//builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true).AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ETailor API", Version = "v1" });

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

builder.Services.AddScoped<IBodySizeRepository, BodySizeRepository>();
builder.Services.AddScoped<IBodySizeService, BodySizeService>();

builder.Services.AddScoped<IBodyAttributeRepository, BodyAttributeRepository>();
builder.Services.AddScoped<IBodyAttributeService, BodyAttributeService>();

builder.Services.AddScoped<ITemplateBodySizeRepository, TemplateBodySizeRepository>();
builder.Services.AddScoped<ITemplateBodySizeService, TemplateBodySizeService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
//builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductTemplateRepository, ProductTemplateRepository>();
builder.Services.AddScoped<IProductTemplateService, ProductTemplateService>();

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
//builder.Services.AddScoped<IMaterialTypeService, MaterialTypeService>();

builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
//builder.Services.AddScoped<IMaterialTypeService, MaterialTypeService>();

builder.Services.AddScoped<IMasteryRepository, MasteryRepository>();
//builder.Services.AddScoped<IMaterialTypeService, MaterialTypeService>();


var credentials = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), AppValue.FIREBASE_KEY));
FirebaseApp.Create(new AppOptions { Credential = credentials });

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ETailor API v1");
});
//}

var MyAllowSpecificOrigins = builder.Configuration.GetSection("MyAllowSpecificOrigins").Get<string[]>();

app.UseCors(option =>
{
    option.AllowAnyHeader()
    .WithOrigins(MyAllowSpecificOrigins)
    .AllowAnyMethod();
});

app.UseStaticFiles();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
