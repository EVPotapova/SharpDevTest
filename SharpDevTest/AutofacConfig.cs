using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
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