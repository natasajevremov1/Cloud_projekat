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
            // Dohvati sva pitanja
            var questions = repo.GetAllQuestions().ToList();

            // Pretraga po naslovu
            if (!string.IsNullOrEmpty(search))
            {
                questions = questions
                    .Where(q => !string.IsNullOrEmpty(q.Title) && q.Title.ToLower().Contains(search.ToLower()))
                    .ToList();
            }

            // Sortiranje
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

            // ViewBag za zadržavanje search i sort vrednosti
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            return View(questions);
        }
        // GET: /Question/Add
        public ActionResult Add()
        {
            return View();
        }

        // POST: /Question/Add
        [HttpPost]
        public ActionResult Add(Question question, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                var user = Session["CurrentUser"] as User;

                // OBAVEZNO postavi PK i RK
                question.PartitionKey = "Question";
                question.RowKey = Guid.NewGuid().ToString();

                question.AuthorEmail = user.PartitionKey;
                question.AuthorName = user.FirstName + " " + user.LastName; // <-- ovo je novo
                question.CreatedAt = DateTime.UtcNow;
                question.TotalVotes = 0;
                question.AnswersCount = 0;

                // Ako je uploadovana slika
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var storageAccount = CloudStorageAccount.Parse(
                        System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("questionimages");
                    container.CreateIfNotExists();
                    container.SetPermissions(new Microsoft.WindowsAzure.Storage.Blob.BlobContainerPermissions
                    {
                        PublicAccess = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Blob
                    });

                    string fileName = question.RowKey + System.IO.Path.GetExtension(ImageFile.FileName);
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromStream(ImageFile.InputStream);

                    question.ImageUrl = blockBlob.Uri.ToString();
                }

                repo.AddQuestion(question);

                return RedirectToAction("Index");
            }
            return View(question);
        }
        // GET: /Question/Edit/{id}
        public ActionResult Edit(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;
            if (question == null || question.AuthorEmail != user.PartitionKey)
                return HttpNotFound();

            return View(question);
        }

        [HttpPost]
        public ActionResult Edit(Question question)
        {
            var user = Session["CurrentUser"] as User;
            if (question.AuthorEmail != user.PartitionKey)
                return new HttpUnauthorizedResult();

            repo.UpdateQuestion(question);
            return RedirectToAction("Index");
        }

        // GET: /Question/Delete/{id}
        // GET: /Question/Delete/{id}
        // GET: /Question/Delete/{id}
        public ActionResult Delete(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;

            if (question == null)
                return HttpNotFound();

            // Neulogovani korisnik ili neautor
            if (user == null || question.AuthorEmail != user.PartitionKey)
            {
                TempData["ErrorMessage"] = "Samo autor može brisati i vršiti izmenu svog pitanja.";
                return RedirectToAction("Details", new { id = id });
            }

            // Ako je autor → dozvoli brisanje
            repo.DeleteQuestion(question);
            TempData["SuccessMessage"] = "Pitanje uspešno obrisano.";
            return RedirectToAction("Index");
        }
        // GET: /Question/Details/{id}
        public ActionResult Details(string id)
        {
            var question = repo.GetQuestionById(id);
            if (question == null)
                return HttpNotFound();

            // Dohvati sve odgovore za ovo pitanje
            var answersRepo = new AnswersRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
            var answers = answersRepo.GetAnswersByQuestionId(id).ToList()
                             .OrderByDescending(a => a.Votes)
                             .ToList();

            // Prosledi odgovore u ViewBag
            ViewBag.Answers = answers;

            return View(question);
        }

    }
}