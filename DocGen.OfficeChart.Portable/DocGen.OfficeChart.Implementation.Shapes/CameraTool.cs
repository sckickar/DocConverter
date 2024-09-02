namespace DocGen.OfficeChart.Implementation.Shapes;

internal class CameraTool
{
	private int m_shapeId;

	private string m_localName;

	private string m_cellRange;

	internal int ShapeID
	{
		get
		{
			return m_shapeId;
		}
		set
		{
			m_shapeId = value;
		}
	}

	internal string LocalName
	{
		get
		{
			return m_localName;
		}
		set
		{
			m_localName = value;
		}
	}

	internal string CellRange
	{
		get
		{
			return m_cellRange;
		}
		set
		{
			m_cellRange = value;
		}
	}

	internal CameraTool Clone(object parent)
	{
		return (CameraTool)MemberwiseClone();
	}
}
