using System;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRemoteGoToAction : PdfAction
{
	private string m_filePath;

	private PdfRemoteDestination m_remoteDestination;

	private bool m_isNewWindow;

	private PdfArray m_array = new PdfArray();

	private PdfDestinationMode m_destinationMode;

	private float m_zoom;

	private PointF m_location = PointF.Empty;

	private RectangleF m_bounds = RectangleF.Empty;

	public string FilePath
	{
		get
		{
			return m_filePath;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FilePath can not be null");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("FilePath - string can not be empty");
			}
			if (value != m_filePath)
			{
				m_filePath = value;
				base.Dictionary.SetString("F", m_filePath);
			}
		}
	}

	public bool IsNewWindow
	{
		get
		{
			return m_isNewWindow;
		}
		set
		{
			m_isNewWindow = value;
			if (m_isNewWindow)
			{
				base.Dictionary.SetBoolean("NewWindow", m_isNewWindow);
			}
		}
	}

	public PdfRemoteDestination PdfRemoteDestination
	{
		get
		{
			return m_remoteDestination;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Destination");
			}
			if (value != m_remoteDestination)
			{
				m_remoteDestination = value;
			}
		}
	}

	public PdfRemoteGoToAction(string filePath, PdfRemoteDestination remoteDestination)
	{
		PdfRemoteDestination = remoteDestination;
		FilePath = filePath;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.BeginSave += Dictionary_BeginSave;
		base.Dictionary.SetProperty("S", new PdfName("GoToR"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		m_destinationMode = PdfRemoteDestination.Mode;
		m_location = PdfRemoteDestination.Location;
		m_zoom = PdfRemoteDestination.Zoom;
		base.Dictionary["D"] = Save();
	}

	private PdfArray Save()
	{
		m_array.Clear();
		m_array.Add(new PdfNumber(PdfRemoteDestination.RemotePageNumber));
		switch (m_destinationMode)
		{
		case PdfDestinationMode.Location:
			m_array.Add(new PdfName("XYZ"));
			m_array.Add(new PdfNumber(m_location.X));
			m_array.Add(new PdfNumber(m_location.Y));
			m_array.Add(new PdfNumber(m_zoom));
			break;
		case PdfDestinationMode.FitToPage:
			m_array.Add(new PdfName("Fit"));
			break;
		case PdfDestinationMode.FitR:
			m_array.Add(new PdfName("FitR"));
			m_array.Add(new PdfNumber(m_location.X));
			m_array.Add(new PdfNumber(m_location.Y));
			m_array.Add(new PdfNumber(m_bounds.Width));
			m_array.Add(new PdfNumber(m_bounds.Height));
			break;
		case PdfDestinationMode.FitH:
			m_array.Add(new PdfName("FitH"));
			m_array.Add(new PdfNumber(m_location.X));
			break;
		case PdfDestinationMode.FitV:
			m_array.Add(new PdfName("FitV"));
			m_array.Add(new PdfNumber(m_location.X));
			break;
		case PdfDestinationMode.FitBH:
			m_array.Add(new PdfName("FitBH"));
			m_array.Add(new PdfNumber(m_location.Y));
			break;
		case PdfDestinationMode.FitB:
			m_array.Add(new PdfName("FitB"));
			break;
		case PdfDestinationMode.FitBV:
			m_array.Add(new PdfName("FitBV"));
			m_array.Add(new PdfNumber(m_location.X));
			break;
		}
		return m_array;
	}
}
