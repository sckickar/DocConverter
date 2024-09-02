namespace DocGen.Office;

internal abstract class SubsetTable
{
	internal abstract int Length { get; }

	internal abstract LookupSubTableRecord[] LookupRecord { get; }

	internal virtual int LookupLength => 0;

	internal virtual int BTCLength => 0;

	internal abstract bool Match(int id, int index);

	internal virtual bool IsLookup(int glyphId, int index)
	{
		return false;
	}

	internal virtual bool IsBackTrack(int glyphId, int index)
	{
		return false;
	}
}
