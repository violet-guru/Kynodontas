using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using System.Net.Http.Headers;
using RestSharp.Authenticators;
using System.Diagnostics;
using Kynodontas.Basic;
using Kynodontas.Basic.Bll;

namespace FunctionApp1
{
    public static partial class Tasks
    {
        public static string LoginNotFound = "appDeveloper: Login not found";
        public static string Ok = "appDeveloper: OK";
        public static string AParameterIsEmpty = "appDeveloper: A parameter is empty";
        public static string AValueIsInvalid = "appDeveloper: A value is invalid";

        [FunctionName("MarkupPage")]
        public static async Task<HttpResponseMessage> MarkupPage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MarkupPage/{pageName}")]HttpRequestMessage req,
            string pageName,
            TraceWriter log)
        {
            log.Info("appDeveloper: MarkupPage method started");
            var page = InitPage(req);
            var accountBll = new AccountBll(new DatabaseHelper<Record>());

            var firstParameter = req.GetQueryNameValuePairs().FirstOrDefault(q => q.Key == "m").Value;
            var secondParameter = req.GetQueryNameValuePairs().FirstOrDefault(q => q.Key == "n").Value;
            var clientLogin = await GetClientLogin(req, accountBll);
            var initResponse = await InitResponse(page, clientLogin, pageName, firstParameter, accountBll);
            if(initResponse.IsNotValid)
            {
                return page.RedirectResponse("Mistake");
            }

            var processedWebPage = new ProcessedWebPage(page.t);
            var processedPage =
                await processedWebPage.Get(pageName, page, initResponse, firstParameter, secondParameter);

            if (processedPage.IsPartial)
            {
                return req.CreateResponse(HttpStatusCode.OK, processedPage.PageText);
            }

            var processedHeader =
                await processedWebPage.Get("LayoutHeader", page, initResponse, firstParameter, page.BrowserLocale);

            return page.GetResponse(pageName, processedHeader.PageText, processedPage.PageText, processedPage.IsValidPage);
        }

        [FunctionName("SendLoginMail")]
        public static async Task<HttpResponseMessage> SendLoginMail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info("appDeveloper: SendLoginMail method started");
            var page = InitPage(req);

            dynamic data = await req.Content.ReadAsAsync<object>();
            string companyId = data?.companyId;
            string companyName = data?.companyName;
            string emailAddress = data?.emailAddress;

