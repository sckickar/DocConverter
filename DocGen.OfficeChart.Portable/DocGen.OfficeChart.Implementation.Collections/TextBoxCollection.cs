using System;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class TextBoxCollection : CollectionBaseEx<ITextBoxShape>, ITextBoxes
{
	private WorksheetBaseImpl m_sheet;

	public new ITextBoxShape this[int index] => base.List[index];

	public ITextBoxShape this[string name]
	{
		get
		{
			ITextBoxShape result = null;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				ITextBoxShape textBoxShape = this[i];
				if (textBoxShape.Name == name)
				{
					result = textBoxShape;
					break;
				}
			}
			return result;
		}
	}

	public TextBoxCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_sheet = FindParent(typeof(WorksheetBaseImpl), bCheckSubclasses: true) as WorksheetBaseImpl;
		if (m_sheet == null)
		{
			throw new ArgumentOutOfRangeException("parent");
		}
	}

	public void AddTextBox(ITextBoxShape textbox)
	{
		if (textbox == null)
		{
			throw new ArgumentNullException("textbox");
		}
		base.Add(textbox);
	}

	public ITextBoxShape AddTextBox(int row, int column, int height, int width)
	{
		TextBoxShapeImpl obj = m_sheet.Shapes.AddTextBox() as TextBoxShapeImpl;
		MsofbtClientAnchor clientAnchor = obj.ClientAnchor;
		clientAnchor.LeftColumn = column - 1;
		clientAnchor.TopRow = row - 1;
		clientAnchor.RightColumn = column;
		clientAnchor.BottomRow = row;
		clientAnchor.LeftOffset = 0;
		clientAnchor.RightOffset = 0;
		clientAnchor.TopOffset = 0;
		clientAnchor.BottomOffset = 0;
		obj.Width = width;
		obj.Height = height;
		if (base.Parent is ChartImpl chartImpl)
		{
			ChartParentAxisImpl chartParentAxisImpl = ((chartImpl.PrimaryParentAxis != null) ? chartImpl.PrimaryParentAxis : chartImpl.SecondaryParentAxis);
			if (chartParentAxisImpl != null)
			{
				ChartAxisParentRecord parentAxisRecord = chartParentAxisImpl.ParentAxisRecord;
				if (parentAxisRecord.XAxisLength == 0)
				{
					parentAxisRecord.TopLeftX = 328;
					parentAxisRecord.TopLeftY = 243;
					parentAxisRecord.XAxisLength = 3125;
					parentAxisRecord.YAxisLength = 3283;
				}
			}
		}
		return obj;
	}
}
