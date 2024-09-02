namespace DocGen.Pdf;

internal class CompTransfSpec : ModuleSpec
{
	public virtual bool CompTransfUsed
	{
		get
		{
			if ((int)def != 0)
			{
				return true;
			}
			if (tileDef != null)
			{
				for (int num = nTiles - 1; num >= 0; num--)
				{
					if (tileDef[num] != null && (int)tileDef[num] != 0)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public CompTransfSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}
}
