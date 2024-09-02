using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class FdfObject
{
	private int m_objNumber;

	private int m_genNumber;

	private IPdfPrimitive m_object;

	private IPdfPrimitive m_trailer;

	internal int ObjectNumber => m_objNumber;

	internal int GenerationNumber => m_genNumber;

	internal IPdfPrimitive Object => m_object;

	internal IPdfPrimitive Trailer => m_trailer;

	internal FdfObject(PdfNumber objNum, PdfNumber genNum, IPdfPrimitive obj)
	{
		m_objNumber = objNum.IntValue;
		m_genNumber = genNum.IntValue;
		m_object = obj;
	}

	internal FdfObject(IPdfPrimitive obj)
	{
		m_trailer = obj;
	}
}
