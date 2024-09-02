using System.Collections.Generic;

namespace DocGen.ChartToImageConverter;

internal class ViewModel
{
	internal IList<ChartPointInternal> Products { get; set; }

	internal ViewModel(int count)
	{
		Products = new List<ChartPointInternal>(count);
		for (int i = 0; i < count; i++)
		{
			Products.Add(new ChartPointInternal());
		}
	}

	internal ViewModel()
	{
		Products = new List<ChartPointInternal>();
	}
}
