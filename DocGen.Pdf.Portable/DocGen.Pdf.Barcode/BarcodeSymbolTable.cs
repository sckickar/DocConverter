namespace DocGen.Pdf.Barcode;

internal class BarcodeSymbolTable
{
	private char symbol;

	private int checkDigit;

	private byte[] bars;

	public char Symbol
	{
		get
		{
			return symbol;
		}
		set
		{
			symbol = value;
		}
	}

	public int CheckDigit
	{
		get
		{
			return checkDigit;
		}
		set
		{
			checkDigit = value;
		}
	}

	public byte[] Bars
	{
		get
		{
			return bars;
		}
		set
		{
			bars = value;
		}
	}

	public BarcodeSymbolTable()
	{
	}

	public BarcodeSymbolTable(char symbol, int checkDigit, byte[] bars)
	{
		this.symbol = symbol;
		this.checkDigit = checkDigit;
		this.bars = bars;
	}

	public BarcodeSymbolTable(int checkDigit, byte[] bars)
	{
		this.checkDigit = checkDigit;
		this.bars = bars;
	}
}
