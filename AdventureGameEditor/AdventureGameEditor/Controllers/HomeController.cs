using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;


using AdventureGameEditor.Models;
using AdventureGameEditor.Data;

namespace AdventureGameEditor.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AdventureGameEditorContext context):base(context)
        {
            _logger = logger;
        }

        /*public IActionResult Index()
        {
            return View("Index", 
                new FeedbackViewModel
                {
                    Feedback = ""
                });
        }*/

        #region Just for testing.

        public ActionResult TestAction()
        {            
            return PartialView("Index");
        }
        public ActionResult GetMapImage()
        {
            int wayDirectionsCode = 0101;
            var image = _context.MapImage
                    .Where(image => image.WayDirectionsCode == wayDirectionsCode && image.Theme == MapTheme.Test)
                    .Select(image => image.Image)
                    .FirstOrDefault();
            return File(image, "image/png");
        }

        #endregion

        public IActionResult Index()
        {            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
