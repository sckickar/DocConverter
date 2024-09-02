using System.Collections.Generic;

namespace DocGen.Pdf.Interactive;

public class Pdf3DCrossSectionCollection : List<Pdf3DCrossSection>
{
	public new Pdf3DCrossSection this[int index]
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

	public new int Add(Pdf3DCrossSection value)
	{
		base.Add(value);
		return base.IndexOf(value);
	}

	public new bool Contains(Pdf3DCrossSection value)
	{
		return base.Contains(value);
	}

	public new int IndexOf(Pdf3DCrossSection value)
	{
		return base.IndexOf(value);
	}

	public new void Insert(int index, Pdf3DCrossSection value)
	{
		base.Insert(index, value);
	}

	public new void Remove(Pdf3DCrossSection value)
	{
		base.Remove(value);
	}
}
