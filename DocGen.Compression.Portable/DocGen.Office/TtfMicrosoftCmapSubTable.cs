namespace DocGen.Office;

internal struct TtfMicrosoftCmapSubTable
{
	public ushort Format;

	public ushort Length;

	public ushort Version;

	public ushort SegCountX2;

	public ushort SearchRange;

	public ushort EntrySelector;

	public ushort RangeShift;

	public ushort[] EndCount;

	public ushort ReservedPad;

	public ushort[] StartCount;

	public ushort[] IdDelta;

	public ushort[] IdRangeOffset;

	public ushort[] GlyphID;
}
