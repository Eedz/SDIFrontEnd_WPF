using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IWordTableImporter
    {
        /// <summary>
        /// Mapping of heading → column index.
        /// </summary>
        IReadOnlyDictionary<string, int> Headings { get; }

        /// <summary>
        /// Scans the table and populates the Headings dictionary.
        /// </summary>
        void ReadHeadings();

        /// <summary>
        /// Reads all rows and returns them as cell-value dictionaries.
        /// </summary>
        IEnumerable<IDictionary<string, string>> ReadRows();
    }

    public class WordTableImporter : IWordTableImporter
    {
        private readonly string _filePath;

        public IReadOnlyDictionary<string, int> Headings => _headings;
        private readonly Dictionary<string, int> _headings = new();

        public WordTableImporter(string filePath)
        {
            _filePath = filePath;
        }

        public void ReadHeadings()
        {
            using var doc = WordprocessingDocument.Open(_filePath, false);

            var table = doc.MainDocumentPart.Document.Body
                .Descendants<Table>()
                .FirstOrDefault();

            if (table == null)
                throw new InvalidOperationException("No table found in document.");

            var headerRow = table.Descendants<TableRow>().FirstOrDefault();
            if (headerRow == null)
                throw new InvalidOperationException("No header row found in document.");

            var cells = headerRow.Descendants<TableCell>().ToList();

            _headings.Clear();

            for (int i = 0; i < cells.Count; i++)
            {
                string headingText = GetHeadingCellText(cells[i]).Trim();

                if (string.IsNullOrWhiteSpace(headingText))
                    continue;

                if (!_headings.ContainsKey(headingText))
                    _headings.Add(headingText, i);
            }

            if (_headings.Count == 0)
                throw new InvalidOperationException("No valid headings found in table.");
        }

        public IEnumerable<IDictionary<string, string>> ReadRows()
        {
            using var doc = WordprocessingDocument.Open(_filePath, false);

            var table = doc.MainDocumentPart.Document.Body
                .Descendants<Table>()
                .FirstOrDefault();

            if (table == null)
                yield break;

            var rows = table.Descendants<TableRow>().Skip(1); // skip header

            foreach (var row in rows)
            {
                var cells = row.Descendants<TableCell>().ToList();
                var dict = new Dictionary<string, string>();

                foreach (var kvp in _headings)
                {
                    int colIndex = kvp.Value;

                    string cellValue = colIndex < cells.Count
                        ? GetCellText(cells[colIndex]).Trim()
                        : string.Empty;

                    dict[kvp.Key] = cellValue;
                }

                yield return dict;
            }
        }

        private static string GetCellText(TableCell cell)
        {
            List<string> html = new List<string>();
            foreach (Paragraph p in cell.Descendants<Paragraph>())
                html.Add(OpenXmlToHtml.OpenXmlToHtml.ConvertOriginalParagraphToHtml(p));
            return string.Join("<br>", html);
        }

        private static string GetHeadingCellText(TableCell cell)
        {
            
            return cell.InnerText;
        }
    }




}
