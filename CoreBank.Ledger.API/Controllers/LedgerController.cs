using Microsoft.AspNetCore.Mvc;

namespace CoreBank.Ledger.API.Controllers
{
    public class LedgerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
