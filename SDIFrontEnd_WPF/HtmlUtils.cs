using MvvmLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
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

    public static class SimpleHtmlConverter
    {
        public static string ToHtml(FlowDocument doc)
        {
            var sb = new StringBuilder();

            foreach (var block in doc.Blocks)
            {
                if (block is Paragraph p)
                {
                    ConvertInlines(p.Inlines, sb);
                    sb.Append("<br>");
                }
            }

            return sb.ToString();
        }

        private static void ConvertInlines(InlineCollection inlines, StringBuilder sb)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case Run run:
                        sb.Append(System.Net.WebUtility.HtmlEncode(run.Text));
                        break;

                    case Bold bold:
                        sb.Append("<strong>");
                        ConvertInlines(bold.Inlines, sb);
                        sb.Append("</strong>");
                        break;

                    case Italic italic:
                        sb.Append("<em>");
                        ConvertInlines(italic.Inlines, sb);
                        sb.Append("</em>");
                        break;

                    case Underline underline:
                        sb.Append("<u>");
                        ConvertInlines(underline.Inlines, sb);
                        sb.Append("</u>");
                        break;

                    case Span span when span.Background is SolidColorBrush brush && brush.Color == Colors.Yellow:
                        sb.Append("<span style=\"background-color:yellow;\">");
                        ConvertInlines(span.Inlines, sb);
                        sb.Append("</span>");
                        break;

                    case LineBreak:
                        sb.Append("<br>");
                        break;
                }
            }
        }

        public static FlowDocument FromHtml(string html)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph();
            doc.Blocks.Add(paragraph);

            var stack = new Stack<Span>();
            Span current = null;

            var tokens = Regex.Split(html, "(<.*?>)");

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (token.StartsWith("<"))
                {
                    switch (token.ToLower())
                    {
                        case "<strong>":
                            current = new Bold();
                            Push(current, stack, paragraph);
                            break;

                        case "</strong>":
                            Pop(stack, ref current);
                            break;

                        case "<em>":
                            current = new Italic();
                            Push(current, stack, paragraph);
                            break;

                        case "</em>":
                            Pop(stack, ref current);
                            break;

                        case "<u>":
                            current = new Underline();
                            Push(current, stack, paragraph);
                            break;

                        case "</u>":
                            Pop(stack, ref current);
                            break;

                        case "<span style=\"background-color:yellow;\">":
                            current = new Span { Background = Brushes.Yellow };
                            Push(current, stack, paragraph);
                            break;

                        case "</span>":
                            Pop(stack, ref current);
                            break;

                        case "<br>":
                        case "<br/>":
                        case "<br />":
                            AddInline(new LineBreak(), current, paragraph);
                            break;
                    }
                }
                else
                {
                    var text = System.Net.WebUtility.HtmlDecode(token);
                    AddInline(new Run(text), current, paragraph);
                }
            }

            return doc;
        }

        private static void Push(Span span, Stack<Span> stack, Paragraph paragraph)
        {
            if (stack.Count > 0)
                stack.Peek().Inlines.Add(span);
            else
                paragraph.Inlines.Add(span);

            stack.Push(span);
        }

        private static void Pop(Stack<Span> stack, ref Span current)
        {
            if (stack.Count > 0)
                stack.Pop();

            current = stack.Count > 0 ? stack.Peek() : null;
        }

        private static void AddInline(Inline inline, Span current, Paragraph paragraph)
        {
            if (current != null)
                current.Inlines.Add(inline);
            else
                paragraph.Inlines.Add(inline);
        }
    }
}
