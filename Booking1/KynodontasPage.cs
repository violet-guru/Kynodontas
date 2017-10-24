using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Kynodontas.Basic;
using NGettext;

namespace FunctionApp1
{
    public partial class KynodontasPage
    {
        public string BrowserLocale { get; set; }
        public bool DebuggerIsAttached { get; set; }
        public Catalog t { get; set; }
        public ClockDate DateNow { get; set; }
        public Common Common = new Common();
        public string MainPath
        {
            get
            {
                return (DebuggerIsAttached ? Common.LocalHostPath : Common.DomainPath) + Common.ApiPath;
            }
        }

        private const string LayoutMinimalHeader = @"<!DOCTYPE html><html><head></head><body>";
        private const string LayoutFooter = @"</body></html>";

        private readonly List<string> _appLanguages = new List<string>
            {
                "en",
                "es",
                "zh-cn",
            };

        public HttpResponseMessage GetResponse(string pageName, string processedHeader, string processedPage, bool isProcessedPageValid)
        {
            var basicPageText = GetBasicPage(pageName, DebuggerIsAttached, out var isValidBasicPage);

            if (string.IsNullOrEmpty(pageName) || !isValidBasicPage && !isProcessedPageValid)
            {
                return RedirectResponse("Mistake");
            }

            var basicHeader = GetBasicPage("LayoutHeader", DebuggerIsAttached, out var _);
            return InitResponse(basicHeader + processedHeader + basicPageText + processedPage + LayoutFooter);
        }

        public Catalog GetCatalog(string browserLocale, bool debuggerIsAttached)
        {
            var pageManager = new PageManager();
            var catalog = new Catalog();
            var cultureInfo = new CultureInfo(browserLocale.Replace("_", "-"));

            if (browserLocale == "en" || !_appLanguages.Contains(browserLocale))
            {
                return catalog;
            }

            Stream moStream;
            if (debuggerIsAttached)
            {
                //Load from disk
                moStream = pageManager.GetTranslationStreamFromDisk(browserLocale);
            }
            else
            {
                //Optimized to load page from dll to output
                var buffer = Convert.FromBase64String(new KynodontasFile().GetTranslationBinary(browserLocale));
                moStream = new MemoryStream(buffer);
            }

            return new Catalog(moStream, cultureInfo);
        }

        public HttpResponseMessage RedirectResponse(string location)
        {
            return InitResponse(
                LayoutMinimalHeader + "<script>window.location.href = '" + Common.ApiPath + "MarkupPage/" +
                location + "'</script>" + LayoutFooter
            );
        }

        public string GetAppLanguage(List<StringWithQualityHeaderValue> browserLanguages)
        {

            foreach (var language in browserLanguages)
            {
                var currentLanguage = language.Value.ToLower();
                foreach (var appLanguage in _appLanguages)
                {
                    if (appLanguage.StartsWith(currentLanguage) || currentLanguage.StartsWith(appLanguage))
                    {
                        return appLanguage;
                    }
                }
            }
            return _appLanguages.First();
        }

        private HttpResponseMessage InitResponse(string pageContent)
        {
            //http://anthonychu.ca/post/azure-functions-serve-html/
            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(pageContent)};
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        private string GetBasicPage(string pageName, bool debuggerIsAttached, out bool isValidPage)
        {
            if (debuggerIsAttached)
            {
                return new PageManager().GetPageFromDisk(pageName, out isValidPage);
            }
            else
            {
                // Optimized to load page from dll to output
                return Encoding.UTF8.GetString(Convert.FromBase64String(new KynodontasFile().GetPageText(pageName, out isValidPage)));
            }
        }
    }
}
