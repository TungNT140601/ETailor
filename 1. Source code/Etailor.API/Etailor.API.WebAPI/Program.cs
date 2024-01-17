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

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
// Add services to the container.

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

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();


var credentials = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "demootp-34065-firebase-adminsdk-8mrpy-f121841f4c.json"));
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
app.UseCors(option =>
{
    option.AllowAnyHeader()
    .WithOrigins("https://demo-notification.vercel.app")
    .AllowAnyMethod();
});

app.UseStaticFiles();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
