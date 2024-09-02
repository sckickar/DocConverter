using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public sealed class PdfBrushes
{
	private static Dictionary<object, object> s_brushes = new Dictionary<object, object>();

	public static PdfBrush AliceBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.AliceBlue))
				{
					pdfBrush = s_brushes[KnownColor.AliceBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.AliceBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush AntiqueWhite
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.AntiqueWhite))
				{
					pdfBrush = s_brushes[KnownColor.AntiqueWhite] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.AntiqueWhite);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Aqua
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Aqua))
				{
					pdfBrush = s_brushes[KnownColor.Aqua] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Aqua);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Aquamarine
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Aquamarine))
				{
					pdfBrush = s_brushes[KnownColor.Aquamarine] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Aquamarine);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Azure
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Azure))
				{
					pdfBrush = s_brushes[KnownColor.Azure] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Azure);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Beige
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Beige))
				{
					pdfBrush = s_brushes[KnownColor.Beige] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Beige);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Bisque
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Bisque))
				{
					pdfBrush = s_brushes[KnownColor.Bisque] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Bisque);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Black
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Black))
				{
					pdfBrush = s_brushes[KnownColor.Black] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Black);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush BlanchedAlmond
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.BlanchedAlmond))
				{
					pdfBrush = s_brushes[KnownColor.BlanchedAlmond] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.BlanchedAlmond);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Blue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Blue))
				{
					pdfBrush = s_brushes[KnownColor.Blue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Blue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush BlueViolet
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.BlueViolet))
				{
					pdfBrush = s_brushes[KnownColor.BlueViolet] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.BlueViolet);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Brown
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Brown))
				{
					pdfBrush = s_brushes[KnownColor.Brown] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Brown);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush BurlyWood
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.BurlyWood))
				{
					pdfBrush = s_brushes[KnownColor.BurlyWood] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.BurlyWood);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush CadetBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.CadetBlue))
				{
					pdfBrush = s_brushes[KnownColor.CadetBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.CadetBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Chartreuse
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Chartreuse))
				{
					pdfBrush = s_brushes[KnownColor.Chartreuse] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Chartreuse);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Chocolate
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Chocolate))
				{
					pdfBrush = s_brushes[KnownColor.Chocolate] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Chocolate);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Coral
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Coral))
				{
					pdfBrush = s_brushes[KnownColor.Coral] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Coral);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush CornflowerBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.CornflowerBlue))
				{
					pdfBrush = s_brushes[KnownColor.CornflowerBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.CornflowerBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Cornsilk
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Cornsilk))
				{
					pdfBrush = s_brushes[KnownColor.Cornsilk] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Cornsilk);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Crimson
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Crimson))
				{
					pdfBrush = s_brushes[KnownColor.Crimson] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Crimson);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Cyan
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Cyan))
				{
					pdfBrush = s_brushes[KnownColor.Cyan] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Cyan);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkBlue))
				{
					pdfBrush = s_brushes[KnownColor.DarkBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkCyan
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkCyan))
				{
					pdfBrush = s_brushes[KnownColor.DarkCyan] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkCyan);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkGoldenrod
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkGoldenrod))
				{
					pdfBrush = s_brushes[KnownColor.DarkGoldenrod] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkGoldenrod);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkGray))
				{
					pdfBrush = s_brushes[KnownColor.DarkGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkGreen))
				{
					pdfBrush = s_brushes[KnownColor.DarkGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkKhaki
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkKhaki))
				{
					pdfBrush = s_brushes[KnownColor.DarkKhaki] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkKhaki);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkMagenta
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkMagenta))
				{
					pdfBrush = s_brushes[KnownColor.DarkMagenta] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkMagenta);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkOliveGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkOliveGreen))
				{
					pdfBrush = s_brushes[KnownColor.DarkOliveGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkOliveGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkOrange
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkOrange))
				{
					pdfBrush = s_brushes[KnownColor.DarkOrange] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkOrange);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkOrchid
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkOrchid))
				{
					pdfBrush = s_brushes[KnownColor.DarkOrchid] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkOrchid);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkRed
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkRed))
				{
					pdfBrush = s_brushes[KnownColor.DarkRed] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkRed);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkSalmon
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkSalmon))
				{
					pdfBrush = s_brushes[KnownColor.DarkSalmon] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkSalmon);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkSeaGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkSeaGreen))
				{
					pdfBrush = s_brushes[KnownColor.DarkSeaGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkSeaGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkSlateBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkSlateBlue))
				{
					pdfBrush = s_brushes[KnownColor.DarkSlateBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkSlateBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkSlateGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkSlateGray))
				{
					pdfBrush = s_brushes[KnownColor.DarkSlateGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkSlateGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkTurquoise
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkTurquoise))
				{
					pdfBrush = s_brushes[KnownColor.DarkTurquoise] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkTurquoise);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DarkViolet
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DarkViolet))
				{
					pdfBrush = s_brushes[KnownColor.DarkViolet] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DarkViolet);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DeepPink
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DeepPink))
				{
					pdfBrush = s_brushes[KnownColor.DeepPink] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DeepPink);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DeepSkyBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DeepSkyBlue))
				{
					pdfBrush = s_brushes[KnownColor.DeepSkyBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DeepSkyBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DimGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DimGray))
				{
					pdfBrush = s_brushes[KnownColor.DimGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DimGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush DodgerBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.DodgerBlue))
				{
					pdfBrush = s_brushes[KnownColor.DodgerBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.DodgerBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Firebrick
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Firebrick))
				{
					pdfBrush = s_brushes[KnownColor.Firebrick] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Firebrick);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush FloralWhite
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.FloralWhite))
				{
					pdfBrush = s_brushes[KnownColor.FloralWhite] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.FloralWhite);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush ForestGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.ForestGreen))
				{
					pdfBrush = s_brushes[KnownColor.ForestGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.ForestGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Fuchsia
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Fuchsia))
				{
					pdfBrush = s_brushes[KnownColor.Fuchsia] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Fuchsia);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Gainsboro
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Gainsboro))
				{
					pdfBrush = s_brushes[KnownColor.Gainsboro] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Gainsboro);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush GhostWhite
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.GhostWhite))
				{
					pdfBrush = s_brushes[KnownColor.GhostWhite] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.GhostWhite);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Gold
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Gold))
				{
					pdfBrush = s_brushes[KnownColor.Gold] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Gold);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Goldenrod
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Goldenrod))
				{
					pdfBrush = s_brushes[KnownColor.Goldenrod] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Goldenrod);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Gray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Gray))
				{
					pdfBrush = s_brushes[KnownColor.Gray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Gray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Green
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Green))
				{
					pdfBrush = s_brushes[KnownColor.Green] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Green);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush GreenYellow
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.GreenYellow))
				{
					pdfBrush = s_brushes[KnownColor.GreenYellow] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.GreenYellow);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Honeydew
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Honeydew))
				{
					pdfBrush = s_brushes[KnownColor.Honeydew] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Honeydew);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush HotPink
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.HotPink))
				{
					pdfBrush = s_brushes[KnownColor.HotPink] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.HotPink);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush IndianRed
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.IndianRed))
				{
					pdfBrush = s_brushes[KnownColor.IndianRed] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.IndianRed);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Indigo
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Indigo))
				{
					pdfBrush = s_brushes[KnownColor.Indigo] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Indigo);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Ivory
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Ivory))
				{
					pdfBrush = s_brushes[KnownColor.Ivory] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Ivory);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Khaki
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Khaki))
				{
					pdfBrush = s_brushes[KnownColor.Khaki] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Khaki);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Lavender
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Lavender))
				{
					pdfBrush = s_brushes[KnownColor.Lavender] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Lavender);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LavenderBlush
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LavenderBlush))
				{
					pdfBrush = s_brushes[KnownColor.LavenderBlush] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LavenderBlush);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LawnGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LawnGreen))
				{
					pdfBrush = s_brushes[KnownColor.LawnGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LawnGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LemonChiffon
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LemonChiffon))
				{
					pdfBrush = s_brushes[KnownColor.LemonChiffon] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LemonChiffon);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightBlue))
				{
					pdfBrush = s_brushes[KnownColor.LightBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightCoral
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightCoral))
				{
					pdfBrush = s_brushes[KnownColor.LightCoral] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightCoral);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightCyan
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightCyan))
				{
					pdfBrush = s_brushes[KnownColor.LightCyan] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightCyan);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightGoldenrodYellow
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightGoldenrodYellow))
				{
					pdfBrush = s_brushes[KnownColor.LightGoldenrodYellow] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightGoldenrodYellow);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightGray))
				{
					pdfBrush = s_brushes[KnownColor.LightGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightGreen))
				{
					pdfBrush = s_brushes[KnownColor.LightGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightPink
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightPink))
				{
					pdfBrush = s_brushes[KnownColor.LightPink] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightPink);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightSalmon
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightSalmon))
				{
					pdfBrush = s_brushes[KnownColor.LightSalmon] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightSalmon);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightSeaGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightSeaGreen))
				{
					pdfBrush = s_brushes[KnownColor.LightSeaGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightSeaGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightSkyBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightSkyBlue))
				{
					pdfBrush = s_brushes[KnownColor.LightSkyBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightSkyBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightSlateGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightSlateGray))
				{
					pdfBrush = s_brushes[KnownColor.LightSlateGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightSlateGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightSteelBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightSteelBlue))
				{
					pdfBrush = s_brushes[KnownColor.LightSteelBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightSteelBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LightYellow
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LightYellow))
				{
					pdfBrush = s_brushes[KnownColor.LightYellow] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LightYellow);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Lime
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Lime))
				{
					pdfBrush = s_brushes[KnownColor.Lime] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Lime);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush LimeGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.LimeGreen))
				{
					pdfBrush = s_brushes[KnownColor.LimeGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.LimeGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Linen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Linen))
				{
					pdfBrush = s_brushes[KnownColor.Linen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Linen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Magenta
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Magenta))
				{
					pdfBrush = s_brushes[KnownColor.Magenta] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Magenta);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Maroon
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Maroon))
				{
					pdfBrush = s_brushes[KnownColor.Maroon] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Maroon);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumAquamarine
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumAquamarine))
				{
					pdfBrush = s_brushes[KnownColor.MediumAquamarine] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumAquamarine);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumBlue))
				{
					pdfBrush = s_brushes[KnownColor.MediumBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumOrchid
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumOrchid))
				{
					pdfBrush = s_brushes[KnownColor.MediumOrchid] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumOrchid);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumPurple
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumPurple))
				{
					pdfBrush = s_brushes[KnownColor.MediumPurple] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumPurple);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumSeaGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumSeaGreen))
				{
					pdfBrush = s_brushes[KnownColor.MediumSeaGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumSeaGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumSlateBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumSlateBlue))
				{
					pdfBrush = s_brushes[KnownColor.MediumSlateBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumSlateBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumSpringGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumSpringGreen))
				{
					pdfBrush = s_brushes[KnownColor.MediumSpringGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumSpringGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumTurquoise
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumTurquoise))
				{
					pdfBrush = s_brushes[KnownColor.MediumTurquoise] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumTurquoise);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MediumVioletRed
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MediumVioletRed))
				{
					pdfBrush = s_brushes[KnownColor.MediumVioletRed] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MediumVioletRed);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MidnightBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MidnightBlue))
				{
					pdfBrush = s_brushes[KnownColor.MidnightBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MidnightBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MintCream
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MintCream))
				{
					pdfBrush = s_brushes[KnownColor.MintCream] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MintCream);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush MistyRose
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.MistyRose))
				{
					pdfBrush = s_brushes[KnownColor.MistyRose] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.MistyRose);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Moccasin
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Moccasin))
				{
					pdfBrush = s_brushes[KnownColor.Moccasin] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Moccasin);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush NavajoWhite
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.NavajoWhite))
				{
					pdfBrush = s_brushes[KnownColor.NavajoWhite] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.NavajoWhite);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Navy
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Navy))
				{
					pdfBrush = s_brushes[KnownColor.Navy] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Navy);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush OldLace
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.OldLace))
				{
					pdfBrush = s_brushes[KnownColor.OldLace] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.OldLace);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Olive
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Olive))
				{
					pdfBrush = s_brushes[KnownColor.Olive] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Olive);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush OliveDrab
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.OliveDrab))
				{
					pdfBrush = s_brushes[KnownColor.OliveDrab] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.OliveDrab);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Orange
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Orange))
				{
					pdfBrush = s_brushes[KnownColor.Orange] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Orange);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush OrangeRed
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.OrangeRed))
				{
					pdfBrush = s_brushes[KnownColor.OrangeRed] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.OrangeRed);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Orchid
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Orchid))
				{
					pdfBrush = s_brushes[KnownColor.Orchid] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Orchid);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PaleGoldenrod
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PaleGoldenrod))
				{
					pdfBrush = s_brushes[KnownColor.PaleGoldenrod] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PaleGoldenrod);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PaleGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PaleGreen))
				{
					pdfBrush = s_brushes[KnownColor.PaleGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PaleGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PaleTurquoise
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PaleTurquoise))
				{
					pdfBrush = s_brushes[KnownColor.PaleTurquoise] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PaleTurquoise);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PaleVioletRed
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PaleVioletRed))
				{
					pdfBrush = s_brushes[KnownColor.PaleVioletRed] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PaleVioletRed);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PapayaWhip
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PapayaWhip))
				{
					pdfBrush = s_brushes[KnownColor.PapayaWhip] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PapayaWhip);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PeachPuff
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PeachPuff))
				{
					pdfBrush = s_brushes[KnownColor.PeachPuff] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PeachPuff);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Peru
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Peru))
				{
					pdfBrush = s_brushes[KnownColor.Peru] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Peru);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Pink
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Pink))
				{
					pdfBrush = s_brushes[KnownColor.Pink] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Pink);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Plum
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Plum))
				{
					pdfBrush = s_brushes[KnownColor.Plum] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Plum);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush PowderBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.PowderBlue))
				{
					pdfBrush = s_brushes[KnownColor.PowderBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.PowderBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Purple
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Purple))
				{
					pdfBrush = s_brushes[KnownColor.Purple] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Purple);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Red
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Red))
				{
					pdfBrush = s_brushes[KnownColor.Red] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Red);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush RosyBrown
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.RosyBrown))
				{
					pdfBrush = s_brushes[KnownColor.RosyBrown] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.RosyBrown);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush RoyalBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.RoyalBlue))
				{
					pdfBrush = s_brushes[KnownColor.RoyalBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.RoyalBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SaddleBrown
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SaddleBrown))
				{
					pdfBrush = s_brushes[KnownColor.SaddleBrown] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SaddleBrown);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Salmon
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Salmon))
				{
					pdfBrush = s_brushes[KnownColor.Salmon] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Salmon);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SandyBrown
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SandyBrown))
				{
					pdfBrush = s_brushes[KnownColor.SandyBrown] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SandyBrown);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SeaGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SeaGreen))
				{
					pdfBrush = s_brushes[KnownColor.SeaGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SeaGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SeaShell
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SeaShell))
				{
					pdfBrush = s_brushes[KnownColor.SeaShell] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SeaShell);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Sienna
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Sienna))
				{
					pdfBrush = s_brushes[KnownColor.Sienna] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Sienna);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Silver
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Silver))
				{
					pdfBrush = s_brushes[KnownColor.Silver] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Silver);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SkyBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SkyBlue))
				{
					pdfBrush = s_brushes[KnownColor.SkyBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SkyBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SlateBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SlateBlue))
				{
					pdfBrush = s_brushes[KnownColor.SlateBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SlateBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SlateGray
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SlateGray))
				{
					pdfBrush = s_brushes[KnownColor.SlateGray] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SlateGray);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Snow
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Snow))
				{
					pdfBrush = s_brushes[KnownColor.Snow] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Snow);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SpringGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SpringGreen))
				{
					pdfBrush = s_brushes[KnownColor.SpringGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SpringGreen);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush SteelBlue
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.SteelBlue))
				{
					pdfBrush = s_brushes[KnownColor.SteelBlue] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.SteelBlue);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Tan
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Tan))
				{
					pdfBrush = s_brushes[KnownColor.Tan] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Tan);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Teal
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Teal))
				{
					pdfBrush = s_brushes[KnownColor.Teal] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Teal);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Thistle
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Thistle))
				{
					pdfBrush = s_brushes[KnownColor.Thistle] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Thistle);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Tomato
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Tomato))
				{
					pdfBrush = s_brushes[KnownColor.Tomato] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Tomato);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Transparent
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Transparent))
				{
					pdfBrush = s_brushes[KnownColor.Transparent] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Transparent);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Turquoise
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Turquoise))
				{
					pdfBrush = s_brushes[KnownColor.Turquoise] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Turquoise);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Violet
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Violet))
				{
					pdfBrush = s_brushes[KnownColor.Violet] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Violet);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Wheat
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Wheat))
				{
					pdfBrush = s_brushes[KnownColor.Wheat] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Wheat);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush White
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.White))
				{
					pdfBrush = s_brushes[KnownColor.White] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.White);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush WhiteSmoke
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.WhiteSmoke))
				{
					pdfBrush = s_brushes[KnownColor.WhiteSmoke] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.WhiteSmoke);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush Yellow
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.Yellow))
				{
					pdfBrush = s_brushes[KnownColor.Yellow] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.Yellow);
				}
				return pdfBrush;
			}
		}
	}

	public static PdfBrush YellowGreen
	{
		get
		{
			lock (s_brushes)
			{
				PdfBrush pdfBrush = null;
				if (s_brushes.ContainsKey(KnownColor.YellowGreen))
				{
					pdfBrush = s_brushes[KnownColor.YellowGreen] as PdfBrush;
				}
				if (pdfBrush == null)
				{
					pdfBrush = GetBrush(KnownColor.YellowGreen);
				}
				return pdfBrush;
			}
		}
	}

	private PdfBrushes()
	{
	}

	private static PdfBrush GetBrush(KnownColor colorName)
	{
		Color color = ColorConverter.FromKnownColor(colorName);
		PdfBrush pdfBrush = new PdfSolidBrush(new PdfColor(color), immutable: true);
		s_brushes[color] = pdfBrush;
		return pdfBrush;
	}
}
