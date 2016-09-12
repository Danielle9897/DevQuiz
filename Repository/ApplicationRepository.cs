using DevQuiz.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System;

namespace DevQuiz.Repository
{
    public class ApplicationRepository
    {
        //****************************//
        //  Categories Table Queries  //
        //****************************//

        #region #region Categories Table Queries

        // Find if this name already exists in DB
        public async Task<bool> CategoryExists(string categoryName)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Categories.AnyAsync(c => c.CategoryName == categoryName);
            }
        }

        // Return a list of all Categories (with sub-categories data)
        public async Task<List<Category>> GetCategoriesListWithSubCategories()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                // Use AsNoTracking so that state of entity is NOT maintained by EF - for perfomance...
                return await db.Categories.AsNoTracking()
                                          .Include(s => s.SubCategoriesList)
                                          .OrderBy(c => c.CategoryName)
                                          .ToListAsync();
            }
        }

        // Return a list of all Categories (w/o data)
        public async Task<List<Category>> GetCategoriesList()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Categories.AsNoTracking()
                                          .OrderBy(c => c.CategoryName)
                                          .ToListAsync();
            }
        }

        // Save a new Category
        public async Task SaveNewCategory(Category newCategory)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Categories.Add(newCategory);
                await db.SaveChangesAsync();
            }
        }

        // Save edited Category
        public async Task SaveEditedCategory(Category category)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(category).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        // Get a category (from its name, w/o data)
        public async Task<Category> GetCategory(string categoryName)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Categories.AsNoTracking()
                                          .Where(c => c.CategoryName == categoryName)
                                          .FirstOrDefaultAsync<Category>();
            }
        }

        // Get a category (from the category id, w/o data)
        public async Task<Category> GetCategory(int? categoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Categories.AsNoTracking()
                                          .Where(c => c.CategoryId == categoryId)
                                          .FirstOrDefaultAsync<Category>();
            }
        }

        // Get a category (from the category id, with related data)
        public async Task<Category> GetCategoryWithSubCategories(int? categoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Categories.AsNoTracking()
                                          .Include(s => s.SubCategoriesList)
                                          .Where(c => c.CategoryId == categoryId)
                                          .FirstOrDefaultAsync<Category>();
            }
        }

        // Delete a category (by its id, including ALL related data !) 
        public async Task DeleteCategory(Category category)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(category).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
        }

        #endregion

        //*******************************//
        // Sub-Categories Table Queries  //
        //*******************************//

        #region #region Sub-Categories Table Queries

        // Find if this sub-category name already exists in DB for the specified category
        public async Task<bool> SubCategoryExists(string subCategoryName, int categoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                List<SubCategory> subCatList = await db.SubCategories.AsNoTracking()
                                                                     .Where(c => c.CategoryId == categoryId)
                                                                     .ToListAsync();

                return subCatList.Any(s => s.SubCategoryName == subCategoryName);
            }
        }

        // Return a list of all Sub-Categories (w/o data for a specific category !)
        public async Task<List<SubCategory>> GetSubCategoriesList(int? categoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.SubCategories.AsNoTracking()
                                             .Where(s => s.CategoryId == categoryId)
                                             .OrderBy(s => s.SubCategoryName)
                                             .ToListAsync();
            }
        }

        // Retrun Sub-Category (w/o Category)
        public async Task<SubCategory> GetSubCategory(int? subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.SubCategories.AsNoTracking()
                                             .SingleOrDefaultAsync(s => s.SubCategoryId == subCategoryId);
            }
        }

        // Retrun Sub-Category (with Category)
        public async Task<SubCategory> GetSubCategoryWithCategory(int? subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.SubCategories.AsNoTracking()
                                             .Include(c => c.Category)
                                             .SingleOrDefaultAsync(s => s.SubCategoryId == subCategoryId);
            }
        }

        // Retrun Sub-Category (with Category and with Questions)
        public async Task<SubCategory> GetSubCategoryWithCategoryAndQuestions(int? subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.SubCategories.AsNoTracking()
                                             .Include(c => c.Category)
                                             .Include(q => q.QuestionsList)
                                             .SingleOrDefaultAsync(s => s.SubCategoryId == subCategoryId);
            }
        }

        // Save a new Sub-Category
        public async Task SaveNewSubCategory(SubCategory subCategory)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.SubCategories.Add(subCategory);
                await db.SaveChangesAsync();
            }
        }

        // Save edited Sub-Category
        public async Task SaveEditedSubCategory(SubCategory subCategory)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(subCategory).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        // Delete a Sub-Category (by its id, including ALL related data !) 
        public async Task DeleteSubCategory(SubCategory subCategory)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(subCategory).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
        }

        // Update the sub-category table with top score info - if needed      
        public async Task UpdateTopScoreInfo(QuizSummaryForUser quizSummary)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                SubCategory s = await GetSubCategory(quizSummary.SubCateogory);
                if (s.TopScore < quizSummary.UserScore)
                {
                    s.TopScore = quizSummary.UserScore;
                    s.UserWithTopScore = quizSummary.UserName;
                    db.Entry(s).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
        }

        #endregion

        //*****************************//
        //    Questions Table Queries  //
        //*****************************//               

        #region #region Questions Table Queries

        // Return number of questions in the specified sub-category - total number
        public async Task<int> getNumberOfQuestionsTotal(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .CountAsync(s => (s.SubCategoryId == subCategoryId));
            }
        }

        // Return number of questions in the specified sub-category - level novice
        public async Task<int> getNumberOfQuestionsLevelNovice(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Where(q => q.QuestionLevel == 1)
                                         .CountAsync(s => (s.SubCategoryId == subCategoryId));
            }
        }

        // Return number of questions in the specified sub-category  - level intermediate
        public async Task<int> getNumberOfQuestionsLevelIntermediate(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Where(q => q.QuestionLevel == 2)
                                         .CountAsync(s => (s.SubCategoryId == subCategoryId));
            }
        }

        // Return number of questions in the specified sub-category  - level expert
        public async Task<int> getNumberOfQuestionsLevelExpert(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Where(q => q.QuestionLevel == 3)
                                         .CountAsync(s => (s.SubCategoryId == subCategoryId));
            }
        }

        // Return the next question in the relevant Sub-Category (with answers data)       
        public async Task<Question> getNextQuestion(CurrentQuestion currentQuestionInfo)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                Question nextQuestion = await db.Questions.AsNoTracking()
                                                          .Where(q => (q.SubCategoryId == currentQuestionInfo.subCategoryId) &&
                                                                (q.QuestionNumber > currentQuestionInfo.questionNumber)      && 
                                                                (q.QuestionLevel == currentQuestionInfo.QuestionLevel))
                                                          .Include(q => q.AnswersList)
                                                          .OrderBy(q => q.QuestionNumber)
                                                          .FirstOrDefaultAsync();
                return nextQuestion;
            }
        }

        // Retrun Question (w/o Sub-Category)
        public async Task<Question> GetQuestion(int? questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .SingleOrDefaultAsync(q => q.QuestionId == questionId);
            }
        }

        // Retrun Question (with Sub-Category and with Category)
        public async Task<Question> GetQuestionWithSubCategoryAndCategory(int? questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Include(s => s.SubCategory)
                                         .Include(c => c.SubCategory.Category)
                                         .SingleOrDefaultAsync(q => q.QuestionId == questionId);
            }
        }

        // Return Question (with Answers)
        public async Task<Question> GetQuestionWithAnswers(int questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Include(a => a.AnswersList)
                                         .SingleOrDefaultAsync(q => q.QuestionId == questionId);
            }
        }

        // Retrun the Question Score
        public int GetQuestionScore(int questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return db.Questions.AsNoTracking()
                                         .Where(q => q.QuestionId == questionId)
                                         .Select(q => q.CreditPoints)
                                         .SingleOrDefault();
            }
        }

        // Return a list of all Questions (w/o answers data !)
        public async Task<List<Question>> GetQuestionsList(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Questions.AsNoTracking()
                                         .Where(q => q.SubCategoryId == subCategoryId)
                                         .OrderBy(n => n.QuestionNumber)
                                         .ToListAsync();
            }
        }

        // Find if this question number already exists in DB for the specified sub-category
        public async Task<bool> NumberExists(int questionNumber, int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                List<Question> questionList = await db.Questions.AsNoTracking()
                                                                .Where(s => s.SubCategoryId == subCategoryId)
                                                                .ToListAsync();

                return questionList.Any(q => q.QuestionNumber == questionNumber);
            }
        }

        // Generate the next question number up for a new question that is created
        public async Task<int> GenerateNextQuestionNumber(int subCategoryId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                int numberOfQuestions = await db.Questions.AsNoTracking()
                                                          .Where(s => s.SubCategoryId == subCategoryId).CountAsync();
                if (numberOfQuestions == 0)
                {
                    return 1;
                }

                return (await db.Questions.AsNoTracking()
                                          .Where(s => s.SubCategoryId == subCategoryId)
                                          .MaxAsync(i => i.QuestionNumber)) + 1;

                // What if larger than 1000 ??? TBD....
            }
        }

        // Save a new Question and return its id
        public async Task<int> SaveNewQuestion(Question question)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Questions.Add(question);
                await db.SaveChangesAsync();
                return question.QuestionId;
            }
        }

        // Save edited Question
        public async Task SaveEditedQuestion(Question question)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(question).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        // Delete a Question (by its id, including ALL related data !) 
        public async Task DeleteQuestion(Question question)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Entry(question).State = EntityState.Deleted;
                await db.SaveChangesAsync();
            }
        }

        // Return list of questions that have the search term in their: title | text | answers
        public async Task<List<Question>> GetSearchResults(string searchTerm)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                // 1. Search questions table
                List<Question> questionsList = new List<Question>();
                questionsList.AddRange(await db.Questions.AsNoTracking()
                                                         .Where(q => q.QuestionText.Contains(searchTerm) ||
                                                                     q.QuestionTitle.Contains(searchTerm))
                                                         .Include(s => s.SubCategory)
                                                         .Include(c => c.SubCategory.Category)
                                                         .Include(a => a.AnswersList)
                                                         .ToListAsync());
                // 2. Search answers table
                List<Answer> answerList = new List<Answer>();
                answerList.AddRange(await db.Answers.AsNoTracking()
                                                    .Where(a => a.AnswerText.Contains(searchTerm))
                                                    .ToListAsync());

                // 3. Retrieve distinct questions data from answers list
                foreach (int questionId in (answerList.Select(x => x.QuestionId).Distinct()))
                {
                    questionsList.Add(await db.Questions.AsNoTracking().Where(q => q.QuestionId == questionId)
                                                                       .Include(s => s.SubCategory)
                                                                       .Include(c => c.SubCategory.Category)
                                                                       .Include(a => a.AnswersList)
                                                                       .SingleOrDefaultAsync());
                }
                return questionsList.Distinct().ToList();
            }
        }

        #endregion

        //***********************//
        //    Answers queries    //
        //***********************//

        #region #region Answers Table Queries

        // Return a list of all Answers for the specified question id
        public async Task<List<Answer>> getAnswersList(int questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Answers.AsNoTracking()
                                       .Where(a => a.QuestionId == questionId)
                                       .ToListAsync();
            }
        }

        // Return a list of only the Correct Answers Ids for the specified question id
        public async Task<List<string>> GetCorrectAnswers(int questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return await db.Answers.AsNoTracking()
                                       .Where(a => (a.QuestionId == questionId) &&
                                                   (a.IsCorrect == true))
                                       .Select(a => a.AnswerId.ToString())
                                       .ToListAsync();
            }
        }

        // Delete all answers for the specified question 
        public async Task DeleteAllAnswers(int questionId)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                List<Answer> answersList = await getAnswersList(questionId); // try....with question object....

                foreach (Answer answer in answersList)
                {
                    db.Entry(answer).State = EntityState.Deleted;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task saveAnswers(List<Answer> answersList)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                foreach (Answer answer in answersList)
                {
                    db.Answers.Add(answer);
                    await db.SaveChangesAsync();
                }
            }
        }

        #endregion

        //****************************//
        //  Categories Table Queries  //
        //****************************//

        #region #region Quiz Summary Table Queries

        // Save the quiz summary
        public async Task SaveSummary(QuizSummaryForUser quizSummary)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                // Note:           
                // Only the latest (last) enry for a user in a sub-category is saved
                // So, if exists already an entry for this user for the same sub-category delete the the previous one before saving

                QuizSummaryForUser summary = await db.QuizSummaryForUsers.AsNoTracking()
                                                                   .Where(u => (u.UserName == quizSummary.UserName) &&
                                                                               (u.SubCateogory == quizSummary.SubCateogory))
                                                                   .FirstOrDefaultAsync();
                if (summary != null)
                {
                    db.Entry(summary).State = EntityState.Deleted;
                    await db.SaveChangesAsync();
                }

                db.QuizSummaryForUsers.Add(quizSummary);
                await db.SaveChangesAsync();
            }
        }

        // Retrun all the quizes taken by the specified user
        public async Task<List<QuizSummaryForUser>> GetListOfQuizesTaken(string userEmail)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return (await db.QuizSummaryForUsers.AsNoTracking()
                                                    .Where(u => u.UserName == userEmail)
                                                    .OrderByDescending(u => u.TimeQuizTaken)
                                                    .ToListAsync());
            }
        }

        #endregion

        //*****************************//
        //  AspNetUsers Table Queries  //
        //*****************************//

        #region #region AspNetUsers Table Queries

        // Return list of all users registered
        public async Task<List<ApplicationUser>> GetUsersList()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {               
                 return (await db.Users.AsNoTracking().ToListAsync());
            }
        }

        // Return user details
        public async Task<ApplicationUser> GetUserInfo(string userEmail)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return (await db.Users.AsNoTracking()
                                      .Where(u => u.Email == userEmail)
                                      .SingleOrDefaultAsync());
            }
        }

        #endregion

        //************************//
        //  Stats Table Queries   //
        //************************//

        #region #region Stats Table Queries

        // Return stats info
        public async Task<Stats> GetStatsInfo()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                return (await db.Statistics.AsNoTracking()
                                           .Where(s => s.StatsId == 1)
                                           .SingleOrDefaultAsync());
            }
        }

        // Set stats info
        // This method is NOT async since I call it form session_start in global.asax which CANNOT be async
        public void SetStatsInfo(DateTime time)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                Stats currentStats = db.Statistics.AsNoTracking()
                                                  .Where(s => s.StatsId == 1)
                                                  .SingleOrDefault();

                currentStats.VisitorsCount++;
                currentStats.LastAccessTime = time;

                db.Entry(currentStats).State = EntityState.Modified;
                db.SaveChanges();
            }
        }       

        #endregion

    }
}
