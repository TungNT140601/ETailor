using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Configuration;
using Microsoft.Extensions.Hosting;

namespace Etailor.API.Ultity.CustomException
{
    public class SystemsException : Exception
    {
        public SystemsException(IWebHostBuilder webHost) : base()
        {

        }

        public SystemsException(string? message, string? errorPosition) : base(message)
        {
            LogError(message);
            Ultils.SendErrorToDev(message + " at " + errorPosition);

        }

        public SystemsException(string? message, Exception? innerException, string? errorPosition) : base(message, innerException)
        {
            LogError(message);
            Ultils.SendErrorToDev(message + "; " + innerException + " at " + errorPosition);
        }
        private void LogError(string? errorMsg)
        {
            // Path to your text file
            string filePath = Path.Combine("./wwwroot", "Log", "Check", "ErrorLog.txt");

            try
            {
                if (!Directory.Exists(Path.Combine("./wwwroot", "Log")))
                {
                    Directory.CreateDirectory(Path.Combine("./wwwroot", "Log"));
                }
                if (!Directory.Exists(Path.Combine("./wwwroot", "Log", "Check")))
                {
                    Directory.CreateDirectory(Path.Combine("./wwwroot", "Log", "Check"));
                }
                if (!File.Exists(filePath))
                {
                    var file = File.Create(filePath);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error when write log");
            }

            // Open the file in append mode so that it appends new lines
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"Error: \"{errorMsg}\" at :{DateTime.UtcNow.AddHours(7).ToString("yyyy/MM/dd HH:mm:ss")}");
            }
        }
    }
}
