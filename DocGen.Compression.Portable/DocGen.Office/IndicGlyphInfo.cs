namespace DocGen.Office;

internal class IndicGlyphInfo : OtfGlyphInfo
{
	private static int[] m_initialChars = new int[12]
	{
		2352, 2480, 2544, 2608, 2736, 2864, 2992, 3120, 3248, 3376,
		3515, 6042
	};

	private int m_group = -1;

	private int m_position = -1;

	private int m_mask;

	private bool m_substitute;

	private bool m_ligate;

	internal int Group
	{
		get
		{
			return m_group;
		}
		set
		{
			m_group = value;
		}
	}

	internal int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal int Mask
	{
		get
		{
			return m_mask;
		}
		set
		{
			m_mask = value;
		}
	}

	internal virtual bool Substitute
	{
		get
		{
			return m_substitute;
		}
		set
		{
			m_substitute = value;
		}
	}

	internal virtual bool Ligate
	{
		get
		{
			return m_ligate;
		}
		set
		{
			m_ligate = value;
		}
	}

	internal IndicGlyphInfo(OtfGlyphInfo glyph)
		: base(glyph)
	{
		Update();
	}

	internal IndicGlyphInfo(OtfGlyphInfo glyph, int category, int indicPos, int mask)
		: base(glyph)
	{
		Group = category;
		Position = indicPos;
		Mask = mask;
	}

	internal IndicGlyphInfo(OtfGlyphInfo glyph, int category, int position, int mask, bool substitute, bool ligate)
		: base(glyph)
	{
		m_group = category;
		m_position = position;
		m_mask = mask;
		m_substitute = substitute;
		m_ligate = ligate;
	}

	public override bool Equals(object obj)
	{
		if (!base.Equals(obj))
		{
			return false;
		}
		IndicGlyphInfo indicGlyphInfo = obj as IndicGlyphInfo;
		if (Group == indicGlyphInfo.Group && Position == indicGlyphInfo.Position && Mask == indicGlyphInfo.Mask && Substitute == indicGlyphInfo.Substitute && Ligate == indicGlyphInfo.Ligate)
		{
			return true;
		}
		return false;
	}

	internal void Update()
	{
		if (base.CharCode <= -1)
		{
			return;
		}
		int charCode = base.CharCode;
		IndicCharacterClassifier indicCharacterClassifier = new IndicCharacterClassifier();
		int @class = indicCharacterClassifier.GetClass(charCode);
		int num = @class & 0x7F;
		int num2 = @class >> 8;
		if ((charCode >= 2385 && charCode <= 2386) || (charCode >= 7376 && charCode <= 7378) || (charCode >= 7380 && charCode <= 7393) || charCode == 7412)
		{
			num = 10;
		}
		else if (charCode >= 2387 && charCode <= 2388)
		{
			num = 8;
		}
		else if ((charCode >= 2674 && charCode <= 2675) || (charCode >= 7413 && charCode <= 7414))
		{
			num = 1;
		}
		else if (charCode >= 7394 && charCode <= 7400)
		{
			num = 10;
		}
		else if (charCode == 7405)
		{
			num = 10;
		}
		else if ((charCode >= 43250 && charCode <= 43255) || (charCode >= 7401 && charCode <= 7404) || (charCode >= 7406 && charCode <= 7409))
		{
			num = 18;
		}
		else if ((charCode >= 6093 && charCode <= 6097) || charCode == 6091 || charCode == 6099 || charCode == 6109)
		{
			num = 7;
			num2 = 6;
		}
		else
		{
			switch (charCode)
			{
			case 6086:
				num = 3;
				break;
			case 6098:
				num = 14;
				break;
			case 8208:
			case 8209:
				num = 11;
				break;
			default:
				switch (charCode)
				{
				case 9676:
					num = 12;
					break;
				case 43394:
					num = 8;
					break;
				case 43454:
					num = 31;
					break;
				case 43453:
					num = 7;
					num2 = 11;
					break;
				}
				break;
			}
		}
		if (((1L << num) & 0x80013806u) != 0L)
		{
			num2 = 4;
			for (int i = 0; i < m_initialChars.Length; i++)
			{
				if (m_initialChars[i] == charCode)
				{
					num = 16;
					break;
				}
			}
		}
		else if (num == 7)
		{
			num2 = indicCharacterClassifier.GetPosition(charCode, num2);
		}
		else if (((1L << num) & 0x40700) != 0L)
		{
			num2 = 14;
		}
		if (charCode == 2817)
		{
			num2 = 7;
		}
		Group = num;
		Position = num2;
	}
}
