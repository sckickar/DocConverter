namespace DocGen.Pdf.Parsing;

internal class SystemFontTop : SystemFontDict, ISystemFontBuildCharHolder
{
	private ISystemFontEncoding encoding;

	private SystemFontCharString charString;

	private SystemFontCharset charset;

	private string familyName;

	private SystemFontMatrix? fontMatrix;

	private SystemFontPrivate priv;

	private int? charstringType;

	private int? defaultWidthX;

	private int? nominalWidthX;

	private bool? usesCIDFontOperators;

	public static SystemFontOperatorDescriptor FamilyNameOperator { get; private set; }

	public static SystemFontOperatorDescriptor WeightOperator { get; private set; }

	public static SystemFontOperatorDescriptor EncodingOperator { get; private set; }

	public static SystemFontOperatorDescriptor CharStringsOperator { get; private set; }

	public static SystemFontOperatorDescriptor ItalicAngleOperator { get; private set; }

	public static SystemFontOperatorDescriptor CharstringTypeOperator { get; private set; }

	public static SystemFontOperatorDescriptor CharsetOperator { get; private set; }

	public static SystemFontOperatorDescriptor FontMatrixOperator { get; private set; }

	public static SystemFontOperatorDescriptor PrivateOperator { get; private set; }

	public static SystemFontOperatorDescriptor DefaultWidthXOperator { get; private set; }

	public static SystemFontOperatorDescriptor NominalWidthXOperator { get; private set; }

	public static SystemFontOperatorDescriptor ROSOperator { get; private set; }

	public static SystemFontOperatorDescriptor FDArrayOperator { get; private set; }

	public static SystemFontOperatorDescriptor FDSelectOperator { get; private set; }

	public int CharstringType
	{
		get
		{
			if (!charstringType.HasValue)
			{
				charstringType = GetInt(CharstringTypeOperator);
			}
			return charstringType.Value;
		}
	}

	public ISystemFontEncoding Encoding
	{
		get
		{
			if (encoding == null)
			{
				ReadEncoding();
			}
			return encoding;
		}
	}

	public SystemFontCharset Charset
	{
		get
		{
			if (charset == null)
			{
				ReadCharset();
			}
			return charset;
		}
	}

	public SystemFontCharString CharString
	{
		get
		{
			if (charString == null)
			{
				ReadCharString();
			}
			return charString;
		}
	}

	public string FamilyName
	{
		get
		{
			if (familyName == null)
			{
				familyName = base.File.ReadString((ushort)GetInt(FamilyNameOperator));
			}
			return familyName;
		}
	}

	public SystemFontMatrix FontMatrix
	{
		get
		{
			if (!fontMatrix.HasValue)
			{
				fontMatrix = GetArray(FontMatrixOperator).ToMatrix();
			}
			return fontMatrix.Value;
		}
	}

	public SystemFontPrivate Private
	{
		get
		{
			if (priv == null)
			{
				ReadPrivate();
			}
			return priv;
		}
	}

	public int DefaultWidthX
	{
		get
		{
			if (!defaultWidthX.HasValue)
			{
				defaultWidthX = GetInt(DefaultWidthXOperator);
			}
			return defaultWidthX.Value;
		}
	}

	public int NominalWidthX
	{
		get
		{
			if (!nominalWidthX.HasValue)
			{
				nominalWidthX = GetInt(NominalWidthXOperator);
			}
			return nominalWidthX.Value;
		}
	}

	public bool UsesCIDFontOperators
	{
		get
		{
			if (!usesCIDFontOperators.HasValue)
			{
				usesCIDFontOperators = base.Data.ContainsKey(ROSOperator);
			}
			return usesCIDFontOperators.Value;
		}
	}

	static SystemFontTop()
	{
		FontMatrixOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 7), new SystemFontPostScriptArray(new object[6] { 0.001, 0, 0, 0.001, 0, 0 }));
		FamilyNameOperator = new SystemFontOperatorDescriptor(3);
		WeightOperator = new SystemFontOperatorDescriptor(4);
		ItalicAngleOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 2), 0);
		CharstringTypeOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 6), 2);
		CharsetOperator = new SystemFontOperatorDescriptor(15, 0);
		EncodingOperator = new SystemFontOperatorDescriptor(16, 0);
		CharStringsOperator = new SystemFontOperatorDescriptor(17);
		PrivateOperator = new SystemFontOperatorDescriptor(18);
		DefaultWidthXOperator = new SystemFontOperatorDescriptor(20, 0);
		NominalWidthXOperator = new SystemFontOperatorDescriptor(21, 0);
		ROSOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 30));
		FDArrayOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 36));
		FDSelectOperator = new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 37));
	}

	public SystemFontTop(SystemFontCFFFontFile file, long offset, int length)
		: base(file, offset, length)
	{
	}

	public byte[] GetSubr(int index)
	{
		return Private.Subrs[index];
	}

	public byte[] GetGlobalSubr(int index)
	{
		return base.File.GlobalSubrs[index];
	}

	public SystemFontType1GlyphData GetGlyphData(string name)
	{
		return CharString[GetGlyphId(name)];
	}

	public ushort GetGlyphId(string name)
	{
		return Charset[name];
	}

	public ushort GetGlyphId(ushort cid)
	{
		return GetGlyphId(base.File.ReadString(cid));
	}

	internal string GetGlyphName(ushort cid)
	{
		return Encoding.GetGlyphName(base.File, cid);
	}

	public ushort GetAdvancedWidth(ushort glyphId)
	{
		return (ushort)CharString.GetAdvancedWidth(glyphId, DefaultWidthX, NominalWidthX);
	}

	public void GetGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
		CharString.GetGlyphOutlines(glyph, fontSize);
	}

	private void ReadEncoding()
	{
		int @int = GetInt(EncodingOperator);
		if (SystemFontCFFPredefinedEncoding.IsPredefinedEncoding(@int))
		{
			encoding = SystemFontCFFPredefinedEncoding.GetPredefinedEncoding(@int);
			return;
		}
		SystemFontEncoding table = new SystemFontEncoding(base.File, Charset, @int);
		base.File.ReadTable(table);
		encoding = table;
	}

	private void ReadPrivate()
	{
		SystemFontOperandsCollection operands = GetOperands(PrivateOperator);
		priv = new SystemFontPrivate(this, operands.GetLastAsInt(), operands.GetLastAsInt());
		base.File.ReadTable(priv);
	}

	private void ReadCharset()
	{
		int @int = GetInt(CharsetOperator);
		if (SystemFontPredefinedCharset.IsPredefinedCharset(@int))
		{
			charset = new SystemFontCharset(base.File, SystemFontPredefinedCharset.GetPredefinedCodes(@int));
			return;
		}
		charset = new SystemFontCharset(base.File, @int, CharString.Count);
		base.File.ReadTable(charset);
	}

	private void ReadCharString()
	{
		charString = new SystemFontCharString(this, GetInt(CharStringsOperator));
		base.File.ReadTable(charString);
	}
}
