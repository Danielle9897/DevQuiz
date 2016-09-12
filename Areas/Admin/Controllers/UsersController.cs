using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.Models;
using DevQuiz.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.Controllers
{
    [Authorize(Roles = "AdminRole")]
    public class UsersController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        // GET: Admin/Users
        public async Task<ActionResult> Index()
        {
            // 1. Get List of users from repository           
            List<ApplicationUser> usersListFromRepository = await repository.GetUsersList();

            // 2. Create users details list for view
            List<UserPartialDetailsViewModel> usersListForView = new List<UserPartialDetailsViewModel>();

            foreach (ApplicationUser user in usersListFromRepository)
            {
                UserPartialDetailsViewModel userPartialDetails = new UserPartialDetailsViewModel
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                usersListForView.Add(userPartialDetails);
            }            

            return View(usersListForView);
        }

        // GET: Admin/Users/Id
        // The email is passed as parameter, used as the id to the tables 
        public async Task<ActionResult> UserInfo([Bind(Prefix = "data")]string userEmail)
        {
            // 0. Check param
            if (userEmail == null) 
            {
                // Go back to index page 
                return RedirectToAction("Index", "Users");
            }

            // 1. Get user details from repository
            ApplicationUser user = await repository.GetUserInfo(userEmail);

            // 2. Get list of quizes taken by this user from repository
            List<QuizSummaryForUser> ListOfQuizesTaken = await repository.GetListOfQuizesTaken(userEmail);

            // 3. Create View info
            UserFullDetailsViewModel userFullDetails = new UserFullDetailsViewModel();
            userFullDetails.Email = user.Email;
            userFullDetails.FirstName = user.FirstName;
            userFullDetails.LastName = user.LastName;
            userFullDetails.Address = user.Address;
            userFullDetails.Phone = user.PhoneNumber;

            foreach (QuizSummaryForUser quiz in ListOfQuizesTaken)
            {
                QuizSummaryViewModel quizInfo = new QuizSummaryViewModel();              
                
                SubCategory subCat = await repository.GetSubCategoryWithCategory(quiz.SubCateogory);
                quizInfo.SubCategory = subCat.SubCategoryName;
                quizInfo.Category = subCat.Category.CategoryName;
                quizInfo.TimeQuizTaken = quiz.TimeQuizTaken;
                quizInfo.UserScore = quiz.UserScore;               

                userFullDetails.ListOfQuizesTaken.Add(quizInfo);
            }          

            return View(userFullDetails);
        }
    }
}