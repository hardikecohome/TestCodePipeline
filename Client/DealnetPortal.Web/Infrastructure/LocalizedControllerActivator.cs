﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DealnetPortal.Web.Infrastructure
{
    public class LocalizedControllerActivator : IControllerActivator
    {
        private string _defaultLanguage = "en";

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            //Get the {language} parameter in the RouteData
            string lang = (string)requestContext.RouteData.Values["lang"] ?? _defaultLanguage;
            //TODO: implement CultureHelper.GetCurrentCulture and compare it
            if (!string.IsNullOrEmpty(lang))
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
                }
                catch (Exception e)
                {
                    throw new NotSupportedException(String.Format("ERROR: Invalid language code '{0}'.", lang));
                }
            }
            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}