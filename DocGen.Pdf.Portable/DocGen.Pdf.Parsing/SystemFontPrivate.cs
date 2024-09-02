namespace DocGen.Pdf.Parsing;

internal class SystemFontPrivate : SystemFontDict
{
	private readonly SystemFontTop top;

	private SystemFontSubrsIndex subrs;

	public static SystemFontOperatorDescriptor SubrsOperator { get; private set; }

	public SystemFontSubrsIndex Subrs
	{
		get
		{
			if (subrs == null)
			{
				subrs = new SystemFontSubrsIndex(base.File, top.CharstringType, base.Offset + GetInt(SubrsOperator));
				base.File.ReadTable(subrs);
			}
			return subrs;
		}
	}

	static SystemFontPrivate()
	{
		SubrsOperator = new SystemFontOperatorDescriptor(19);
	}

	public SystemFontPrivate(SystemFontTop top, long offset, int length)
		: base(top.File, offset, length)
	{
		this.top = top;
	}
}
