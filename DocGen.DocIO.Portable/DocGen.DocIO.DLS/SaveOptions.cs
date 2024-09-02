using System.Globalization;
using System.IO;

namespace DocGen.DocIO.DLS;

public class SaveOptions
{
	private byte m_bFlags = 1;

	private byte m_bFlags1 = 1;

	private CssStyleSheetType m_htmlExportCssStyleSheetType = CssStyleSheetType.Internal;

	private string m_htmlExportCssStyleSheetFileName;

	private string m_htmlExportImagesFolder = string.Empty;

	private short m_EPubHeadingLevels = 3;

	private string[] m_fontFiles;

	private string m_markdownExportImagesFolder = string.Empty;

	internal string[] FontFiles
	{
		get
		{
			return m_fontFiles;
		}
		set
		{
			m_fontFiles = value;
		}
	}

	public string MarkdownExportImagesFolder
	{
		get
		{
			return m_markdownExportImagesFolder;
		}
		set
		{
			m_markdownExportImagesFolder = value;
		}
	}

	internal bool EPubExportFont
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal short EPubHeadingLevels
	{
		get
		{
			return m_EPubHeadingLevels;
		}
		set
		{
			m_EPubHeadingLevels = value;
		}
	}

	public CssStyleSheetType HtmlExportCssStyleSheetType
	{
		get
		{
			return m_htmlExportCssStyleSheetType;
		}
		set
		{
			m_htmlExportCssStyleSheetType = value;
		}
	}

	internal string HtmlExportCssStyleSheetFileName
	{
		get
		{
			return m_htmlExportCssStyleSheetFileName;
		}
		set
		{
			m_htmlExportCssStyleSheetFileName = value;
		}
	}

	internal string HtmlExportImagesFolder
	{
		get
		{
			return m_htmlExportImagesFolder;
		}
		set
		{
			m_htmlExportImagesFolder = value;
		}
	}

	public bool HtmlExportHeadersFooters
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public bool OptimizeRtfFileSize
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public bool UseContextualSpacing
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	public bool HtmlExportTextInputFormFieldAsText
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public bool HtmlExportOmitXmlDeclaration
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public bool MaintainCompatibilityMode
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	public bool HTMLExportWithWordCompatibility
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public bool HtmlExportBodyContentAlone
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsEventSubscribed => this.ImageNodeVisited != null;

	public event ImageNodeVisitedEventHandler ImageNodeVisited;

	internal ImageNodeVisitedEventArgs ExecuteSaveImageEvent(Stream imageStream, string uri)
	{
		ImageNodeVisitedEventArgs imageNodeVisitedEventArgs = new ImageNodeVisitedEventArgs(imageStream, uri);
		if (this.ImageNodeVisited != null)
		{
			this.ImageNodeVisited(this, imageNodeVisitedEventArgs);
		}
		return imageNodeVisitedEventArgs;
	}

	internal void Close()
	{
	}

	internal void EnsureImagesFolder(bool m_bImagesFolderCreated, bool m_cacheFilesInternally, string m_imagesFolder, string m_fileNameWithoutExt)
	{
		if (!m_bImagesFolderCreated && !m_cacheFilesInternally)
		{
			Directory.CreateDirectory(m_imagesFolder + m_fileNameWithoutExt + "_images\\");
			m_bImagesFolderCreated = true;
		}
	}

	internal void ProcessImageUsingFileStream(string m_imagesFolder, string imgPath, byte[] imageBytes)
	{
		using FileStream stream = new FileStream(m_imagesFolder + imgPath, FileMode.Create);
		using MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
		memoryStream.WriteTo(stream);
	}

	internal string GetImagePath(ref int m_imgCounter, bool m_cacheFilesInternally, string m_fileNameWithoutExt)
	{
		m_imgCounter++;
		if (!m_cacheFilesInternally)
		{
			return m_fileNameWithoutExt + "_images\\" + m_fileNameWithoutExt + "_img" + m_imgCounter.ToString(CultureInfo.InvariantCulture);
		}
		return "images/img" + m_imgCounter.ToString(CultureInfo.InvariantCulture);
	}
}
