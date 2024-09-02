using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class EndCellLayoutEventArgs : CellLayoutEventArgs
{
	internal EndCellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellInder, RectangleF bounds, string value)
		: base(graphics, rowIndex, cellInder, bounds, value)
	{
	}
}
