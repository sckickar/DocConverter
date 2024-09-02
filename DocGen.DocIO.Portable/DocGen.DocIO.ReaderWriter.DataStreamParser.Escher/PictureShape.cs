using System;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

[CLSCompliant(false)]
internal class PictureShape : ShapeBase
{
	private ImageRecord m_imageRecord;

	internal ImageRecord ImageRecord => m_imageRecord;

	internal PictureShapeProps PictureProps => base.ShapeProps as PictureShapeProps;

	internal PictureShape(ImageRecord imageRecord)
	{
		m_imageRecord = imageRecord;
	}

	internal PictureShape()
	{
	}

	protected internal override void CreateShapeImpl()
	{
		m_shapeProps = new PictureShapeProps();
	}
}
