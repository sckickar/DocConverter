using DocGen.Drawing;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf;

internal class GlyphDescription
{
	internal const int ARG_1_AND_2_ARE_WORDS = 0;

	internal const int ARGS_ARE_XY_VALUES = 1;

	internal const int ROUND_XY_TO_GRID = 2;

	internal const int WE_HAVE_A_SCALE = 3;

	internal const int MORE_COMPONENTS = 5;

	internal const int WE_HAVE_AN_X_AND_Y_SCALE = 6;

	internal const int WE_HAVE_A_TWO_BY_TWO = 7;

	internal const int WE_HAVE_INSTRUCTIONS = 8;

	internal const int USE_MY_METRICS = 9;

	internal const int OVERLAP_COMPOUND = 10;

	private ushort m_flags;

	private ushort m_glyphIndex;

	private DocGen.PdfViewer.Base.Matrix m_transform;

	public ushort Flags
	{
		get
		{
			return m_flags;
		}
		private set
		{
			m_flags = value;
		}
	}

	public ushort GlyphIndex
	{
		get
		{
			return m_glyphIndex;
		}
		private set
		{
			m_glyphIndex = value;
		}
	}

	public DocGen.PdfViewer.Base.Matrix Transform
	{
		get
		{
			return m_transform;
		}
		private set
		{
			m_transform = value;
		}
	}

	internal bool CheckFlag(byte bit)
	{
		return GetBit(Flags, bit);
	}

	public bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	public PointF Transformpoint(PointF point)
	{
		double num = point.X;
		double num2 = point.Y;
		double num3 = num * m_transform.M11 + num2 * m_transform.M12 + m_transform.OffsetX;
		double num4 = num * m_transform.M21 + num2 * m_transform.M22 + m_transform.OffsetY;
		return new PointF((float)num3, (float)num4);
	}

	public void Read(ReadFontArray reader)
	{
		m_flags = reader.getnextUshort();
		m_glyphIndex = reader.getnextUshort();
		int num = 0;
		int num2 = 0;
		if (CheckFlag(0))
		{
			if (CheckFlag(1))
			{
				num = reader.getnextshort();
				num2 = reader.getnextshort();
			}
			else
			{
				reader.getnextUshort();
				reader.getnextUshort();
			}
		}
		else if (CheckFlag(1))
		{
			num = reader.ReadChar();
			num2 = reader.ReadChar();
		}
		else
		{
			reader.ReadChar();
			reader.ReadChar();
		}
		float num3 = 1f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 1f;
		if (CheckFlag(3))
		{
			num6 = (num3 = reader.Get2Dot14());
		}
		else if (CheckFlag(6))
		{
			num3 = reader.Get2Dot14();
			num6 = reader.Get2Dot14();
		}
		else if (CheckFlag(7))
		{
			num3 = reader.Get2Dot14();
			num4 = reader.Get2Dot14();
			num5 = reader.Get2Dot14();
			num6 = reader.Get2Dot14();
		}
		m_transform = new DocGen.PdfViewer.Base.Matrix(num3, num4, num5, num6, (float)num, (float)num2);
	}
}
