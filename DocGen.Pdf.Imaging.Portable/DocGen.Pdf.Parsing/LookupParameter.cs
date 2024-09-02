using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class LookupParameter
{
	private byte[] data;

	internal byte[] Data => data;

	public LookupParameter()
	{
	}

	public LookupParameter(PdfStream stream)
	{
		if (stream != null)
		{
			data = stream.Data;
		}
	}

	public LookupParameter(byte[] lookupData)
	{
		if (lookupData != null)
		{
			data = lookupData;
		}
	}

	internal void Load(PdfStream stream)
	{
		if (stream != null)
		{
			data = stream.Data;
		}
	}

	internal void Load(PdfString str)
	{
		if (str != null)
		{
			data = str.Bytes;
		}
	}

	internal void Load(PdfReferenceHolder indirectObject)
	{
		if (indirectObject.Object is PdfStream stream)
		{
			Load(stream);
		}
		else if (indirectObject.Object is PdfString str)
		{
			Load(str);
		}
	}
}
