using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace WinRTNewsReader.Common.Helpers
{
    public static class HtmlToXamlConverter
    {
        private static readonly char[] SKIP_CHARS = new char[] { '\ufffd' };

        public static FrameworkElement GetTextElementFromHTML(string html, string direction = "ltr")
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var fd = FlowDirection.LeftToRight;
            if (string.Compare(direction, "rtl", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                fd = FlowDirection.RightToLeft;
            }

            TextBlock textBlock = new TextBlock();
            textBlock.FlowDirection = fd;
            if (fd == FlowDirection.RightToLeft)
            {
                textBlock.Padding = new Thickness(20, 10, 5, 10);
            }
            else
            {
                textBlock.Padding = new Thickness(5, 10, 20, 10);
            }

            foreach (var node in doc.DocumentNode.Descendants("p"))
            {
                var pureText = CleanupHTML(node.InnerText);
                var r = new Run();
                r.Text = pureText;

                textBlock.Inlines.Add(r);
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new LineBreak());
            }

            if (textBlock.Inlines.Count > 2)
            {
                textBlock.Inlines.RemoveAt(textBlock.Inlines.Count - 1);
                textBlock.Inlines.RemoveAt(textBlock.Inlines.Count - 1);
            }
            return textBlock;
        }

        private static string CleanupHTML(string input)
        {
            StringBuilder sb = new StringBuilder();
            bool beginning = true;
            foreach (var c in input)
            {
                if (beginning)
                {
                    if (c == '\n' || c == '\t' || c == ' ')
                        continue;
                }
                beginning = false;

                if (Array.IndexOf(SKIP_CHARS, c) > -1)
                    continue;

                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}