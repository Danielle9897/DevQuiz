using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.ViewModels
{
    // This View Model is used to show the ADMIN category details 

    public class CategoryViewModel
    {        
        [Required, StringLength(50, 
                                MinimumLength = 2, 
                                ErrorMessage = "Min Length is 2, Max Length is 50")]

        [Display(Name = "Category Name")]

        [Remote("IsCategoryAvailable", 
                "Categories", 
                ErrorMessage = "Category name already in use.")]

        public string CategoryName { get; set; }

        public int CategoryId { get; set; }   // Will not be seen on screen ...

        public int SubCategoriesCount { get; set; }
    }
}