namespace DocGen.Layouting;

internal interface ITableLayoutInfo : ILayoutSpacingsInfo
{
	float Width { get; set; }

	float Height { get; }

	float[] CellsWidth { get; set; }

	int HeadersRowCount { get; }

	bool[] IsDefaultCells { get; }

	bool IsSplittedTable { get; set; }

	double CellSpacings { get; }

	double CellPaddings { get; }
}
