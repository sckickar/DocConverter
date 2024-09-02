using System;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

[CLSCompliant(false)]
internal class ShapeBase
{
	protected BaseProps m_shapeProps;

	internal BaseProps ShapeProps => m_shapeProps;

	internal ShapeBase()
	{
		CreateShapeImpl();
	}

	internal virtual void Close()
	{
		m_shapeProps = null;
	}

	protected internal virtual void CreateShapeImpl()
	{
		throw new NotImplementedException("Not implemented");
	}
}
