using System;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class Border : FormatBase
{
	internal enum BorderPositions
	{
		Left = 0,
		Top = 1,
		Right = 2,
		Bottom = 3,
		Vertical = 5,
		Horizontal = 6,
		DiagonalDown = 7,
		DiagonalUp = 8
	}

	public const int ColorKey = 1;

	internal const int BorderTypeKey = 2;

	internal const int LineWidthKey = 3;

	protected const int SpaceKey = 4;

	protected const int ShadowKey = 5;

	protected const int HasNoneStyleKey = 6;

	private BorderPositions m_borderPosition;

	private byte m_bFlags;

	internal bool IsRead
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsHTMLRead
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal BorderPositions BorderPosition
	{
		get
		{
			return m_borderPosition;
		}
		set
		{
			m_borderPosition = value;
		}
	}

	public Color Color
	{
		get
		{
			return (Color)base[1];
		}
		set
		{
			base[1] = value;
		}
	}

	public float LineWidth
	{
		get
		{
			return (float)base[3];
		}
		set
		{
			base[3] = value;
			if (value == 0f && !IsRead)
			{
				if (BorderType != 0 && BorderType != BorderStyle.Cleared)
				{
					BorderType = BorderStyle.None;
				}
			}
			else if (!IsRead && BorderType == BorderStyle.None && !IsHTMLRead)
			{
				BorderType = BorderStyle.Single;
			}
			if (!IsRead)
			{
				UpdateTableCells();
			}
		}
	}

	public BorderStyle BorderType
	{
		get
		{
			return (BorderStyle)base[2];
		}
		set
		{
			SetBorderStyle(value);
		}
	}

	public float Space
	{
		get
		{
			return (float)base[4];
		}
		set
		{
			byte b = (byte)Math.Round(value);
			b = (byte)(b & 0x1Fu);
			base[4] = (float)(int)b;
		}
	}

	public bool Shadow
	{
		get
		{
			return (bool)base[5];
		}
		set
		{
			base[5] = value;
			if (!IsRead)
			{
				UpdateTableCells();
			}
		}
	}

	internal bool HasNoneStyle
	{
		get
		{
			return (bool)base[6];
		}
		set
		{
			base[6] = value;
		}
	}

	internal bool IsBorderDefined
	{
		get
		{
			if (BorderType == BorderStyle.None)
			{
				if (HasNoneStyle)
				{
					return HasKey(6);
				}
				return false;
			}
			return true;
		}
	}

	public Border(FormatBase parent, int baseKey)
		: base(parent, baseKey)
	{
	}

	internal static bool IsSkipBorder(Border value1, Border value2, bool isFirstRead)
	{
		float lineWeight = value1.GetLineWeight();
		float lineWeight2 = value2.GetLineWeight();
		bool flag = false;
		if (lineWeight == lineWeight2)
		{
			int stylePriority = value1.GetStylePriority();
			int stylePriority2 = value2.GetStylePriority();
			if (stylePriority == stylePriority2)
			{
				int r = value1.Color.R;
				int g = value1.Color.G;
				int b = value1.Color.B;
				int r2 = value2.Color.R;
				int g2 = value2.Color.G;
				int b2 = value2.Color.B;
				if (r + b + 2 * g == r2 + b2 + 2 * g2)
				{
					if (b + 2 * g == b2 + 2 * g2)
					{
						if (g == g2)
						{
							return isFirstRead;
						}
						if (g > g2)
						{
							return true;
						}
						return false;
					}
					if (b + 2 * g > b2 + 2 * g2)
					{
						return true;
					}
					return false;
				}
				if (r + b + 2 * g > r2 + b2 + 2 * g2)
				{
					return true;
				}
				return false;
			}
			if (stylePriority > stylePriority2)
			{
				return false;
			}
			return true;
		}
		if (lineWeight > lineWeight2)
		{
			return false;
		}
		return true;
	}

	private Border OwnerBorder()
	{
		if (base.OwnerBase is Borders { CurrentRow: not null } borders)
		{
			Borders borders2 = borders.CurrentRow.RowFormat.Borders;
			switch (BorderPosition)
			{
			case BorderPositions.Left:
				return borders2.Left;
			case BorderPositions.Right:
				return borders2.Right;
			case BorderPositions.Top:
				return borders2.Top;
			case BorderPositions.Bottom:
				return borders2.Bottom;
			}
		}
		return null;
	}

	internal void CopyBorderFormatting(Border sourceBorder)
	{
		base[2] = sourceBorder.BorderType;
		base[3] = sourceBorder.LineWidth;
		base[1] = sourceBorder.Color;
		base[5] = sourceBorder.Shadow;
		if (sourceBorder.BorderType != BorderStyle.Cleared)
		{
			base[6] = sourceBorder.HasNoneStyle;
		}
	}

	private void UpdateTableCells()
	{
		if (base.OwnerBase == null || !(base.OwnerBase is Borders))
		{
			return;
		}
		Borders borders = base.OwnerBase as Borders;
		if (borders.CurrentRow == null)
		{
			return;
		}
		WTable ownerTable = borders.CurrentRow.OwnerTable;
		if (ownerTable == null)
		{
			return;
		}
		int rowIndex = borders.CurrentRow.GetRowIndex();
		if (borders.CurrentCell == null)
		{
			UpdateAruondRow(ownerTable, rowIndex);
			return;
		}
		int curCellIndex = borders.CurrentCell.CellFormat.CurCellIndex;
		if (HasNoneStyle || BorderType != 0)
		{
			UpdateAroundCell(ownerTable, borders, rowIndex, curCellIndex);
		}
	}

	internal float GetLineWidthValue()
	{
		switch (BorderType)
		{
		case BorderStyle.None:
		case BorderStyle.Hairline:
		case BorderStyle.Cleared:
			return 0f;
		case BorderStyle.Double:
		case BorderStyle.Triple:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThickThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
		case BorderStyle.ThinThickThinLargeGap:
		case BorderStyle.Emboss3D:
		case BorderStyle.Engrave3D:
		{
			float[] borderLineWidthArray = GetBorderLineWidthArray(BorderType, LineWidth);
			float num = 0f;
			float[] array = borderLineWidthArray;
			foreach (float num2 in array)
			{
				num += num2;
			}
			return num;
		}
		case BorderStyle.Single:
		case BorderStyle.Thick:
		case BorderStyle.Dot:
		case BorderStyle.DashLargeGap:
		case BorderStyle.DotDash:
		case BorderStyle.DotDotDash:
		case BorderStyle.DashSmallGap:
			return LineWidth;
		case BorderStyle.Wave:
			if (LineWidth != 1.5f)
			{
				return 2.5f;
			}
			return 3f;
		case BorderStyle.DoubleWave:
			return 6.75f;
		case BorderStyle.DashDotStroker:
		case BorderStyle.Outset:
			return LineWidth;
		default:
			return LineWidth;
		}
	}

	internal float GetLineWeight()
	{
		int lineNumber = GetLineNumber();
		float result = 1f;
		switch (BorderType)
		{
		case BorderStyle.Single:
		case BorderStyle.Thick:
		case BorderStyle.Double:
		case BorderStyle.DotDash:
		case BorderStyle.DotDotDash:
		case BorderStyle.Triple:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThickThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
		case BorderStyle.ThinThickThinLargeGap:
		case BorderStyle.Wave:
		case BorderStyle.DoubleWave:
		case BorderStyle.DashSmallGap:
		case BorderStyle.DashDotStroker:
		case BorderStyle.Emboss3D:
		case BorderStyle.Engrave3D:
		case BorderStyle.Outset:
		case BorderStyle.Inset:
			result = LineWidth * (float)lineNumber;
			break;
		case BorderStyle.Dot:
		case BorderStyle.DashLargeGap:
			result = 1f;
			break;
		}
		return result;
	}

	private int GetLineNumber()
	{
		int result = 0;
		switch (BorderType)
		{
		case BorderStyle.Single:
			result = 1;
			break;
		case BorderStyle.Thick:
			result = 2;
			break;
		case BorderStyle.Double:
			result = 3;
			break;
		case BorderStyle.DotDash:
			result = 8;
			break;
		case BorderStyle.DotDotDash:
			result = 9;
			break;
		case BorderStyle.Triple:
			result = 10;
			break;
		case BorderStyle.ThinThickSmallGap:
			result = 11;
			break;
		case BorderStyle.ThinThinSmallGap:
			result = 12;
			break;
		case BorderStyle.ThinThickThinSmallGap:
			result = 13;
			break;
		case BorderStyle.ThinThickMediumGap:
			result = 14;
			break;
		case BorderStyle.ThickThinMediumGap:
			result = 15;
			break;
		case BorderStyle.ThickThickThinMediumGap:
			result = 16;
			break;
		case BorderStyle.ThinThickLargeGap:
			result = 17;
			break;
		case BorderStyle.ThickThinLargeGap:
			result = 18;
			break;
		case BorderStyle.ThinThickThinLargeGap:
			result = 19;
			break;
		case BorderStyle.Wave:
			result = 20;
			break;
		case BorderStyle.DoubleWave:
			result = 21;
			break;
		case BorderStyle.DashSmallGap:
			result = 22;
			break;
		case BorderStyle.DashDotStroker:
			result = 23;
			break;
		case BorderStyle.Emboss3D:
			result = 24;
			break;
		case BorderStyle.Engrave3D:
			result = 25;
			break;
		case BorderStyle.Outset:
			result = 26;
			break;
		case BorderStyle.Inset:
			result = 27;
			break;
		}
		return result;
	}

	internal int GetStylePriority()
	{
		int result = 0;
		switch (BorderType)
		{
		case BorderStyle.Dot:
			result = 1;
			break;
		case BorderStyle.DashLargeGap:
			result = 2;
			break;
		case BorderStyle.Single:
			result = 3;
			break;
		case BorderStyle.Thick:
			result = 4;
			break;
		case BorderStyle.Double:
			result = 5;
			break;
		case BorderStyle.DotDash:
			result = 6;
			break;
		case BorderStyle.DotDotDash:
			result = 7;
			break;
		case BorderStyle.Triple:
			result = 8;
			break;
		case BorderStyle.ThinThickSmallGap:
			result = 9;
			break;
		case BorderStyle.ThinThinSmallGap:
			result = 10;
			break;
		case BorderStyle.ThinThickThinSmallGap:
			result = 11;
			break;
		case BorderStyle.ThinThickMediumGap:
			result = 12;
			break;
		case BorderStyle.ThickThinMediumGap:
			result = 13;
			break;
		case BorderStyle.ThickThickThinMediumGap:
			result = 14;
			break;
		case BorderStyle.ThinThickLargeGap:
			result = 15;
			break;
		case BorderStyle.ThickThinLargeGap:
			result = 16;
			break;
		case BorderStyle.ThinThickThinLargeGap:
			result = 17;
			break;
		case BorderStyle.Wave:
			result = 18;
			break;
		case BorderStyle.DoubleWave:
			result = 19;
			break;
		case BorderStyle.DashSmallGap:
			result = 20;
			break;
		case BorderStyle.DashDotStroker:
			result = 21;
			break;
		case BorderStyle.Emboss3D:
			result = 22;
			break;
		case BorderStyle.Engrave3D:
			result = 23;
			break;
		case BorderStyle.Outset:
			result = 24;
			break;
		case BorderStyle.Inset:
			result = 25;
			break;
		}
		return result;
	}

	private float[] GetBorderLineWidthArray(BorderStyle borderType, float lineWidth)
	{
		float[] array = new float[1] { lineWidth };
		switch (borderType)
		{
		case BorderStyle.Double:
			array = new float[3] { 1f, 1f, 1f };
			break;
		case BorderStyle.ThinThickSmallGap:
			array = new float[3] { 1f, -0.75f, -0.75f };
			break;
		case BorderStyle.ThinThinSmallGap:
			array = new float[3] { -0.75f, -0.75f, 1f };
			break;
		case BorderStyle.ThinThickMediumGap:
			array = new float[3] { 1f, 0.5f, 0.5f };
			break;
		case BorderStyle.ThickThinMediumGap:
			array = new float[3] { 0.5f, 0.5f, 1f };
			break;
		case BorderStyle.ThinThickLargeGap:
			array = new float[3] { -1.5f, 1f, -0.75f };
			break;
		case BorderStyle.ThickThinLargeGap:
			array = new float[3] { -0.75f, 1f, -1.5f };
			break;
		case BorderStyle.Triple:
			array = new float[5] { 1f, 1f, 1f, 1f, 1f };
			break;
		case BorderStyle.ThinThickThinSmallGap:
			array = new float[5] { -0.75f, -0.75f, 1f, -0.75f, -0.75f };
			break;
		case BorderStyle.ThickThickThinMediumGap:
			array = new float[5] { 0.5f, 0.5f, 1f, 0.5f, 0.5f };
			break;
		case BorderStyle.ThinThickThinLargeGap:
			array = new float[5] { -0.75f, 1f, -1.5f, 1f, -0.75f };
			break;
		case BorderStyle.Emboss3D:
		case BorderStyle.Engrave3D:
			array = new float[5] { 0.25f, 0f, 1f, 0f, 0.25f };
			break;
		}
		if (array.Length == 1)
		{
			return new float[1] { lineWidth };
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] >= 0f)
			{
				array[i] *= lineWidth;
			}
			else
			{
				array[i] = Math.Abs(array[i]);
			}
		}
		return array;
	}

	private void UpdateAroundCell(WTable table, Borders borders, int rowIndex, int cellIndex)
	{
		if (rowIndex == -1)
		{
			return;
		}
		switch (BorderPosition)
		{
		case BorderPositions.Left:
			if (cellIndex > 0 && cellIndex - 1 < table.Rows[rowIndex].Cells.Count)
			{
				WTableCell wTableCell4 = table[rowIndex, cellIndex - 1];
				if (wTableCell4.CellFormat.Borders.Right.HasNoneStyle)
				{
					wTableCell4.CellFormat.Borders.Right.CopyBorderFormatting(this);
				}
			}
			break;
		case BorderPositions.Top:
			if (rowIndex > 0 && table.Rows[rowIndex - 1].Cells.Count > cellIndex)
			{
				WTableCell wTableCell3 = table[rowIndex - 1, cellIndex];
				if (wTableCell3.CellFormat.Borders.Bottom.HasNoneStyle)
				{
					wTableCell3.CellFormat.Borders.Bottom.CopyBorderFormatting(this);
				}
			}
			break;
		case BorderPositions.Right:
			if (cellIndex + 1 < table.Rows[rowIndex].Cells.Count)
			{
				WTableCell wTableCell2 = table[rowIndex, cellIndex + 1];
				if (wTableCell2.CellFormat.Borders.Left.HasNoneStyle)
				{
					wTableCell2.CellFormat.Borders.Left.CopyBorderFormatting(this);
				}
			}
			break;
		case BorderPositions.Bottom:
			if (rowIndex + 1 < table.Rows.Count && table.Rows[rowIndex + 1].Cells.Count > cellIndex)
			{
				WTableCell wTableCell = table[rowIndex + 1, cellIndex];
				if (wTableCell.CellFormat.Borders.Top.HasNoneStyle)
				{
					wTableCell.CellFormat.Borders.Top.CopyBorderFormatting(this);
				}
			}
			break;
		}
	}

	private void UpdateAruondRow(WTable table, int rowIndex)
	{
		BorderPositions borderPosition = BorderPosition;
		if (borderPosition != BorderPositions.Top && borderPosition == BorderPositions.Bottom)
		{
			_ = table.Rows.Count - 1;
		}
	}

	private void SetBorderStyle(BorderStyle value)
	{
		if (value == BorderStyle.None || value == BorderStyle.Cleared)
		{
			if (LineWidth != 0f)
			{
				LineWidth = 0f;
			}
			if (value == BorderStyle.None)
			{
				HasNoneStyle = true;
			}
			else
			{
				HasNoneStyle = false;
			}
		}
		else if ((BorderStyle)base[2] == BorderStyle.None && !IsHTMLRead)
		{
			if (LineWidth == 0f)
			{
				LineWidth = 0.5f;
			}
			if (Color == Color.Empty)
			{
				Color = Color.Black;
			}
			HasNoneStyle = false;
		}
		if (!IsHTMLRead && ((BorderStyle)base[2] == BorderStyle.None || (BorderStyle)base[2] == BorderStyle.Cleared) && value != BorderStyle.Cleared && value != 0)
		{
			if (LineWidth == 0f)
			{
				LineWidth = 0.5f;
			}
			if (Color == Color.Empty || Color == Color.White || Color.Name.ToLower() == "ffffff")
			{
				Color = Color.Black;
			}
		}
		base[2] = value;
	}

	internal void SetDefaultProperties()
	{
		base.PropertiesHash.Add(GetFullKey(3), 0f);
		base.PropertiesHash.Add(GetFullKey(2), BorderStyle.None);
		base.PropertiesHash.Add(GetFullKey(5), false);
		base.PropertiesHash.Add(GetFullKey(1), Color.Empty);
		base.PropertiesHash.Add(GetFullKey(4), 0f);
	}

	internal bool Compare(Border border)
	{
		if (BorderPosition != border.BorderPosition)
		{
			return false;
		}
		if (!Compare(GetFullKey(2), border))
		{
			return false;
		}
		if (!Compare(GetFullKey(1), border))
		{
			return false;
		}
		if (!Compare(GetFullKey(3), border))
		{
			return false;
		}
		if (!Compare(GetFullKey(5), border))
		{
			return false;
		}
		if (!Compare(GetFullKey(4), border))
		{
			return false;
		}
		if (!Compare(GetFullKey(6), border))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Color.ToArgb() + ";");
		string text = (Shadow ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasNoneStyle ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsBorderDefined ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)BorderPosition + ";");
		stringBuilder.Append(LineWidth + ";");
		stringBuilder.Append((int)BorderType + ";");
		stringBuilder.Append(Space + ";");
		return stringBuilder;
	}

	public void InitFormatting(Color color, float lineWidth, BorderStyle borderType, bool shadow)
	{
		base[1] = color;
		base[3] = lineWidth;
		base[2] = borderType;
		base[5] = shadow;
		if (borderType == BorderStyle.None)
		{
			base[6] = true;
		}
		else
		{
			base[6] = false;
		}
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			1 => Color.Empty, 
			2 => BorderStyle.None, 
			3 => 0f, 
			5 => false, 
			4 => 0f, 
			6 => false, 
			_ => throw new ArgumentException("key has invalid value"), 
		};
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(1) && !Color.IsEmpty)
		{
			writer.WriteValue("Color", Color);
		}
		if (HasKey(3))
		{
			writer.WriteValue("LineWidth", LineWidth);
		}
		if (HasKey(2))
		{
			writer.WriteValue("BorderType", BorderType);
		}
		if (HasKey(4))
		{
			writer.WriteValue("Space", Space);
		}
		if (HasKey(5))
		{
			writer.WriteValue("Shadow", Shadow);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Color"))
		{
			Color = reader.ReadColor("Color");
		}
		if (reader.HasAttribute("LineWidth"))
		{
			LineWidth = reader.ReadFloat("LineWidth");
		}
		if (reader.HasAttribute("BorderType"))
		{
			BorderType = (BorderStyle)(object)reader.ReadEnum("BorderType", typeof(BorderStyle));
		}
		if (reader.HasAttribute("Space"))
		{
			Space = reader.ReadFloat("Space");
		}
		if (reader.HasAttribute("Shadow"))
		{
			Shadow = reader.ReadBoolean("Shadow");
		}
	}

	protected override void InitXDLSHolder()
	{
		if (base.IsDefault)
		{
			base.XDLSHolder.SkipMe = true;
		}
	}

	protected override void OnChange(FormatBase format, int propertyKey)
	{
		base.OnChange(format, propertyKey);
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		base.ApplyBase(baseFormat);
	}

	internal void UpdateSourceFormatting(Border border)
	{
		if (border.BorderPosition != BorderPosition)
		{
			border.BorderPosition = BorderPosition;
		}
		if (border.BorderType != BorderType)
		{
			border.BorderType = BorderType;
		}
		if (border.Color != Color)
		{
			border.Color = Color;
		}
		if (border.HasNoneStyle != HasNoneStyle)
		{
			border.HasNoneStyle = HasNoneStyle;
		}
		if (border.IsRead != IsRead)
		{
			border.IsRead = IsRead;
		}
		if (border.LineWidth != LineWidth)
		{
			border.LineWidth = LineWidth;
		}
		if (border.Shadow != Shadow)
		{
			border.Shadow = Shadow;
		}
		if (border.Space != Space)
		{
			border.Space = Space;
		}
	}
}
