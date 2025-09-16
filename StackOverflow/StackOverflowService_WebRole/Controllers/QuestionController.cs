using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService_WebRole.Controllers
{
    public class QuestionController : Controller
    {
        private QuestionsRepository repo = new QuestionsRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

        // GET: /Question/Add
        public ActionResult Add()
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: /Question/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Question question, HttpPostedFileBase ImageFile)
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                question.PartitionKey = "Question";
                question.RowKey = Guid.NewGuid().ToString();
                question.AuthorEmail = user.Email;
                question.AuthorName = user.FirstName;
                question.CreatedAt = DateTime.UtcNow;
                question.AnswersCount = 0;
                question.TotalVotes = 0;

                // Ako se uploaduje slika
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
                        System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("questionimages");
                    container.CreateIfNotExists();

                    string fileName = question.RowKey + System.IO.Path.GetExtension(ImageFile.FileName);
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromStream(ImageFile.InputStream);

                    question.ImageUrl = blockBlob.Uri.ToString();
                }

                repo.AddQuestion(question);
                TempData["SuccessMessage"] = "Pitanje uspešno dodato!";
                return RedirectToAction("Index");
            }

            return View(question);
        }


        // GET: /Question
        public ActionResult Index(string search = "", string sort = "")
        {
            var questions = repo.GetAllQuestions().ToList();
            var answersRepo = new AnswersRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

            // SINHRONIZACIJA broja odgovora
            foreach (var q in questions)
            {
                var realCount = answersRepo.GetAnswersByQuestionId(q.RowKey).ToList().Count;
                if (q.AnswersCount != realCount)
                {
                    q.AnswersCount = realCount;
                    repo.UpdateQuestion(q);
                }
            }

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

        // GET: /Question/Details/{id}
        public ActionResult Details(string id)
        {
            var question = repo.GetQuestionById(id);
            if (question == null)
                return HttpNotFound();

            var answersRepo = new AnswersRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

            var answersList = answersRepo.GetAnswersByQuestionId(id).ToList();
            var sortedAnswers = answersList.OrderByDescending(a => a.Votes).ToList();

            // SINHRONIZACIJA broja odgovora u Question
            if (question.AnswersCount != sortedAnswers.Count)
            {
                question.AnswersCount = sortedAnswers.Count;
                repo.UpdateQuestion(question);
            }

            ViewBag.Answers = sortedAnswers;
            return View(question);
        }

        // GET: /Question/Edit/{id}
        public ActionResult Edit(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;

            // SAMO autor može uređivati
            if (question == null || user == null || question.AuthorEmail != user.Email)
                return new HttpUnauthorizedResult();

            return View(question);
        }

        // POST: /Question/Edit
        [HttpPost]
        public ActionResult Edit(Question question, HttpPostedFileBase ImageFile)
        {
            var user = Session["CurrentUser"] as User;
            if (user == null || question.AuthorEmail != user.Email)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var existing = repo.GetQuestionById(question.RowKey);
                if (existing == null)
                    return HttpNotFound();

                existing.Title = question.Title;
                existing.Description = question.Description;

                // Upload slike
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
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

            // SAMO autor može brisati
            if (user == null || question.AuthorEmail != user.Email)
            {
                TempData["ErrorMessage"] = "Samo autor može brisati i menjati svoje pitanje.";
                return RedirectToAction("Details", new { id = id });
            }

            repo.DeleteQuestion(question);
            TempData["SuccessMessage"] = "Pitanje uspešno obrisano.";
            return RedirectToAction("Index");
        }
    }
}
