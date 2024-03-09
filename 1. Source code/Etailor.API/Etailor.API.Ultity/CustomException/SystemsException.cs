using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Etailor.API.Ultity.CustomException
{
    public class SystemsException : Exception
    {
        public SystemsException() : base()
        {
        }

        public SystemsException(string? message, string? errorPosition) : base(message)
        {
            Ultils.SendErrorToDev(message); // Assuming this method sends error to developers
            Log.Error($"An error occurred at {errorPosition}: {message}"); // Logging the error along with the error position
            Log.CloseAndFlush();
        }

        public SystemsException(string? message, Exception? innerException, string? errorPosition) : base(message, innerException)
        {
            Ultils.SendErrorToDev(message + "; " + innerException); // Assuming this method sends error to developers
            Log.Error($"An error occurred at {errorPosition}: {message}. Inner exception: {innerException}"); // Logging the error along with the error position
            Log.CloseAndFlush();
        }
    }
}
