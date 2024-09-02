using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.OfficeChart;
using DocGen.Pdf;

namespace DocGen.DocIORenderer;

public class DocIORendererSettings
{
	private bool m_autoDetectComplexScript;

	private bool m_enableAlternateChunks = true;

	private bool m_updateDocumentFields;

	private int m_imageQuality = 100;

	internal int m_imageResolution;

	private byte m_bFlags = 1;

	private byte m_bFlags1 = 2;

	private bool m_preserveFormFields;

	private IWarning m_warning;

	private PdfConformanceLevel m_pdfConformanceLevel;

	private ExportBookmarkType m_exportBookmarkType;

	private ChartRenderingOptions m_chartRenderingOptions;

	public bool AutoDetectComplexScript
	{
		get
		{
			return m_autoDetectComplexScript;
		}
		set
		{
			m_autoDetectComplexScript = value;
		}
	}

	public bool EnableAlternateChunks
	{
		get
		{
			return m_enableAlternateChunks;
		}
		set
		{
			m_enableAlternateChunks = value;
		}
	}

	internal int ImageQuality
	{
		get
		{
			return m_imageQuality;
		}
		set
		{
			if (value <= 100)
			{
				m_imageQuality = value;
				return;
			}
			throw new PdfException("The value should be between 0 and 100");
		}
	}

	public bool PreserveFormFields
	{
		get
		{
			return m_preserveFormFields;
		}
		set
		{
			m_preserveFormFields = value;
		}
	}

	internal int ImageResolution
	{
		set
		{
			if (value > 0)
			{
				m_imageResolution = value;
				return;
			}
			throw new PdfException("The value should be valid DPI");
		}
	}

	public IWarning Warning
	{
		get
		{
			return m_warning;
		}
		set
		{
			m_warning = value;
		}
	}

	public bool OptimizeIdenticalImages
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

	public PdfConformanceLevel PdfConformanceLevel
	{
		get
		{
			return m_pdfConformanceLevel;
		}
		set
		{
			m_pdfConformanceLevel = value;
		}
	}

	public bool EmbedFonts
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

	public bool EmbedCompleteFonts
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

	public bool AutoTag
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
			if (value)
			{
				m_exportBookmarkType = ExportBookmarkType.Headings;
			}
		}
	}

	public ExportBookmarkType ExportBookmarks
	{
		get
		{
			return m_exportBookmarkType;
		}
		set
		{
			if (m_exportBookmarkType != ExportBookmarkType.Headings)
			{
				m_exportBookmarkType = value;
			}
		}
	}

	public bool UpdateDocumentFields
	{
		get
		{
			return m_updateDocumentFields;
		}
		set
		{
			m_updateDocumentFields = value;
		}
	}

	public ChartRenderingOptions ChartRenderingOptions
	{
		get
		{
			if (m_chartRenderingOptions == null)
			{
				m_chartRenderingOptions = new ChartRenderingOptions();
			}
			return m_chartRenderingOptions;
		}
	}
}
