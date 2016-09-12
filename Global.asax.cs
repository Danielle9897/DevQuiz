using DevQuiz.Migrations;
using DevQuiz.Repository;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DevQuiz
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private ApplicationRepository repository = new ApplicationRepository();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Run the latest migration --- only needed when running on AZURE 
            // var configuration = new Configuration();
            // var migrator = new DbMigrator(configuration);
            // migrator.Update();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Set session timeout to 30 for now, 
            // TBD - Check if user is still taking the test and prolong session life...
            Session.Timeout = 30;

            // Update stats table in repository
            repository.SetStatsInfo(System.DateTime.Now);            
        }

    }
}
