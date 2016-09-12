using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.ViewModels
{
    // This View Model is used to show the ADMIN question details 

    public class QuestionViewModel
    {
        // Id
        public int QuestionId { get; set; }

        // Title
        [Required]
        [Display(Name = "Question Title")]
        [StringLength(100, MinimumLength = 5,
                      ErrorMessage = "Min Length is 5, Max Length is 100 characters")]
        public string QuestionTitle { get; set; }

        // Body
        [Required]
        [Display(Name = "Question Body")]
        //[DataType(DataType.MultilineText)] // Using tinyMCE instead..
        [StringLength(500, MinimumLength = 2,
                      ErrorMessage = "Min Length is 2, Max Length is 500 characters")]
        [AllowHtml]
        public string QuestionText { get; set; }

        // Number
        [Required]
        [Display(Name = "Question number")]
        [Range(1, 2000, ErrorMessage = "Number should be between 1 and 2000")]             
        [Remote("IsNumberAvailable",
                "Questions",
                 AdditionalFields = "SubCategoryId,QuestionId",
                 ErrorMessage = "Number already in use by other question")]
        public int QuestionNumber { get; set; }

        // Level
        [Required]
        [Display(Name = "Level")]        
        public int QuestionLevel { get; set; }

        // Time (Seconds to answer) 
        [Display(Name = "Time to Answer (Seconds)")]
        [Range(0, int.MaxValue, ErrorMessage = "Time to answer, in seconds, should be any number greater than 0")]        
        public int TimeToAnswer { get; set; }

        // Points
        [Display(Name = "Points")]        
        public int CreditPoints { get; set; }

        // Image
        [Display(Name = "Image (Optional)")]
        public string RelatedImage { get; set; }

        // Sub-Category Info (hidden on screen)
        public int SubCategoryId { get; set; }   
        public string SubCategoryName { get; set; }        

        // Explanation
        [Display(Name = "Explanation")]
        //[DataType(DataType.MultilineText)] // Using tinyMCE instead..
        [AllowHtml]
        public string Explanation { get; set; }       

        // Single or Multiple answers
        public bool SingleAnswer { get; set; }

        // Answers
        public List<AnswerViewModel> AnswersList { get; set; } 

        // Constructor                                                                                         
        public QuestionViewModel()
        {
            AnswersList = new List<AnswerViewModel>();            
        }
    }   
    
}