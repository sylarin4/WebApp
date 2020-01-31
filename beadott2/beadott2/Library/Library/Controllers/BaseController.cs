using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Library.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILibraryService _libraryService;

        public BaseController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        /// <summary>
        /// Egy akció meghívása után végrehajtandó metódus.
        /// </summary>
        /// <param name="context">Az akció kontextus argumentuma.</param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            ViewBag.CurrentGuestName = String.IsNullOrEmpty(User.Identity.Name) ?
                null : User.Identity.Name;
            Trace.WriteLine("\n\n\n\n" + String.IsNullOrEmpty(User.Identity.Name) + "\n\n\n\n");
        }
    }
}
