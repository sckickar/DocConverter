using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal abstract class WidthTable : ICloneable
{
	public abstract float this[int index] { get; }

	public abstract WidthTable Clone();

	object ICloneable.Clone()
	{
		return Clone();
	}

	internal abstract PdfArray ToArray();
}
