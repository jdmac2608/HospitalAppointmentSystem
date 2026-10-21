using HospitalAppointmentSystem.Services; // Needed to register DoctorService

// Create the builder object that is used to configure services and the app
var builder = WebApplication.CreateBuilder(args);

// Register MVC support (controllers + Razor views) in the dependency-injection container
builder.Services.AddControllersWithViews();

// Register IMemoryCache so it can be injected into controllers (used to cache the doctor list)
builder.Services.AddMemoryCache();

// Register the Response Caching services (needed for the [ResponseCache] attribute to work)
builder.Services.AddResponseCaching();

// Register Session services (used to store the selected specialization)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Session expires after 20 minutes of inactivity
    options.Cookie.HttpOnly = true;                 // JavaScript cannot read the session cookie (more secure)
    options.Cookie.IsEssential = true;              // Cookie is always sent, even without cookie-consent
});

// Register IHttpClientFactory so we can create HttpClient objects for the asynchronous web request
builder.Services.AddHttpClient();

// Register our DoctorService; Scoped = one instance per HTTP request
builder.Services.AddScoped<DoctorService>();

// Build the web application from the configured builder
var app = builder.Build();

// Enable routing so incoming URLs can be matched to controller actions
app.UseRouting();

// Add the Response Caching middleware to the request pipeline (serves cached responses)
app.UseResponseCaching();

// Add the Session middleware to the pipeline (makes HttpContext.Session available)
app.UseSession();

// Define the default URL pattern: /Controller/Action/optional-id (default page = Home/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the web server and keep running until stopped (Ctrl + C)
app.Run();
