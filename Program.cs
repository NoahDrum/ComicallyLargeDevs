using BucStop;

/*
 * This is the base program which starts the project.
 */

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var configuration = builder.Configuration;

builder.Services.AddHttpClient<MicroClient>(client =>
{
    var baseAddress = new Uri(configuration.GetValue<string>("Gateway"));

    client.BaseAddress = baseAddress;
});

builder.Services.AddAuthentication("CustomAuthenticationScheme").AddCookie("CustomAuthenticationScheme", options =>
{
    options.LoginPath = "/Account/Login";
});

// This creates the timestamp for the logger.
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// More dynamic was of defining how the server is listening for traffic. (resulted in commenting a line out later in this file).
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8000); // Listen on port 8000
});

builder.Services.AddSingleton<GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Commented out this line of code from the original because it was not set up for HTTPS and was causing errors.
//app.UseHttpsRedirection();
app.UseStaticFiles();

// De-bug line that would show when a request was sent for a certain method. Helps determine where the files were getting stuck.
app.Use(async (context, next) =>
{
    Console.WriteLine($"[REQUEST] {context.Request.Method} {context.Request.Path}");
    await next();
});


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//Handles routing to "separate" game pages by setting the Play page to have subpages depending on ID
app.MapControllerRoute(
    name: "Games",
    pattern: "Games/{action}/{id?}",
    defaults: new { controller = "Games", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Commented out this line of code from the original because it was a duplicate that was creating conflicts.
//app.Urls.Add("http://0.0.0.0:8000");

app.Run();
