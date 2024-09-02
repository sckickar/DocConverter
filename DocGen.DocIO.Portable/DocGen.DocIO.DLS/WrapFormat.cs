using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class WrapFormat
{
	private bool m_AllowOverlap;

	private float m_DistanceBottom;

	private float m_DistanceLeft;

	private float m_DistanceRight;

	private float m_DistanceTop;

	private TextWrappingType m_TextWrappingType;

	private TextWrappingStyle m_TextWrappingStyle;

	private WrapPolygon m_wrapPolygon;

	internal bool IsWrappingBoundsAdded;

	internal int WrapCollectionIndex = -1;

	private byte m_bFlags;

	public bool AllowOverlap
	{
		get
		{
			return m_AllowOverlap;
		}
		set
		{
			m_AllowOverlap = value;
		}
	}

	public float DistanceBottom
	{
		get
		{
			if (m_DistanceBottom < 0f || m_DistanceBottom > 1584f)
			{
				return 0f;
			}
			return m_DistanceBottom;
		}
		set
		{
			m_DistanceBottom = value;
		}
	}

	public float DistanceLeft
	{
		get
		{
			if (m_DistanceLeft < 0f || m_DistanceLeft > 1584f)
			{
				return 0f;
			}
			return m_DistanceLeft;
		}
		set
		{
			m_DistanceLeft = value;
		}
	}

	public float DistanceRight
	{
		get
		{
			if (m_DistanceRight < 0f || m_DistanceRight > 1584f)
			{
				return 0f;
			}
			return m_DistanceRight;
		}
		set
		{
			m_DistanceRight = value;
		}
	}

	public float DistanceTop
	{
		get
		{
			if (m_DistanceTop < 0f || m_DistanceTop > 1584f)
			{
				return 0f;
			}
			return m_DistanceTop;
		}
		set
		{
			m_DistanceTop = value;
		}
	}

	internal bool IsBelowText
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			if (value && TextWrappingStyle == TextWrappingStyle.InFrontOfText)
			{
				m_TextWrappingStyle = TextWrappingStyle.Behind;
			}
			else if (!value && TextWrappingStyle == TextWrappingStyle.Behind)
			{
				m_TextWrappingStyle = TextWrappingStyle.InFrontOfText;
			}
		}
	}

	public TextWrappingType TextWrappingType
	{
		get
		{
			return m_TextWrappingType;
		}
		set
		{
			m_TextWrappingType = value;
		}
	}

	public TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return m_TextWrappingStyle;
		}
		set
		{
			m_TextWrappingStyle = value;
			if (m_TextWrappingStyle == TextWrappingStyle.Behind)
			{
				m_bFlags = (byte)((m_bFlags & 0xFEu) | 1u);
			}
			else
			{
				m_bFlags = (byte)((m_bFlags & 0xFEu) | 0u);
			}
		}
	}

	internal WrapPolygon WrapPolygon
	{
		get
		{
			if (m_wrapPolygon == null)
			{
				m_wrapPolygon = new WrapPolygon();
				m_wrapPolygon.Edited = false;
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
			}
			return m_wrapPolygon;
		}
		set
		{
			m_wrapPolygon = value;
		}
	}

	internal void SetTextWrappingStyleValue(TextWrappingStyle textWrappingStyle)
	{
		m_TextWrappingStyle = textWrappingStyle;
	}

	internal void Close()
	{
		if (m_wrapPolygon != null)
		{
			m_wrapPolygon.Close();
			m_wrapPolygon = null;
		}
	}

	internal bool Compare(WrapFormat wrapFormat)
	{
		if (AllowOverlap != wrapFormat.AllowOverlap || TextWrappingType != wrapFormat.TextWrappingType || TextWrappingStyle != wrapFormat.TextWrappingStyle || DistanceBottom != wrapFormat.DistanceBottom || DistanceLeft != wrapFormat.DistanceLeft || DistanceRight != wrapFormat.DistanceRight || DistanceTop != wrapFormat.DistanceTop)
		{
			return false;
		}
		if ((WrapPolygon == null && wrapFormat.WrapPolygon != null) || (WrapPolygon != null && wrapFormat.WrapPolygon == null))
		{
			return false;
		}
		if (WrapPolygon != null && !WrapPolygon.Compare(wrapFormat.WrapPolygon))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(AllowOverlap ? "1" : "0;");
		stringBuilder.Append((int)TextWrappingType + ",");
		stringBuilder.Append((int)TextWrappingStyle + ",");
		stringBuilder.Append(DistanceBottom + ",");
		stringBuilder.Append(DistanceLeft + ",");
		stringBuilder.Append(DistanceRight + ",");
		stringBuilder.Append(DistanceTop + ",");
		if (WrapPolygon != null)
		{
			stringBuilder.Append(WrapPolygon.GetAsString());
		}
		return stringBuilder;
	}
}
