using NGettext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kynodontas.Basic;
using Kynodontas.Basic.Bll;

namespace FunctionApp1
{
    public class ProcessedWebPage
    {
        public CommonMarkupHelper Helper;
        private readonly ICatalog t;

        public ProcessedWebPage(ICatalog catalog)
        {
            t = catalog;
            Helper = new CommonMarkupHelper(t);
        }

        public async Task<QuickPage> Get(string pageName, KynodontasPage page, InitResponse initResponse,
           string firstParameterValue, string secondParameterValue)
        {
            var companyId = initResponse.CompanyId;
            var clientRole = initResponse.ClientRole;
            var companyName = new TextShow(initResponse.CompanyName);
            var contactText = new TextShow(initResponse.ContactText);
            var firstParameter = new TextShow(firstParameterValue);
            var secondParameter = new TextShow(secondParameterValue);

            switch (pageName)
            {
                case "CloseThisTab": return CloseThisTab(companyName);
                case "Index": return Index(companyId, companyName, clientRole, contactText);
                case "LayoutHeader": return LayoutHeader(companyName, contactText, secondParameter);
                case "SignIn": return SignIn(companyName, firstParameter);
            }

            return new QuickPage("") { IsValidPage = false };
        }

        private QuickPage CloseThisTab(TextShow companyName)
        {
            return new QuickPage(Helper.CloseThisTab(companyName));
        }

        private QuickPage Index(string companyId, TextShow companyName, ClientRole clientRole, TextShow contactText)
        {
            return new QuickPage(Helper.Index(companyName, clientRole));
        }

        private QuickPage LayoutHeader(TextShow companyName, TextShow emailAccount, TextShow browserLocale)
        {
            return new QuickPage(Helper.LayoutHeader(emailAccount, browserLocale, companyName));
        }

        private QuickPage SignIn(TextShow companyName, TextShow companyId)
        {
            return new QuickPage(Helper.SignIn(companyName, companyId));
        }
    }
}
