using System.Collections.Generic;

namespace DocGen.Pdf.Interactive;

public class Pdf3DNodeCollection : List<Pdf3DNode>
{
	public new Pdf3DNode this[int index]
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

	public new int Add(Pdf3DNode value)
	{
		base.Add(value);
		return base.IndexOf(value);
	}

	public new bool Contains(Pdf3DNode value)
	{
		return base.Contains(value);
	}

	public new int IndexOf(Pdf3DNode value)
	{
		return base.IndexOf(value);
	}

	public new void Insert(int index, Pdf3DNode value)
	{
		base.Insert(index, value);
	}

	public new void Remove(Pdf3DNode value)
	{
		base.Remove(value);
	}
}
