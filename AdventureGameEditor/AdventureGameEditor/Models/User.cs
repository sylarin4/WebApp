using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AdventureGameEditor.Models
{
    public class User : IdentityUser<int>
    {
        public String Name { get; set; }
        public String NickName { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        //public virtual ICollection<User> FriendList { get; set; }

        //public virtual ICollection<User> FriendRequestsSent { get; set; }

        //public virtual ICollection<User> FriendRequestsGet { get; set; }

    }
}
