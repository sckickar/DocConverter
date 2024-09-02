using System;
using DocGen.DocIO.Rendering;

namespace DocGen.Layouting;

internal class SplitTableWidget : IWidget
{
	private ITableWidget m_tableWidget;

	private int m_rowNumber;

	private int m_colNumber;

	private SplitWidgetContainer[] m_splittedCells;

	public ITableWidget TableWidget => m_tableWidget;

	public int StartRowNumber => m_rowNumber;

	public int StartColumnNumber => m_colNumber;

	public SplitWidgetContainer[] SplittedCells => m_splittedCells;

	public ILayoutInfo LayoutInfo => null;

	public SplitTableWidget(ITableWidget tableWidget, int rowNumber)
		: this(tableWidget, rowNumber, 0)
	{
	}

	public SplitTableWidget(ITableWidget tableWidget, int rowNumber, SplitWidgetContainer[] splittedCells)
		: this(tableWidget, rowNumber, 0)
	{
		m_splittedCells = splittedCells;
	}

	public SplitTableWidget(ITableWidget tableWidget, int rowNumber, int colNumber)
	{
		m_tableWidget = tableWidget;
		m_rowNumber = rowNumber;
		m_colNumber = colNumber;
	}

	public void Draw(DrawingContext dc, LayoutedWidget layoutedWidget)
	{
		throw new NotImplementedException();
	}

	public void InitLayoutInfo()
	{
	}

	public void InitLayoutInfo(IWidget widget)
	{
	}
}
