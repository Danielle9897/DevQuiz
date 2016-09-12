using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.ViewModels
{
    // This View Model is used to show the ADMIN the details about the Sub-Category

    public class SubCategoryViewModel
    {
        [Required, StringLength(50, 
                                MinimumLength = 2, 
                                ErrorMessage = "Min Length is 2, Max Length is 50")]

        [Display(Name = "Sub-Category Name")]

        [Remote("IsSubCategoryAvailable", 
                "SubCategories", 
                 AdditionalFields = "CategoryId", 
                 ErrorMessage = "Sub-Category name already in use in this category.")] 

        public string SubCategoryName { get; set; }

        public int SubCategoryId { get; set; }   // Hidden on screen ...

        public string CategoryName { get; set; }

        public int CategoryId { get; set; }

        public int QuestionsCount { get; set; }
    }
}