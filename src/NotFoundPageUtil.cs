using System;
using System.Web;
using BVNetwork.FileNotFound.Content;
using EPiServer.Configuration;

namespace BVNetwork.FileNotFound.MvcContrib
{
    public static class NotFoundPageUtil
    {
        public static PageContent Get404PageLanguageResourceContent()
        {
            return new PageContent();
        }

        public static string GetReferer(HttpContextBase contextBase)
        {
            string str = contextBase.Request.ServerVariables["HTTP_REFERER"];
            if (str == null)
                return string.Empty;

            string str2 = Settings.Instance.SiteUrl.ToString();
            if (str.StartsWith(str2))
                str = str.Remove(0, str2.Length);

            return str;
        }

        public static Uri GetUrlNotFound(HttpContextBase contextBase)
        {
            Uri uri = null;
            string str = contextBase.Request.ServerVariables["QUERY_STRING"];

            if ((str != null) && str.StartsWith("404;"))
            {
                uri = new Uri(str.Split(new[] {';'})[1]);
            }

            if ((uri == null) && str.StartsWith("aspxerrorpath="))
            {
                string[] strArray = str.Split(new char[] {'='});
                uri =
                    new Uri(contextBase.Request.Url.GetLeftPart(UriPartial.Authority) +
                            HttpUtility.UrlDecode(strArray[1]));
            }

            return uri;
        }
    }
}

 
