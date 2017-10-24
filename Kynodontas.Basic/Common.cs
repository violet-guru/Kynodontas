using Microsoft.Azure.Documents;
using NGettext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

namespace Kynodontas.Basic
{
    public class BaseDocument
    {
        public string id { get; set; }
    }

    public class Record : BaseDocument
    {
        public bool IsDeleted { get; set; }
        public int RecordType { get; set; }
        public string ContactText { get; set; }
        public string CompanyId { get; set; }

        //Field creation: "2017-10-10T00:00:00Z"
        public DateTime? AddedDate { get; set; }

        public override string ToString()
        {
            return "r:" + RecordType + " | id:" + id + " | CompanyId:" + CompanyId;
        }
    }

    public class Login
    {
        public bool NotFound { get; set; }
        public Record Client { get; set; }
    }

    public class QuickPage
    {
        public bool IsValidPage { get; set; }
        public bool IsPartial { get; set; }
        public string PageText { get; set; }

        public QuickPage(string pageText)
        {
            IsValidPage = true;
            PageText = pageText;
        }
    }

    public class InitResponse
    {
        public bool IsNotValid { get; set; }
        public ClientRole ClientRole { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ContactText { get; set; }
    }

    public class ClientRole
    {
        public int RoleId { get; }

        public ClientRole(int roleId)
        {
            RoleId = roleId;
        }
    }

    /// <summary>
    /// Counteracts html/js injection in code like this: string1 + "<script>alert('injection')</script>" + string2
    /// </summary>
    public class TextShow
    {
        private readonly string _text;
        public TextShow(string value)
        {
            _text = value;
        }

        public override string ToString()
        {
            return WebUtility.HtmlEncode(_text);
        }
    }

    public class Common
    {
        public List<string> NonAuthPages = new List<string> { "SignIn", "Mistake", "CloseThisTab" };
        public string LocalHostPath = "http://localhost:7071";
        public string ApiPath = "/api/";
        public string DatabaseId = "Database1";
        public string DatabaseEndpoint = "https://localhost:8081/";
        public string DatabaseDocumentKey = "C2y*******";
        public string DomainPath = "https://example.azurewebsites.net";
        public string MailgunKey = "key-*********";
        public string MailgunDomain = "sandboxbe********.mailgun.org";
    }
}
