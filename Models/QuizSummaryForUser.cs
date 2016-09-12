using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DevQuiz.Models
{
    public class QuizSummaryForUser
    {
        // This class is designed to keep the LATEST (not all) results for a user per sub-category      

        [Key]
        [Column(Order = 1)]  
        public string UserName { get; set; }          // (KEY 1) The user that took the quiz      

        [Key]
        [Column(Order = 2)]
        public int SubCateogory { get; set; }         // (KEY 2) The quiz sub-category   

        // The folowing 2 are not really needed so in comment for now..
        //// Foreign Key - defined as foreign key in fluent api instead of using: [ForeignKey("UserId")]
        //public string UserId { get; set; }                   // ???
        //public ApplicationUser ApplicationUser { get; set; } // ???

        public DateTime TimeQuizTaken { get; set; }   // The date and time user took the quiz      
        public int UserScore { get; set; }            // The score the user got for this quiz
        public string UserAnswers { get; set; }       // A string that needs to be parsed, has the user selected Answers Ids

        // Constructors
        public QuizSummaryForUser(string userNameThatTookTheQuiz)
        {            
            UserName = userNameThatTookTheQuiz;          
        }
        public QuizSummaryForUser()
        {
            // Need this since I used a parameterized constructor above,
            // so no default constructor is provided by the .net
        }
    }    
}


