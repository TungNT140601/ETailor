using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Builder.Extensions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Notification = FirebaseAdmin.Messaging.Notification;

namespace Etailor.API.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private string FilePath = "";

        public WeatherForecastController()
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), "tokens.txt"); // Specify your file path
        }

        [HttpPost]
        public IActionResult SendMail(string body)
        {
            try
            {
                //string fromMail = "tungnt14062001@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"
                //string fromMail = "tuetailor@gmail.com";
                //string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"
                string fromMail = "tudase151149@gmail.com";
                string frompassword = "abrxaexoqqpkrjiz"; //"tudase151149@gmail.com"

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Test Mail " + DateTime.Now.Ticks;
                message.To.Add(new MailAddress("tudase151149@gmail.com"));
                message.Body = body;
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, frompassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult AddTokenDevice(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token");
            }

            List<string> existingTokens = GetTokensFromFile(FilePath);

            // Check if the token already exists in the loaded tokens
            if (!existingTokens.Contains(token.Trim()))
            {
                // If the token doesn't exist, add it to the file
                AddTokenToFile(token, FilePath);
                return Ok("Token added successfully");
            }
            else
            {
                Console.WriteLine($"Token already exists: {token}");
                return Ok("Token already exists");
            }
        }
        private List<string> GetTokensFromFile(string filePath)
        {
            List<string> tokens = new List<string>();

            try
            {
                // Read tokens from the file and add them to the list
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        tokens.Add(line.Trim());
                    }
                }

                Console.WriteLine($"Tokens loaded from file: {string.Join(", ", tokens)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tokens from file: {ex.Message}");
            }

            return tokens;
        }


        private void AddTokenToFile(string token, string filePath)
        {
            try
            {
                // Check if the token already exists in the file
                if (!TokenExistsInFile(token, filePath))
                {
                    // If the token doesn't exist, add it to the file
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(token);
                    }

                    Console.WriteLine($"Token added to file: {token}");
                }
                else
                {
                    Console.WriteLine($"Token already exists in the file: {token}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding token to file: {ex.Message}");
            }
        }

        private bool TokenExistsInFile(string token, string filePath)
        {
            try
            {
                // Check if the token exists in the file
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Trim() == token.Trim())
                        {
                            return true; // Token already exists
                        }
                    }
                }

                return false; // Token not found in the file
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking token in file: {ex.Message}");
                return false; // Assume token not found in case of an error
            }
        }
        //-----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SendNotifyAsync([FromBody] Notify notify)
        {
            List<string> tokens = GetTokensFromFile(FilePath);
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = notify.Title,
                    Body = notify.Body,
                },
                Tokens = tokens
            };
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            return Ok($"Successfully sent message: {response}");
        }
    }
    public class Notify
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }
}