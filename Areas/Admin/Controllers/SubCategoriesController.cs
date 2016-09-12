using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DevQuiz.Models;
using DevQuiz.Areas.Admin.ViewModels;
using DevQuiz.Repository;

namespace DevQuiz.Areas.Admin.Controllers
{
    [Authorize(Roles = "AdminRole")]
    public class SubCategoriesController : Controller
    {       
        private ApplicationRepository repository = new ApplicationRepository();

        // GET: Admin/SubCategories for the specific category !       
        public async Task<ActionResult> Index([Bind(Prefix="id")] int? categoryId)
        {
            Category category;
            if ((categoryId == null) || 
                ((category = await repository.GetCategory(categoryId)) == null))
            {
                // Go back to Categories list page if:
                // id is NOT supplied in the url OR if category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }            

            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = (await repository.GetCategory(categoryId)).CategoryName;
            
            return View(await repository.GetSubCategoriesList(categoryId));
        }

        // GET: Admin/SubCategories/Create
        public async Task<ActionResult> Create([Bind(Prefix = "id")] int? categoryId)              
        {
            Category category;
            if ((categoryId == null) ||
                ((category = await repository.GetCategory(categoryId)) == null))
            {
                // Go back to Categories list if:
                // id is NOT supplied in the url OR if category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }

            SubCategoryViewModel subCategoryViewModel = new SubCategoryViewModel()
                    {
                        CategoryId = category.CategoryId,                
                        CategoryName = category.CategoryName
                    };  

            return View(subCategoryViewModel);
        }

        // POST: Admin/SubCategories/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SubCategoryViewModel subCategoryInfoFromUser) 
        {
            if (ModelState.IsValid)
            {
                // 1. Check if this sub-category already exists in DB for the specified category               
                if (await repository.SubCategoryExists(subCategoryInfoFromUser.SubCategoryName, subCategoryInfoFromUser.CategoryId ))                    
                {
                    // report error
                    ModelState.AddModelError("", "This Sub-Category already exists in this category !");
                    return View(subCategoryInfoFromUser);
                }

                // 2. Create the DB/EF model and take info from the ViewModel 
                SubCategory subCategory = new SubCategory();
                subCategory.SubCategoryName = subCategoryInfoFromUser.SubCategoryName;
                subCategory.CategoryId = subCategoryInfoFromUser.CategoryId;

                // 3. Save the new sub-category 
                await repository.SaveNewSubCategory(subCategory);              

                return RedirectToAction("Index", "SubCategories", new { id = subCategory.CategoryId });                
            }           

            return View(subCategoryInfoFromUser);
        }

        // GET: Admin/SubCategories/Edit/5
        public async Task<ActionResult> Edit([Bind(Prefix = "id")]int? subCategoryId)
        {
            SubCategory subCategory;
            if ((subCategoryId == null) ||
                ((subCategory = await repository.GetSubCategoryWithCategory(subCategoryId)) == null))
            {
                // Go back to categories list page if: 
                // id is NOT supplied in the url OR if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }           
            
            return View(new SubCategoryViewModel() { SubCategoryId = subCategory.SubCategoryId,
                                                     SubCategoryName = subCategory.SubCategoryName,
                                                     CategoryId = subCategory.CategoryId,
                                                     CategoryName = subCategory.Category.CategoryName } );
        }

        // POST: Admin/SubCategories/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SubCategoryViewModel subCategoryInfoFromUser)
        {
            if (ModelState.IsValid)
            {
                    // 1. Get Sub-Category from DB - by the ID
                    SubCategory subCategory = await repository.GetSubCategory(subCategoryInfoFromUser.SubCategoryId);

                    if (subCategory == null)
                    {
                        // report error
                        ModelState.AddModelError("", "This Sub-Category does not exists, Enter another name !");
                        return View(subCategoryInfoFromUser);
                    }

                    else if (subCategory != null)
                    {
                        // new name == old name ==> do nothing, just return to index
                        // new name != old name ==> update only if no such name already exists !!!, and return to index

                        if (subCategory.SubCategoryName != subCategoryInfoFromUser.SubCategoryName)
                        {
                            // if new name does not already exist...
                            if (!await repository.SubCategoryExists(subCategoryInfoFromUser.SubCategoryName, subCategoryInfoFromUser.CategoryId))
                            {
                                // 2. Edit the category !!!  
                                subCategory.SubCategoryName = subCategoryInfoFromUser.SubCategoryName; 

                                // 3. Save the edited category 
                                await repository.SaveEditedSubCategory(subCategory);
                            }
                            else
                            {
                                // report error
                                ModelState.AddModelError("", "This Sub-Category name already exists, choose another name !");
                                return View(subCategoryInfoFromUser);
                            }
                        }
                    }

                // 4. Go back to sub-categories list view
                return RedirectToAction("Index", new { id = subCategoryInfoFromUser.CategoryId });
            }           

            return View(subCategoryInfoFromUser);
        }

        // GET: Admin/SubCategories/Delete/5
        public async Task<ActionResult> Delete([Bind(Prefix = "id")]int? subCategoryId)
        {
            SubCategory subCategory;
            if ((subCategoryId == null) ||
                ((subCategory = await repository.GetSubCategoryWithCategoryAndQuestions(subCategoryId)) == null))
            {
                // Go back to categories list page if: 
                // id is NOT supplied in the url OR if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }
                        
            return View(new SubCategoryViewModel() { SubCategoryName = subCategory.SubCategoryName,
                                                     SubCategoryId = subCategory.SubCategoryId,
                                                     CategoryName = subCategory.Category.CategoryName,
                                                     CategoryId = subCategory.CategoryId,
                                                     QuestionsCount = subCategory.QuestionsList.Count() });
        }

        // POST: Admin/SubCategories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(SubCategoryViewModel subCategoryInfoFromUser)
        {
            // 1. Get Sub-Category from DB - by the ID
            SubCategory subCategory = await repository.GetSubCategory(subCategoryInfoFromUser.SubCategoryId);

            if (subCategory == null)
            {
                // Go back to categories list page if sub-category does not exist in DB..
                return RedirectToAction("Index", "Categories");
            }

            if (subCategory != null)
            {
                // 2. Delete the sub-category and all its related questions data !!!                       
                await repository.DeleteSubCategory(subCategory); 
            }

            return RedirectToAction("Index", new { id = subCategory.CategoryId }); 
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ??                
            }
            base.Dispose(disposing);
        }

        // This is for MVC AJAX call from the client -
        // Used together with the '[Remote]' attribute declared on the SubCategory view model - 
        // Check if SubCategory is available in the specified category

        public async Task<JsonResult> IsSubCategoryAvailable(string SubCategoryName, int CategoryId)
        {
            return Json(!await repository.SubCategoryExists(SubCategoryName, CategoryId), JsonRequestBehavior.AllowGet);
        }
    }
}
