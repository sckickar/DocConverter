using System.Text;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class LineStyleBooleanProperties
{
	internal const uint DefaultValue = 46u;

	private int m_key;

	private msofbtRGFOPTE m_prop;

	internal bool HasDefined => m_prop.ContainsKey(m_key);

	internal bool PenAlignInset
	{
		get
		{
			if (UsefInsetPenOK && InsetPenOK && UsefInsetPen)
			{
				return InsetPen;
			}
			return false;
		}
		set
		{
			bool flag2 = (InsetPen = value);
			bool flag4 = (UsefInsetPen = flag2);
			bool usefInsetPenOK = (InsetPenOK = flag4);
			UsefInsetPenOK = usefInsetPenOK;
		}
	}

	internal bool UsefLineOpaqueBackColor
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x2000000) >> 25 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFDFFFFFFu) | (int)((value ? 1u : 0u) << 25));
			}
		}
	}

	internal bool UsefInsetPen
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x400000) >> 22 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFBFFFFFu) | (int)((value ? 1u : 0u) << 22));
			}
		}
	}

	internal bool UsefInsetPenOK
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x200000) >> 21 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFDFFFFFu) | (int)((value ? 1u : 0u) << 21));
			}
		}
	}

	internal bool UsefArrowheadsOK
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x100000) >> 20 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFEFFFFFu) | (int)((value ? 1u : 0u) << 20));
			}
		}
	}

	internal bool UsefLine
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x80000) >> 19 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFF7FFFFu) | (int)((value ? 1u : 0u) << 19));
			}
		}
	}

	internal bool UsefHitTestLine
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x40000) >> 18 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFBFFFFu) | (int)((value ? 1u : 0u) << 18));
			}
		}
	}

	internal bool UsefLineFillShape
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x20000) >> 17 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFDFFFFu) | (int)((value ? 1u : 0u) << 17));
			}
		}
	}

	internal bool UsefNoLineDrawDash
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x10000) >> 16 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFEFFFFu) | (int)((value ? 1u : 0u) << 16));
			}
		}
	}

	internal bool LineOpaqueBackColor
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x200) >> 9 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFDFFu) | (int)((value ? 1u : 0u) << 9));
			}
		}
	}

	internal bool InsetPen
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x40) >> 6 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFBFu) | (int)((value ? 1u : 0u) << 6));
			}
		}
	}

	internal bool InsetPenOK
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x20) >> 5 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFDFu) | (int)((value ? 1u : 0u) << 5));
			}
		}
	}

	internal bool ArrowheadsOK
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 0x10) >> 4 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFEFu) | (int)((value ? 1u : 0u) << 4));
			}
		}
	}

	internal bool Line
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 8) >> 3 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFF7u) | (int)((value ? 1u : 0u) << 3));
			}
		}
	}

	internal bool HitTestLine
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 4) >> 2 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFFBu) | (int)((value ? 1u : 0u) << 2));
			}
		}
	}

	internal bool LineFillShape
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 2) >> 1 != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFFDu) | (int)((value ? 1u : 0u) << 1));
			}
		}
	}

	internal bool NoLineDrawDash
	{
		get
		{
			if (!HasDefined)
			{
				return false;
			}
			return ((m_prop[m_key] as FOPTEBid).Value & 1) != 0;
		}
		set
		{
			if (HasDefined)
			{
				(m_prop[m_key] as FOPTEBid).Value = (uint)(((m_prop[m_key] as FOPTEBid).Value & 0xFFFFFFFEu) | (value ? 1 : 0));
			}
		}
	}

	internal LineStyleBooleanProperties(msofbtRGFOPTE prop, int key)
	{
		m_prop = prop;
		m_key = key;
	}

	internal bool Compare(LineStyleBooleanProperties lineProperties)
	{
		if (HasDefined != lineProperties.HasDefined || PenAlignInset != lineProperties.PenAlignInset || UsefLineOpaqueBackColor != lineProperties.UsefLineOpaqueBackColor || UsefInsetPen != lineProperties.UsefInsetPen || UsefInsetPenOK != lineProperties.UsefInsetPenOK || UsefArrowheadsOK != lineProperties.UsefArrowheadsOK || UsefLine != lineProperties.UsefLine || UsefHitTestLine != lineProperties.UsefHitTestLine || UsefLineFillShape != lineProperties.UsefLineFillShape || UsefNoLineDrawDash != lineProperties.UsefNoLineDrawDash || LineOpaqueBackColor != lineProperties.LineOpaqueBackColor || InsetPen != lineProperties.InsetPen || InsetPenOK != lineProperties.InsetPenOK || ArrowheadsOK != lineProperties.ArrowheadsOK || Line != lineProperties.Line || HitTestLine != lineProperties.HitTestLine || LineFillShape != lineProperties.LineFillShape || NoLineDrawDash != lineProperties.NoLineDrawDash)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (HasDefined ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (PenAlignInset ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefLineOpaqueBackColor ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefInsetPen ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefInsetPenOK ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefArrowheadsOK ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefLine ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefHitTestLine ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefLineFillShape ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UsefNoLineDrawDash ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (LineOpaqueBackColor ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (InsetPen ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (InsetPenOK ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (ArrowheadsOK ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Line ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HitTestLine ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (LineFillShape ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (NoLineDrawDash ? "1" : "0");
		stringBuilder.Append(text + ";");
		return stringBuilder;
	}
}
