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

        // GET: /Question
        public ActionResult Index(string search = "", string sort = "")
        {
            // Dohvati pitanja i odmah ih pretvori u List<Question>
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
                default:
                    questions = questions.OrderByDescending(q => q.CreatedAt).ToList();
                    break;
            }

            return View(questions);
        }

        // GET: /Question/Add
        public ActionResult Add()
        {
            return View();
        }

        // POST: /Question/Add
        [HttpPost]
        public ActionResult Add(Question question)
        {
            if (ModelState.IsValid)
            {
                var user = Session["CurrentUser"] as User;
                question.AuthorEmail = user.PartitionKey;
                question.CreatedAt = System.DateTime.UtcNow;
                question.TotalVotes= 0;
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
        public ActionResult Delete(string id)
        {
            var question = repo.GetQuestionById(id);
            var user = Session["CurrentUser"] as User;
            if (question == null || question.AuthorEmail != user.PartitionKey)
                return HttpNotFound();

            repo.DeleteQuestion(question);
            return RedirectToAction("Index");
        }

        // GET: /Question/Details/{id}
        public ActionResult Details(string id)
        {
            var question = repo.GetQuestionById(id);
            if (question == null)
                return HttpNotFound();

            return View(question);
        }
    }
}