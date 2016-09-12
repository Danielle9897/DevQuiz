using System.ComponentModel.DataAnnotations;

namespace DevQuiz.Areas.Admin.ViewModels
{
    // This View Model is used to show the ADMIN details about search results

    public class SearchResultViewModel
    {              
        // Id       
        public int QuestionId { get; set; }

        // Title
        [Display(Name = "Question Title")]
        public string QuestionTitle { get; set; }

        // Body
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }

        // Number
        [Display(Name = "Question No.")]
        public int QuestionNumber { get; set; }

        // Category Info 
        public int CategoryId { get; set; }
        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        // Sub-Category Info 
        public int SubCategoryId { get; set; }
        [Display(Name = "Sub-Category")]
        public string SubCategoryName { get; set; }
    }   
    
}