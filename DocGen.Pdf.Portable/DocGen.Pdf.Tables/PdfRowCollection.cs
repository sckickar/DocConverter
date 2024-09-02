namespace DocGen.Pdf.Tables;

public class PdfRowCollection : PdfCollection
{
	public PdfRow this[int index] => base.List[index] as PdfRow;

	internal PdfRowCollection()
	{
	}

	public void Add(PdfRow row)
	{
		base.List.Add(row);
	}

	public void Add(object[] values)
	{
		PdfRow item = new PdfRow(values);
		base.List.Add(item);
	}
}
