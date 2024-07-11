using Boxed.Mapping;
using CrystalQuartz.AspNetCore;
using DemoApiMongo.Configuration;
using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.Mappers;
using DemoApiMongo.Entities.ViewModels;
using DemoApiMongo.Filter;
using DemoApiMongo.HangFireScheduler;
using DemoApiMongo.Middleware;
using DemoApiMongo.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Quartz.Impl;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// logs entry
Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
    .WriteTo.File("log/productsLogs.txt", rollingInterval: RollingInterval.Day).CreateBootstrapLogger();
builder.Host.UseSerilog();

var mongoClient = new MongoClient("your connection string");
var database = mongoClient.GetDatabase("Demo");

// Register MongoDB database instance for DI
builder.Services.AddSingleton(database);


builder.Services.AddSingleton<QuartzManager>();

// DB Settings
builder.Services.Configure<ProductDBSettings>(
    builder.Configuration.GetSection("ProductDatabase"));

// service implementation
builder.Services.AddSingleton<IProductRepo, ProductRepo>();
builder.Services.AddSingleton<IUserRepo, UserRepo>();

// authorization & authentication
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// exception handling
//builder.Services.AddSingleton<ExceptionLoggingFilter>();
builder.Services.AddControllers(options => options.Filters.Add(typeof(ExceptionLoggingFilter))).AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//AutoMapping
builder.Services.AddAutoMapper(typeof(AutoMapping));

//BoxedMapping
builder.Services.AddTransient<IMapper<ProductDetailModel, ProductDetails>, BoxedMapping>();

//Cached Memory
builder.Services.AddMemoryCache();

// cors to allow access to frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// jwt settings
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var scheduler = new StdSchedulerFactory().GetScheduler().Result;
scheduler.Start().Wait();

// Add Quartz Scheduler to services (if needed)
builder.Services.AddSingleton(scheduler);
var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Quartz Dashboard
app.UseCrystalQuartz(() => scheduler);

app.MapControllers();


app.Run();