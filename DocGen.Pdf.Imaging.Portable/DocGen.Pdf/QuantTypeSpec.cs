namespace DocGen.Pdf;

internal class QuantTypeSpec : ModuleSpec
{
	public QuantTypeSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	public virtual bool isDerived(int t, int c)
	{
		if (((string)getTileCompVal(t, c)).Equals("derived"))
		{
			return true;
		}
		return false;
	}

	public virtual bool isReversible(int t, int c)
	{
		if (((string)getTileCompVal(t, c)).Equals("reversible"))
		{
			return true;
		}
		return false;
	}
}
