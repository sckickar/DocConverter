namespace DocGen.OfficeChart;

public class ChartRenderingOptions
{
	private ExportImageFormat m_imageFormat;

	private ScalingMode m_scalingMode;

	public ExportImageFormat ImageFormat
	{
		get
		{
			return m_imageFormat;
		}
		set
		{
			m_imageFormat = value;
		}
	}

	public ScalingMode ScalingMode
	{
		get
		{
			return m_scalingMode;
		}
		set
		{
			m_scalingMode = value;
		}
	}

	public ChartRenderingOptions()
	{
		m_imageFormat = ExportImageFormat.Png;
		m_scalingMode = ScalingMode.Best;
	}
}
