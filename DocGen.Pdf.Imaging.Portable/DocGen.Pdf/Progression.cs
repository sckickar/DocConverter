namespace DocGen.Pdf;

internal class Progression
{
	public int type;

	public int cs;

	public int ce;

	public int rs;

	public int re;

	public int lye;

	public Progression(int type, int cs, int ce, int rs, int re, int lye)
	{
		this.type = type;
		this.cs = cs;
		this.ce = ce;
		this.rs = rs;
		this.re = re;
		this.lye = lye;
	}

	public override string ToString()
	{
		string text = "type= ";
		switch (type)
		{
		case 0:
			text += "layer, ";
			break;
		case 1:
			text += "res, ";
			break;
		case 2:
			text += "res-pos, ";
			break;
		case 3:
			text += "pos-comp, ";
			break;
		case 4:
			text += "pos-comp, ";
			break;
		}
		text = text + "comp.: " + cs + "-" + ce + ", ";
		text = text + "res.: " + rs + "-" + re + ", ";
		return text + "layer: up to " + lye;
	}
}
