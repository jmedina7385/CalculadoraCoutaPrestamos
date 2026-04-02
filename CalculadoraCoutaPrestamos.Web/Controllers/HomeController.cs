using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CalculadoraCoutaPrestamos.Web.Models;

namespace CalculadoraCoutaPrestamos.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() =>
        RedirectToAction(nameof(PrestamosController.Index), "Prestamos");

    public IActionResult Privacy() =>
        View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
