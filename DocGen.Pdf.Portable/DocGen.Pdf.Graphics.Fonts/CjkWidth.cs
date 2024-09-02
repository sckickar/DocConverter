using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal abstract class CjkWidth : ICloneable
{
	internal abstract int From { get; }

	internal abstract int To { get; }

	internal abstract int this[int index] { get; }

	internal abstract void AppendToArray(PdfArray arr);

	object ICloneable.Clone()
	{
		return Clone();
	}

	public abstract CjkWidth Clone();
}
