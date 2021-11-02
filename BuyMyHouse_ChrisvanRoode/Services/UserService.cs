using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Models;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;
using MongoDB.Driver;
using MongoDB.Bson;
using Services;
using MongoDB.Bson.Serialization;
using System;
using System.Linq;
using System.Text;
using DAL;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Layout;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using System.Configuration;

namespace Services
{
    public interface IUsersService
    {
        Task<User> CreateUser(User user);
        Task<User> GetUser(int userId);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> UpdateUser(User user);
        Task<bool> DeleteUser(int userId);

        void CalculateMortgage();
    }

    public class UserService : IUsersService
    {
        private readonly IUserRepository _users;
        private readonly IBlobService _blobs;

        public UserService(ILogger<UserService> Logger, IUserRepository userRepository, IBlobService blobs)
        {
            this._users = userRepository;
            this._blobs = blobs;
        }

        public Task<User> CreateUser(User user)
        {
            if (_users.GetUser(user.userId) == null) {
                if (!IsValidEmail(user.email)) return Task.FromResult<User>(null);
                _users.CreateUser(user);
                return Task.FromResult(user);
            }
            return Task.FromResult<User>(null);
        }

        public Task<User> GetUser(int userId)
        {
            BsonDocument retrievedUser = _users.GetUser(userId);
            if (retrievedUser == null) return Task.FromResult<User>(null);
            retrievedUser.Remove("_id");
            User user = BsonSerializer.Deserialize<User>(retrievedUser);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetAllUsers()
        { 
            List<User> users = new List<User>();
            IEnumerable<BsonDocument> bsonUsers = _users.GetAllUsers();
            foreach (BsonDocument bson in bsonUsers)
            {
                bson.Remove("_id");
                User newUser = BsonSerializer.Deserialize<User>(bson);
                users.Add(newUser);
            };
            IEnumerable<User> newList = users;
            return Task.FromResult(newList);
        }

        public Task<User> UpdateUser(User user)
        {
            _users.UpdateUser(user);
            BsonDocument newUser = _users.GetUser(user.userId);
            newUser.Remove("_id");
            return Task.FromResult(BsonSerializer.Deserialize<User>(newUser));
        }

        public Task<bool> DeleteUser(int userId)
        {
            if (_users.GetUser(userId) == null) return Task.FromResult(false);
            return Task.FromResult(_users.DeleteUser(userId));
        }

        private bool IsValidEmail(string email)
        {
            if (email.Trim().EndsWith(".")) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async void CalculateMortgage()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IEnumerable<User> users = await GetAllUsers();
            foreach (User u in users)
            {
                u.mortgage = u.userIncome * 12;

                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Arial", 20, XFontStyle.Bold);

                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(40, 40, page.Width, page.Height);
                gfx.DrawRectangle(XBrushes.White, rect);
                string text = DateTime.UtcNow.Date.ToString();
                tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 80, page.Width, page.Height);
                gfx.DrawRectangle(XBrushes.White, rect);
                text = "Hello, " + u.name;
                tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 120, page.Width, page.Height);
                gfx.DrawRectangle(XBrushes.White, rect);
                text = "Your mortgage is: €" + u.mortgage;
                tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 160, page.Width, page.Height);
                gfx.DrawRectangle(XBrushes.White, rect);
                text = "This is calculated from your income: €" + u.userIncome;
                tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                string filename = "MortgageUser" + u.userId;
                document.Save(filename);
                _blobs.AddFile(filename, "mortgages");
                string mortgageUri = _blobs.GetUri(filename);
                await sendMail(u, mortgageUri);
            }
        }

        public static async Task sendMail(User user, string url)
        {
            var apiKey = ConfigurationManager.AppSettings["sendGridKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("cbvroode@gmail.com", "Chris van Roode");
            var subject = "Mortgage for " + user.name;
            var to = new EmailAddress(user.email, user.name);
            var htmlContent = "<a href=" + url + ">Mortgage pdf</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
