using PloliticalScienceSystemApi.Extension;
using PloliticalScienceSystemApi.GlobalSetting;
using PloliticalScienceSystemApi.Log;
using PloliticalScienceSystemApi.MongoDbHelper;
using PloliticalScienceSystemApi.ServiceInject;
using System.Text.Json;
using NLog.Web;
using PloliticalScienceSystemApi.MiddlerWare;
using Microsoft.Extensions.FileProviders;
using Dumpify;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InjectAppsettingConf();
builder.Services.AddControllers();
builder.Services.IdGenInit();
builder.RegistEnvironment();
builder.Services.AddControllers().AddJsonOptions(
        options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new NumberConverter());
        });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCorsService();

builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.RegisterGlobalLogger();
await builder.Services.DBInit();

builder.Services.AddJwtService();
builder.Services.AddJWTAuthorizationService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorsService();
app.UseErrorMiddleware();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "/StaticFiles"
});


app.UseJWTService();
app.Map("/", () => Results.Redirect("/staticfiles/404.html#")).RequireCors(t => t.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
app.MapControllers();
var hello = new HelloWorld();
hello.Dump();
app.Run();
