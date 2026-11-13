using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Api.Controllers;

public sealed class PatientsController : Controller
{
    public IActionResult Index() => View();
}