using System.Collections;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IPictures : IParentApplication, IEnumerable
{
	int Count { get; }

	IPictureShape this[int Index] { get; }

	IPictureShape this[string name] { get; }

	IPictureShape AddPicture(int topRow, int leftColumn, Image image);

	IPictureShape AddPicture(int topRow, int leftColumn, Image image, ExcelImageFormat imageFormat);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream stream);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream svgStream, Stream imageStream);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream svgStream, Stream imageStream, int scaleWidth, int scaleHeight);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, ExcelImageFormat imageFormat);

	IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Image image);

	IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Image image, ExcelImageFormat imageFormat);

	IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Stream stream);

	IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Stream stream, ExcelImageFormat imageFormat);

	IPictureShape AddPicture(int topRow, int leftColumn, Image image, int scaleWidth, int scaleHeight);

	IPictureShape AddPicture(int topRow, int leftColumn, Image image, int scaleWidth, int scaleHeight, ExcelImageFormat imageFormat);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, int scaleWidth, int scaleHeight);

	IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, int scaleWidth, int scaleHeight, ExcelImageFormat imageFormat);
}
