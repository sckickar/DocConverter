using System;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser;

[CLSCompliant(false)]
internal class BaseProps : FileShapeAddress
{
	protected ShapeHorizontalAlignment m_horAlignment;

	protected ShapeVerticalAlignment m_vertAlignment;

	internal ShapeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horAlignment;
		}
		set
		{
			m_horAlignment = value;
		}
	}

	internal ShapeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_vertAlignment;
		}
		set
		{
			m_vertAlignment = value;
		}
	}

	internal BaseProps()
	{
	}
}
