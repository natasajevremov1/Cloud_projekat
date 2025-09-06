using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using StackOverflowService_WebRole.Models;

namespace StackOverflowService_WebRole.Controllers
{
    public class UserController : Controller
    {
        private UsersRepository repo = new UsersRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

        // GET: /User/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Formatiranje email-a (trim + lowercase)
                if (string.IsNullOrEmpty(user.Email))
                {
                    ModelState.AddModelError("", "Email je obavezan.");
                    return View(user);
                }
                user.Email = user.Email.Trim().ToLower();

                // Provera da li korisnik već postoji
                var existingUser = repo.GetUserByEmail(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Korisnik sa ovim emailom već postoji.");

                    // Očisti ostala polja osim emaila
                    user.PasswordHash = string.Empty;
                    user.FirstName = string.Empty;
                    user.LastName = string.Empty;
                    user.Gender = string.Empty;
                    user.Country = string.Empty;
                    user.City = string.Empty;
                    user.Address = string.Empty;

                    return View(user);
                }

                // Provera lozinke
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    ModelState.AddModelError("", "Lozinka je obavezna.");
                    return View(user);
                }

                // Postavljanje PartitionKey i RowKey
                user.PartitionKey = "User";
                user.RowKey = user.Email.Trim().ToLower(); // fiksno, jedinstveno
                user.Email = user.RowKey;

                // Hash lozinke
                user.PasswordHash = HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.UtcNow;

                // Dodavanje korisnika
                repo.AddUser(user);

                TempData["Success"] = "Registracija uspešna! Sada se možete prijaviti.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /User/EditProfile
        public ActionResult EditProfile()
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
            {
                TempData["ErrorMessage"] = "Morate biti prijavljeni da biste menjali profil.";
                return RedirectToAction("Login");
            }

            // Dohvati svež objekat iz baze (sigurnije)
            var currentUser = repo.GetUserByEmail(user.Email);
            if (currentUser == null)
                return HttpNotFound();

            return View(currentUser);
        }

        // POST: /User/EditProfile
        [HttpPost]
        public ActionResult EditProfile(User updatedUser, HttpPostedFileBase ProfileImage)
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
            {
                TempData["ErrorMessage"] = "Morate biti prijavljeni da biste menjali profil.";
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var existingUser = repo.GetUserByEmail(user.Email);
                if (existingUser == null)
                    return HttpNotFound();

                // Ažuriranje polja koja korisnik može menjati
                existingUser.FirstName = updatedUser.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Gender = updatedUser.Gender;
                existingUser.Country = updatedUser.Country;
                existingUser.City = updatedUser.City;
                existingUser.Address = updatedUser.Address;

                // Upload nove profilne slike ako postoji
                if (ProfileImage != null && ProfileImage.ContentLength > 0)
                {
                    var storageAccount = CloudStorageAccount.Parse(
                        System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("profileimages");
                    container.CreateIfNotExists();
                    container.SetPermissions(new Microsoft.WindowsAzure.Storage.Blob.BlobContainerPermissions
                    {
                        PublicAccess = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Blob
                    });

                    string fileName = existingUser.Email + System.IO.Path.GetExtension(ProfileImage.FileName);
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromStream(ProfileImage.InputStream);

                    existingUser.ProfileImageUrl = blockBlob.Uri.ToString();
                }

                // Sačuvaj promene u bazi
                repo.UpdateUser(existingUser);

                // Update session
                Session["CurrentUser"] = existingUser;

                TempData["SuccessMessage"] = "Profil uspešno izmenjen!";
                return RedirectToAction("EditProfile");
            }

            return View(updatedUser);
        }


        // GET: /User/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Unesite email i lozinku.");
                return View();
            }

            // Formatiranje email-a
            email = email.Trim().ToLower();

            var user = repo.GetUserByEmail(email);

            if (user == null || user.PasswordHash != HashPassword(password))
            {
                ModelState.AddModelError("", "Ovaj korisnik nema nalog ili je neispravna lozinka.");
                return View();
            }

            Session["CurrentUser"] = user;
            return RedirectToAction("Index", "Question"); // kasnije lista pitanja
        }

        // GET: /User/Profile
        public ActionResult Profile()
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // POST: /User/Profile
        [HttpPost]
        public ActionResult Profile(User updatedUser)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(updatedUser.Email))
                {
                    ModelState.AddModelError("", "Email je obavezan.");
                    return View(updatedUser);
                }

                updatedUser.Email = updatedUser.Email.Trim().ToLower();
                updatedUser.PartitionKey = "User";
                updatedUser.RowKey = updatedUser.Email;

                repo.UpdateUser(updatedUser);
                Session["CurrentUser"] = updatedUser;
                return RedirectToAction("Index", "Question");
            }
            return View(updatedUser);
        }

        // GET: /User/Logout
        public ActionResult Logout()
        {
            Session["CurrentUser"] = null;
            return RedirectToAction("Login");
        }

        // Hash funkcija za lozinku
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }


        }
    }
}
