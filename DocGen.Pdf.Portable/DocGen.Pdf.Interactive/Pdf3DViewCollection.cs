using System.Collections.Generic;

namespace DocGen.Pdf.Interactive;

public class Pdf3DViewCollection : List<Pdf3DView>
{
	public new Pdf3DView this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = value;
		}
	}

	public new int Add(Pdf3DView value)
	{
		base.Add(value);
		return base.IndexOf(value);
	}

	public new bool Contains(Pdf3DView value)
	{
		return base.Contains(value);
	}

	public new int IndexOf(Pdf3DView value)
	{
		return base.IndexOf(value);
	}

	public new void Insert(int index, Pdf3DView value)
	{
		base.Insert(index, value);
	}

	public new void Remove(Pdf3DView value)
	{
		base.Remove(value);
	}
}
