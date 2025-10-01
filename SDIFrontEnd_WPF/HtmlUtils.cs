using MvvmLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
namespace SDIFrontEnd_WPF
{
    public static class HtmlUtils
    {
        public static string ConvertFlowDocumentToHtml(FlowDocument flowDocument)
        {
            if (flowDocument == null)
                return string.Empty;

            // convert FlowDocument to XAML
            string? content = RichTextBoxHelper.ConvertFlowDocumentToXaml(flowDocument);
            // convert XAML to HTML
            string html = MarkupConverter.Core.HtmlFromXamlConverter.ConvertXamlToHtml(content, false);
            html = HtmlUtils.ConvertHtmlTagsToLowerCase(html); // make tags lower case
            // convert HTML tags to lowercase
            html = ConvertHtmlTagsToLowerCase(html);
            return html;
        }

        public static string ConvertHtmlTagsToLowerCase(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // Regex pattern to match HTML tags
            return Regex.Replace(html, @"<[^>]+>", match =>
            {
                string tag = match.Value;

                // Convert the tag name and attribute names to lowercase
                // Split tag into parts like: "<DIV", "CLASS=...", ">"
                return Regex.Replace(tag, @"\b[A-Z][A-Z0-9-]*\b", m => m.Value.ToLower());
            });
        }
    }
}
