using Business.Interfaces;
using Business.Services;
using Data.Contexts;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using MVC.Hubs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
                      ?? Environment.GetEnvironmentVariable("PostgreSQL");

builder.Services.AddDbContext<DataContext>(x => x.UseNpgsql(connectionString));


builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedEmail = false;

})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(x =>
{
    x.LoginPath = "/Auth/Login";
    x.AccessDeniedPath = "/Auth/AccessDenied";
    x.ExpireTimeSpan = TimeSpan.FromHours(1);
    x.SlidingExpiration = true;
});


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRespository, ProjectRespository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IStatusTypeRepository, StatusTypeRepository>();
builder.Services.AddScoped<IProjectRequestRepository, ProjectRequestRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DatabaseInitializationService>();
builder.Services.AddScoped<IStatusTypeService, StatusTypeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IProjectsService, ProjectService>();
builder.Services.AddScoped<ICustomersService, CustomerService>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IProjectMembershipService, ProjectMembershipService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddSignalR();


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.ConfigureApplicationCookie(options =>
{

    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Register";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<CommentHub>("/commentHub");
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/css") &&
        !context.Request.Path.StartsWithSegments("/js") &&
        !context.Request.Path.StartsWithSegments("/lib") &&
        !context.Request.Path.StartsWithSegments("/images") &&
        !context.Request.Path.StartsWithSegments("/Error"))
    {
        var dbInitService = context.RequestServices.GetRequiredService<DatabaseInitializationService>();
        var isInitialized = await dbInitService.IsDatabaseInitializedAsync();


        if (!isInitialized &&
            !context.Request.Path.StartsWithSegments("/Auth") &&
            context.Request.Path != "/")
        {
            context.Response.Redirect("/Auth/Register");
            return;
        }
    }

    await next();
});


app.Run();
