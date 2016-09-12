using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.ViewModels
{
    // These View Models are used to show the ADMIN the details about the users

    public class UserPartialDetailsViewModel
    {       
        [Display(Name="Email")]
        public string Email { get; set; }

        [Display(Name = "Name")]
        public string FirstName { get; set; } 

        public string LastName { get; set;  } 
    }

    public class UserFullDetailsViewModel
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public List<QuizSummaryViewModel> ListOfQuizesTaken { get; set; }

        public UserFullDetailsViewModel()
        {
            ListOfQuizesTaken = new List<QuizSummaryViewModel>();
        }
    }

    public class QuizSummaryViewModel
    {        
        public DateTime TimeQuizTaken { get; set; }     
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int UserScore { get; set; }         
    }
}