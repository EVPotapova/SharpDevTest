using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using SharpDevTest.Models;
using SharpDevTest.Providers;
using SharpDevTest.Services.Interfaces;
using SharpDevTest.Services.Services;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace SharpDevTest
{
    public class AutofacConfig
    {

        public static IContainer Container;

        public static void Initialize(HttpConfiguration config)
        {
            Initialize(config, RegisterServices(new ContainerBuilder()));
        }


        public static void Initialize(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static IContainer RegisterServices(ContainerBuilder builder)
        {
            //Register your Web API controllers.  
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterType<PwAccountService>().As<IPwAccountService>().PropertiesAutowired().SingleInstance();            
            builder.RegisterType<ApplicationDbContext>().As<System.Data.Entity.DbContext>();

            builder.RegisterType<ApplicationDbContext>().AsSelf().SingleInstance();
            builder.Register<IdentityFactoryOptions<ApplicationUserManager>>(c => new IdentityFactoryOptions<ApplicationUserManager>() { DataProtectionProvider = new DpapiDataProtectionProvider("your app name") });
            builder.RegisterType<ApplicationUserManager>().AsSelf().SingleInstance();

            builder.Register(c => new ApplicationOAuthProvider("self",c.Resolve<ApplicationUserManager>())).AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new UserStore<ApplicationUser>(c.Resolve<ApplicationDbContext>())).AsImplementedInterfaces().SingleInstance();
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).As<IAuthenticationManager>();

            //builder.Register(c => new ApplicationDbContext());
            builder.Register(c => new UserStore<ApplicationUser>(new ApplicationDbContext())).AsImplementedInterfaces();
            builder.Register(c => new IdentityFactoryOptions<ApplicationUserManager>()
            {
                DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("ApplicationName")
            });
            builder.RegisterType<ApplicationUserManager>();

            //Set the dependency resolver to be Autofac.  
            Container = builder.Build();

            return Container;
        }

    }

    public class Bootstrapper
    {

        public static void Run()
        {
            //Configure AutoFac  
            AutofacConfig.Initialize(GlobalConfiguration.Configuration);
        }

    }
}