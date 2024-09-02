using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorkbooksCollection : CollectionBaseEx<object>, IWorkbooks, IEnumerable
{
	public new IWorkbook this[int Index] => (IWorkbook)base.InnerList[Index];

	public IWorkbook Create(string[] names)
	{
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		if (names.Length == 0)
		{
			throw new ArgumentException("Names array must contain at least one name.");
		}
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, names.Length, base.Application.DefaultVersion);
		for (int i = 0; i < names.Length; i++)
		{
			workbookImpl.Worksheets[i].Name = names[i];
		}
		base.Add(workbookImpl);
		workbookImpl.Activate();
		SetAplicatioName(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Create(int sheetsQuantity)
	{
		if (sheetsQuantity < 0)
		{
			throw new ArgumentOutOfRangeException("sheetsQuantity", "Quantity of worksheets must be greater than zero.");
		}
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, sheetsQuantity, base.Application.DefaultVersion);
		if (base.Application.DefaultVersion == OfficeVersion.Excel97to2003)
		{
			workbookImpl.BeginVersion = 2;
		}
		base.Add(workbookImpl);
		workbookImpl.Activate();
		SetAplicatioName(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Create()
	{
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, base.Application.DefaultVersion);
		if (base.Application.DefaultVersion == OfficeVersion.Excel97to2003)
		{
			workbookImpl.BeginVersion = 2;
		}
		base.Add(workbookImpl);
		workbookImpl.Activate();
		SetAplicatioName(workbookImpl);
		return workbookImpl;
	}

	private void SetAplicatioName(IWorkbook book)
	{
	}

	public IWorkbook Open(Stream stream, string separator, int row, int column)
	{
		return Open(stream, separator, row, column, null, null, base.Application.DefaultVersion);
	}

	public IWorkbook Open(Stream stream, string separator, int row, int column, Encoding encoding)
	{
		return Open(stream, separator, row, column, null, encoding, base.Application.DefaultVersion);
	}

	private IWorkbook Open(Stream stream, string separator, int row, int column, string fileName, Encoding encoding, OfficeVersion version)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		if (separator.Length == 0)
		{
			throw new ArgumentException("separator");
		}
		return OpenInternal(stream, separator, row, column, fileName, encoding, version);
	}

	private IWorkbook OpenInternal(Stream stream, string separator, int row, int column, string fileName, Encoding encoding, OfficeVersion version)
	{
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, stream, separator, row, column, version, fileName, encoding);
		base.Add(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Open(Stream stream, string separator)
	{
		return Open(stream, separator, 1, 1, null);
	}

	public IWorkbook Open(Stream stream, string separator, OfficeVersion version)
	{
		return Open(stream, separator, 1, 1, null, null, version);
	}

	public IWorkbook Open(Stream stream, string separator, Encoding encoding)
	{
		return Open(stream, separator, 1, 1, encoding);
	}

	public IWorkbook Open(Stream stream, OfficeParseOptions options, bool isReadOnly, string password)
	{
		return Open(stream, options, isReadOnly, password, base.Application.DefaultVersion);
	}

	public IWorkbook Open(Stream stream, OfficeParseOptions options, bool isReadOnly, string password, OfficeVersion excelVersion)
	{
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, stream, options, isReadOnly, password, excelVersion);
		base.Add(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Open(Stream stream, OfficeParseOptions options, bool isReadOnly, string password, OfficeOpenType openType)
	{
		if (openType == OfficeOpenType.Automatic)
		{
			openType = base.AppImplementation.DetectFileFromStream(stream);
		}
		return openType switch
		{
			OfficeOpenType.BIFF => Open(stream, options, isReadOnly, password, OfficeVersion.Excel97to2003), 
			OfficeOpenType.SpreadsheetML => OpenFromXml(stream, OfficeXmlOpenType.MSExcel), 
			OfficeOpenType.CSV => Open(stream, base.Application.CSVSeparator), 
			OfficeOpenType.SpreadsheetML2007 => Open(stream, options, isReadOnly, password, OfficeVersion.Excel2007), 
			OfficeOpenType.SpreadsheetML2010 => Open(stream, options, isReadOnly, password, OfficeVersion.Excel2010), 
			_ => throw new ArgumentOutOfRangeException("openType"), 
		};
	}

	public IWorkbook Open(Stream stream)
	{
		return Open(stream, OfficeOpenType.Automatic);
	}

	public IWorkbook Open(Stream stream, OfficeVersion version)
	{
		return Open(stream, version, OfficeParseOptions.Default);
	}

	public IWorkbook Open(Stream stream, OfficeVersion version, OfficeParseOptions options)
	{
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, stream, version, options);
		base.Add(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Open(Stream stream, OfficeParseOptions options)
	{
		WorkbookImpl workbookImpl = base.AppImplementation.CreateWorkbook(this, stream, options, base.Application.DefaultVersion);
		base.Add(workbookImpl);
		return workbookImpl;
	}

	public IWorkbook Open(Stream stream, OfficeOpenType openType, OfficeVersion version)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (openType == OfficeOpenType.Automatic)
		{
			openType = base.AppImplementation.DetectFileFromStream(stream);
		}
		return openType switch
		{
			OfficeOpenType.Automatic => throw new ArgumentException("Cannot recognize current file type."), 
			OfficeOpenType.BIFF => Open(stream, OfficeVersion.Excel97to2003), 
			OfficeOpenType.SpreadsheetML => OpenFromXml(stream, OfficeXmlOpenType.MSExcel), 
			OfficeOpenType.CSV => Open(stream, base.Application.CSVSeparator, version), 
			OfficeOpenType.SpreadsheetML2007 => Open(stream, OfficeVersion.Excel2007), 
			OfficeOpenType.SpreadsheetML2010 => Open(stream, OfficeVersion.Excel2010), 
			_ => throw new ArgumentOutOfRangeException("openType"), 
		};
	}

	public IWorkbook Open(Stream stream, OfficeOpenType openType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (openType == OfficeOpenType.Automatic)
		{
			openType = base.AppImplementation.DetectFileFromStream(stream);
		}
		return openType switch
		{
			OfficeOpenType.Automatic => throw new ArgumentException("Cannot recognize current file type."), 
			OfficeOpenType.BIFF => Open(stream, OfficeVersion.Excel97to2003), 
			OfficeOpenType.SpreadsheetML => OpenFromXml(stream, OfficeXmlOpenType.MSExcel), 
			OfficeOpenType.CSV => Open(stream, base.Application.CSVSeparator), 
			OfficeOpenType.SpreadsheetML2007 => Open(stream, OfficeVersion.Excel2007), 
			OfficeOpenType.SpreadsheetML2010 => Open(stream, OfficeVersion.Excel2010), 
			_ => throw new ArgumentOutOfRangeException("openType"), 
		};
	}

	public IWorkbook Open(Stream stream, OfficeOpenType openType, OfficeParseOptions options)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (openType == OfficeOpenType.Automatic)
		{
			openType = base.AppImplementation.DetectFileFromStream(stream);
		}
		return openType switch
		{
			OfficeOpenType.Automatic => throw new ArgumentException("Cannot recognize current file type."), 
			OfficeOpenType.BIFF => Open(stream, OfficeVersion.Excel97to2003, options), 
			OfficeOpenType.SpreadsheetML => OpenFromXml(stream, OfficeXmlOpenType.MSExcel), 
			OfficeOpenType.CSV => Open(stream, base.Application.CSVSeparator), 
			OfficeOpenType.SpreadsheetML2007 => Open(stream, OfficeVersion.Excel2007, options), 
			OfficeOpenType.SpreadsheetML2010 => Open(stream, OfficeVersion.Excel2010, options), 
			_ => throw new ArgumentOutOfRangeException("openType"), 
		};
	}

	private IWorkbook Open(Stream stream, OfficeOpenType openType, string fileName, OfficeVersion version)
	{
		return Open(stream, openType, fileName, version, OfficeParseOptions.Default);
	}

	private IWorkbook Open(Stream stream, OfficeOpenType openType, string fileName, OfficeVersion version, OfficeParseOptions options)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (openType == OfficeOpenType.Automatic)
		{
			openType = base.AppImplementation.DetectFileFromStream(stream);
		}
		return openType switch
		{
			OfficeOpenType.Automatic => throw new ArgumentException("Cannot recognize current file type."), 
			OfficeOpenType.BIFF => Open(stream, OfficeVersion.Excel97to2003, options), 
			OfficeOpenType.SpreadsheetML => OpenFromXml(stream, OfficeXmlOpenType.MSExcel), 
			OfficeOpenType.CSV => Open(stream, base.Application.CSVSeparator, 1, 1, fileName, null, version), 
			OfficeOpenType.SpreadsheetML2007 => Open(stream, OfficeVersion.Excel2007, options), 
			OfficeOpenType.SpreadsheetML2010 => Open(stream, OfficeVersion.Excel2010, options), 
			_ => throw new ArgumentOutOfRangeException("openType"), 
		};
	}

	public IWorkbook OpenFromXml(Stream stream, OfficeXmlOpenType openType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		XmlReader reader = XmlReader.Create(new StreamReader(stream));
		return OpenFromXml(reader, openType);
	}

	public IWorkbook OpenFromXml(XmlReader reader, OfficeXmlOpenType openType)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		IWorkbook workbook = OpenFromXmlInternal(reader, openType);
		if (workbook != null)
		{
			return workbook;
		}
		throw new ApplicationException("Unable to read from XML.");
	}

	private IWorkbook OpenFromXmlInternal(XmlReader reader, OfficeXmlOpenType openType)
	{
		WorkbookImpl workbookImpl = new WorkbookImpl(base.Application, this, reader, openType);
		if (workbookImpl != null)
		{
			base.Add(workbookImpl);
			return workbookImpl;
		}
		return workbookImpl;
	}

	public void Close()
	{
		IWorkbook activeWorkbook = base.Application.ActiveWorkbook;
		if (activeWorkbook != null && base.InnerList.IndexOf(activeWorkbook) >= 0 && base.InnerList.Count > 0)
		{
			activeWorkbook.Close(SaveChanges: true, null);
		}
	}

	public WorkbooksCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	private OfficeVersion DetectVersion(string strTemplateFile)
	{
		return base.Application.DefaultVersion;
	}
}
