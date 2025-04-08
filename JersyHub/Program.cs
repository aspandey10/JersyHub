using Hangfire;
using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services;
using JersyHub.Application.Services.ServiceImplementation;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Data;
using JersyHub.Infrastructure.Data;
using JersyHub.Domain.Entities;

using JersyHub.Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using JersyHub.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHangfire(configuration =>
    configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("dbcs"))); 

builder.Services.AddHangfireServer();
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));  

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}
    );

builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IOrderHeaderService, OrderHeaderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IAppEmailSender, AppEmailSender>();
builder.Services.AddScoped<EmailRemainderService>();
var app = builder.Build();



// Configure the HTTP request pipeline. 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseHangfireDashboard();
app.MapRazorPages();
app.MapStaticAssets();


app.MapControllerRoute(
    name: "Customer",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var emailReminderService = scope.ServiceProvider.GetRequiredService<EmailRemainderService>();

    RecurringJob.AddOrUpdate(
    "send-cart-reminder", 
    () => emailReminderService.SendReminderEmails(),
     "0 0 */3 * *" 
);
}
app.Run();


