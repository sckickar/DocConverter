using System;
using System.Collections;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class PicturesCollection : CollectionBaseEx<IPictureShape>, IPictures, IParentApplication, IEnumerable
{
	private string DEF_PICTURE_NAME = "Picture";

	private string[] m_indexedpixel_notsupport = new string[5] { "Format1bppIndexed", "Format4bppIndexed", "Format8bppIndexed", "b96b3cac-0728-11d3-9d7b-0000f81ef32e", "b96b3cad-0728-11d3-9d7b-0000f81ef32e" };

	private WorksheetBaseImpl m_sheet;

	public IPictureShape this[string name]
	{
		get
		{
			IPictureShape result = null;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				IPictureShape pictureShape = base[i];
				if (pictureShape.Name == name)
				{
					result = pictureShape;
					break;
				}
			}
			return result;
		}
	}

	internal IPictureShape AddPicture(Image image, string pictureName, ExcelImageFormat imageFormat)
	{
		return m_sheet.Shapes.AddPicture(image, pictureName, imageFormat);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Image image)
	{
		return AddPicture(topRow, leftColumn, image, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Image image, ExcelImageFormat imageFormat)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		BitmapShapeImpl obj = (BitmapShapeImpl)AddPicture(image, GeneratePictureName(), imageFormat);
		obj.LeftColumn = leftColumn;
		obj.TopRow = topRow;
		obj.EvaluateTopLeftPosition();
		return obj;
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream stream)
	{
		return AddPicture(topRow, leftColumn, stream, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream svgStream, Stream imageStream)
	{
		BitmapShapeImpl obj = (BitmapShapeImpl)AddPicture(topRow, leftColumn, imageStream, ExcelImageFormat.Original);
		obj.SvgData = svgStream;
		return obj;
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream svgStream, Stream imageStream, int scaleWidth, int scaleHeight)
	{
		BitmapShapeImpl obj = (BitmapShapeImpl)AddPicture(topRow, leftColumn, imageStream, ExcelImageFormat.Original);
		obj.SvgData = svgStream;
		obj.Scale(scaleWidth, scaleHeight);
		return obj;
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, ExcelImageFormat imageFormat)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Image image = ApplicationImpl.CreateImage(stream);
		return AddPicture(topRow, leftColumn, image, imageFormat);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		return AddPicture(topRow, leftColumn, bottomRow, rightColumn, image, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Image image, ExcelImageFormat imageFormat)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		BitmapShapeImpl obj = (BitmapShapeImpl)AddPicture(topRow, leftColumn, image, imageFormat);
		obj.RightColumn = rightColumn;
		obj.BottomRow = bottomRow;
		obj.UpdateHeight();
		obj.UpdateWidth();
		obj.ClearShapeOffset(clear: true);
		return obj;
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Stream stream)
	{
		return AddPicture(topRow, leftColumn, bottomRow, rightColumn, stream, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, int bottomRow, int rightColumn, Stream stream, ExcelImageFormat imageFormat)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Image image = ApplicationImpl.CreateImage(stream);
		return AddPicture(topRow, leftColumn, bottomRow, rightColumn, image, imageFormat);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Image image, int scaleWidth, int scaleHeight)
	{
		return AddPicture(topRow, leftColumn, image, scaleWidth, scaleHeight, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Image image, int scaleWidth, int scaleHeight, ExcelImageFormat imageFormat)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		IPictureShape pictureShape = AddPicture(topRow, leftColumn, image, imageFormat);
		pictureShape.Scale(scaleWidth, scaleHeight);
		return pictureShape;
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, int scaleWidth, int scaleHeight)
	{
		return AddPicture(topRow, leftColumn, stream, scaleWidth, scaleHeight, ExcelImageFormat.Original);
	}

	public IPictureShape AddPicture(int topRow, int leftColumn, Stream stream, int scaleWidth, int scaleHeight, ExcelImageFormat imageFormat)
	{
		Image image = ApplicationImpl.CreateImage(stream);
		IPictureShape pictureShape = AddPicture(topRow, leftColumn, image, imageFormat);
		pictureShape.Scale(scaleWidth, scaleHeight);
		return pictureShape;
	}

	public PicturesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	internal void RemovePicture(IPictureShape picture)
	{
		base.InnerList.Remove(picture);
	}

	internal void AddPicture(IPictureShape picture)
	{
		base.InnerList.Add(picture);
	}

	private void SetParents()
	{
		m_sheet = FindParent(typeof(WorksheetBaseImpl), bCheckSubclasses: true) as WorksheetBaseImpl;
		if (m_sheet == null)
		{
			throw new ArgumentNullException("Can't find parent worksheet.");
		}
	}

	private string GeneratePictureName()
	{
		return CollectionBaseEx<IPictureShape>.GenerateDefaultName(this, DEF_PICTURE_NAME);
	}
}
