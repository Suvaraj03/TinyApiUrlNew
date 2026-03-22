using Microsoft.EntityFrameworkCore;
using TinyApiUrl.Data;
using TinyApiUrl.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApp",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});




var dbPath = Path.Combine(
    Environment.GetEnvironmentVariable("HOME") ?? "",
    "data",
    "urls.db");

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(options =>
   //options.UseSqlite("Data Source=urls.db"));
   // options.UseSqlite("Data Source=/home/site/wwwroot/urls.db"));
   options.UseSqlite($"Data Source={dbPath}"));


var app = builder.Build();

//create DB Automatically
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        //db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB ERROR: " + ex.Message);
        throw;
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowApp");

app.UseHttpsRedirection();

app.UseAuthorization();

//app.Urls.Add("http://0.0.0.0:80");

app.MapControllers();
//var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
//app.Urls.Add($"http://*:{port}");

app.Run();
