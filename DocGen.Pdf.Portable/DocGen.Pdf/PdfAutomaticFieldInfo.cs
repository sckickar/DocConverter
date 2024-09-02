using System;
using DocGen.Drawing;

namespace DocGen.Pdf;

internal class PdfAutomaticFieldInfo
{
	private PointF m_location = PointF.Empty;

	private PdfAutomaticField m_field;

	private float m_scalingX = 1f;

	private float m_scalingY = 1f;

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public PdfAutomaticField Field
	{
		get
		{
			return m_field;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Field");
			}
			m_field = value;
		}
	}

	public float ScalingX
	{
		get
		{
			return m_scalingX;
		}
		set
		{
			m_scalingX = value;
		}
	}

	public float ScalingY
	{
		get
		{
			return m_scalingY;
		}
		set
		{
			m_scalingY = value;
		}
	}

	public PdfAutomaticFieldInfo(PdfAutomaticField field, PointF location)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_field = field;
		m_location = location;
	}

	public PdfAutomaticFieldInfo(PdfAutomaticField field, PointF location, float scalingX, float scalingY)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_field = field;
		m_location = location;
		m_scalingX = scalingX;
		m_scalingY = scalingY;
	}

	public PdfAutomaticFieldInfo(PdfAutomaticFieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		m_field = fieldInfo.Field;
		m_location = fieldInfo.Location;
		m_scalingX = fieldInfo.ScalingX;
		m_scalingY = fieldInfo.ScalingY;
	}
}
