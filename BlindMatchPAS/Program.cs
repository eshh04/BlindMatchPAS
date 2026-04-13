using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;

var builder = WebApplication.CreateBuilder(args);

// --- MEMBER 7: DATABASE CONNECTION SETUP ---
// This registers the ApplicationDbContext to use SQL Server 
// with the connection string defined in appsettings.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container (Controllers and Views).
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Use a user-friendly error page in production.
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days for security.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Enables support for static assets (CSS, JS, Images).
app.MapStaticAssets();

// Configures the default route for the application.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();