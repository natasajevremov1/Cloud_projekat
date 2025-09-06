using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace StackOverflowService_WebRole.Controllers
{
    public class QuestionController : Controller
    {
        private QuestionsRepository repo = new QuestionsRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

        // GET: /Question
        public ActionResult Index(string search = "", string sort = "")
        {
            var questions = repo.GetAllQuestions().ToList();

            if (!string.IsNullOrEmpty(search))
            {
                questions = questions
                    .Where(q => !string.IsNullOrEmpty(q.Title) && q.Title.ToLower().Contains(search.ToLower()))
                    .ToList();
            }

            switch (sort)
            {
                case "votes":
                    questions = questions.OrderByDescending(q => q.TotalVotes).ToList();
                    break;
                case "date":
                    questions = questions.OrderByDescending(q => q.CreatedAt).ToList();
                    break;
                default:
                    questions = questions.OrderByDescending(q => q.CreatedAt).ToList();
                    break;
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            return View(questions);
        }

        // GET: /Question/Add
        public ActionResult Add()
        {
            if (Session["CurrentUser"] == null)
            {
                TempData["ErrorMessage"] = "Morate se prvo ulogovati da biste postavili pitanje.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Question/Add
        [HttpPost]
        public ActionResult Add(Question question, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                var user = Session["CurrentUser"] as User;
                if (user == null)
                    return RedirectToAction("Login", "User");

                question.PartitionKey = "Question";
                question.RowKey = Guid.NewGuid().ToString();

                question.AuthorEmail = user.Email; // ✅ sad koristimo Email
                question.AuthorName = user.FirstName + " " + user.LastName;
                question.CreatedAt = DateTime.UtcNow;
                question.TotalVotes = 0;
                question.AnswersCount = 0;

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var storageAccount = CloudStorageAccount.Parse(
                        System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("questionimages");
                    container.CreateIfNotExists();
                    container.SetPermissions(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    string fileName = question.RowKey + System.IO.Path.GetExtension(ImageFile.FileName);
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromStream(ImageFile.InputStream);

                    question.ImageUrl = blockBlob.Uri.ToString();
                }

                repo.AddQuestion(question);
                TempData["SuccessMessage"] = "Pitanje uspešno postavljeno!";
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: /Question/Edit/{id}
        public ActionResult Edit(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;
            if (question == null || user == null || question.AuthorEmail != user.Email) // ✅ Email check
                return new HttpUnauthorizedResult();

            return View(question);
        }

        [HttpPost]
        public ActionResult Edit(Question question, HttpPostedFileBase ImageFile)
        {
            var user = Session["CurrentUser"] as User;
            if (user == null || question.AuthorEmail != user.Email) // ✅ Email check
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var existing = repo.GetQuestionById(question.RowKey);
                if (existing == null)
                    return HttpNotFound();

                existing.Title = question.Title;
                existing.Description = question.Description;

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var storageAccount = CloudStorageAccount.Parse(
                        System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("questionimages");
                    container.CreateIfNotExists();

                    string fileName = existing.RowKey + System.IO.Path.GetExtension(ImageFile.FileName);
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromStream(ImageFile.InputStream);

                    existing.ImageUrl = blockBlob.Uri.ToString();
                }

                repo.UpdateQuestion(existing);
                TempData["SuccessMessage"] = "Pitanje je uspešno izmenjeno!";
                return RedirectToAction("Index");
            }

            return View(question);
        }

        // GET: /Question/Delete/{id}
        public ActionResult Delete(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;

            if (question == null)
                return HttpNotFound();

            if (user == null || question.AuthorEmail != user.Email) // ✅ Email check
            {
                TempData["ErrorMessage"] = "Samo autor može brisati i menjati svoje pitanje.";
                return RedirectToAction("Details", new { id = id });
            }

            repo.DeleteQuestion(question);
            TempData["SuccessMessage"] = "Pitanje uspešno obrisano.";
            return RedirectToAction("Index");
        }

        // GET: /Question/Details/{id}
        // GET: /Question/Details/{id}
        public ActionResult Details(string id)
        {
            var question = repo.GetQuestionById(id);
            if (question == null)
                return HttpNotFound();

            var answersRepo = new AnswersRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

            // Prvo preuzmi podatke u listu
            var answersList = answersRepo.GetAnswersByQuestionId(id).ToList();

            // Sada možeš da sortiraš
            var sortedAnswers = answersList.OrderByDescending(a => a.Votes).ToList();

            ViewBag.Answers = sortedAnswers;
            return View(question);
        }

    }
}
