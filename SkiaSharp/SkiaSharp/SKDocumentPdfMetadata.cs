using System;

namespace SkiaSharp;

public struct SKDocumentPdfMetadata : IEquatable<SKDocumentPdfMetadata>
{
	public const float DefaultRasterDpi = 72f;

	public const int DefaultEncodingQuality = 101;

	public static readonly SKDocumentPdfMetadata Default;

	public string Title { get; set; }

	public string Author { get; set; }

	public string Subject { get; set; }

	public string Keywords { get; set; }

	public string Creator { get; set; }

	public string Producer { get; set; }

	public DateTime? Creation { get; set; }

	public DateTime? Modified { get; set; }

	public float RasterDpi { get; set; }

	public bool PdfA { get; set; }

	public int EncodingQuality { get; set; }

	static SKDocumentPdfMetadata()
	{
		Default = new SKDocumentPdfMetadata
		{
			RasterDpi = 72f,
			PdfA = false,
			EncodingQuality = 101
		};
	}

	public SKDocumentPdfMetadata(float rasterDpi)
	{
		Title = null;
		Author = null;
		Subject = null;
		Keywords = null;
		Creator = null;
		Producer = null;
		Creation = null;
		Modified = null;
		RasterDpi = rasterDpi;
		PdfA = false;
		EncodingQuality = 101;
	}

	public SKDocumentPdfMetadata(int encodingQuality)
	{
		Title = null;
		Author = null;
		Subject = null;
		Keywords = null;
		Creator = null;
		Producer = null;
		Creation = null;
		Modified = null;
		RasterDpi = 72f;
		PdfA = false;
		EncodingQuality = encodingQuality;
	}

	public SKDocumentPdfMetadata(float rasterDpi, int encodingQuality)
	{
		Title = null;
		Author = null;
		Subject = null;
		Keywords = null;
		Creator = null;
		Producer = null;
		Creation = null;
		Modified = null;
		RasterDpi = rasterDpi;
		PdfA = false;
		EncodingQuality = encodingQuality;
	}

	public readonly bool Equals(SKDocumentPdfMetadata obj)
	{
		if (Title == obj.Title && Author == obj.Author && Subject == obj.Subject && Keywords == obj.Keywords && Creator == obj.Creator && Producer == obj.Producer && Creation == obj.Creation)
		{
			DateTime? modified = Modified;
			DateTime? modified2 = obj.Modified;
			if (modified.HasValue == modified2.HasValue && (!modified.HasValue || modified.GetValueOrDefault() == modified2.GetValueOrDefault()) && RasterDpi == obj.RasterDpi && PdfA == obj.PdfA)
			{
				return EncodingQuality == obj.EncodingQuality;
			}
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKDocumentPdfMetadata obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKDocumentPdfMetadata left, SKDocumentPdfMetadata right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKDocumentPdfMetadata left, SKDocumentPdfMetadata right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Title);
		hashCode.Add(Author);
		hashCode.Add(Subject);
		hashCode.Add(Keywords);
		hashCode.Add(Creator);
		hashCode.Add(Producer);
		hashCode.Add(Creation);
		hashCode.Add(Modified);
		hashCode.Add(RasterDpi);
		hashCode.Add(PdfA);
		hashCode.Add(EncodingQuality);
		return hashCode.ToHashCode();
	}
}
