using System;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class Borders : FormatBase
{
	public const int LeftKey = 1;

	public const int TopKey = 2;

	public const int BottomKey = 3;

	public const int RightKey = 4;

	public const int VerticalKey = 5;

	public const int HorizontalKey = 6;

	public const int DiagonalDownKey = 7;

	public const int DiagonalUpKey = 8;

	private WTableCell m_currTableCell;

	private WTableRow m_currTableRow;

	public bool NoBorder
	{
		get
		{
			if (Left.BorderType == BorderStyle.None && Right.BorderType == BorderStyle.None && Top.BorderType == BorderStyle.None && Bottom.BorderType == BorderStyle.None)
			{
				return Horizontal.BorderType == BorderStyle.None;
			}
			return false;
		}
	}

	internal bool IsCellHasNoBorder
	{
		get
		{
			if (Left.BorderType == BorderStyle.None && Right.BorderType == BorderStyle.None && Top.BorderType == BorderStyle.None && Bottom.BorderType == BorderStyle.None && DiagonalDown.BorderType == BorderStyle.None)
			{
				return DiagonalUp.BorderType == BorderStyle.None;
			}
			return false;
		}
	}

	public Border Left => base[1] as Border;

	public Border Top => base[2] as Border;

	public Border Right => base[4] as Border;

	public Border Bottom => base[3] as Border;

	public Border Vertical => base[5] as Border;

	public Border Horizontal => base[6] as Border;

	internal Border DiagonalDown => base[7] as Border;

	internal Border DiagonalUp => base[8] as Border;

	public Color Color
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			Color color2 = (Bottom.Color = value);
			Color color4 = (top.Color = color2);
			Color color6 = (right.Color = color4);
			left.Color = color6;
		}
	}

	public float LineWidth
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			float num2 = (Bottom.LineWidth = value);
			float num4 = (top.LineWidth = num2);
			float lineWidth = (right.LineWidth = num4);
			left.LineWidth = lineWidth;
		}
	}

	public BorderStyle BorderType
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			BorderStyle borderStyle2 = (Bottom.BorderType = value);
			BorderStyle borderStyle4 = (top.BorderType = borderStyle2);
			BorderStyle borderType = (right.BorderType = borderStyle4);
			left.BorderType = borderType;
			Border vertical = Vertical;
			borderType = (Horizontal.BorderType = value);
			vertical.BorderType = borderType;
		}
	}

	public float Space
	{
		set
		{
			SetSpacing(value);
		}
	}

	public bool Shadow
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			bool flag2 = (Bottom.Shadow = value);
			bool flag4 = (top.Shadow = flag2);
			bool shadow = (right.Shadow = flag4);
			left.Shadow = shadow;
		}
	}

	internal WTableCell CurrentCell
	{
		get
		{
			if (m_currTableCell == null && base.OwnerBase != null && base.OwnerBase is CellFormat)
			{
				CellFormat cellFormat = base.OwnerBase as CellFormat;
				if (cellFormat.OwnerBase != null)
				{
					m_currTableCell = cellFormat.OwnerBase as WTableCell;
				}
			}
			return m_currTableCell;
		}
	}

	internal WTableRow CurrentRow
	{
		get
		{
			if (m_currTableRow == null)
			{
				if (CurrentCell != null)
				{
					m_currTableRow = CurrentCell.OwnerRow;
				}
				else if (base.OwnerBase != null && base.OwnerBase is WTableRow)
				{
					return null;
				}
			}
			return m_currTableRow;
		}
	}

	internal bool IsHTMLRead
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			bool flag2 = (Bottom.IsHTMLRead = value);
			bool flag4 = (top.IsHTMLRead = flag2);
			bool isHTMLRead = (right.IsHTMLRead = flag4);
			left.IsHTMLRead = isHTMLRead;
			Border vertical = Vertical;
			isHTMLRead = (Horizontal.IsHTMLRead = value);
			vertical.IsHTMLRead = isHTMLRead;
		}
	}

	internal bool IsRead
	{
		set
		{
			Border left = Left;
			Border right = Right;
			Border top = Top;
			bool flag2 = (Bottom.IsRead = value);
			bool flag4 = (top.IsRead = flag2);
			bool isRead = (right.IsRead = flag4);
			left.IsRead = isRead;
		}
	}

	internal Borders(FormatBase parent, int baseKey)
		: base(parent, baseKey)
	{
		InitBorders();
	}

	public Borders()
	{
		InitBorders();
	}

	internal Borders(Borders borders)
	{
		ImportContainer(borders);
		InitBorders();
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(1, 4, 2, 3, 5, 6, 7, 8);
	}

	protected override object GetDefValue(int key)
	{
		throw new ArgumentException("key has invalid value");
	}

	protected override FormatBase GetDefComposite(int key)
	{
		return key switch
		{
			1 => GetDefComposite(1, new Border(this, 1)), 
			2 => GetDefComposite(2, new Border(this, 2)), 
			4 => GetDefComposite(4, new Border(this, 4)), 
			3 => GetDefComposite(3, new Border(this, 3)), 
			5 => GetDefComposite(5, new Border(this, 5)), 
			6 => GetDefComposite(6, new Border(this, 6)), 
			7 => GetDefComposite(7, new Border(this, 7)), 
			8 => GetDefComposite(8, new Border(this, 8)), 
			_ => null, 
		};
	}

	protected override void InitXDLSHolder()
	{
		if (base.IsDefault)
		{
			base.XDLSHolder.SkipMe = true;
		}
		base.XDLSHolder.AddElement("Bottom", Bottom);
		base.XDLSHolder.AddElement("Top", Top);
		base.XDLSHolder.AddElement("Left", Left);
		base.XDLSHolder.AddElement("Right", Right);
		base.XDLSHolder.AddElement("Horizontal", Horizontal);
		base.XDLSHolder.AddElement("Vertical", Vertical);
	}

	public Borders Clone()
	{
		return (Borders)CloneImpl();
	}

	protected override object CloneImpl()
	{
		return new Borders(this);
	}

	protected override void OnChange(FormatBase format, int propertyKey)
	{
		base.OnChange(format, propertyKey);
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		base.ApplyBase(baseFormat);
		if (baseFormat == null)
		{
			Left.ApplyBase(null);
			Right.ApplyBase(null);
			Top.ApplyBase(null);
			Bottom.ApplyBase(null);
			Horizontal.ApplyBase(null);
			Vertical.ApplyBase(null);
			DiagonalDown.ApplyBase(null);
			DiagonalUp.ApplyBase(null);
		}
		else
		{
			Left.ApplyBase((baseFormat as Borders).Left);
			Right.ApplyBase((baseFormat as Borders).Right);
			Top.ApplyBase((baseFormat as Borders).Top);
			Bottom.ApplyBase((baseFormat as Borders).Bottom);
			Horizontal.ApplyBase((baseFormat as Borders).Horizontal);
			Vertical.ApplyBase((baseFormat as Borders).Vertical);
			DiagonalDown.ApplyBase((baseFormat as Borders).DiagonalDown);
			DiagonalUp.ApplyBase((baseFormat as Borders).DiagonalUp);
		}
	}

	internal void SetDefaultProperties()
	{
		Top.SetDefaultProperties();
		Left.SetDefaultProperties();
		Bottom.SetDefaultProperties();
		Right.SetDefaultProperties();
	}

	private void SetSpacing(float value)
	{
		if (base.ParentFormat is RowFormat || base.ParentFormat is CellFormat)
		{
			if (base.ParentFormat is RowFormat)
			{
				(base.ParentFormat as RowFormat).Paddings.All = value;
			}
			else
			{
				(base.ParentFormat as CellFormat).Paddings.All = value;
			}
		}
		else if (base.ParentFormat is WParagraphFormat)
		{
			Left.Space = value;
			Right.Space = value;
			Top.Space = value;
			Bottom.Space = value;
		}
	}

	internal bool IsAdjacentBorderSame(Border currentParagraphBorder, Border nextParagraphBorder)
	{
		if (currentParagraphBorder.BorderType == nextParagraphBorder.BorderType && currentParagraphBorder.LineWidth == nextParagraphBorder.LineWidth && currentParagraphBorder.Color == nextParagraphBorder.Color && currentParagraphBorder.Space == nextParagraphBorder.Space)
		{
			return currentParagraphBorder.Shadow == nextParagraphBorder.Shadow;
		}
		return false;
	}

	private void InitBorders()
	{
		Left.SetOwner(this);
		Left.BorderPosition = Border.BorderPositions.Left;
		Top.SetOwner(this);
		Top.BorderPosition = Border.BorderPositions.Top;
		Right.SetOwner(this);
		Right.BorderPosition = Border.BorderPositions.Right;
		Bottom.SetOwner(this);
		Bottom.BorderPosition = Border.BorderPositions.Bottom;
		Vertical.SetOwner(this);
		Vertical.BorderPosition = Border.BorderPositions.Vertical;
		Horizontal.SetOwner(this);
		Horizontal.BorderPosition = Border.BorderPositions.Horizontal;
		DiagonalDown.SetOwner(this);
		DiagonalDown.BorderPosition = Border.BorderPositions.DiagonalDown;
		DiagonalUp.SetOwner(this);
		DiagonalUp.BorderPosition = Border.BorderPositions.DiagonalUp;
	}

	internal override void Close()
	{
		m_currTableCell = null;
		m_currTableRow = null;
	}

	internal void UpdateSourceFormatting(Borders borders)
	{
		Left.UpdateSourceFormatting(borders.Left);
		Right.UpdateSourceFormatting(borders.Right);
		Top.UpdateSourceFormatting(borders.Top);
		Bottom.UpdateSourceFormatting(borders.Bottom);
		Horizontal.UpdateSourceFormatting(borders.Horizontal);
		Vertical.UpdateSourceFormatting(borders.Vertical);
		DiagonalDown.UpdateSourceFormatting(borders.DiagonalDown);
		DiagonalUp.UpdateSourceFormatting(borders.DiagonalUp);
	}

	internal bool Compare(Borders borders)
	{
		if (!DiagonalUp.Compare(borders.DiagonalUp))
		{
			return false;
		}
		if (!DiagonalDown.Compare(borders.DiagonalDown))
		{
			return false;
		}
		if (!Horizontal.Compare(borders.Horizontal))
		{
			return false;
		}
		if (!Vertical.Compare(borders.Vertical))
		{
			return false;
		}
		if (!Top.Compare(borders.Top))
		{
			return false;
		}
		if (!Bottom.Compare(borders.Bottom))
		{
			return false;
		}
		if (!Left.Compare(borders.Left))
		{
			return false;
		}
		if (!Right.Compare(borders.Right))
		{
			return false;
		}
		return true;
	}
}
