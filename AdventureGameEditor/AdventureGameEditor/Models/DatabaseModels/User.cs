using System;
using Microsoft.AspNetCore.Identity;

namespace AdventureGameEditor.Models
{
    public class User : IdentityUser<int>
    {
        public String NickName { get; set; }

    }
}
