using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevQuiz.Models
{
    public class Category
    {
        // Primary Key - Category Id
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Navigation property - Questions List for this Category/SubCategory (One To Many relationship with Questions Table)
        public List<SubCategory> SubCategoriesList { get; set; }

        // Constructor
        public Category()
        {
            SubCategoriesList = new List<SubCategory>();
        }
    }

    public class SubCategory
    {
        // Primary Key - SubCategory Id
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }       

        // Foriegn Key - Category Id
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserWithTopScore { get; set; } // user with top score for this sub-category
        public int TopScore { get; set; }            // the top score for this sub-category

        // Navigation property - Questions List for this Category/SubCategory (One To Many relationship with Questions Table)
        public List<Question> QuestionsList { get; set; }

        // Constructor
        public SubCategory()
        {
            QuestionsList = new List<Question>();
        }
    }

    public class Question
    {
        // Primary Key - Question Id
        public int QuestionId { get; set; }

        public int QuestionNumber { get; set; }           // Question number, the order of the question      
        public int QuestionLevel { get; set; }            // Question level, 1=novice / 2=intermediate / 3=expert
        public string QuestionTitle { get; set; }         // Question description, A short heading..
        public string QuestionText { get; set; }          // The question istelf..

        // Foriegn Key - SubCategory Id
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
                
        public int TimeToAnswer { get; set; }     // Time in milliseconds, 0 for no time limit..
        public int CreditPoints { get; set; }     

        // Navigation property - Answers List for this Question (One To Many relationship with Answers Table)
        public List<Answer> AnswersList { get; set; }

        public string RelatedImage { get; set; } // This field is here, but I am not using it for now..
        public string Explanation { get; set; }        

        public bool IsSingleAnswer { get; set; } // radio buttons (only one correct -vs- checkboxes (multiple correct)

        // Constructor
        public Question()
        {
            AnswersList = new List<Answer>();
        }

        // The following 2 methods are used by the 'Distinct' method in the search operation...!!
        public override int GetHashCode()
        {
            return QuestionId == 0 ? 0 : QuestionId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType().Name != "Question")
                return false;

            Question q = (Question)obj;
            return q.QuestionId == this.QuestionId;
        }
    }

    public class Answer
    {
        // Primary Key - Answer Id
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }

        // Foriegn Key - the Question Id
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public bool IsCorrect { get; set; }              
    }

    // Used by the AJAX referenced action (GetNextQuestion)
    // Was not mandatory to use this, but does save a trip to DB in the repository when getting the next question
    public class CurrentQuestion
    {
        public int currentQuizSessionId { get; set; }
        public int subCategoryId { get; set; } // fix to capital letters !!! TBD..
        public int questionNumber { get; set; }
        public int QuestionLevel { get; set; }
    }

    // Used by the AJAX referenced action (ProcessResults) 
    // When client sends the user quiz data back to the server for processing and saving
    public class UserQuizData
    {       
        public int currentQuizSessionId { get; set; } // fix to capital letters !!! TBD..
        public int SubCategoryId { get; set; }
        public List<UserQuestionData> QuestionsData { get; set; } // The questions list with the answers that user selected
    }
    public class UserQuestionData
    {        
        public int QuestionId { get; set; }
        public List<UserAnswerData>  AnswersSelected { get; set; } // List of answers Ids for this question that user has answered
    }
    public class UserAnswerData
    {      
        public int AnswerId { get; set; }
    }
}