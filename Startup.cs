using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DevQuiz.Startup))]
namespace DevQuiz
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
