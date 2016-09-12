using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.Models;
using DevQuiz.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.Controllers
{
    [Authorize(Roles = "AdminRole")]
    public class QuestionsController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        // INDEX GET: Admin/Questions for the specific Sub-Category
        public async Task<ActionResult> Index([Bind(Prefix = "id")] int? subCategoryId)
        {
            SubCategory subCategory;
            if ((subCategoryId == null) ||
                ((subCategory = await repository.GetSubCategoryWithCategory(subCategoryId)) == null))
            {
                // Go back to Categories list page if:
                // id is NOT supplied in the url OR if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }           

            ViewBag.SubCategoryId   = subCategoryId;
            ViewBag.SubCategoryName = subCategory.SubCategoryName;
            ViewBag.CategoryId      = subCategory.CategoryId;
            ViewBag.CategoryName    = subCategory.Category.CategoryName;

            return View(await repository.GetQuestionsList(subCategory.SubCategoryId));
        }

        // CREATE GET: Admin/Questions/Create
        public async Task<ActionResult> Create([Bind(Prefix = "id")] int? subCategoryId)
        {
            SubCategory subCategory;
            if ((subCategoryId == null) ||
                ((subCategory = await repository.GetSubCategoryWithCategory(subCategoryId)) == null))
            {
                // Go back to Categories list if:
                // id is NOT supplied in the url OR if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }           

            ViewBag.CategoryId = subCategory.CategoryId;
            ViewBag.CategoryName = subCategory.Category.CategoryName;

            QuestionViewModel questionViewModel = new QuestionViewModel()
            {
                SubCategoryId = subCategory.SubCategoryId,
                SubCategoryName = (await repository.GetSubCategory(subCategoryId)).SubCategoryName,
                QuestionNumber = await repository.GenerateNextQuestionNumber(subCategory.SubCategoryId),
                QuestionLevel = 1,
                TimeToAnswer = 0,
                CreditPoints = 1,
            };

            for (int i = 1; i <= 5; i++)
                 questionViewModel.AnswersList.Add(new AnswerViewModel());

            return View(questionViewModel);
        }

        // CTEATE POST: Admin/Questions/Create  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(QuestionViewModel questionInfoFromUser)
        {
            // The following is needed for when returning back to the 'CREATE' view upon Errors
            SubCategory sc = await repository.GetSubCategoryWithCategory(questionInfoFromUser.SubCategoryId);

            if (sc == null)
            {
                // Go back to Categories list if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }

            ViewBag.CategoryId   = sc.CategoryId;
            ViewBag.CategoryName = sc.Category.CategoryName;

            if (ModelState.IsValid)
            {              
                // 1. Create new question
                Question question = new Question();

                // 2. Validate user input, fill the quesion object, save question in DB
                string status = await UpdateQuestion(question, questionInfoFromUser, true);
                if (status != "OK")
                {
                    ModelState.AddModelError("", status);
                    return View(questionInfoFromUser);
                }

                return RedirectToAction("Index", new { id = question.SubCategoryId });
            }

            return View(questionInfoFromUser);
        }

        // EDIT GET: Admin/Questions/Edit/5
        public async Task<ActionResult> Edit([Bind(Prefix = "id")]int? questionId)
        {
            // 1. Get question info from DB and create a view model for the view    
            Question question;
            if ((questionId == null) ||
                ((question = await repository.GetQuestionWithSubCategoryAndCategory(questionId)) == null))
            {
                // Go back to Categories list if:
                // id is NOT supplied in the url OR if question does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }
            
            ViewBag.CategoryId = question.SubCategory.CategoryId;
            ViewBag.CategoryName = question.SubCategory.Category.CategoryName;

            QuestionViewModel questionViewModel = new QuestionViewModel();

            questionViewModel.QuestionId        = question.QuestionId;
            questionViewModel.SubCategoryId     = question.SubCategoryId;
            questionViewModel.SubCategoryName   = question.SubCategory.SubCategoryName;           

            questionViewModel.QuestionTitle     = question.QuestionTitle;
            questionViewModel.QuestionText      = question.QuestionText;
            questionViewModel.QuestionLevel     = question.QuestionLevel;
            questionViewModel.QuestionNumber    = question.QuestionNumber;
            questionViewModel.CreditPoints      = question.CreditPoints;
            questionViewModel.Explanation       = question.Explanation;            
            //questionViewModel.RelatedImage      = question.RelatedImage; // ???
            questionViewModel.TimeToAnswer      = question.TimeToAnswer;


            // 2. Get answers info from DB for the view model
            List<Answer> answersList = await repository.getAnswersList(question.QuestionId);

            questionViewModel.AnswersList = new List<AnswerViewModel>();          
            for (int i = 0; i < answersList.Count(); i++)
            {
                AnswerViewModel answer = new AnswerViewModel() { AnswerText = answersList[i].AnswerText,
                                                                 IsCorrect  = answersList[i].IsCorrect };
                questionViewModel.AnswersList.Add(answer);
            }

            // 2.1 Add up to 5 (so that user can add more options to the answers already existing 
            for (int i = 5; i > answersList.Count(); i--)
                 questionViewModel.AnswersList.Add(new AnswerViewModel());

            return View(questionViewModel);
        }

        // POST: Admin/Questions/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionViewModel questionInfoFromUser)
        {
            // The following is needed for when returning back to the 'EDIT' view upon Errors
            SubCategory sc = await repository.GetSubCategoryWithCategory(questionInfoFromUser.SubCategoryId);
            if (sc == null)
            {
                // Go back to Categories list if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }
            ViewBag.CategoryId = sc.CategoryId;
            ViewBag.CategoryName = sc.Category.CategoryName;

            if (ModelState.IsValid)
            {
                // 0. Get Question from DB 
                Question question = await repository.GetQuestion(questionInfoFromUser.QuestionId); 

                if (question == null)
                {
                    // report error
                    ModelState.AddModelError("", "This Question does not exists !");
                    return View(questionInfoFromUser);
                }

                else if (question != null)
                { 
                    //0. Optional - compare question from user with question from DB 
                    //              and continue only if different... TBD

                    // 1. Validate user input, fill the quesion object, save question in DB
                    string status = await UpdateQuestion(question, questionInfoFromUser, false);
                    if (status != "OK")
                    {
                        ModelState.AddModelError("", status);
                        return View(questionInfoFromUser);
                    }            

                    // 2. Go back to Questions list view
                    return RedirectToAction("Index", new { id = questionInfoFromUser.SubCategoryId });
                }
            }
            return View(questionInfoFromUser);
        }

        // DELETE GET: Admin/Questions/Delete/5
        public async Task<ActionResult> Delete([Bind(Prefix = "id")]int? questionId)
        {
            Question question;
            if ((questionId == null) ||
                ((question = await repository.GetQuestionWithSubCategoryAndCategory(questionId)) == null))
            {
                // Go back to categories list page if: 
                // id is NOT supplied in the url OR if question does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }           

            ViewBag.CategoryName = question.SubCategory.Category.CategoryName;
            ViewBag.CategoryId = question.SubCategory.Category.CategoryId;

            return View(new QuestionViewModel() { QuestionId = question.QuestionId,
                                                  QuestionTitle = question.QuestionTitle,
                                                  SubCategoryName = question.SubCategory.SubCategoryName,
                                                  SubCategoryId = question.SubCategoryId });
        }

        // DELETE POST: Admin/Questions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(QuestionViewModel questionInfoFromUser)
        {
            // 1. Get Question from DB - by the ID           
            Question question = await repository.GetQuestion(questionInfoFromUser.QuestionId);

            if (question == null)
            {
                // Go back to categories list page if question does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }

            if (question != null)
            {
                // 2. Delete the sub-category and all its related questions data !!!                       
                await repository.DeleteQuestion(question);
            }

            return RedirectToAction("Index", new { id = question.SubCategoryId });
        }

        // Fill in the question object with the question info from user, perform verifications,
        // This method is used by the Post Edit & Create Actions..
        // Returns 'OK' OR an err msg to be added to 'ModelState.AddModelError..'
        [NonAction]
        private async Task<string> UpdateQuestion(Question question, QuestionViewModel questionInfoFromUser, bool newQuestion)
        {

            // 0. Verify that the question number is not already in use
            //    Note: this was also verified on the client using the 'Remote' attribute on the view model 
            //          but should be verified here as well...

            if ( (newQuestion) ||
                 ( (!newQuestion) && (question.QuestionNumber != questionInfoFromUser.QuestionNumber) ) )

            {
                if (await repository.NumberExists(questionInfoFromUser.QuestionNumber, questionInfoFromUser.SubCategoryId))
                {
                    // Report error
                    return "Question number is already in use !";
                }
            }

            if (questionInfoFromUser.QuestionNumber > 2000)
            {
                // Report error
                return "Please choose a question number less than 2000";
            }

            // 1. Copy data
            question.QuestionId     = questionInfoFromUser.QuestionId;
            question.SubCategoryId  = questionInfoFromUser.SubCategoryId;
            question.QuestionNumber = questionInfoFromUser.QuestionNumber;
            question.QuestionLevel  = questionInfoFromUser.QuestionLevel;
            question.QuestionTitle  = questionInfoFromUser.QuestionTitle; 
            question.QuestionText   = questionInfoFromUser.QuestionText;                      
            question.TimeToAnswer   = questionInfoFromUser.TimeToAnswer;
            question.CreditPoints   = questionInfoFromUser.CreditPoints;
            question.Explanation    = questionInfoFromUser.Explanation;

            // 2. Get answers         
            int numberOfAnswers = 0;
            int numberOfCorrectAnswers = 0;
          
            List<Answer> tempAnswersList = new List<Answer>(); 

            for (int i = 0; i < questionInfoFromUser.AnswersList.Count; i++)
            {
                // Use answer only if text is not blank
                if (!string.IsNullOrWhiteSpace(questionInfoFromUser.AnswersList[i].AnswerText))
                {
                    Answer answer = new Answer()
                    {
                        AnswerText = questionInfoFromUser.AnswersList[i].AnswerText,     
                        IsCorrect  = questionInfoFromUser.AnswersList[i].IsCorrect,
                        // Updating the question id here is useful for 'Edit',
                        // Will be 0 for 'Create' but that gets updated later after we save the new question and get its id..
                        QuestionId = question.QuestionId 
                    };                                                       
                    
                    if (answer.IsCorrect)
                    {
                        numberOfCorrectAnswers++;
                    }

                    numberOfAnswers++;
                    tempAnswersList.Add(answer);
                }
            }

            // 3. Verify we have at least 2 answers supplied
            if (numberOfAnswers <= 1)
            {
                // Report error
                return "Minimum of 2 answers is required !";
            }

            // 4. Determine if single or multiple correct answers....            
            question.IsSingleAnswer = (numberOfCorrectAnswers == 1) ? true : false;

            if (numberOfCorrectAnswers == 0)
            {
                // Report error
                return "At least one answer must be marked as 'correct' !";
            }

            // 5. All validation passed => Save the created/updated question 

            if (newQuestion) // coming from 'CREATE'
            {
                // 5.1 Save the newly reated question
                int newQuestionId = await repository.SaveNewQuestion(question);

                // 5.2 Update the answers question id with the newly created question id                
                foreach (Answer answer in tempAnswersList)
                {
                    answer.QuestionId = newQuestionId;
                }
            }
            else // coming from 'EDIT'
            {
                // 5.3 Delete original answers list..
                await repository.DeleteAllAnswers(question.QuestionId);

                // 5.4 Save the edited question
                await repository.SaveEditedQuestion(question);

                // Here we don't need to update the answers question id..
            }

            // 6. Save the newly created answers (for 'create' and 'edit')
            await repository.saveAnswers(tempAnswersList);

            return "OK";
        }

        // This is for MVC AJAX call from the client -
        // Used together with the '[Remote]' attribute declared on the Question view model - 
        // Check if question number is available in the specified sub-category
        public async Task<JsonResult> IsNumberAvailable(int QuestionNumber, int SubCategoryId, int QuestionId)
        {
            // If we come from 'EDIT' view than its ok for the question number be the same as in the DB for this question
            // If we come from 'CREATE' view than question number must be a new one
                  
            Question questionInDB = await repository.GetQuestion(QuestionId);

            // 1. If question already exists in DB (i.e. Got here from 'EDIT' view)     
            if (questionInDB != null) 
            {
                // And if same number than ok...
                if (questionInDB.QuestionNumber == QuestionNumber)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            // 2. Question does NOT exist yet - Got come from 'CREATE' view            
            return Json(!await repository.NumberExists(QuestionNumber, SubCategoryId), JsonRequestBehavior.AllowGet);            
        }        

    }
}