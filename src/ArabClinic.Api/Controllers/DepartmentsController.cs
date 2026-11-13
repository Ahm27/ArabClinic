using Microsoft.AspNetCore.Mvc;

namespace ArabClinic.Api.Controllers;

public sealed class DepartmentsController : Controller
{
    public IActionResult Index() => View();
}