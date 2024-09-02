namespace DocGen.Pdf;

internal class SynWTFilterSpec : ModuleSpec
{
	public SynWTFilterSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	public virtual int getWTDataType(int t, int c)
	{
		return ((SynWTFilter[][])getSpec(t, c))[0][0].DataType;
	}

	public virtual SynWTFilter[] getHFilters(int t, int c)
	{
		return ((SynWTFilter[][])getSpec(t, c))[0];
	}

	public virtual SynWTFilter[] getVFilters(int t, int c)
	{
		return ((SynWTFilter[][])getSpec(t, c))[1];
	}

	public override string ToString()
	{
		string text = "";
		text = text + "nTiles=" + nTiles + "\nnComp=" + nComp + "\n\n";
		for (int i = 0; i < nTiles; i++)
		{
			for (int j = 0; j < nComp; j++)
			{
				SynWTFilter[][] array = (SynWTFilter[][])getSpec(i, j);
				text = text + "(t:" + i + ",c:" + j + ")\n";
				text += "\tH:";
				for (int k = 0; k < array[0].Length; k++)
				{
					text = text + " " + array[0][k];
				}
				text += "\n\tV:";
				for (int l = 0; l < array[1].Length; l++)
				{
					text = text + " " + array[1][l];
				}
				text += "\n";
			}
		}
		return text;
	}

	public virtual bool isReversible(int t, int c)
	{
		SynWTFilter[] hFilters = getHFilters(t, c);
		SynWTFilter[] vFilters = getVFilters(t, c);
		for (int num = hFilters.Length - 1; num >= 0; num--)
		{
			if (!hFilters[num].Reversible || !vFilters[num].Reversible)
			{
				return false;
			}
		}
		return true;
	}
}
