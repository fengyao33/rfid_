using Microsoft.AspNetCore.Mvc;

namespace rfidServer_C_.Controllers;

public class RFIDHistoryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
