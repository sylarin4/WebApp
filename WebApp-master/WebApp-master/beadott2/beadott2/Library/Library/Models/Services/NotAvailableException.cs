using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class NotAvailableException : Exception
    {
        public NotAvailableException(String message) : base(message) { }
    }
}
