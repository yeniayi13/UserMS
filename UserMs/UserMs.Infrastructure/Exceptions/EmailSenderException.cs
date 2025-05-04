using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMs.Common.Exceptions
{
    public class EmailSenderException :Exception
    {
        public EmailSenderException() { }

        public EmailSenderException(string message)
            : base(message) { }

        public EmailSenderException(string message, Exception inner)
            : base(message, inner) { }
    }
}
