namespace DocGen.Office;

internal struct TtfNameTable
{
	public ushort FormatSelector;

	public ushort RecordsCount;

	public ushort Offset;

	public TtfNameRecord[] NameRecords;
}
