using DocGen.Drawing;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Redaction;

internal class PdfElementsRendererNet
{
	internal static DocGen.Drawing.Matrix GetTransformationMatrix(DocGen.PdfViewer.Base.Matrix transform)
	{
		return new DocGen.Drawing.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}
}
