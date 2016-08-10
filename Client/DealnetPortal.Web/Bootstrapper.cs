using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Mvc;

namespace DealnetPortal.Web
{
    public class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            //IDependencyResolver resolver = DependencyResolver.Current;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            //var container = new UnityContainer();
            //container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();

            //container.RegisterType<UserManager<ApplicationUser>>();
            //container.RegisterType<ApplicationDbContext>();
            //container.RegisterType<ApplicationUserManager>();
            //container.RegisterType<ApplicationSignInManager>();

            //container.RegisterType<AccountController>(new InjectionConstructor());            
            //container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(
            //        new InjectionConstructor(typeof(ApplicationDbContext)));

            var container = new UnityContainer();

            container.RegisterType<ApplicationDbContext>(new PerRequestLifetimeManager(), new InjectionConstructor());

            // Identity
            container.RegisterType<UserManager<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<SignInManager<ApplicationUser, string>>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new PerRequestLifetimeManager(), new InjectionFactory(x => new UserStore<ApplicationUser>(new ApplicationDbContext())));
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(x => HttpContext.Current.GetOwinContext().Authentication));            

            return container;
        }
    }
}