using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

internal interface IPictureRecord
{
	Image Picture { get; set; }

	Stream PictureStream { get; set; }

	byte[] RgbUid { get; }
}
