using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class AutoShapeImpl : ShapeImpl
{
	private ShapeImplExt m_shapeExt;

	private bool m_isMoveWithCell;

	private bool m_isSizeWithCell;

	private bool m_isFill;

	private bool m_isNoFill;

	internal bool FlipVertical
	{
		get
		{
			return m_shapeExt.FlipVertical;
		}
		set
		{
			m_shapeExt.FlipVertical = value;
		}
	}

	internal bool FlipHorizontal
	{
		get
		{
			return m_shapeExt.FlipHorizontal;
		}
		set
		{
			m_shapeExt.FlipHorizontal = value;
		}
	}

	internal ShapeImplExt ShapeExt
	{
		get
		{
			return m_shapeExt;
		}
		set
		{
			m_shapeExt = value;
		}
	}

	internal bool IsFill
	{
		get
		{
			return m_isFill;
		}
		set
		{
			m_isFill = true;
		}
	}

	internal bool IsNoFill
	{
		get
		{
			return m_isNoFill;
		}
		set
		{
			m_isNoFill = value;
		}
	}

	public override ITextFrame TextFrame => m_shapeExt.TextFrame;

	internal TextFrame TextFrameInternal => m_shapeExt.TextFrame;

	public override string AlternativeText
	{
		get
		{
			return m_shapeExt.Description;
		}
		set
		{
			m_shapeExt.Description = value;
		}
	}

	public override int Id => m_shapeExt.ShapeID;

	public override string Name
	{
		get
		{
			return m_shapeExt.Name;
		}
		set
		{
			m_shapeExt.Name = value;
		}
	}

	public override int BottomRow
	{
		get
		{
			return m_shapeExt.ClientAnchor.BottomRow + 1;
		}
		set
		{
			m_shapeExt.ClientAnchor.BottomRow = value - 1;
		}
	}

	public override int BottomRowOffset
	{
		get
		{
			return m_shapeExt.ClientAnchor.BottomRowOffset;
		}
		set
		{
			m_shapeExt.ClientAnchor.BottomRowOffset = value;
		}
	}

	public override int Height
	{
		get
		{
			return m_shapeExt.ClientAnchor.Height;
		}
		set
		{
			m_shapeExt.ClientAnchor.Height = value;
		}
	}

	public override int Left
	{
		get
		{
			return m_shapeExt.ClientAnchor.Left;
		}
		set
		{
			m_shapeExt.ClientAnchor.Left = value;
		}
	}

	public override int LeftColumn
	{
		get
		{
			return m_shapeExt.ClientAnchor.LeftColumn + 1;
		}
		set
		{
			m_shapeExt.ClientAnchor.LeftColumn = value - 1;
		}
	}

	public override int LeftColumnOffset
	{
		get
		{
			return m_shapeExt.ClientAnchor.LeftColumnOffset;
		}
		set
		{
			m_shapeExt.ClientAnchor.LeftColumnOffset = value;
		}
	}

	public override int RightColumn
	{
		get
		{
			return m_shapeExt.ClientAnchor.RightColumn + 1;
		}
		set
		{
			m_shapeExt.ClientAnchor.RightColumn = value - 1;
		}
	}

	public override int RightColumnOffset
	{
		get
		{
			return m_shapeExt.ClientAnchor.RightColumnOffset;
		}
		set
		{
			m_shapeExt.ClientAnchor.RightColumnOffset = value;
		}
	}

	public override int Top
	{
		get
		{
			return m_shapeExt.ClientAnchor.Top;
		}
		set
		{
			m_shapeExt.ClientAnchor.Top = value;
		}
	}

	public override int TopRow
	{
		get
		{
			return m_shapeExt.ClientAnchor.TopRow + 1;
		}
		set
		{
			m_shapeExt.ClientAnchor.TopRow = value - 1;
		}
	}

	public override int TopRowOffset
	{
		get
		{
			return m_shapeExt.ClientAnchor.TopRowOffset;
		}
		set
		{
			m_shapeExt.ClientAnchor.TopRowOffset = value;
		}
	}

	public override int Width
	{
		get
		{
			return m_shapeExt.ClientAnchor.Width;
		}
		set
		{
			m_shapeExt.ClientAnchor.Width = value;
		}
	}

	public override int ShapeRotation
	{
		get
		{
			return base.ShapeRotation;
		}
		set
		{
			base.ShapeRotation = value;
		}
	}

	public override IOfficeFill Fill => m_shapeExt.Fill;

	public override IShapeLineFormat Line => m_shapeExt.Line;

	public bool IsHidden
	{
		get
		{
			return m_shapeExt.IsHidden;
		}
		set
		{
			m_shapeExt.IsHidden = value;
		}
	}

	public string Title
	{
		get
		{
			return m_shapeExt.Title;
		}
		set
		{
			m_shapeExt.Title = value;
		}
	}

	public override bool IsMoveWithCell
	{
		get
		{
			return m_isMoveWithCell;
		}
		set
		{
			m_isMoveWithCell = value;
			SetPlacementValue();
		}
	}

	public override bool IsSizeWithCell
	{
		get
		{
			return m_isSizeWithCell;
		}
		set
		{
			m_isSizeWithCell = value;
			SetPlacementValue();
		}
	}

	internal AutoShapeImpl(IApplication application, object parent)
		: base(application, parent)
	{
		base.ShapeType = OfficeShapeType.AutoShape;
		m_bSupportOptions = true;
		m_isMoveWithCell = true;
		m_isSizeWithCell = false;
	}

	internal void CreateShape(AutoShapeType type, WorksheetBaseImpl sheetImpl)
	{
		if (sheetImpl is WorksheetImpl)
		{
			m_shapeExt = new ShapeImplExt(type, sheetImpl as WorksheetImpl);
		}
		else
		{
			m_shapeExt = new ShapeImplExt(type, sheetImpl);
		}
	}

	internal void SetShapeID(int shapeId)
	{
		m_shapeExt.ShapeID = shapeId;
	}

	private void SetPlacementValue()
	{
		if (m_isMoveWithCell && m_isSizeWithCell)
		{
			m_shapeExt.ClientAnchor.Placement = PlacementType.MoveAndSize;
		}
		if (m_isMoveWithCell && !m_isSizeWithCell)
		{
			m_shapeExt.ClientAnchor.Placement = PlacementType.Move;
		}
		if (!m_isMoveWithCell && !m_isSizeWithCell)
		{
			m_shapeExt.ClientAnchor.Placement = PlacementType.FreeFloating;
		}
	}
}
