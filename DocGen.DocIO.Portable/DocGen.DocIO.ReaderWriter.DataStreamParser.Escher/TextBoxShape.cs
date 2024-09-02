using System;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

[CLSCompliant(false)]
internal class TextBoxShape : ShapeBase
{
	internal TextBoxProps TextBoxProps => base.ShapeProps as TextBoxProps;

	internal TextBoxShape()
	{
	}

	protected internal override void CreateShapeImpl()
	{
		m_shapeProps = new TextBoxProps();
	}
}
