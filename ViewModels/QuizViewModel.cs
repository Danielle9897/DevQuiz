using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevQuiz.ViewModels
{
    // This View Model is used to present the main quiz menu to the user,
    // Showing the available categories and subcategories for user to choose from

    public class QuizViewModel
    {
        public List<QuizCategory> categoriesList;

        public QuizViewModel()
        {
            categoriesList = new List<QuizCategory>();
        }
    }

    public class QuizCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<QuizSubCategory> subCategoriesList;

        public QuizCategory()
        {
            subCategoriesList = new List<QuizSubCategory>();
        }
    }

    public class QuizSubCategory
    {
        public int    SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public bool   UseTimeLimit { get; set; }
        public int    NumberOfQuestionsLevelNovice { get; set; }
        public int    NumberOfQuestionsLevelIntermediate { get; set; }
        public int    NumberOfQuestionsLevelExpert { get; set; }
    }

}
