using System.Data.Entity;
using DevQuiz.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DevQuiz.Repository
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        //public ApplicationDBContext()
        //    : base("DefaultConnection", throwIfV1Schema: false)
        //{
        //}

        public ApplicationDBContext()
            : base("DevQuiz", throwIfV1Schema: false)
        {
        }

        public static ApplicationDBContext Create()
        {
            return new ApplicationDBContext();            
        }

        // The Tables: (using code first !)
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<Stats> Statistics { get; set; } 
        public DbSet<QuizSummaryForUser> QuizSummaryForUsers { get; set;}

        // Override tables default definition
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Add Fluent API here...  

            // Not really used - so in comment for now..
            // Define the foreign key of the user id for the UsersQuizData class
            // modelBuilder.Entity<QuizSummaryForUser>().HasOptional(u => u.ApplicationUser).WithMany().HasForeignKey(u => u.UserId);

            base.OnModelCreating(modelBuilder);
        }

    }
}