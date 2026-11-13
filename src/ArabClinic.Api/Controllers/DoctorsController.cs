using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Api.Controllers;

public sealed class DoctorsController : Controller
{
    public IActionResult Index() => View();
}