using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public sealed class PdfPens
{
	private static Dictionary<object, object> s_pens = new Dictionary<object, object>();

	public static PdfPen AliceBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.AliceBlue))
				{
					pdfPen = s_pens[KnownColor.AliceBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.AliceBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen AntiqueWhite
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.AntiqueWhite))
				{
					pdfPen = s_pens[KnownColor.AntiqueWhite] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.AntiqueWhite);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Aqua
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Aqua))
				{
					pdfPen = s_pens[KnownColor.Aqua] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Aqua);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Aquamarine
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Aquamarine))
				{
					pdfPen = s_pens[KnownColor.Aquamarine] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Aquamarine);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Azure
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Azure))
				{
					pdfPen = s_pens[KnownColor.Azure] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Azure);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Beige
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Beige))
				{
					pdfPen = s_pens[KnownColor.Beige] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Beige);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Bisque
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Bisque))
				{
					pdfPen = s_pens[KnownColor.Bisque] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Bisque);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Black
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Black))
				{
					pdfPen = s_pens[KnownColor.Black] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Black);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen BlanchedAlmond
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.BlanchedAlmond))
				{
					pdfPen = s_pens[KnownColor.BlanchedAlmond] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.BlanchedAlmond);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Blue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Blue))
				{
					pdfPen = s_pens[KnownColor.Blue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Blue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen BlueViolet
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.BlueViolet))
				{
					pdfPen = s_pens[KnownColor.BlueViolet] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.BlueViolet);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Brown
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Brown))
				{
					pdfPen = s_pens[KnownColor.Brown] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Brown);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen BurlyWood
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.BurlyWood))
				{
					pdfPen = s_pens[KnownColor.BurlyWood] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.BurlyWood);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen CadetBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.CadetBlue))
				{
					pdfPen = s_pens[KnownColor.CadetBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.CadetBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Chartreuse
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Chartreuse))
				{
					pdfPen = s_pens[KnownColor.Chartreuse] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Chartreuse);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Chocolate
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Chocolate))
				{
					pdfPen = s_pens[KnownColor.Chocolate] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Chocolate);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Coral
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Coral))
				{
					pdfPen = s_pens[KnownColor.Coral] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Coral);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen CornflowerBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.CornflowerBlue))
				{
					pdfPen = s_pens[KnownColor.CornflowerBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.CornflowerBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Cornsilk
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Cornsilk))
				{
					pdfPen = s_pens[KnownColor.Cornsilk] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Cornsilk);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Crimson
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Crimson))
				{
					pdfPen = s_pens[KnownColor.Crimson] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Crimson);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Cyan
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Cyan))
				{
					pdfPen = s_pens[KnownColor.Cyan] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Cyan);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkBlue))
				{
					pdfPen = s_pens[KnownColor.DarkBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkCyan
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkCyan))
				{
					pdfPen = s_pens[KnownColor.DarkCyan] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkCyan);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkGoldenrod
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkGoldenrod))
				{
					pdfPen = s_pens[KnownColor.DarkGoldenrod] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkGoldenrod);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkGray))
				{
					pdfPen = s_pens[KnownColor.DarkGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkGreen))
				{
					pdfPen = s_pens[KnownColor.DarkGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkKhaki
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkKhaki))
				{
					pdfPen = s_pens[KnownColor.DarkKhaki] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkKhaki);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkMagenta
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkMagenta))
				{
					pdfPen = s_pens[KnownColor.DarkMagenta] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkMagenta);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkOliveGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkOliveGreen))
				{
					pdfPen = s_pens[KnownColor.DarkOliveGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkOliveGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkOrange
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkOrange))
				{
					pdfPen = s_pens[KnownColor.DarkOrange] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkOrange);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkOrchid
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkOrchid))
				{
					pdfPen = s_pens[KnownColor.DarkOrchid] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkOrchid);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkRed
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkRed))
				{
					pdfPen = s_pens[KnownColor.DarkRed] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkRed);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkSalmon
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkSalmon))
				{
					pdfPen = s_pens[KnownColor.DarkSalmon] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkSalmon);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkSeaGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkSeaGreen))
				{
					pdfPen = s_pens[KnownColor.DarkSeaGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkSeaGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkSlateBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkSlateBlue))
				{
					pdfPen = s_pens[KnownColor.DarkSlateBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkSlateBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkSlateGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkSlateGray))
				{
					pdfPen = s_pens[KnownColor.DarkSlateGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkSlateGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkTurquoise
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkTurquoise))
				{
					pdfPen = s_pens[KnownColor.DarkTurquoise] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkTurquoise);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DarkViolet
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DarkViolet))
				{
					pdfPen = s_pens[KnownColor.DarkViolet] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DarkViolet);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DeepPink
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DeepPink))
				{
					pdfPen = s_pens[KnownColor.DeepPink] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DeepPink);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DeepSkyBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DeepSkyBlue))
				{
					pdfPen = s_pens[KnownColor.DeepSkyBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DeepSkyBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DimGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DimGray))
				{
					pdfPen = s_pens[KnownColor.DimGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DimGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen DodgerBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.DodgerBlue))
				{
					pdfPen = s_pens[KnownColor.DodgerBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.DodgerBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Firebrick
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Firebrick))
				{
					pdfPen = s_pens[KnownColor.Firebrick] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Firebrick);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen FloralWhite
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.FloralWhite))
				{
					pdfPen = s_pens[KnownColor.FloralWhite] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.FloralWhite);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen ForestGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.ForestGreen))
				{
					pdfPen = s_pens[KnownColor.ForestGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.ForestGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Fuchsia
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Fuchsia))
				{
					pdfPen = s_pens[KnownColor.Fuchsia] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Fuchsia);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Gainsboro
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Gainsboro))
				{
					pdfPen = s_pens[KnownColor.Gainsboro] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Gainsboro);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen GhostWhite
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.GhostWhite))
				{
					pdfPen = s_pens[KnownColor.GhostWhite] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.GhostWhite);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Gold
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Gold))
				{
					pdfPen = s_pens[KnownColor.Gold] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Gold);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Goldenrod
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Goldenrod))
				{
					pdfPen = s_pens[KnownColor.Goldenrod] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Goldenrod);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Gray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Gray))
				{
					pdfPen = s_pens[KnownColor.Gray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Gray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Green
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Green))
				{
					pdfPen = s_pens[KnownColor.Green] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Green);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen GreenYellow
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.GreenYellow))
				{
					pdfPen = s_pens[KnownColor.GreenYellow] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.GreenYellow);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Honeydew
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Honeydew))
				{
					pdfPen = s_pens[KnownColor.Honeydew] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Honeydew);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen HotPink
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.HotPink))
				{
					pdfPen = s_pens[KnownColor.HotPink] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.HotPink);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen IndianRed
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.IndianRed))
				{
					pdfPen = s_pens[KnownColor.IndianRed] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.IndianRed);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Indigo
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Indigo))
				{
					pdfPen = s_pens[KnownColor.Indigo] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Indigo);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Ivory
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Ivory))
				{
					pdfPen = s_pens[KnownColor.Ivory] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Ivory);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Khaki
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Khaki))
				{
					pdfPen = s_pens[KnownColor.Khaki] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Khaki);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Lavender
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Lavender))
				{
					pdfPen = s_pens[KnownColor.Lavender] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Lavender);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LavenderBlush
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LavenderBlush))
				{
					pdfPen = s_pens[KnownColor.LavenderBlush] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LavenderBlush);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LawnGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LawnGreen))
				{
					pdfPen = s_pens[KnownColor.LawnGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LawnGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LemonChiffon
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LemonChiffon))
				{
					pdfPen = s_pens[KnownColor.LemonChiffon] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LemonChiffon);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightBlue))
				{
					pdfPen = s_pens[KnownColor.LightBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightCoral
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightCoral))
				{
					pdfPen = s_pens[KnownColor.LightCoral] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightCoral);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightCyan
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightCyan))
				{
					pdfPen = s_pens[KnownColor.LightCyan] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightCyan);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightGoldenrodYellow
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightGoldenrodYellow))
				{
					pdfPen = s_pens[KnownColor.LightGoldenrodYellow] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightGoldenrodYellow);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightGray))
				{
					pdfPen = s_pens[KnownColor.LightGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightGreen))
				{
					pdfPen = s_pens[KnownColor.LightGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightPink
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightPink))
				{
					pdfPen = s_pens[KnownColor.LightPink] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightPink);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightSalmon
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightSalmon))
				{
					pdfPen = s_pens[KnownColor.LightSalmon] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightSalmon);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightSeaGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightSeaGreen))
				{
					pdfPen = s_pens[KnownColor.LightSeaGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightSeaGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightSkyBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightSkyBlue))
				{
					pdfPen = s_pens[KnownColor.LightSkyBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightSkyBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightSlateGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightSlateGray))
				{
					pdfPen = s_pens[KnownColor.LightSlateGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightSlateGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightSteelBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightSteelBlue))
				{
					pdfPen = s_pens[KnownColor.LightSteelBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightSteelBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LightYellow
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LightYellow))
				{
					pdfPen = s_pens[KnownColor.LightYellow] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LightYellow);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Lime
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Lime))
				{
					pdfPen = s_pens[KnownColor.Lime] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Lime);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen LimeGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.LimeGreen))
				{
					pdfPen = s_pens[KnownColor.LimeGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.LimeGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Linen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Linen))
				{
					pdfPen = s_pens[KnownColor.Linen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Linen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Magenta
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Magenta))
				{
					pdfPen = s_pens[KnownColor.Magenta] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Magenta);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Maroon
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Maroon))
				{
					pdfPen = s_pens[KnownColor.Maroon] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Maroon);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumAquamarine
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumAquamarine))
				{
					pdfPen = s_pens[KnownColor.MediumAquamarine] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumAquamarine);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumBlue))
				{
					pdfPen = s_pens[KnownColor.MediumBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumOrchid
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumOrchid))
				{
					pdfPen = s_pens[KnownColor.MediumOrchid] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumOrchid);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumPurple
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumPurple))
				{
					pdfPen = s_pens[KnownColor.MediumPurple] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumPurple);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumSeaGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumSeaGreen))
				{
					pdfPen = s_pens[KnownColor.MediumSeaGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumSeaGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumSlateBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumSlateBlue))
				{
					pdfPen = s_pens[KnownColor.MediumSlateBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumSlateBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumSpringGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumSpringGreen))
				{
					pdfPen = s_pens[KnownColor.MediumSpringGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumSpringGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumTurquoise
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumTurquoise))
				{
					pdfPen = s_pens[KnownColor.MediumTurquoise] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumTurquoise);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MediumVioletRed
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MediumVioletRed))
				{
					pdfPen = s_pens[KnownColor.MediumVioletRed] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MediumVioletRed);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MidnightBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MidnightBlue))
				{
					pdfPen = s_pens[KnownColor.MidnightBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MidnightBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MintCream
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MintCream))
				{
					pdfPen = s_pens[KnownColor.MintCream] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MintCream);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen MistyRose
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.MistyRose))
				{
					pdfPen = s_pens[KnownColor.MistyRose] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.MistyRose);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Moccasin
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Moccasin))
				{
					pdfPen = s_pens[KnownColor.Moccasin] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Moccasin);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen NavajoWhite
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.NavajoWhite))
				{
					pdfPen = s_pens[KnownColor.NavajoWhite] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.NavajoWhite);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Navy
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Navy))
				{
					pdfPen = s_pens[KnownColor.Navy] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Navy);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen OldLace
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.OldLace))
				{
					pdfPen = s_pens[KnownColor.OldLace] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.OldLace);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Olive
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Olive))
				{
					pdfPen = s_pens[KnownColor.Olive] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Olive);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen OliveDrab
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.OliveDrab))
				{
					pdfPen = s_pens[KnownColor.OliveDrab] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.OliveDrab);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Orange
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Orange))
				{
					pdfPen = s_pens[KnownColor.Orange] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Orange);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen OrangeRed
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.OrangeRed))
				{
					pdfPen = s_pens[KnownColor.OrangeRed] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.OrangeRed);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Orchid
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Orchid))
				{
					pdfPen = s_pens[KnownColor.Orchid] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Orchid);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PaleGoldenrod
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PaleGoldenrod))
				{
					pdfPen = s_pens[KnownColor.PaleGoldenrod] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PaleGoldenrod);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PaleGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PaleGreen))
				{
					pdfPen = s_pens[KnownColor.PaleGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PaleGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PaleTurquoise
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PaleTurquoise))
				{
					pdfPen = s_pens[KnownColor.PaleTurquoise] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PaleTurquoise);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PaleVioletRed
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PaleVioletRed))
				{
					pdfPen = s_pens[KnownColor.PaleVioletRed] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PaleVioletRed);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PapayaWhip
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PapayaWhip))
				{
					pdfPen = s_pens[KnownColor.PapayaWhip] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PapayaWhip);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PeachPuff
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PeachPuff))
				{
					pdfPen = s_pens[KnownColor.PeachPuff] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PeachPuff);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Peru
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Peru))
				{
					pdfPen = s_pens[KnownColor.Peru] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Peru);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Pink
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Pink))
				{
					pdfPen = s_pens[KnownColor.Pink] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Pink);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Plum
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Plum))
				{
					pdfPen = s_pens[KnownColor.Plum] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Plum);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen PowderBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.PowderBlue))
				{
					pdfPen = s_pens[KnownColor.PowderBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.PowderBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Purple
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Purple))
				{
					pdfPen = s_pens[KnownColor.Purple] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Purple);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Red
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Red))
				{
					pdfPen = s_pens[KnownColor.Red] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Red);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen RosyBrown
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.RosyBrown))
				{
					pdfPen = s_pens[KnownColor.RosyBrown] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.RosyBrown);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen RoyalBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.RoyalBlue))
				{
					pdfPen = s_pens[KnownColor.RoyalBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.RoyalBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SaddleBrown
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SaddleBrown))
				{
					pdfPen = s_pens[KnownColor.SaddleBrown] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SaddleBrown);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Salmon
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Salmon))
				{
					pdfPen = s_pens[KnownColor.Salmon] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Salmon);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SandyBrown
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SandyBrown))
				{
					pdfPen = s_pens[KnownColor.SandyBrown] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SandyBrown);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SeaGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SeaGreen))
				{
					pdfPen = s_pens[KnownColor.SeaGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SeaGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SeaShell
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SeaShell))
				{
					pdfPen = s_pens[KnownColor.SeaShell] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SeaShell);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Sienna
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Sienna))
				{
					pdfPen = s_pens[KnownColor.Sienna] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Sienna);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Silver
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Silver))
				{
					pdfPen = s_pens[KnownColor.Silver] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Silver);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SkyBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SkyBlue))
				{
					pdfPen = s_pens[KnownColor.SkyBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SkyBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SlateBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SlateBlue))
				{
					pdfPen = s_pens[KnownColor.SlateBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SlateBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SlateGray
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SlateGray))
				{
					pdfPen = s_pens[KnownColor.SlateGray] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SlateGray);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Snow
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Snow))
				{
					pdfPen = s_pens[KnownColor.Snow] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Snow);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SpringGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SpringGreen))
				{
					pdfPen = s_pens[KnownColor.SpringGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SpringGreen);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen SteelBlue
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.SteelBlue))
				{
					pdfPen = s_pens[KnownColor.SteelBlue] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.SteelBlue);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Tan
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Tan))
				{
					pdfPen = s_pens[KnownColor.Tan] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Tan);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Teal
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Teal))
				{
					pdfPen = s_pens[KnownColor.Teal] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Teal);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Thistle
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Thistle))
				{
					pdfPen = s_pens[KnownColor.Thistle] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Thistle);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Tomato
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Tomato))
				{
					pdfPen = s_pens[KnownColor.Tomato] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Tomato);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Transparent
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Transparent))
				{
					pdfPen = s_pens[KnownColor.Transparent] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Transparent);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Turquoise
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Turquoise))
				{
					pdfPen = s_pens[KnownColor.Turquoise] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Turquoise);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Violet
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Violet))
				{
					pdfPen = s_pens[KnownColor.Violet] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Violet);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Wheat
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Wheat))
				{
					pdfPen = s_pens[KnownColor.Wheat] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Wheat);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen White
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.White))
				{
					pdfPen = s_pens[KnownColor.White] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.White);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen WhiteSmoke
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.WhiteSmoke))
				{
					pdfPen = s_pens[KnownColor.WhiteSmoke] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.WhiteSmoke);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen Yellow
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.Yellow))
				{
					pdfPen = s_pens[KnownColor.Yellow] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.Yellow);
				}
				return pdfPen;
			}
		}
	}

	public static PdfPen YellowGreen
	{
		get
		{
			lock (s_pens)
			{
				PdfPen pdfPen = null;
				if (s_pens.ContainsKey(KnownColor.YellowGreen))
				{
					pdfPen = s_pens[KnownColor.YellowGreen] as PdfPen;
				}
				if (pdfPen == null)
				{
					pdfPen = GetPen(KnownColor.YellowGreen);
				}
				return pdfPen;
			}
		}
	}

	private static PdfPen GetPen(KnownColor colorName)
	{
		Color color = ColorConverter.FromKnownColor(colorName);
		new PdfColor(color);
		PdfPen pdfPen = new PdfPen(color, immutable: true);
		s_pens[colorName] = pdfPen;
		return pdfPen;
	}

	private PdfPens()
	{
	}
}
