using System;
using System.Web.Mvc;
using StackOverflowService_WebRole.Models;

namespace StackOverflowService_WebRole.Controllers
{
    public class AnswerController : Controller
    {
        private AnswersRepository repo = new AnswersRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
        private VotesRepository votesRepo = new VotesRepository(
            System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);

        // POST: /Answer/Add
        [HttpPost]
        public ActionResult Add(string questionId, string content)
        {
            var user = Session["CurrentUser"] as User;
            if (user == null)
                return RedirectToAction("Login", "User");

            var answer = new Answer(questionId, Guid.NewGuid().ToString())
            {
                AuthorEmail = user.Email,
                AuthorName = user.FirstName + " " + user.LastName,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                Votes = 0,
                IsAccepted = false
            };
            repo.AddAnswer(answer);

            return RedirectToAction("Details", "Question", new { id = questionId });
        }

        // POST: /Answer/Vote (sada samo 👍, jedan glas po korisniku)
        [HttpPost]
        public ActionResult Vote(string questionId, string answerId)
        {
            var user = Session["CurrentUser"] as User;
            if (user != null)
            {
                // Dodaj glas samo ako korisnik još nije glasao
                if (!votesRepo.HasUserVoted(answerId, user.Email))
                {
                    votesRepo.AddVote(answerId, user.Email);
                }
            }
            return RedirectToAction("Details", "Question", new { id = questionId });
        }

        [HttpPost]
        public ActionResult Accept(string questionId, string answerId)
        {
            var answer = repo.GetAnswerById(questionId, answerId);
            var user = Session["CurrentUser"] as User;

            var questionRepo = new QuestionsRepository(
                System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"]);
            var question = questionRepo.GetQuestionById(questionId);

            // Samo autor pitanja može označiti najbolji odgovor
            if (question == null || user == null || question.AuthorEmail != user.Email)
                return new HttpUnauthorizedResult();

            // Resetuj prethodni najbolji odgovor
            var allAnswers = repo.GetAnswersByQuestionId(questionId);
            foreach (var ans in allAnswers)
            {
                ans.IsAccepted = false;
                repo.MarkAsAccepted(ans);
            }

            // Obeleži izabrani odgovor kao najbolji
            answer.IsAccepted = true;
            repo.MarkAsAccepted(answer);

            return RedirectToAction("Details", "Question", new { id = questionId });
        }

    }
}
