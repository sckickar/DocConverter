namespace DocGen.Layouting;

internal interface ITableWidget : IWidget
{
	int MaxRowIndex { get; }

	int RowsCount { get; }

	IWidgetContainer GetCellWidget(int row, int column);

	IWidget GetRowWidget(int row);
}
