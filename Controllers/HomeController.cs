using System.Net.Http.Json;                       // Gives GetFromJsonAsync() to read JSON from a web API
using HospitalAppointmentSystem.Models;           // Doctor, Post, AppointmentRequest classes
using HospitalAppointmentSystem.Services;         // DoctorService class
using Microsoft.AspNetCore.Mvc;                   // Controller, IActionResult, [ResponseCache] etc.
using Microsoft.Extensions.Caching.Memory;        // IMemoryCache and MemoryCacheEntryOptions

namespace HospitalAppointmentSystem.Controllers; // Namespace for all controllers

// HomeController handles all pages of the Hospital Appointment Management System
public class HomeController : Controller
{
    private const string DoctorListCacheKey = "DoctorList";            // Key (name) under which the doctor list is cached
    private const string PatientCookieName = "PatientName";            // Name of the cookie that stores the patient name
    private const string SpecializationSessionKey = "SelectedSpec";    // Key used to store specialization in Session

    private readonly IMemoryCache _cache;                // Memory cache object (injected)
    private readonly DoctorService _doctorService;       // Service that supplies doctor data (injected)
    private readonly IHttpClientFactory _httpClientFactory; // Factory that creates HttpClient objects (injected)

    // Constructor: ASP.NET Core automatically passes the registered services (Dependency Injection)
    public HomeController(IMemoryCache cache, DoctorService doctorService, IHttpClientFactory httpClientFactory)
    {
        _cache = cache;                         // Store the cache in the private field
        _doctorService = doctorService;         // Store the doctor service in the private field
        _httpClientFactory = httpClientFactory; // Store the HttpClient factory in the private field
    }

