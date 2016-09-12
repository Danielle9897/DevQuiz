using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevQuiz.ViewModels
{
    // This View Model is used to show the quiz results to the USER

    public class QuizResultsViewModel
    {
        public int quizSessionId { get; set; }

        public string SubCategoryName { get; set; }
        public int Score { get; set; }
        //public int OutOfTotalPoints { get; set; }
        public int NumberOfCorrectQuestions { get; set; }
        public int TotalNumberOfQuestions { get; set; }          
        public List<QuizResultsItem> ResultsItems { get; set; }

        public QuizResultsViewModel()
        {
            ResultsItems = new List<QuizResultsItem>();
        }
    }

    public class QuizResultsItem
    {
        public string QuestionText { get; set; }
        public string Explanation { get; set; }
        public List<QuizResultsAnswerItem> ResultsAnswersItems { get; set; }

        public QuizResultsItem()
        {
            ResultsAnswersItems = new List<QuizResultsAnswerItem>();
        }
    }

    public class QuizResultsAnswerItem
    {
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public bool SelectedByUser { get; set; }
    }
}
