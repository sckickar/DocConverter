namespace DocGen.Chart;

internal struct Unit
{
	public static readonly Unit Empty;

	private readonly UnitType type;

	private readonly double value;

	public bool IsEmpty => type == (UnitType)0;

	public UnitType Type
	{
		get
		{
			if (!IsEmpty)
			{
				return type;
			}
			return UnitType.Pixel;
		}
	}

	public double Value => value;

	public Unit(double value, UnitType type)
	{
		this.value = ((type == UnitType.Pixel) ? ((double)(int)value) : value);
		this.type = type;
	}
}
