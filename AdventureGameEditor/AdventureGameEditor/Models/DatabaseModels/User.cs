using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AdventureGameEditor.Models
{
    public class User : IdentityUser<int>
    {
        public String NickName { get; set; }

    }
}
