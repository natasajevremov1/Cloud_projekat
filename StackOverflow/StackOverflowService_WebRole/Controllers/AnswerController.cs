using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackOverflowService_WebRole.Models;

namespace StackOverflowService_WebRole.Controllers
{
    public class AnswerController : Controller
    {
        private AnswersRepository repo = new AnswersRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

        // POST: /Answer/Add
        [HttpPost]
        public ActionResult Add(string questionId, string content)
        {
            var user = Session["CurrentUser"] as User;
            var answer = new Answer(questionId, System.Guid.NewGuid().ToString())
            {
                AuthorEmail = user.PartitionKey,
                Content = content,
                CreatedAt = System.DateTime.UtcNow,
                Votes = 0,
                IsAccepted = false
            };
            repo.AddAnswer(answer);
            return RedirectToAction("Details", "Question", new { id = questionId });
        }

        // POST: /Answer/Vote
        [HttpPost]
        public ActionResult Vote(string questionId, string answerId, int voteChange)
        {
            var answer = repo.GetAnswerById(questionId, answerId);
            repo.VoteAnswer(answer, voteChange);
            return RedirectToAction("Details", "Question", new { id = questionId });
        }

        // POST: /Answer/Accept
        [HttpPost]
        public ActionResult Accept(string questionId, string answerId)
        {
            var answer = repo.GetAnswerById(questionId, answerId);
            var user = Session["CurrentUser"] as User;

            // Proveri da li je trenutni korisnik autor pitanja
            var questionRepo = new QuestionsRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
            var question = questionRepo.GetQuestionById(questionId);

            if (question.AuthorEmail != user.PartitionKey)
                return new HttpUnauthorizedResult();

            // Oznaci odgovor kao najbolji
            repo.MarkAsAccepted(answer);

            // POZIV NotificationService (slanje mejlova)
          //  NotificationService.SendBestAnswerNotification(questionId, answerId);

            return RedirectToAction("Details", "Question", new { id = questionId });
        }
    }
}
