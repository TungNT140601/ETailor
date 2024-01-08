using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity.CustomException
{
    public class SystemsException : Exception
    {
        public SystemsException() : base()
        {
        }

        public SystemsException(string? message) : base(message)
        {
        }

        public SystemsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
