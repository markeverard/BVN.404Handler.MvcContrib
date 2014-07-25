using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using BVNetwork.Bvn.FileNotFound.Logging;
using BVNetwork.Bvn.FileNotFound.Upgrade;
using BVNetwork.FileNotFound.Configuration;
using BVNetwork.FileNotFound.Redirects;
using log4net;

namespace BVNetwork.FileNotFound.MvcContrib
{
    public class NotFoundPageAttribute : ActionFilterAttribute
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var referer = NotFoundPageUtil.GetReferer(filterContext.HttpContext);
            var urlNotFound = NotFoundPageUtil.GetUrlNotFound(filterContext.HttpContext);
            if (urlNotFound == null)
                return;

            if (Log.IsDebugEnabled)
                Log.DebugFormat("Trying to handle 404 for \"{0}\" (Referrer: \"{1}\")", urlNotFound, referer);

            CustomRedirectHandler current = CustomRedirectHandler.Current;
            CustomRedirect redirect = current.CustomRedirects.Find(HttpUtility.HtmlEncode(urlNotFound.AbsoluteUri));

            string oldUrl = HttpUtility.HtmlEncode(urlNotFound.LocalPath);
            if (redirect == null)
                redirect = current.CustomRedirects.Find(oldUrl);

            if (redirect == null)
            {
                if ((Configuration.Configuration.Logging == LoggerMode.On) && Upgrader.Valid)
                    Logger.LogRequest(oldUrl, referer);

                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.Response.StatusCode = 0x194;
                filterContext.HttpContext.Response.Status = "404 File not found";
            }
            else //redirect found
            {
                string newurl = redirect.NewUrl;
                if (redirect.WildCardSkipAppend) //if wildcard, replace the 'old part' of url
                {
                    newurl = urlNotFound.LocalPath.Replace(redirect.OldUrl, redirect.NewUrl);
                }

                //always add querystring
                newurl += urlNotFound.Query;

                if (redirect.State.Equals(0) && (string.Compare(newurl, oldUrl, StringComparison.InvariantCultureIgnoreCase) != 0))
                {
                    Log.Info(string.Format("404 Custom Redirect: To: '{0}' (from: '{1}')", newurl, oldUrl));
                    
                    filterContext.Result = new RedirectResult(newurl, true);
                }
            }
        }

    }
}