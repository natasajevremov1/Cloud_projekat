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
            // Uzmi connection string iz AppSettings
            string storageConnectionString = System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"];

            var questionRepo = new QuestionsRepository(storageConnectionString);
            var answerRepo = new AnswersRepository(storageConnectionString);

            var question = questionRepo.GetQuestionById(questionId);
            var answer = answerRepo.GetAnswerById(questionId, answerId);

            if (question == null || answer == null)
                return HttpNotFound();

            // Dozvoljeno samo autoru pitanja
            var currentUser = Session["CurrentUser"] as User;
            if (currentUser == null || currentUser.Email != question.AuthorEmail)
                return new HttpUnauthorizedResult();

            // Poništi prethodni najbolji odgovor i setuj novi
            answerRepo.MarkAsAccepted(answer);

            return RedirectToAction("Details", "Question", new { id = questionId });
        }



    }
}
