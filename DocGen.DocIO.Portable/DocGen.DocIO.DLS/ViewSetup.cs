using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class ViewSetup : XDLSSerializableBase
{
	public const int DEF_ZOOMING = 100;

	private ZoomType m_zoomType;

	private int m_zoomPercent;

	private DocumentViewType m_docViewType;

	public int ZoomPercent
	{
		get
		{
			return m_zoomPercent;
		}
		set
		{
			if (value < 10 || value > 500)
			{
				throw new ArgumentOutOfRangeException("Zoom percentage must be between 10 and 500 percent.");
			}
			m_zoomPercent = value;
		}
	}

	public ZoomType ZoomType
	{
		get
		{
			return m_zoomType;
		}
		set
		{
			m_zoomType = value;
		}
	}

	public DocumentViewType DocumentViewType
	{
		get
		{
			return m_docViewType;
		}
		set
		{
			m_docViewType = value;
		}
	}

	public ViewSetup(IWordDocument doc)
		: base((WordDocument)doc, null)
	{
		m_zoomType = ZoomType.None;
		m_docViewType = DocumentViewType.PrintLayout;
		m_zoomPercent = 100;
	}

	internal ViewSetup Clone(WordDocument doc)
	{
		ViewSetup obj = (ViewSetup)CloneImpl();
		obj.SetOwner(doc);
		return obj;
	}

	internal void SetZoomPercentValue(int value)
	{
		if (value == 0)
		{
			value = 100;
		}
		else if (value < 10)
		{
			value = 10;
		}
		else if (value > 500)
		{
			value = 500;
		}
		m_zoomPercent = value;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (ZoomPercent != 100)
		{
			writer.WriteValue("ZoomPercent", ZoomPercent);
		}
		if (ZoomType != 0)
		{
			writer.WriteValue("ZoomType", ZoomType);
		}
		if (DocumentViewType != DocumentViewType.PrintLayout)
		{
			writer.WriteValue("ViewType", DocumentViewType);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("ZoomPercent"))
		{
			ZoomPercent = reader.ReadInt("ZoomPercent");
		}
		if (reader.HasAttribute("ZoomType"))
		{
			ZoomType = (ZoomType)(object)reader.ReadEnum("ZoomType", typeof(ZoomType));
		}
		if (reader.HasAttribute("ViewType"))
		{
			DocumentViewType = (DocumentViewType)(object)reader.ReadEnum("ViewType", typeof(DocumentViewType));
		}
	}
}