            if (string.IsNullOrEmpty(companyId) ||
                string.IsNullOrEmpty(companyName) ||
                string.IsNullOrEmpty(emailAddress))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, AParameterIsEmpty);
            }

            var tokenClientId = Guid.NewGuid().ToString();

            //Login creation
            var document = new Record
            {
                id = tokenClientId,
                CompanyId = companyId,
                ContactText = emailAddress.ToLower(),
                RecordType = 5,
                AddedDate = page.DateNow.TimeDate
            };
            await new DatabaseHelper<Record>().CreateDocument(document);

            //Send mail
            var signInButton =
                new CommonMarkupHelper(page.t)
                    .SignInEmailMessage(page.MainPath + "Sign?In=" + tokenClientId, false, companyName);

            var emailText = page.t.GetString("Click the button:");
            var response = SendEmail(page.Common, emailText, emailAddress, emailText, signInButton);
            var message = "appDeveloper: Mailgun response characters: " + response.Content.Length;
            return req.CreateResponse(response.StatusCode, message);
        }

        [FunctionName("Sign")]
        public static async Task<HttpResponseMessage> Sign(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info("appDeveloper: Sign method started");
            var page = InitPage(req);
            var accountBll = new AccountBll(new DatabaseHelper<Record>());

            var tokenClientId = req.GetQueryNameValuePairs().FirstOrDefault(q => q.Key == "In").Value;
            if (string.IsNullOrEmpty(tokenClientId) ||
                !Guid.TryParse(tokenClientId, out var _))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, AValueIsInvalid);
            }

            //Check client permissions
            var userLogins = await accountBll.GetUserLogins(tokenClientId);
            if (!userLogins.Any())
            {
                return page.RedirectResponse("Mistake");
            }

            var returned = page.RedirectResponse("Index");
            returned.Headers.AddCookies(SetAuthCookie(req, tokenClientId, page.DebuggerIsAttached));
            return returned;
        }

        [FunctionName("SignOut")]
        public static async Task<HttpResponseMessage> SignOut(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info("appDeveloper: SignOut method started");
            var page = InitPage(req);
            var accountBll = new AccountBll(new DatabaseHelper<Record>());

            //Check client permissions
            var clientLogin = await GetClientLogin(req, accountBll);
            if (clientLogin.NotFound)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, LoginNotFound);
            }

            var returned = page.RedirectResponse("SignIn?m=" + clientLogin.Client.CompanyId);
            returned.Headers.AddCookies(DeleteCookie(req, page.DebuggerIsAttached));
            return returned;
        }

        private static CookieHeaderValue[] SetAuthCookie(HttpRequestMessage req, string tokenClientId,
            bool debuggerIsAttached)
        {
            var cookie =
                new CookieHeaderValue("tokenClientId", tokenClientId) {Expires = ClockDate.MaxValue, Path = "/"};
            if (!debuggerIsAttached)
            {
                cookie.Domain = req.RequestUri.Host;
            }

            return new[] {cookie};
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/ms178195.aspx
        /// </summary>
        private static CookieHeaderValue[] DeleteCookie(HttpRequestMessage req, bool debuggerIsAttached)
        {
            var cookie = new CookieHeaderValue("tokenClientId", "")
            {
                Expires = ClockDate.UtcNow.AddDays(-1d).TimeDate,
                Path = "/"
            };
            if (!debuggerIsAttached)
            {
                cookie.Domain = req.RequestUri.Host;
            }

            return new[] { cookie };
        }

        private static KynodontasPage InitPage(HttpRequestMessage req)
        {
            var page = new KynodontasPage();
            page.BrowserLocale = page.GetAppLanguage(req.Headers.AcceptLanguage.ToList());
            page.DebuggerIsAttached = Debugger.IsAttached;
            page.DateNow = ClockDate.UtcNow;
            page.t = page.GetCatalog(page.BrowserLocale, page.DebuggerIsAttached);
            return page;
        }

        private static async Task<InitResponse> InitResponse(KynodontasPage page, Login clientLogin, string pageName,
            string firstParameter, AccountBll accountBll)
        {
            var returned = new InitResponse();

            //Check client permissions
            if (!page.Common.NonAuthPages.Contains(pageName) && clientLogin.NotFound)
            {
                return new InitResponse { IsNotValid = true };
            }

            var hasCompanyId = false;
            if (!clientLogin.NotFound)
            {
                returned.CompanyId = clientLogin.Client.CompanyId;
                returned.ContactText = clientLogin.Client.ContactText;
                hasCompanyId = true;
            }
            else if (pageName != "Mistake")
            {
                returned.CompanyId = new Guid(firstParameter).ToString();
                hasCompanyId = true;
            }

            if (!hasCompanyId)
            {
                return returned;
            }

            returned.CompanyName = "CompanyName";
            returned.ClientRole = new ClientRole(11);
            return returned;
        }

        private static async Task<Login> GetClientLogin(HttpRequestMessage req, AccountBll accountBll)
        {
            var returned = new Login();
            var tokenClientId = GetTokenClient(req);
            var userLogins = await accountBll.GetUserLogins(tokenClientId);
            if (userLogins.Any())
            {
                returned.Client = userLogins.First();
            }
            returned.NotFound = !userLogins.Any() || string.IsNullOrEmpty(tokenClientId);

            return returned;
        }

        private static string GetTokenClient(HttpRequestMessage req)
        {
            var cookieHeaderValue = req.Headers.GetCookies("tokenClientId").FirstOrDefault();
            if (cookieHeaderValue == null)
            {
                return "";
            }

            var cookieState = cookieHeaderValue.Cookies.FirstOrDefault(t => t.Name == "tokenClientId");
            return cookieState != null ? new Guid(cookieState.Value).ToString() : "";
        }

        private static IRestResponse SendEmail(Common common, string fromAddressName, string toField, string subjectText, string htmlText)
        {
            //Get Started of mailgun.com
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", common.MailgunKey)
            };

            var request = new RestRequest();
            request.AddParameter("domain", common.MailgunDomain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", $@"{fromAddressName} <postmaster@{common.MailgunDomain}>");
            request.AddParameter("to", toField); //"Eugene <example@mail.com>"
            request.AddParameter("subject", subjectText);
            request.AddParameter("html", htmlText);
            request.Method = Method.POST;

            return client.Execute(request);
        }
    }
}