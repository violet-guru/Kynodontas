using System;
using System.Collections.Generic;
using NGettext;
using System.Net;
using System.Linq;
using Kynodontas.Basic;

namespace FunctionApp1
{
    public partial class CommonMarkupHelper
    {
        private readonly ICatalog t;

        public CommonMarkupHelper(ICatalog catalog)
        {
            t = catalog;
        }

        public string CloseThisTab(TextShow companyName)
        {
            var buttonText = $@"<span class=""text-nowrap"">{t.GetString("'Sign in to {0}'", companyName)}</span>";

            return
$@"<script>document.title = '{t.GetString("Close this tab")}';</script>
<div id=""mainContainer"" class=""container"">
    <div class=""row"">
        <div class=""col-lg"">
            <h2>
                {t.GetString("We sent you an email with the button {0}. Please check your email inbox and click that button. You can close this tab.", buttonText)}
            </h2>
        </div>
    </div>
</div>";
        }

        public string Index(TextShow companyName, ClientRole clientRole)
        {
            var headMessage = t.GetString("Home");

            return
$@"<script>document.title = '{headMessage}';</script>
{ScriptingMessages()}
<div id=""mainContainer"" class=""container"">
    <div id=""indexActions"" class=""row"">
        <div class=""col-lg"">
        </div>
        <div class=""col-lg"">
            <h2 class=""mt-5"">{t.GetString("Account")}</h2>
            <a class=""btn btn-outline-info d-block"" role=""button"" href=""/api/SignOut"">
                {t.GetString("Sign out")}
            </a>
        </div>
    </div>
</div>";
        }

        public string LayoutHeader(TextShow emailAccount, TextShow browserLocale, TextShow companyName)
        {
            //http://getbootstrap.com/docs/4.0/examples/navbars/
            return
$@"<nav class=""navbar navbar-expand navbar-light bg-light mb-4 py-0"">
    <a class=""navbar-brand"" href=""/api/MarkupPage/Index"">{companyName}</a>
    <button class=""navbar-toggler"" type=""button"" data-toggle=""collapse"" data-target=""#navbarsExample02"" aria-controls=""navbarsExample02"" aria-expanded=""false"" aria-label=""Toggle navigation"">
        <span class=""navbar-toggler-icon""></span>
    </button>
    <div class=""collapse navbar-collapse"" id=""navbarsExample02"">
        <ul class=""navbar-nav mr-auto"">
        </ul>
        <div class=""my-2 my-md-0"">
            {emailAccount}
        </div>
    </div>
</nav>
<span id='browserLocale' class='d-none'>{browserLocale}</span>";
        }

        public string SignIn(TextShow companyName, TextShow companyId)
        {
            var headMessage = t.GetString("Sign in / Sign up");

            var returned =
$@"<script>document.title = '{headMessage}';</script>
{ScriptingMessages()}
{CompanyNameScriptingMessage(companyName)}
<form id=""mainContainer"" class=""container"">
    <div class=""row"">
        <div class=""col-lg"">
        </div>
        <div class=""col-lg"">
            <h2>{t.GetString("Sign in with email. Or sign up with email.")}</h2>
            <div class=""form-group"">
                <input type=""email"" name=""inputEmail"" class=""form-control"" autofocus=""autofocus"" spellcheck=""false"" />
            </div>
            <button type='button' class='btn btn-outline-info' id=""btnSubmit"" companyid=""{
            companyId}"">{t.GetString("Submit")}</button>
        </div>
    </div>
</form>";

            return returned;
        }

        /// <summary>
        /// Counteracts html/js injection in code like this: string1 + "<script>alert('injection')</script>" + string2
        /// </summary>
        private string A(string text)
        {
            return WebUtility.HtmlEncode(text);
        }

        private string ScriptingMessages()
        {
            return
$@"<span id=""minLengthMessage"" class=""d-none"">{t.GetString("This field needs to be longer.")}</span>
<span id=""requiredMessage"" class=""d-none"">{t.GetString("This field is required.")}</span>
<span id=""urlInvalidMessage"" class=""d-none"">{t.GetString("This field needs a valid URL.")}</span>
<span id=""emailInvalidMessage"" class=""d-none"">{t.GetString("This field needs a valid email address.")}</span>

<span id=""confirmMessage"" class=""d-none"">{t.GetString("Are you sure?")}</span>";
        }

        private string CompanyNameScriptingMessage(TextShow companyName)
        {
            return
$@"<span id=""companyNameMessage"" class=""d-none"">{companyName}</span>";
        }
    }
}
