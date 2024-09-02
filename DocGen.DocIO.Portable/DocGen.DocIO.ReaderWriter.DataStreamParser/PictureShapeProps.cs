using System;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser;

[CLSCompliant(false)]
internal class PictureShapeProps : BaseProps
{
	private float m_brightness = 50f;

	private float m_contrast = 50f;

	private PictureColor m_color;

	private string m_altText;

	private string m_name;

	internal float PictureBrightness
	{
		get
		{
			return m_brightness;
		}
		set
		{
			m_brightness = value;
		}
	}

	internal float PictureContrast
	{
		get
		{
			return m_contrast;
		}
		set
		{
			m_contrast = value;
		}
	}

	internal PictureColor PictureColor
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	internal string AlternativeText
	{
		get
		{
			return m_altText;
		}
		set
		{
			m_altText = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal PictureShapeProps()
	{
	}
}
