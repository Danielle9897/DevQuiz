using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.Models;
using DevQuiz.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DevQuiz.Areas.Admin.Controllers
{
    [Authorize(Roles = "AdminRole")]
    public class HomeController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Home/SearchTerm=xxx
        public async Task<ActionResult> Search(string SearchTerm)
        {            
            if (String.IsNullOrWhiteSpace(SearchTerm))
            {
                // 1. If no search term then go back to page we came from...
                return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.ToString());
            }
            else
            {
                // 2. Get search results from DB and pass to View
                List<Question> results;
                List<SearchResultViewModel> viewModelResults;

                results = await repository.GetSearchResults(SearchTerm);
                viewModelResults = new List<SearchResultViewModel>();

                foreach (Question question in results)
                {
                    viewModelResults.Add(new SearchResultViewModel() { QuestionId = question.QuestionId,
                                                                       QuestionTitle = question.QuestionTitle,
                                                                       QuestionText = question.QuestionText,
                                                                       QuestionNumber = question.QuestionNumber,
                                                                       SubCategoryId = question.SubCategoryId,
                                                                       SubCategoryName = question.SubCategory.SubCategoryName,
                                                                       CategoryId = question.SubCategory.CategoryId,
                                                                       CategoryName = question.SubCategory.Category.CategoryName
                                                                     });
                }
                ViewBag.SearchTerm = SearchTerm;
                return View(viewModelResults);
            }            
        }

        // POST:  *** Called by an AJAX call from the client !       
        //        *** Return Stats information
        [HttpPost]       
        public async Task<ActionResult> GetStats()
        {            
            Stats statusInfo = await repository.GetStatsInfo();

            int visitorsCount = statusInfo.VisitorsCount;

            int day = statusInfo.LastAccessTime.Day;
            int month = statusInfo.LastAccessTime.Month;
            int year = statusInfo.LastAccessTime.Year;

            string hours = (statusInfo.LastAccessTime.Hour < 10) ? ("0" + statusInfo.LastAccessTime.Hour.ToString()) : 
                                                                    statusInfo.LastAccessTime.Hour.ToString();

            string minutes = (statusInfo.LastAccessTime.Minute < 10) ? ("0" + statusInfo.LastAccessTime.Minute.ToString()) :
                                                                        statusInfo.LastAccessTime.Minute.ToString();

            return Json( new { visitorsCount, day, month, year, hours, minutes });
        }
    }
}