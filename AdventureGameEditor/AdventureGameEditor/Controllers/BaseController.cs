﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

using AdventureGameEditor.Models;
using AdventureGameEditor.Data;

namespace AdventureGameEditor.Controllers
{
    public class BaseController : Controller 
    {
        protected readonly AdventureGameEditorContext _context;
        public BaseController(AdventureGameEditorContext context)
        {
            _context = context;
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            ViewBag.CurrentGuestName = String.IsNullOrEmpty(User.Identity.Name) ?
                null : User.Identity.Name;
        }

        
    }
}