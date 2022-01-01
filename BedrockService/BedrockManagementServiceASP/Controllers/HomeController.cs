using BedrockService.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BedrockManagementServiceASP.Controllers {
    public class HomeController : Controller {
        private readonly IServiceConfiguration _serviceConfiguration;

        public HomeController(IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public IActionResult Index() {
            return View(_serviceConfiguration);
        }
    }
}