    // Helper method: returns the doctor list from IMemoryCache, loading it only when the cache is empty
    private List<Doctor> GetDoctorsFromCache(out bool fromCache)
    {
        // Try to read the list from the cache; 'doctors' receives the value if the key exists
        if (_cache.TryGetValue(DoctorListCacheKey, out List<Doctor>? doctors) && doctors != null)
        {
            fromCache = true;   // Data was found in the cache (cache HIT)
            return doctors;     // Return the cached list without rebuilding it
        }

        fromCache = false;                          // Data was not in the cache (cache MISS)
        doctors = _doctorService.GetAllDoctors();   // Load the list from the service (like a database call)

        // Create cache options for this entry
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) // Cache expires 3 minutes after being stored
        };

        _cache.Set(DoctorListCacheKey, doctors, options); // Save the list in the cache with the 3-minute expiry
        return doctors;                                   // Return the freshly loaded list
    }

    // GET: /  or  /Home/Index  -> shows the doctor list (optionally filtered by specialization)
    public IActionResult Index(string? specialization)
    {
        // If the user chose a specialization in the filter (query string contains it)
        if (specialization != null)
        {
            if (specialization == "All")
            {
                HttpContext.Session.Remove(SpecializationSessionKey); // "All" means clear the stored choice
            }
            else
            {
                HttpContext.Session.SetString(SpecializationSessionKey, specialization); // SESSION: remember the chosen specialization
            }
        }

        // SESSION: read the specialization stored earlier (null if nothing stored yet)
        string? selected = HttpContext.Session.GetString(SpecializationSessionKey);

        // Get the full doctor list (from the cache if available) and learn whether it came from the cache
        List<Doctor> allDoctors = GetDoctorsFromCache(out bool fromCache);

        // Start with all doctors; narrow down only when a specialization is selected
        List<Doctor> shown = allDoctors;
        if (!string.IsNullOrEmpty(selected))
        {
            shown = allDoctors.Where(d => d.Specialization == selected).ToList(); // LINQ filter by specialization
        }

        // COOKIE: read the patient name stored earlier (null if cookie does not exist)
        ViewBag.PatientName = Request.Cookies[PatientCookieName];

        ViewBag.SelectedSpecialization = selected ?? "All";  // Value shown in the dropdown and the info line
        ViewBag.Specializations = allDoctors.Select(d => d.Specialization).Distinct().OrderBy(s => s).ToList(); // Dropdown items
        ViewBag.FromCache = fromCache;                       // Tells the view whether the list came from the cache
        ViewBag.ServerTime = DateTime.Now.ToString("hh:mm:ss tt"); // Current server time (to see page generation time)

        return View(shown); // Send the doctor list to Views/Home/Index.cshtml
    }

    // POST: /Home/SetPatient  -> stores the patient name in a COOKIE
    [HttpPost]
    public IActionResult SetPatient(string patientName)
    {
        // Only save the cookie when the user actually typed a name
        if (!string.IsNullOrWhiteSpace(patientName))
        {
            // Cookie settings
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1), // Cookie stays in the browser for 1 day
                HttpOnly = true,                         // Not readable by JavaScript
                IsEssential = true                       // Always allowed (no consent banner needed)
            };

            // COOKIE: write the patient name to the browser
            Response.Cookies.Append(PatientCookieName, patientName.Trim(), cookieOptions);
        }

        return RedirectToAction("Index"); // Go back to the doctor list page
    }

    // GET: /Home/Details?doctorId=1  -> DoctorId arrives through the QUERY STRING
    // RESPONSE CACHING: the page is cached for 60 seconds; a separate copy is kept for each doctorId value
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "doctorId" })]
    public IActionResult Details(int doctorId)
    {
        // Get the doctor list (from IMemoryCache) and find the doctor whose id matches the query string value
        Doctor? doctor = GetDoctorsFromCache(out _).FirstOrDefault(d => d.DoctorId == doctorId);

        // If no doctor has that id, return HTTP 404 Not Found
        if (doctor == null)
        {
            return NotFound();
        }

        // Show the time this page was generated; during the 60 seconds it will NOT change (proves response caching)
        ViewBag.GeneratedAt = DateTime.Now.ToString("hh:mm:ss tt");

        return View(doctor); // Send the doctor to Views/Home/Details.cshtml
    }

    // POST: /Home/Book  -> receives the appointment form (DoctorId comes from the HIDDEN FIELD)
    [HttpPost]
    public IActionResult Book(AppointmentRequest request)
    {
        // COOKIE: read the patient name saved earlier
        string? patientName = Request.Cookies[PatientCookieName];

        // If the patient name cookie is missing, ask the user to enter the name first
        if (string.IsNullOrWhiteSpace(patientName))
        {
            TempData["Message"] = "Please enter the patient name on the home page before booking an appointment.";
            return RedirectToAction("Index");
        }

        // Find the doctor using the id that was submitted by the hidden field
        Doctor? doctor = GetDoctorsFromCache(out _).FirstOrDefault(d => d.DoctorId == request.DoctorId);

        // If the id is invalid (e.g. hidden field edited), return 404
        if (doctor == null)
        {
            return NotFound();
        }

        ViewBag.PatientName = patientName;                                            // From the COOKIE
        ViewBag.Specialization = HttpContext.Session.GetString(SpecializationSessionKey) ?? "Not selected"; // From the SESSION
        ViewBag.Doctor = doctor;                                                      // Doctor found through the HIDDEN FIELD id

        return View("Confirmation", request); // Show Views/Home/Confirmation.cshtml
    }

    // GET: /Home/Posts  -> ASYNCHRONOUS web request to JSONPlaceholder
    // 'async Task<IActionResult>' lets the thread do other work while waiting for the web response
    public async Task<IActionResult> Posts()
    {
        // Create an HttpClient object from the factory
        HttpClient client = _httpClientFactory.CreateClient();

        // Some APIs reject requests without a User-Agent header, so we add one
        client.DefaultRequestHeaders.UserAgent.ParseAdd("HospitalAppointmentSystem/1.0");

        try
        {
            // 'await' pauses here WITHOUT blocking the thread until the API replies, then converts JSON -> List<Post>
            List<Post>? posts = await client.GetFromJsonAsync<List<Post>>("https://jsonplaceholder.typicode.com/posts");

            return View(posts ?? new List<Post>()); // Send the posts (or an empty list if null) to the view
        }
        catch (Exception ex)
        {
            // If the internet is down or the API fails, show a friendly message instead of crashing
            ViewBag.Error = "Could not load posts: " + ex.Message;
            return View(new List<Post>()); // Show the page with an empty table
        }
    }
}
