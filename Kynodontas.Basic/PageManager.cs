using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kynodontas.Basic
{
    public class PageManager
    {
        public string AppFolder = @"D:\Users\Hugo\Source\Repos\Kynodontas\Booking1";

        public string GetPagesInAppFolder()
        {
            var returned = new List<string>();
            var htmlFiles = Directory.GetFiles($@"{AppFolder}\Webpage\", "*.html");
            foreach (var path in htmlFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                returned.Add($@"case ""{fileName}"": return @""{GetPage(path)}"";");
            }

            return string.Join(Environment.NewLine, returned) + Environment.NewLine;
        }

        public string GetTranslationFileFromDisk(string browserLocale)
        {
            var absolutePath = TranslationFileLocation(browserLocale);
            return ToClassCode(File.ReadAllBytes(absolutePath));
        }

        public FileStream GetTranslationStreamFromDisk(string browserLocale)
        {
            return File.OpenRead(TranslationFileLocation(browserLocale));
        }

        public string GetPageFromDisk(string pageName, out bool isValidPage)
        {
            var absolutePath = PageLocation(pageName);
            isValidPage = File.Exists(absolutePath);
            return isValidPage ? File.ReadAllText(absolutePath) : "";
        }

        private string GetPage(string absolutePath)
        {
            return ToClassCode(File.ReadAllBytes(absolutePath));
        }

        private string ToClassCode(byte[] text)
        {
            return Convert.ToBase64String(text).Replace("\"", "\"\"");
        }

        private string TranslationFileLocation(string browserLocale)
        {
            return $@"{AppFolder}\locales\{browserLocale}\LC_MESSAGES\Test.mo";
        }

        private string PageLocation(string pageName)
        {
            return $@"{AppFolder}\Webpage\{pageName}.html";
        }
    }
}
