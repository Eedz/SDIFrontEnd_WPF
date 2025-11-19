using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IImporter<T>
    {
        IEnumerable<T> Import();
    }

    public class WordImporter<T> : IImporter<T>
    {
        private readonly IWordTableImporter _tableImporter;
        private readonly IDataRowParser<T> _parser;

        public WordImporter(IWordTableImporter tableImporter, IDataRowParser<T> parser)
        {
            _tableImporter = tableImporter;
            _parser = parser;
        }

        public IEnumerable<T> Import()
        {
            _tableImporter.ReadHeadings();

            foreach (var row in _tableImporter.ReadRows())
                yield return _parser.Parse(row);
        }
    }
}
