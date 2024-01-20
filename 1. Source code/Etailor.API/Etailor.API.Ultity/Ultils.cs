using Etailor.API.Ultity.CommonValue;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Google.Cloud.Storage.V1;

namespace Etailor.API.Ultity
{
    public static class Ultils
    {
        public static string GenGuidString()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString().Substring(0, 30);
        }
        public static String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        public static string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password + AppValue.SALT_STRING, salt);
            return hashedPassword;
        }

        public static bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword + AppValue.SALT_STRING, hashedPassword);
        }

        public static void SendOTPMail(string email, string otp)
        {
            try
            {
                //string fromMail = "tungnt14062001@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"

                string fromMail = "tuetailor@gmail.com";
                string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"

                //string fromMail = "tudase151149@gmail.com";
                //string frompassword = "abrxaexoqqpkrjiz"; //"tudase151149@gmail.com"

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Mã xác thực cho [Tuệ Tailor]";
                message.To.Add(new MailAddress(email));
                message.Body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Your Email Title</title>\r\n         <style>\r\n           body {\r\n      background-color: #f4f4f4;\r\n      margin: 0;\r\n      padding: 0;\r\n    }\r\n\r\n    .container {\r\n      max-width: 600px;\r\n      height: 460px;\r\n      margin: 20px auto;\r\n      background-color: #fff;\r\n      border-radius: 8px;\r\n      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n      overflow: hidden;\r\n    }\r\n\r\n    header {\r\n      color: #000;\r\n      text-align: center;\r\n      position: relative;\r\n    }\r\n\r\n    h1 {\r\n      margin: 0;\r\n      font-size: 35px;\r\n      font-family: 'Agbalumo', system-ui;\r\n    }\r\n\r\n    .content {\r\n      padding: 20px 40px;\r\n      padding-bottom: 0;\r\n      font-size: 16px;\r\n      line-height: 1.6;\r\n      z-index: 1000;\r\n    }\r\n\r\n    footer {\r\n      text-align: center;\r\n      padding: 10px 0;\r\n      background-color: #3498db;\r\n      color: #fff;\r\n    }\r\n.logo-image{\r\n  width: 130px;\r\n  height: 130px;\r\n  border-radius: 50%;\r\n\r\n}\r\n\r\n.code{\r\n  background-color: rgba(217, 217, 217, 0.2);\r\n  padding: 20px 60px;\r\n  border: 1px solid #000;\r\n  border-radius: 10px;\r\n  text-align: center;\r\n  letter-spacing: 20px;\r\n  font-size: 30px;\r\n  font-height: bold;\r\n}\r\nh3 {\r\n  text-align: center;\r\n  margin-bottom: 30px;\r\n}\r\n.verify-code{\r\n  text-align: center;\r\n}\r\n@media only screen and (max-width: 600px){\r\n  .code{\r\npadding: 20px 40px;\r\npadding-right: 20px;\r\nletter-spacing: 20px;\r\n\r\n}\r\n}\r\n  </style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n        \r\n    <div style=\"text-align: center\">\r\n      <img src=\"https://drive.google.com/uc?export=view&id=1wT0wV5tdvkVM9L9gkKKddlgbrQggITYn\"\r\n             class=\"logo-image\" />\r\n    </div>\r\n  \r\n      \r\n   \r\n    <hr style=\"width: 500px; margin: 0 auto\">\r\n    <div class=\"content\">\r\n      <p>Xin chào <b>";
                message.Body += email.Replace("@gmail.com", "");
                message.Body += "</b>,</p>\r\n      <p>Để đảm bảo an toàn cho tài khoản của <b>Quý Khách</b>, vui lòng không chia sẻ mã xác thực này với bất kì ai.</p>\r\n      \r\n    </div>\r\n    \r\n       <h3>Mã xác thực là:</h3>\r\n    <div class=\"verify-code\">\r\n    <span class=\"code\"><b>";
                message.Body += otp;
                message.Body += "</b></span>\r\n    </div>\r\n  </div>\r\n</body>\r\n</html>";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void SendResetPassMail(string email, string newPass)
        {
            try
            {
                //string fromMail = "tungnt14062001@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"

                string fromMail = "tuetailor@gmail.com";
                string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"

                //string fromMail = "tudase151149@gmail.com";
                //string frompassword = "abrxaexoqqpkrjiz"; //"tudase151149@gmail.com"

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Xác nhận quên mật khẩu [Tuệ Tailor]";
                message.To.Add(new MailAddress(email));
                message.Body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Your Email Title</title>\r\n         <style>\r\n           body {\r\n      background-color: #f4f4f4;\r\n      margin: 0;\r\n      padding: 0;\r\n    }\r\n\r\n    .container {\r\n      max-width: 600px;\r\n      height: 350px;\r\n      margin: 20px auto;\r\n      background-color: #fff;\r\n      border-radius: 8px;\r\n      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n      overflow: hidden;\r\n    }\r\n\r\n    header {\r\n      color: #000;\r\n      text-align: center;\r\n      position: relative;\r\n    }\r\n\r\n    h1 {\r\n      margin: 0;\r\n      font-size: 35px;\r\n      font-family: 'Agbalumo', system-ui;\r\n    }\r\n\r\n    .content {\r\n      padding: 20px 40px;\r\n      padding-bottom: 0;\r\n      font-size: 16px;\r\n      line-height: 1.6;\r\n      z-index: 1000;\r\n    }\r\n\r\n    footer {\r\n      text-align: center;\r\n      padding: 10px 0;\r\n      background-color: #3498db;\r\n      color: #fff;\r\n    }\r\n.logo-image{\r\n  width: 130px;\r\n  height: 130px;\r\n  border-radius: 50%;\r\n\r\n}\r\n\r\n.code{\r\n  background-color: rgba(217, 217, 217, 0.2);\r\n  padding: 20px 60px;\r\n  border: 1px solid #000;\r\n  border-radius: 10px;\r\n  text-align: center;\r\n  letter-spacing: 20px;\r\n  font-size: 30px;\r\n  font-height: bold;\r\n}\r\nh3 {\r\n  text-align: center;\r\n  margin-bottom: 30px;\r\n}\r\n.verify-code{\r\n  text-align: center;\r\n}\r\n@media only screen and (max-width: 600px){\r\n  .code{\r\npadding: 20px 40px;\r\npadding-right: 20px;\r\nletter-spacing: 20px;\r\n\r\n}\r\n}\r\n  </style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n        \r\n    <div style=\"text-align: center\">\r\n      <img src=\"https://drive.google.com/uc?export=view&id=1wT0wV5tdvkVM9L9gkKKddlgbrQggITYn\"\r\n             class=\"logo-image\" />\r\n    </div>\r\n  \r\n      \r\n   \r\n    <hr style=\"width: 500px; margin: 0 auto\">\r\n    <div class=\"content\">\r\n      <p>Xin chào <b>";
                message.Body += email.Replace("@gmail.com", "");
                message.Body += "</b>,</p>\r\n      <p>Mật khẩu mới để truy cập vào website của quý khách là <b>";
                message.Body += newPass;
                message.Body += "</b>.</p>\r\n      <p>Để đảm bảo an toàn cho tài khoản của quý khách, vui lòng không chia sẻ <b>Mật Khẩu</b> này với bất kì ai.</p>\r\n    </div>\r\n  </div>\r\n</body>\r\n</html>";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void SendOTPPhone(string phone, string otp)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string GetToken(string id, string name, string role, string secrectKey, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.CookiePath, secrectKey)
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";

            return Regex.IsMatch(email, pattern);
        }

        public static bool IsValidVietnamesePhoneNumber(string phoneNumber)
        {
            string pattern = @"^0[3|5|7|8|9][0-9]{8}$";

            return Regex.IsMatch(phoneNumber, pattern);
        }

        public static string GenerateRandom6Digits()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000000);

            return randomNumber.ToString("D6");
        }

        public static string GenerateRandomString(int length)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(characters, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task<string> UploadImage(StorageClient _storage, string wwwrootPath, string generalPath, IFormFile file)
        {
            // Process the file
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(wwwrootPath, "Upload", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Upload to Firebase Storage
            var bucketName = AppValue.BUCKET_NAME;
            var objectName = $"Uploads/{generalPath}/{fileName}";

            Google.Apis.Storage.v1.Data.Object uploadFile = new Google.Apis.Storage.v1.Data.Object();

            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                uploadFile = _storage.UploadObject(bucketName, objectName, file.ContentType, fileStream);
            }

            // Clean up: delete the local file
            System.IO.File.Delete(filePath);

            // Get the view link
            FirebaseStorage storage = new FirebaseStorage(AppValue.BUCKET_NAME);
            var starsRef = storage.Child(objectName);
            string link = await starsRef.GetDownloadUrlAsync();
            return link;
        }

        public static async Task<List<string>> UploadImages(StorageClient _storage, string wwwrootPath, string generalPath, List<IFormFile> files)
        {
            List<Task<string>> uploadTasks = new List<Task<string>>();

            foreach (var file in files)
            {
                uploadTasks.Add(UploadImage(_storage, wwwrootPath, generalPath, file));
            }

            // Wait for all tasks to complete
            var links = await Task.WhenAll(uploadTasks);

            return links.ToList();
        }

    }
}