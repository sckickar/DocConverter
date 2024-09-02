using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfResetAction : PdfFormAction
{
	public override bool Include
	{
		get
		{
			return base.Include;
		}
		set
		{
			if (base.Include != value)
			{
				base.Include = value;
				base.Dictionary.SetNumber("Flags", (!base.Include) ? 1 : 0);
			}
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("S", new PdfName("ResetForm"));
	}
}
