using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IPictureShape : IShape, IParentApplication
{
	string FileName { get; }

	Image Picture { get; set; }

	Stream SvgData { get; set; }

	void Remove(bool removeImage);
}
