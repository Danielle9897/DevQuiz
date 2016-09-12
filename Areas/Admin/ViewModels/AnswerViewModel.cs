using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.ViewModels
{   
    public class AnswerViewModel
    {
        //[DataType(DataType.MultilineText)] // Using tinyMCE instead..
        [AllowHtml]
        public string AnswerText { get; set; }

        public bool IsCorrect { get; set; } 

        public int AnswerId { get; set;  } 
    }
}