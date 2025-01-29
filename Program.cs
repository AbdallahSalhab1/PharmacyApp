using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewWebApplicationProject.Models;
using PharmacyApp.Data;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();




var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated &&
        !context.Request.Path.StartsWithSegments("/Identity/Account/Login"))
    {
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

ApplicationDbInitializer.Seed(app);

using (var scope = app.Services.CreateScope()) {
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Patient", "Pharmacy" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

}


using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string email = "admin@admin.com";
    string password = "Test1234,";

    if (await userManager.FindByEmailAsync(email) == null) {
        var user = new IdentityUser();
        user.UserName = email;
        user.Email = email;

        await userManager.CreateAsync(user, password);

        await userManager.AddToRoleAsync(user, "Admin");
    }
}


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!await roleManager.RoleExistsAsync("Pharmacy"))
    {
        await roleManager.CreateAsync(new IdentityRole("Pharmacy"));
    }

    string email = "pharmacy@pharmacy.com";
    string password = "Test1234,";

    
    if (!await dbContext.Pharmacies.AnyAsync(p => p.Email == email))
    {
        var pharmacy = new Pharmacy
        {
            Email = email,
            PharmacyName = "Sample Pharmacy",
            LicenseNumber = "LICENSE123",
            Address = "123 Main St",
            PhoneNumber = "1234567890",
            IsVerified = true
        };
        dbContext.Pharmacies.Add(pharmacy);
        await dbContext.SaveChangesAsync();
    }

    
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Pharmacy");
    }
}


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    
    if (!await roleManager.RoleExistsAsync("Patient"))
    {
        await roleManager.CreateAsync(new IdentityRole("Patient"));
    }

    string email = "patient@patient.com";
    string password = "Test1234,";

    
    if (!await dbContext.Patients.AnyAsync(p => p.Email == email))
    {
        var patient = new Patient
        {
            Name = "Mareed",
            Email = email, 
            PhoneNumber = "1234567890" 
        };
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();
    }

    
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Patient");
    }
}



app.Run();
