using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal struct Pair
{
	public static readonly Pair Empty = new Pair(null, null);

	public PdfName Name;

	public IPdfPrimitive Value;

	public static bool operator ==(Pair pair, object obj)
	{
		bool flag = false;
		if (obj == null || !(obj is Pair pair2))
		{
			return false;
		}
		return pair2.Name == pair.Name && pair.Value == pair2.Value;
	}

	public static bool operator !=(Pair pair, object obj)
	{
		return !(pair == obj);
	}

	public Pair(PdfName name, IPdfPrimitive value)
	{
		Name = name;
		Value = value;
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
