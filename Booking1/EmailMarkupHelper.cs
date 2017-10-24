namespace FunctionApp1
{
    public partial class CommonMarkupHelper
    {
        public string SignInEmailMessage(string linkUrl, bool isInvitation, string companyName)
        {
            var message = t.GetString("Sign in to {0}", companyName);
            if(isInvitation)
            {
                message = t.GetString("Confirm invitation to {0}", companyName);
            }

            //http://www.industrydive.com/news/post/how-to-make-html-email-buttons-that-rock/#outlook
            return
$@"
<html>
<body>
<table cellspacing='0' cellpadding='0'>
<tr>
<td align='center' width='400' height='40' bgcolor='#000091' style='border-radius: 5px; color: #ffffff; display: block;'>
<a href='{linkUrl}' style='font-size:16px; font-weight: bold; font-family: Helvetica, Arial, sans-serif; text-decoration: none; line-height:40px; width:100%; display:inline-block'>
<span style='color: #FFFFFF'>{message}&nbsp;&nbsp;&nbsp;►</span></a>
</td>
</tr>
</table>
</body>
</html>";
        }
    }
}
