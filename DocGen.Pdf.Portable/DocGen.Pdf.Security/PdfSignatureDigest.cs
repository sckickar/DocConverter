using System.Collections.Generic;
using DocGen.Pdf.Native;

namespace DocGen.Pdf.Security;

internal class PdfSignatureDigest
{
	private List<object> m_objList;

	private PdfPrimitiveId m_id;

	internal PdfSignatureDigest()
	{
		m_objList = new List<object>();
		m_id = default(PdfPrimitiveId);
	}
}
