namespace DocGen.OfficeChart.Implementation;

internal class Column
{
	private byte binrayInfo;

	public double defaultWidth;

	private int styleIndex;

	private short minCol;

	private WorksheetImpl worksheet;

	public int Index => minCol;

	public bool IsHidden
	{
		get
		{
			return GetHiddenInfo();
		}
		set
		{
			SetHiddenInfo(value);
			SetBestFitInfo(isBestFit: false);
			if (!value && defaultWidth == 0.0)
			{
				defaultWidth = worksheet.Columnss.Width;
			}
		}
	}

	public double Width
	{
		get
		{
			return defaultWidth;
		}
		set
		{
			if (value < double.Epsilon)
			{
				SetHiddenInfo(isHidden: true);
			}
			else
			{
				defaultWidth = value;
				SetHiddenInfo(isHidden: false);
			}
			SetBestFitInfo(isBestFit: false);
		}
	}

	internal Column(int minCol, WorksheetImpl worksheet, double defaultWidth)
	{
		styleIndex = -1;
		this.minCol = (short)minCol;
		this.worksheet = worksheet;
		this.defaultWidth = defaultWidth;
	}

	internal void SetMinColumnIndex(int minCol)
	{
		this.minCol = (short)minCol;
	}

	internal void SetCollapsedInfo(bool isCollapsed)
	{
		if (isCollapsed)
		{
			binrayInfo |= 16;
		}
		else
		{
			binrayInfo &= 239;
		}
	}

	internal void SetBestFitInfo(bool isBestFit)
	{
		if (isBestFit)
		{
			binrayInfo |= 64;
		}
		else
		{
			binrayInfo &= 191;
		}
	}

	internal void SetWidth(int width)
	{
		int fontCalc = worksheet.GetAppImpl().GetFontCalc2();
		int fontCalc2 = worksheet.GetAppImpl().GetFontCalc1();
		int fontCalc3 = worksheet.GetAppImpl().GetFontCalc3();
		if (width < fontCalc + fontCalc3)
		{
			double num = 1.0 * (double)width / (double)(fontCalc + fontCalc3);
			defaultWidth = num;
		}
		else
		{
			double num2 = (double)(int)((double)(width - (int)((double)(fontCalc * fontCalc2) / 256.0 + 0.5)) * 100.0 / (double)fontCalc + 0.5) / 100.0;
			defaultWidth = num2;
		}
		SetBestFitInfo(isBestFit: false);
	}

	internal void SetOutLineLevel(byte outLineLevel)
	{
		binrayInfo &= 240;
		binrayInfo |= outLineLevel;
	}

	internal void SetStyleIndex(int styleIndex)
	{
		this.styleIndex = styleIndex;
	}

	internal bool GetHiddenInfo()
	{
		return (binrayInfo & 0x20) != 0;
	}

	internal void SetHiddenInfo(bool isHidden)
	{
		if (!isHidden)
		{
			binrayInfo &= 223;
		}
		else
		{
			binrayInfo |= 32;
		}
	}
}
