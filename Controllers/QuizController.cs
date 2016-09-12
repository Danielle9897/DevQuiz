using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.ManageResults;
using DevQuiz.Models;
using DevQuiz.Repository;
using DevQuiz.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DevQuiz.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        // GET:  \quiz       
        public async Task<ActionResult> Index()
        {
            // 1. Get categories and sub-categories info from DB and pass to the quiz view
            QuizViewModel quizInfo = new QuizViewModel();

            List<Category> categoriesList = await repository.GetCategoriesListWithSubCategories();
            // At this point, the categories are ordered alphbetically while the inside sub-categories are not
            // Need to order them later on as well..

            bool subCategoryAdded = false;
            int totalNumberOfQuestionsInSubCategory = 0;

            foreach (Category category in categoriesList)
            {
                QuizCategory c = new QuizCategory();
                c.CategoryId = category.CategoryId;
                c.CategoryName = category.CategoryName;

                // 2. Order the sub-categories
                category.SubCategoriesList.Sort((x, y) => string.Compare(x.SubCategoryName, y.SubCategoryName));

                foreach (SubCategory subCategory in category.SubCategoriesList)
                {
                    // 3. Add sub-category ONLY IF it contains questions...
                    totalNumberOfQuestionsInSubCategory = await repository.getNumberOfQuestionsTotal(subCategory.SubCategoryId);
                    if (totalNumberOfQuestionsInSubCategory > 0)
                    {
                        QuizSubCategory s = new QuizSubCategory()
                        {
                            UseTimeLimit = false, // The default for now...
                            NumberOfQuestionsLevelNovice = await repository.getNumberOfQuestionsLevelNovice(subCategory.SubCategoryId),
                            NumberOfQuestionsLevelIntermediate = await repository.getNumberOfQuestionsLevelIntermediate(subCategory.SubCategoryId),
                            NumberOfQuestionsLevelExpert = await repository.getNumberOfQuestionsLevelExpert(subCategory.SubCategoryId),
                            SubCategoryId = subCategory.SubCategoryId,
                            SubCategoryName = subCategory.SubCategoryName
                        };
                        c.subCategoriesList.Add(s);
                        subCategoryAdded = true;
                    }
                }

                if (subCategoryAdded)
                {
                    quizInfo.categoriesList.Add(c);
                    subCategoryAdded = false;
                }
            }

            return View(quizInfo);
        }

        // POST:  *** Called by an AJAX call from the client !
        //        *** Will return the next question up as JSON !
        //        *** If no more questions then return a PARTIAL VIEW with results summary !
        [HttpPost]
        public async Task<ActionResult> GetNextQuestion(CurrentQuestion currentQuestionInfo)
        {
            // 0. Sleep for testing.....
            // System.Threading.Thread.Sleep(5000);

            // 1. Get next question info from db
            Question nextQuestion = await repository.getNextQuestion(currentQuestionInfo);

            // 2. We should not be in this situation,
            //    since client detects the last question and ask directly for results, but just in case.
            if (nextQuestion == null)
            {               
                return Json("NoMoreQuestions");
            }

            // 3. Fill view model with info  
            //    Note: QuestionViewModel model was defined in the Admin area ViewModels folder
            QuestionViewModel nextQuestionView = new QuestionViewModel();
            nextQuestionView.SingleAnswer = nextQuestion.IsSingleAnswer;
            nextQuestionView.QuestionText = nextQuestion.QuestionText;
            nextQuestionView.QuestionId = nextQuestion.QuestionId;
            nextQuestionView.QuestionNumber = nextQuestion.QuestionNumber;
            nextQuestionView.CreditPoints = nextQuestion.CreditPoints;
            nextQuestionView.TimeToAnswer = nextQuestion.TimeToAnswer;
            nextQuestionView.Explanation = nextQuestion.Explanation;

            foreach (Answer answer in nextQuestion.AnswersList)
            {
                AnswerViewModel answerView = new AnswerViewModel()
                {
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect,
                    AnswerId = answer.AnswerId
                };
                nextQuestionView.AnswersList.Add(answerView);
            }

            // 4. Return NEXT QUESTION    
            //    Note: Loop back the quiz session id we came from...                                   
            return Json( new { nextQuestionView, currentQuestionInfo.currentQuizSessionId } );
        }

        // POST:  *** Called by an AJAX call from the client !
        //        *** 0. Calculate credit points
        //        *** 1. Save the quiz results to DB for the logged user
        //        *** 2. Return results as a partial view to client so user can see results
        [HttpPost]
        public async Task<ActionResult> ProcessResults(UserQuizData userQuizData)
        {
            // 0. Sleep for testing
            // System.Threading.Thread.Sleep(5000);

            // 1. Calculate score and save quiz results to db     
            Results results = new Results(userQuizData, User.Identity.Name);          
            await results.SaveResultsToRepository();

            // 2. Return results view to client
            QuizResultsViewModel quizResultsViewModel = new QuizResultsViewModel();            

            quizResultsViewModel.SubCategoryName = (await repository.GetSubCategory(userQuizData.SubCategoryId)).SubCategoryName;         
            quizResultsViewModel.TotalNumberOfQuestions = userQuizData.QuestionsData.Count;
            
            quizResultsViewModel.Score = results.Score;
            quizResultsViewModel.NumberOfCorrectQuestions = results.NumberOfCorrectQuestions;
            quizResultsViewModel.ResultsItems = results.ResultsItems;

            // 3. insert the quizSessionId to the html somehow, so the js can read it back in the client !
            quizResultsViewModel.quizSessionId = userQuizData.currentQuizSessionId;

            // 4. Return an html view
            return PartialView("Results", quizResultsViewModel);       
        }
    }
}