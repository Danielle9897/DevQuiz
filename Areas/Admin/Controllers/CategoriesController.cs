using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DevQuiz.Models;
using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.Repository;

namespace DevQuiz.Areas.Admin.Controllers
{
    [Authorize(Roles = "AdminRole")]
    public class CategoriesController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        // GET: Admin/Categories 
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await repository.GetCategoriesList());
        }

        // GET: Admin/Categories/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryViewModel categoryInfoFromUser)
        {
            if (ModelState.IsValid)
            {
                // 1. Check if this category already exists in DB               
                if (await repository.CategoryExists(categoryInfoFromUser.CategoryName))
                {
                    // report error
                    ModelState.AddModelError("", "This Category already exists !");
                    return View(categoryInfoFromUser);
                }

                // 2. Create the DB/EF model and take info from the ViewModel 
                Category category = new Category();
                category.CategoryName = categoryInfoFromUser.CategoryName;

                // 3. Save the new category 
                await repository.SaveNewCategory(category);                
                return RedirectToAction("Index");
            }

            return View(categoryInfoFromUser);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<ActionResult> Edit([Bind(Prefix = "id")]int? categoryId)
        {
            Category category;
            if ((categoryId == null) ||
                ((category = await repository.GetCategory(categoryId)) == null))
            {
                // Go back to categories list page if: 
                // id is NOT supplied in the url OR if category does not exist in DB..
                return RedirectToAction("Index");                
            }           

            return View(new CategoryViewModel() { CategoryName = category.CategoryName, CategoryId = category.CategoryId } );
        }

        // POST: Admin/Categories/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CategoryViewModel categoryInfoFromUser)
        {
            if (ModelState.IsValid)
            {
                // 1. Get Category from DB - by the ID
                Category category = await repository.GetCategory(categoryInfoFromUser.CategoryId);

                if (category == null) 
                {
                        // report error
                        ModelState.AddModelError("", "This Category does not exists, Enter another name !");
                        return View(categoryInfoFromUser);
                }

                else if (category != null)
                {
                        // new name == old name ==> do nothing, just return to index
                        // new name != old name ==> update only if no such name already exists !!!, and return to index

                        if (category.CategoryName != categoryInfoFromUser.CategoryName)
                        {
                            // if new name does not already exist...
                            if (! await repository.CategoryExists(categoryInfoFromUser.CategoryName)) 
                            {
                                // 2. Edit the category !!!  
                                category.CategoryName = categoryInfoFromUser.CategoryName; 

                                // 3. Save the edited category 
                                await repository.SaveEditedCategory(category);
                            }
                            else
                            {
                                // report error
                                ModelState.AddModelError("", "This Category name already exists, choose another name !");
                                return View(categoryInfoFromUser);
                            }
                        }

                    // 4. Go back to categories list view
                    return RedirectToAction("Index");
                }                
            }
            return View(categoryInfoFromUser);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<ActionResult> Delete([Bind(Prefix = "id")]int? categoryId)
        {
            Category category;
            if ((categoryId == null) ||
                ((category = await repository.GetCategoryWithSubCategories(categoryId)) == null))
            {
                // Go back to categories list page if: 
                //    id is NOT supplied in the url -OR- if category does not exist in DB..
                return RedirectToAction("Index");
            }
            
            return View(new CategoryViewModel() { CategoryName = category.CategoryName,
                                                  CategoryId = category.CategoryId,
                                                  SubCategoriesCount = category.SubCategoriesList.Count() });                     
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(CategoryViewModel CategoryInfoFromUser)
        {
           // 1. Get Category from DB - by the ID
           Category category = await repository.GetCategory(CategoryInfoFromUser.CategoryId);

           if (category != null)
           {                   
               // 2. Delete the category and all its related data !!!                       
               await repository.DeleteCategory(category);                             
           }

            return RedirectToAction("Index");
        }                      

        protected override void Dispose(bool disposing)
        {           
            base.Dispose(disposing);  
        }

        // This is for MVC AJAX call from the client -
        // Used together with the '[Remote]' attribute declared on the category view model - 
        // Check if category is available
        public async Task<JsonResult> IsCategoryAvailable(string CategoryName)
        {
            return Json(! await repository.CategoryExists(CategoryName), JsonRequestBehavior.AllowGet);
        }

    }
}
