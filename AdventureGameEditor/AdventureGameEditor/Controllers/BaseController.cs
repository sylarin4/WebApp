using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

using AdventureGameEditor.Models;

namespace AdventureGameEditor.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            ViewBag.CurrentGuestName = String.IsNullOrEmpty(User.Identity.Name) ?
                null : User.Identity.Name;
            //Trace.WriteLine("\n\n\n\n" + String.IsNullOrEmpty(User.Identity.Name) + "\n\n\n\n");
        }
    }
}