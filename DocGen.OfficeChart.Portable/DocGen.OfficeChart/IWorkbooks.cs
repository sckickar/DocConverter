using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace DocGen.OfficeChart;

internal interface IWorkbooks : IEnumerable
{
	IApplication Application { get; }

	int Count { get; }

	IWorkbook this[int Index] { get; }

	object Parent { get; }

	IWorkbook Create();

	IWorkbook Create(int worksheetsQuantity);

	IWorkbook Create(string[] names);

	IWorkbook Open(Stream stream);

	IWorkbook Open(Stream stream, OfficeVersion version);

	IWorkbook Open(Stream stream, OfficeParseOptions options);

	IWorkbook Open(Stream stream, string separator, int row, int column);

	IWorkbook Open(Stream stream, string separator, int row, int column, Encoding encoding);

	IWorkbook Open(Stream stream, string separator);

	IWorkbook Open(Stream stream, string separator, Encoding encoding);

	IWorkbook Open(Stream stream, OfficeParseOptions options, bool bReadOnly, string password);

	IWorkbook Open(Stream stream, OfficeParseOptions options, bool isReadOnly, string password, OfficeVersion version);

	IWorkbook Open(Stream stream, OfficeParseOptions options, bool isReadOnly, string password, OfficeOpenType openType);

	IWorkbook Open(Stream stream, OfficeOpenType openType);

	IWorkbook Open(Stream stream, OfficeOpenType openType, OfficeParseOptions options);

	IWorkbook Open(Stream stream, OfficeOpenType openType, OfficeVersion version);

	IWorkbook OpenFromXml(Stream stream, OfficeXmlOpenType openType);

	IWorkbook OpenFromXml(XmlReader reader, OfficeXmlOpenType openType);

	void Close();
}
