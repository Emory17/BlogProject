using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BlogProject.Data;

namespace BlogProject.Helpers
{
    public static class StringHelper
    {
        public static string BlogPostSlug(string? title) 
        {
            string? output = RemoveAccents(title).ToLower();

            output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");

            output = Regex.Replace(output, @"\s+", " ");

            output = Regex.Replace(output, @"\s", "-");

            return output;
        }

        public static string RemoveAccents(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return title!;
            }

            title = title.Normalize(NormalizationForm.FormD);

            char[] chars = title.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();

            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
