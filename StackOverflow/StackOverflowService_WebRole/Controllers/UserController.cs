using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackOverflowService_WebRole.Models;


    namespace StackOverflowService_WebRole.Controllers
    {
        public class UserController : Controller
        {
            private UsersRepository repo = new UsersRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

            // GET: /User
            public ActionResult Index()
            {
                var users = repo.GetAllUsers().ToList();
                return View(users);
            }

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
                    repo.AddUser(user);
                    return RedirectToAction("Index");
                }
                return View(user);
            }

            // GET: /User/Login
            public ActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public ActionResult Login(string email, string password)
            {
                var user = repo.GetUserByEmail(email);
                if (user != null && user.PasswordHash == password) // kasnije hash
                {
                    Session["CurrentUser"] = user;
                    return RedirectToAction("Index", "Question");
                }
                ModelState.AddModelError("", "Neispravan email ili lozinka");
                return View();
            }

            // GET: /User/Profile
            public ActionResult Profile() { 
            
                var user = Session["CurrentUser"] as User;
                return View(user);
            }

            // POST: /User/Profile
            [HttpPost]
            public ActionResult Profile(User updatedUser)
            {
                if (ModelState.IsValid)
                {
                    repo.UpdateUser(updatedUser);
                    Session["CurrentUser"] = updatedUser;
                    return RedirectToAction("Index");
                }
                return View(updatedUser);
            }
        }
    }

