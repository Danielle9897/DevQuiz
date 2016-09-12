using DevQuiz.Models;
using DevQuiz.Repository;
using DevQuiz.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DevQuiz.ManageResults
{
    public class Results
    {
        private ApplicationRepository repository = new ApplicationRepository();

        private string userName;

        // The input object from the user quiz
        private int subCategoryId;
        private List<UserQuestionData> userQuestionsData;

        // The output object needed by the results view
        private List<QuizResultsItem> resultsItems;
        public List<QuizResultsItem> ResultsItems { get { return resultsItems; } }

        // User score will be calcutlated
        private int score;
        public int Score { get { return score; } }

        // Number of correct questions will be calcutlated
        private int numberOfCorrectQuestions;
        public int NumberOfCorrectQuestions { get { return numberOfCorrectQuestions; } }

        // Constructor
        public Results(UserQuizData userQuizData, string userNameThatTookTheQuiz)
        {
            userName = userNameThatTookTheQuiz;
            subCategoryId = userQuizData.SubCategoryId;
            userQuestionsData = userQuizData.QuestionsData;
            resultsItems = new List<QuizResultsItem>();            
        }

        public async Task SaveResultsToRepository()
        {
            // 1. Calculate score
            await CalcScore();

            // 2. Save results summary to repository
            QuizSummaryForUser quizSummary = new QuizSummaryForUser(userName);
            quizSummary.SubCateogory = subCategoryId;
            quizSummary.TimeQuizTaken = DateTime.Now;            
            quizSummary.UserScore = Score;      

            // 2.1 Create the answers string
            foreach (UserQuestionData question in userQuestionsData)
            {
                quizSummary.UserAnswers += ("Q" + question.QuestionId);
                foreach(UserAnswerData answer in question.AnswersSelected)
                {
                    quizSummary.UserAnswers += (":A" + answer.AnswerId);
                }

                quizSummary.UserAnswers += "-";
            }

            // 3. Save results in db (but not for admin user..)
            if (userName != "AdminUser@admin.com")
            {
                // 3.1 Save the results in Quiz Summary Table 
                await repository.SaveSummary(quizSummary);

                // 3.2 Update the sub-category db table with top score info
                await repository.UpdateTopScoreInfo(quizSummary);
            }
        }

        private async Task CalcScore()
        {
            score = 0;
            int correctQuestions = 0;           

            // 1. Loop on questions and add up the credit points
            for (int i = 0; i < userQuestionsData.Count; i++)
            {
               
                List<string> correctAnswersFromRepository = await repository.GetCorrectAnswers(userQuestionsData[i].QuestionId);

                // Convert user answers list of int to list of strings so its easy to compare..
                List<string> userAnswers = new List<string>();
                foreach(UserAnswerData a in userQuestionsData[i].AnswersSelected)
                {
                    userAnswers.Add(a.AnswerId.ToString());
                }

                // 2. Compare
                IEnumerable<string> wrongAnswers = userAnswers.Except(correctAnswersFromRepository).ToList();                
                IEnumerable<string> correctAnswersThatWhereNotSelected = correctAnswersFromRepository.Except(userAnswers).ToList();
                if ((wrongAnswers.Count() == 0) && (correctAnswersThatWhereNotSelected.Count() == 0))
                {
                    // All answers correct -- add up score !
                    score += repository.GetQuestionScore(userQuestionsData[i].QuestionId);
                    correctQuestions++;
                }

                Question question = await repository.GetQuestionWithAnswers(userQuestionsData[i].QuestionId);

                // 3. Create the results item
                QuizResultsItem resultsItem = new QuizResultsItem();
                resultsItem.Explanation = question.Explanation;
                resultsItem.QuestionText = question.QuestionText;

                List<QuizResultsAnswerItem> resultsAnswers = new List<QuizResultsAnswerItem>();
                foreach (Answer a in question.AnswersList)
                {
                    QuizResultsAnswerItem resultAnswerItem = new QuizResultsAnswerItem()
                    {
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect,
                        SelectedByUser = userAnswers.Contains(a.AnswerId.ToString())
                    };
                    resultsAnswers.Add(resultAnswerItem);
                }                            
                resultsItem.ResultsAnswersItems = resultsAnswers;

                // 4. Add the results item to the collection
                resultsItems.Add(resultsItem);
            }

            numberOfCorrectQuestions = correctQuestions;
        }            
    }
}