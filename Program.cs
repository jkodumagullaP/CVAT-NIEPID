using Microsoft.EntityFrameworkCore;
using CVAT_NIEPID.Data;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container

builder.Services.AddControllersWithViews();   // MVC Web
builder.Services.AddControllers();            // API Controllers


// Database Connection (PostgreSQL)

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Enable CORS for Mobile App

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();


// Configure the HTTP request pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();


// API Routes

app.MapControllers();


// MVC Routes

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
