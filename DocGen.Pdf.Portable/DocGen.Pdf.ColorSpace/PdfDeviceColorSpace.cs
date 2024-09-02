using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.ColorSpace;

public class PdfDeviceColorSpace : PdfColorSpaces
{
	private PdfColorSpace m_DeviceColorSpaceType;

	public PdfColorSpace DeviceColorSpaceType
	{
		get
		{
			return m_DeviceColorSpaceType;
		}
		set
		{
			m_DeviceColorSpaceType = value;
		}
	}

	public PdfDeviceColorSpace(PdfColorSpace colorspace)
	{
		m_DeviceColorSpaceType = colorspace;
	}
}
