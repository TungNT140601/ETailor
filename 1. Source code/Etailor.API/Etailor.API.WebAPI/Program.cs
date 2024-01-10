using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ETailor API", Version = "v1" });
});

builder.Services.AddDbContext<ETailor_DBContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("ETailor_DB")));

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();



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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
