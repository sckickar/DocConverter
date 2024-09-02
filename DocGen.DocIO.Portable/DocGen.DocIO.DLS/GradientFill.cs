using System.Collections.Generic;
using System.Text;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.DLS;

internal class GradientFill
{
	private FlipOrientation m_flip;

	private List<GradientStop> m_gradientStops;

	private LinearGradient m_linearGradient;

	private PathGradient m_pathGradient;

	private TileRectangle m_tileRectangle;

	private string m_Focus;

	private byte m_bFlags;

	internal string Focus
	{
		get
		{
			return m_Focus;
		}
		set
		{
			m_Focus = value;
		}
	}

	internal bool RotateWithShape
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

	internal FlipOrientation Flip
	{
		get
		{
			return m_flip;
		}
		set
		{
			m_flip = value;
		}
	}

	internal List<GradientStop> GradientStops
	{
		get
		{
			if (m_gradientStops == null)
			{
				m_gradientStops = new List<GradientStop>();
			}
			return m_gradientStops;
		}
	}

	internal LinearGradient LinearGradient
	{
		get
		{
			return m_linearGradient;
		}
		set
		{
			m_linearGradient = value;
		}
	}

	internal PathGradient PathGradient
	{
		get
		{
			return m_pathGradient;
		}
		set
		{
			m_pathGradient = value;
		}
	}

	internal TileRectangle TileRectangle
	{
		get
		{
			if (m_tileRectangle == null)
			{
				m_tileRectangle = new TileRectangle();
			}
			return m_tileRectangle;
		}
	}

	internal bool IsEmptyElement
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

	internal GradientFill()
	{
	}

	internal GradientFill Clone()
	{
		GradientFill gradientFill = (GradientFill)MemberwiseClone();
		if (GradientStops != null && GradientStops.Count > 0)
		{
			gradientFill.m_gradientStops = new List<GradientStop>();
			foreach (GradientStop gradientStop in GradientStops)
			{
				gradientFill.GradientStops.Add(gradientStop.Clone());
			}
		}
		if (LinearGradient != null)
		{
			gradientFill.LinearGradient = LinearGradient.Clone();
		}
		if (PathGradient != null)
		{
			gradientFill.PathGradient = PathGradient.Clone();
		}
		if (TileRectangle != null)
		{
			gradientFill.m_tileRectangle = TileRectangle.Clone();
		}
		return gradientFill;
	}

	internal void Close()
	{
		if (m_gradientStops != null)
		{
			m_gradientStops.Clear();
			m_gradientStops = null;
		}
		m_linearGradient = null;
		m_pathGradient = null;
		m_tileRectangle = null;
	}

	internal bool Compare(GradientFill gradientFill)
	{
		if (RotateWithShape != gradientFill.RotateWithShape || Focus != gradientFill.Focus || Flip != gradientFill.Flip)
		{
			return false;
		}
		if ((TileRectangle != null && gradientFill.TileRectangle == null) || (TileRectangle == null && gradientFill.TileRectangle != null) || (LinearGradient != null && gradientFill.LinearGradient == null) || (LinearGradient == null && gradientFill.LinearGradient != null) || (PathGradient != null && gradientFill.PathGradient == null) || (PathGradient == null && gradientFill.PathGradient != null) || (GradientStops != null && gradientFill.GradientStops == null) || (GradientStops == null && gradientFill.GradientStops != null))
		{
			return false;
		}
		if (TileRectangle != null && gradientFill.TileRectangle != null && !TileRectangle.Compare(gradientFill.TileRectangle))
		{
			return false;
		}
		if (LinearGradient != null && gradientFill.LinearGradient != null && !LinearGradient.Compare(gradientFill.LinearGradient))
		{
			return false;
		}
		if (PathGradient != null && gradientFill.PathGradient != null && !PathGradient.Compare(gradientFill.PathGradient))
		{
			return false;
		}
		if (GradientStops != null && gradientFill.GradientStops != null)
		{
			if (GradientStops.Count != gradientFill.GradientStops.Count)
			{
				return false;
			}
			for (int i = 0; i < GradientStops.Count; i++)
			{
				if (!GradientStops[i].Compare(gradientFill.GradientStops[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (RotateWithShape ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(Focus + ";");
		stringBuilder.Append((int)Flip + ";");
		if (TileRectangle != null)
		{
			stringBuilder.Append(TileRectangle.GetAsString());
		}
		if (LinearGradient != null)
		{
			stringBuilder.Append(LinearGradient.GetAsString());
		}
		if (PathGradient != null)
		{
			stringBuilder.Append(PathGradient.GetAsString());
		}
		if (GradientStops != null)
		{
			foreach (GradientStop gradientStop in GradientStops)
			{
				stringBuilder.Append(gradientStop.GetAsString());
			}
		}
		return stringBuilder;
	}
}
