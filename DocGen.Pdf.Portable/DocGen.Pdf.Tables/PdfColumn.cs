using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfColumn
{
	private const float DefaultWidth = 10f;

	private float m_width;

	private PdfStringFormat m_stringFormat;

	private string m_columnName;

	internal bool isCustomWidth;

	internal PdfLightTableDataSourceType m_dataSourceType;

	public PdfStringFormat StringFormat
	{
		get
		{
			return m_stringFormat;
		}
		set
		{
			m_stringFormat = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentException("The width should be a positive number.", "Width");
			}
			if (m_dataSourceType != PdfLightTableDataSourceType.TableDirect)
			{
				isCustomWidth = true;
			}
			m_width = value;
		}
	}

	public string ColumnName
	{
		get
		{
			return m_columnName;
		}
		set
		{
			m_columnName = value;
		}
	}

	public PdfColumn()
	{
	}

	internal PdfColumn(float width)
		: this()
	{
		m_width = width;
	}

	public PdfColumn(string columnName)
	{
		m_columnName = columnName;
		m_width = 10f;
	}
}
