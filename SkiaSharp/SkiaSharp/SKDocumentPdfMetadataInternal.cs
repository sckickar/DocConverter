using System;

namespace SkiaSharp;

internal struct SKDocumentPdfMetadataInternal : IEquatable<SKDocumentPdfMetadataInternal>
{
	public IntPtr fTitle;

	public IntPtr fAuthor;

	public IntPtr fSubject;

	public IntPtr fKeywords;

	public IntPtr fCreator;

	public IntPtr fProducer;

	public unsafe SKTimeDateTimeInternal* fCreation;

	public unsafe SKTimeDateTimeInternal* fModified;

	public float fRasterDPI;

	public byte fPDFA;

	public int fEncodingQuality;

	public unsafe readonly bool Equals(SKDocumentPdfMetadataInternal obj)
	{
		if (fTitle == obj.fTitle && fAuthor == obj.fAuthor && fSubject == obj.fSubject && fKeywords == obj.fKeywords && fCreator == obj.fCreator && fProducer == obj.fProducer && fCreation == obj.fCreation && fModified == obj.fModified && fRasterDPI == obj.fRasterDPI && fPDFA == obj.fPDFA)
		{
			return fEncodingQuality == obj.fEncodingQuality;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKDocumentPdfMetadataInternal obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fTitle);
		hashCode.Add(fAuthor);
		hashCode.Add(fSubject);
		hashCode.Add(fKeywords);
		hashCode.Add(fCreator);
		hashCode.Add(fProducer);
		hashCode.Add((void*)fCreation);
		hashCode.Add((void*)fModified);
		hashCode.Add(fRasterDPI);
		hashCode.Add(fPDFA);
		hashCode.Add(fEncodingQuality);
		return hashCode.ToHashCode();
	}
}
