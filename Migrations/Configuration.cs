namespace DevQuiz.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DevQuiz.Repository.ApplicationDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }       

        protected override void Seed(DevQuiz.Repository.ApplicationDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            // Step 1: Insert the Admin Role & User

            string adminRole = "AdminRole";
            string adminUser = "AdminUser@admin.com";
            string adminPswd = "AaBb1!";    // pswd will get hashed by the userManager....

            if (!context.Users.Any(u => u.UserName == adminUser))
            {
                // 1. Create Role Admin                
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                roleManager.Create(new IdentityRole(adminRole));

                // 2. Create User Admin               
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                ApplicationUser user = new ApplicationUser() { UserName = adminUser };
                var adminresult = userManager.Create(user, adminPswd);

                // 3. Add User Admin to Role Admin
                if (adminresult.Succeeded)
                {
                    var result = userManager.AddToRole(user.Id, adminRole);
                }
                else
                {
                    // ???
                }
            }

            // Step 2: Insert fist line for Stats table

            if (!context.Statistics.Any(u => u.StatsId == 1))
            {
                Stats stats = new Stats() { StatsId = 1, VisitorsCount = 1, LastAccessTime = DateTime.Now };
                context.Statistics.Add(stats);
            }

            // Step 3: Insert some Quiz data

            //Answer answer1 = new Answer() { AnswerText = "Answer Text 1", IsCorrect = false };
            //Answer answer2 = new Answer() { AnswerText = "Answer Text 2", IsCorrect = false };
            //Answer answer3 = new Answer() { AnswerText = "Answer Text 3", IsCorrect = false };
            //Answer answer4 = new Answer() { AnswerText = "Answer Text 4", IsCorrect = false };
            //Answer answer5 = new Answer() { AnswerText = "Answer Text 5", IsCorrect = false };
            //Answer answer6 = new Answer() { AnswerText = "Answer Text 6", IsCorrect = false };

            //List<Answer> answersList1 = new List<Answer>() { answer1, answer2 };
            //List<Answer> answersList2 = new List<Answer>() { answer3, answer4 };
            //List<Answer> answersList3 = new List<Answer>() { answer5, answer6 };

            //Question question1 = new Question() { QuestionText = "Question Text 1", QuestionLevel = 1,
            //    CreditPoints = 25, TimeToAnswer = 30, AnswersList = answersList1,
            //    Explanation = "Explenation 1", QuestionNumber = 1 };
            //Question question2 = new Question() { QuestionText = "Question Text 2", QuestionLevel = 2,
            //    CreditPoints = 25, TimeToAnswer = 30, AnswersList = answersList2,
            //    Explanation = "Explenation 2", QuestionNumber = 1 };
            //Question question3 = new Question() { QuestionText = "Question Text 3", QuestionLevel = 3,
            //    CreditPoints = 25, TimeToAnswer = 30, AnswersList = answersList3,
            //    Explanation = "Explenation 3", QuestionNumber = 1 };

            //List<Question> questionsList1 = new List<Question>() { question1 };
            //List<Question> questionsList2 = new List<Question>() { question2 };
            //List<Question> questionsList3 = new List<Question>() { question3 };

            //SubCategory subCategory1 = new SubCategory() { SubCategoryName = "Sub Category Name 1", QuestionsList = questionsList1 };
            //SubCategory subCategory2 = new SubCategory() { SubCategoryName = "Sub Category Name 2", QuestionsList = questionsList2 };
            //SubCategory subCategory3 = new SubCategory() { SubCategoryName = "Sub Category Name 3", QuestionsList = questionsList3 };

            //List<SubCategory> subCategoriesList1 = new List<SubCategory>() { subCategory1 };
            //List<SubCategory> subCategoriesList2 = new List<SubCategory>() { subCategory2 };
            //List<SubCategory> subCategoriesList3 = new List<SubCategory>() { subCategory3 };

            //Category category1 = new Category() { CategoryName = "Web Development", SubCategoriesList = subCategoriesList1 };
            //Category category2 = new Category() { CategoryName = "DB", SubCategoriesList = subCategoriesList2 };
            //Category category3 = new Category() { CategoryName = "Languages", SubCategoriesList = subCategoriesList3 };

            //context.Categories.AddOrUpdate(category1);
            //context.Categories.AddOrUpdate(category2);
            //context.Categories.AddOrUpdate(category3);
        }
    }
}
