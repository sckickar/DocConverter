using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedMatrixWidget : LayoutedFuntionWidget
{
	private List<List<LayoutedOMathWidget>> rows;

	internal List<List<LayoutedOMathWidget>> Rows
	{
		get
		{
			return rows;
		}
		set
		{
			rows = value;
		}
	}

	internal LayoutedMatrixWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedMatrixWidget(LayoutedMatrixWidget srcWidget)
		: base(srcWidget)
	{
		List<List<LayoutedOMathWidget>> list = new List<List<LayoutedOMathWidget>>();
		for (int i = 0; i < srcWidget.Rows.Count; i++)
		{
			List<LayoutedOMathWidget> list2 = new List<LayoutedOMathWidget>();
			for (int j = 0; j < srcWidget.Rows[i].Count; j++)
			{
				list2.Add(new LayoutedOMathWidget(srcWidget.Rows[i][j]));
			}
			list.Add(list2);
		}
		Rows = list;
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		for (int i = 0; i < Rows.Count; i++)
		{
			for (int j = 0; j < Rows[i].Count; j++)
			{
				Rows[i][j].ShiftXYPosition(xPosition, yPosition);
			}
		}
	}

	public override void Dispose()
	{
		if (rows != null)
		{
			for (int i = 0; i < rows.Count; i++)
			{
				for (int j = 0; j < rows[i].Count; j++)
				{
					rows[i][j].Dispose();
					rows[i][j] = null;
				}
				rows[i].Clear();
			}
			rows.Clear();
			rows = null;
		}
		base.Dispose();
	}
}
