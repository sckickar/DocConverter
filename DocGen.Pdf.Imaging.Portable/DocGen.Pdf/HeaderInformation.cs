using System;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class HeaderInformation
{
	internal class SIZ
	{
		private HeaderInformation enclosingInstance;

		public int lsiz;

		public int rsiz;

		public int xsiz;

		public int ysiz;

		public int x0siz;

		public int y0siz;

		public int xtsiz;

		public int ytsiz;

		public int xt0siz;

		public int yt0siz;

		public int csiz;

		public int[] ssiz;

		public int[] xrsiz;

		public int[] yrsiz;

		private int[] compWidth;

		private int maxCompWidth = -1;

		private int[] compHeight;

		private int maxCompHeight = -1;

		private int numTiles = -1;

		private bool[] origSigned;

		private int[] origBitDepth;

		public virtual int MaxCompWidth
		{
			get
			{
				if (compWidth == null)
				{
					compWidth = new int[csiz];
					for (int i = 0; i < csiz; i++)
					{
						compWidth[i] = (int)(Math.Ceiling((double)xsiz / (double)xrsiz[i]) - Math.Ceiling((double)x0siz / (double)xrsiz[i]));
					}
				}
				if (maxCompWidth == -1)
				{
					for (int j = 0; j < csiz; j++)
					{
						if (compWidth[j] > maxCompWidth)
						{
							maxCompWidth = compWidth[j];
						}
					}
				}
				return maxCompWidth;
			}
		}

		public virtual int MaxCompHeight
		{
			get
			{
				if (compHeight == null)
				{
					compHeight = new int[csiz];
					for (int i = 0; i < csiz; i++)
					{
						compHeight[i] = (int)(Math.Ceiling((double)ysiz / (double)yrsiz[i]) - Math.Ceiling((double)y0siz / (double)yrsiz[i]));
					}
				}
				if (maxCompHeight == -1)
				{
					for (int j = 0; j < csiz; j++)
					{
						if (compHeight[j] != maxCompHeight)
						{
							maxCompHeight = compHeight[j];
						}
					}
				}
				return maxCompHeight;
			}
		}

		public virtual int NumTiles
		{
			get
			{
				if (numTiles == -1)
				{
					numTiles = (xsiz - xt0siz + xtsiz - 1) / xtsiz * ((ysiz - yt0siz + ytsiz - 1) / ytsiz);
				}
				return numTiles;
			}
		}

		public virtual SIZ Copy
		{
			get
			{
				SIZ result = null;
				try
				{
					result = (SIZ)Clone();
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public SIZ(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public virtual int getCompImgWidth(int c)
		{
			if (compWidth == null)
			{
				compWidth = new int[csiz];
				for (int i = 0; i < csiz; i++)
				{
					compWidth[i] = (int)(Math.Ceiling((double)xsiz / (double)xrsiz[i]) - Math.Ceiling((double)x0siz / (double)xrsiz[i]));
				}
			}
			return compWidth[c];
		}

		public virtual int getCompImgHeight(int c)
		{
			if (compHeight == null)
			{
				compHeight = new int[csiz];
				for (int i = 0; i < csiz; i++)
				{
					compHeight[i] = (int)(Math.Ceiling((double)ysiz / (double)yrsiz[i]) - Math.Ceiling((double)y0siz / (double)yrsiz[i]));
				}
			}
			return compHeight[c];
		}

		public virtual bool isOrigSigned(int c)
		{
			if (origSigned == null)
			{
				origSigned = new bool[csiz];
				for (int i = 0; i < csiz; i++)
				{
					origSigned[i] = SupportClass.URShift(ssiz[i], 7) == 1;
				}
			}
			return origSigned[c];
		}

		public virtual int getOrigBitDepth(int c)
		{
			if (origBitDepth == null)
			{
				origBitDepth = new int[csiz];
				for (int i = 0; i < csiz; i++)
				{
					origBitDepth[i] = (ssiz[i] & 0x7F) + 1;
				}
			}
			return origBitDepth[c];
		}

		public override string ToString()
		{
			string text = "\n --- SIZ (" + lsiz + " bytes) ---\n";
			text = text + " Capabilities : " + rsiz + "\n";
			text = text + " Image dim.   : " + (xsiz - x0siz) + "x" + (ysiz - y0siz) + ", (off=" + x0siz + "," + y0siz + ")\n";
			text = text + " Tile dim.    : " + xtsiz + "x" + ytsiz + ", (off=" + xt0siz + "," + yt0siz + ")\n";
			text = text + " Component(s) : " + csiz + "\n";
			text += " Orig. depth  : ";
			for (int i = 0; i < csiz; i++)
			{
				text = text + getOrigBitDepth(i) + " ";
			}
			text += "\n";
			text += " Orig. signed : ";
			for (int j = 0; j < csiz; j++)
			{
				text = text + isOrigSigned(j) + " ";
			}
			text += "\n";
			text += " Subs. factor : ";
			for (int k = 0; k < csiz; k++)
			{
				text = text + xrsiz[k] + "," + yrsiz[k] + " ";
			}
			return text + "\n";
		}

		public virtual object Clone()
		{
			return null;
		}
	}

	internal class SOT
	{
		private HeaderInformation enclosingInstance;

		public int lsot;

		public int isot;

		public int psot;

		public int tpsot;

		public int tnsot;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public SOT(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("\n --- SOT (" + lsot + " bytes) ---\n", "Tile index         : ", isot.ToString(), "\n"), "Tile-part length   : ", psot.ToString(), " bytes\n"), "Tile-part index    : ", tpsot.ToString(), "\n"), "Num. of tile-parts : ", tnsot.ToString(), "\n"), "\n");
		}
	}

	internal class COD
	{
		private HeaderInformation enclosingInstance;

		public int lcod;

		public int scod;

		public int sgcod_po;

		public int sgcod_nl;

		public int sgcod_mct;

		public int spcod_ndl;

		public int spcod_cw;

		public int spcod_ch;

		public int spcod_cs;

		public int[] spcod_t = new int[1];

		public int[] spcod_ps;

		public virtual COD Copy
		{
			get
			{
				COD result = null;
				try
				{
					result = (COD)Clone();
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public COD(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- COD (" + lcod + " bytes) ---\n";
			text += " Coding style   : ";
			if (scod == 0)
			{
				text += "Default";
			}
			else
			{
				if (((uint)scod & (true ? 1u : 0u)) != 0)
				{
					text += "Precints ";
				}
				if (((uint)scod & 2u) != 0)
				{
					text += "SOP ";
				}
				if (((uint)scod & 4u) != 0)
				{
					text += "EPH ";
				}
				int num = (((scod & 8) != 0) ? 1 : 0);
				int num2 = (((scod & 0x10) != 0) ? 1 : 0);
				if (num != 0 || num2 != 0)
				{
					text += "Code-blocks offset";
					text = text + "\n Cblk partition : " + num + "," + num2;
				}
			}
			text += "\n";
			text += " Cblk style     : ";
			if (spcod_cs == 0)
			{
				text += "Default";
			}
			else
			{
				if (((uint)spcod_cs & (true ? 1u : 0u)) != 0)
				{
					text += "Bypass ";
				}
				if (((uint)spcod_cs & 2u) != 0)
				{
					text += "Reset ";
				}
				if (((uint)spcod_cs & 4u) != 0)
				{
					text += "Terminate ";
				}
				if (((uint)spcod_cs & 8u) != 0)
				{
					text += "Vert_causal ";
				}
				if (((uint)spcod_cs & 0x10u) != 0)
				{
					text += "Predict ";
				}
				if (((uint)spcod_cs & 0x20u) != 0)
				{
					text += "Seg_symb ";
				}
			}
			text += "\n";
			text = text + " Num. of levels : " + spcod_ndl + "\n";
			switch (sgcod_po)
			{
			case 0:
				text += " Progress. type : LY_RES_COMP_POS_PROG\n";
				break;
			case 1:
				text += " Progress. type : RES_LY_COMP_POS_PROG\n";
				break;
			case 2:
				text += " Progress. type : RES_POS_COMP_LY_PROG\n";
				break;
			case 3:
				text += " Progress. type : POS_COMP_RES_LY_PROG\n";
				break;
			case 4:
				text += " Progress. type : COMP_POS_RES_LY_PROG\n";
				break;
			}
			text = text + " Num. of layers : " + sgcod_nl + "\n";
			text = text + " Cblk dimension : " + (1 << spcod_cw + 2) + "x" + (1 << spcod_ch + 2) + "\n";
			switch (spcod_t[0])
			{
			case 0:
				text += " Filter         : 9-7 irreversible\n";
				break;
			case 1:
				text += " Filter         : 5-3 reversible\n";
				break;
			}
			text = text + " Multi comp tr. : " + (sgcod_mct == 1) + "\n";
			if (spcod_ps != null)
			{
				text += " Precincts      : ";
				for (int i = 0; i < spcod_ps.Length; i++)
				{
					text = text + (1 << (spcod_ps[i] & 0xF)) + "x" + (1 << ((spcod_ps[i] & 0xF0) >> 4)) + " ";
				}
			}
			return text + "\n";
		}

		public virtual object Clone()
		{
			return null;
		}
	}

	internal class COC
	{
		private HeaderInformation enclosingInstance;

		public int lcoc;

		public int ccoc;

		public int scoc;

		public int spcoc_ndl;

		public int spcoc_cw;

		public int spcoc_ch;

		public int spcoc_cs;

		public int[] spcoc_t = new int[1];

		public int[] spcoc_ps;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public COC(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- COC (" + lcoc + " bytes) ---\n";
			text = text + " Component      : " + ccoc + "\n";
			text += " Coding style   : ";
			if (scoc == 0)
			{
				text += "Default";
			}
			else
			{
				if (((uint)scoc & (true ? 1u : 0u)) != 0)
				{
					text += "Precints ";
				}
				if (((uint)scoc & 2u) != 0)
				{
					text += "SOP ";
				}
				if (((uint)scoc & 4u) != 0)
				{
					text += "EPH ";
				}
			}
			text += "\n";
			text += " Cblk style     : ";
			if (spcoc_cs == 0)
			{
				text += "Default";
			}
			else
			{
				if (((uint)spcoc_cs & (true ? 1u : 0u)) != 0)
				{
					text += "Bypass ";
				}
				if (((uint)spcoc_cs & 2u) != 0)
				{
					text += "Reset ";
				}
				if (((uint)spcoc_cs & 4u) != 0)
				{
					text += "Terminate ";
				}
				if (((uint)spcoc_cs & 8u) != 0)
				{
					text += "Vert_causal ";
				}
				if (((uint)spcoc_cs & 0x10u) != 0)
				{
					text += "Predict ";
				}
				if (((uint)spcoc_cs & 0x20u) != 0)
				{
					text += "Seg_symb ";
				}
			}
			text += "\n";
			text = text + " Num. of levels : " + spcoc_ndl + "\n";
			text = text + " Cblk dimension : " + (1 << spcoc_cw + 2) + "x" + (1 << spcoc_ch + 2) + "\n";
			switch (spcoc_t[0])
			{
			case 0:
				text += " Filter         : 9-7 irreversible\n";
				break;
			case 1:
				text += " Filter         : 5-3 reversible\n";
				break;
			}
			if (spcoc_ps != null)
			{
				text += " Precincts      : ";
				for (int i = 0; i < spcoc_ps.Length; i++)
				{
					text = text + (1 << (spcoc_ps[i] & 0xF)) + "x" + (1 << ((spcoc_ps[i] & 0xF0) >> 4)) + " ";
				}
			}
			return text + "\n";
		}
	}

	internal class RGN
	{
		private HeaderInformation enclosingInstance;

		public int lrgn;

		public int crgn;

		public int srgn;

		public int sprgn;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public RGN(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- RGN (" + lrgn + " bytes) ---\n";
			text = text + " Component : " + crgn + "\n";
			text = ((srgn != 0) ? (text + " ROI style : Unsupported\n") : (text + " ROI style : Implicit\n"));
			text = text + " ROI shift : " + sprgn + "\n";
			return text + "\n";
		}
	}

	internal class QCD
	{
		private HeaderInformation enclosingInstance;

		public int lqcd;

		public int sqcd;

		public int[][] spqcd;

		private int qType = -1;

		private int gb = -1;

		public virtual int QuantType
		{
			get
			{
				if (qType == -1)
				{
					qType = sqcd & -225;
				}
				return qType;
			}
		}

		public virtual int NumGuardBits
		{
			get
			{
				if (gb == -1)
				{
					gb = (sqcd >> 5) & 7;
				}
				return gb;
			}
		}

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public QCD(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- QCD (" + lqcd + " bytes) ---\n";
			text += " Quant. type    : ";
			int quantType = QuantType;
			switch (quantType)
			{
			case 0:
				text += "No quantization \n";
				break;
			case 1:
				text += "Scalar derived\n";
				break;
			case 2:
				text += "Scalar expounded\n";
				break;
			}
			text = text + " Guard bits     : " + NumGuardBits + "\n";
			if (quantType == 0)
			{
				text += " Exponents   :\n";
				for (int i = 0; i < spqcd.Length; i++)
				{
					for (int j = 0; j < spqcd[i].Length; j++)
					{
						if (i == 0 && j == 0)
						{
							text = text + "\tr=0 : " + ((spqcd[0][0] >> 3) & 0x1F) + "\n";
						}
						else if (i != 0 && j > 0)
						{
							int num = (spqcd[i][j] >> 3) & 0x1F;
							text = text + "\tr=" + i + ",s=" + j + " : " + num + "\n";
						}
					}
				}
			}
			else
			{
				text += " Exp / Mantissa : \n";
				for (int k = 0; k < spqcd.Length; k++)
				{
					for (int l = 0; l < spqcd[k].Length; l++)
					{
						if (k == 0 && l == 0)
						{
							int num2 = (spqcd[0][0] >> 11) & 0x1F;
							double num3 = (-1f - (float)(spqcd[0][0] & 0x7FF) / 2048f) / (float)(-1 << num2);
							text = text + "\tr=0 : " + num2 + " / " + num3 + "\n";
						}
						else if (k != 0 && l > 0)
						{
							int num2 = (spqcd[k][l] >> 11) & 0x1F;
							double num3 = (-1f - (float)(spqcd[k][l] & 0x7FF) / 2048f) / (float)(-1 << num2);
							text = text + "\tr=" + k + ",s=" + l + " : " + num2 + " / " + num3 + "\n";
						}
					}
				}
			}
			return text + "\n";
		}
	}

	internal class QCC
	{
		private HeaderInformation enclosingInstance;

		public int lqcc;

		public int cqcc;

		public int sqcc;

		public int[][] spqcc;

		private int qType = -1;

		private int gb = -1;

		public virtual int QuantType
		{
			get
			{
				if (qType == -1)
				{
					qType = sqcc & -225;
				}
				return qType;
			}
		}

		public virtual int NumGuardBits
		{
			get
			{
				if (gb == -1)
				{
					gb = (sqcc >> 5) & 7;
				}
				return gb;
			}
		}

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public QCC(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- QCC (" + lqcc + " bytes) ---\n";
			text = text + " Component      : " + cqcc + "\n";
			text += " Quant. type    : ";
			int quantType = QuantType;
			switch (quantType)
			{
			case 0:
				text += "No quantization \n";
				break;
			case 1:
				text += "Scalar derived\n";
				break;
			case 2:
				text += "Scalar expounded\n";
				break;
			}
			text = text + " Guard bits     : " + NumGuardBits + "\n";
			if (quantType == 0)
			{
				text += " Exponents   :\n";
				for (int i = 0; i < spqcc.Length; i++)
				{
					for (int j = 0; j < spqcc[i].Length; j++)
					{
						if (i == 0 && j == 0)
						{
							text = text + "\tr=0 : " + ((spqcc[0][0] >> 3) & 0x1F) + "\n";
						}
						else if (i != 0 && j > 0)
						{
							int num = (spqcc[i][j] >> 3) & 0x1F;
							text = text + "\tr=" + i + ",s=" + j + " : " + num + "\n";
						}
					}
				}
			}
			else
			{
				text += " Exp / Mantissa : \n";
				for (int k = 0; k < spqcc.Length; k++)
				{
					for (int l = 0; l < spqcc[k].Length; l++)
					{
						if (k == 0 && l == 0)
						{
							int num2 = (spqcc[0][0] >> 11) & 0x1F;
							double num3 = (-1f - (float)(spqcc[0][0] & 0x7FF) / 2048f) / (float)(-1 << num2);
							text = text + "\tr=0 : " + num2 + " / " + num3 + "\n";
						}
						else if (k != 0 && l > 0)
						{
							int num2 = (spqcc[k][l] >> 11) & 0x1F;
							double num3 = (-1f - (float)(spqcc[k][l] & 0x7FF) / 2048f) / (float)(-1 << num2);
							text = text + "\tr=" + k + ",s=" + l + " : " + num2 + " / " + num3 + "\n";
						}
					}
				}
			}
			return text + "\n";
		}
	}

	internal class POC
	{
		private HeaderInformation enclosingInstance;

		public int lpoc;

		public int[] rspoc;

		public int[] cspoc;

		public int[] lyepoc;

		public int[] repoc;

		public int[] cepoc;

		public int[] ppoc;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public POC(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- POC (" + lpoc + " bytes) ---\n";
			text += " Chg_idx RSpoc CSpoc LYEpoc REpoc CEpoc Ppoc\n";
			for (int i = 0; i < rspoc.Length; i++)
			{
				text = text + "   " + i + "      " + rspoc[i] + "     " + cspoc[i] + "     " + lyepoc[i] + "      " + repoc[i] + "     " + cepoc[i];
				switch (ppoc[i])
				{
				case 0:
					text += "  LY_RES_COMP_POS_PROG\n";
					break;
				case 1:
					text += "  RES_LY_COMP_POS_PROG\n";
					break;
				case 2:
					text += "  RES_POS_COMP_LY_PROG\n";
					break;
				case 3:
					text += "  POS_COMP_RES_LY_PROG\n";
					break;
				case 4:
					text += "  COMP_POS_RES_LY_PROG\n";
					break;
				}
			}
			return text + "\n";
		}
	}

	internal class CRG
	{
		private HeaderInformation enclosingInstance;

		public int lcrg;

		public int[] xcrg;

		public int[] ycrg;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public CRG(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- CRG (" + lcrg + " bytes) ---\n";
			for (int i = 0; i < xcrg.Length; i++)
			{
				text = text + " Component " + i + " offset : " + xcrg[i] + "," + ycrg[i] + "\n";
			}
			return text + "\n";
		}
	}

	internal class COM
	{
		private HeaderInformation enclosingInstance;

		public int lcom;

		public int rcom;

		public byte[] ccom;

		public HeaderInformation Enclosing_Instance => enclosingInstance;

		public COM(HeaderInformation enclosingInstance)
		{
			InitBlock(enclosingInstance);
		}

		private void InitBlock(HeaderInformation enclosingInstance)
		{
			this.enclosingInstance = enclosingInstance;
		}

		public override string ToString()
		{
			string text = "\n --- COM (" + lcom + " bytes) ---\n";
			text = ((rcom == 0) ? (text + " Registration : General use (binary values)\n") : ((rcom != 1) ? (text + " Registration : Unknown\n") : (text + " Registration : General use (IS 8859-15:1999 (Latin) values)\n")));
			return text + "\n";
		}
	}

	public SIZ sizValue;

	public Dictionary<object, object> sotValue = new Dictionary<object, object>();

	public Dictionary<object, object> codValue = new Dictionary<object, object>();

	public Dictionary<object, object> cocValue = new Dictionary<object, object>();

	public Dictionary<object, object> rgnValue = new Dictionary<object, object>();

	public Dictionary<object, object> qcdValue = new Dictionary<object, object>();

	public Dictionary<object, object> qccValue = new Dictionary<object, object>();

	public Dictionary<object, object> pocValue = new Dictionary<object, object>();

	public CRG crgValue;

	public Dictionary<object, object> comValue = new Dictionary<object, object>();

	private int ncom;

	public virtual SIZ NewSIZ => new SIZ(this);

	public virtual SOT NewSOT => new SOT(this);

	public virtual COD NewCOD => new COD(this);

	public virtual COC NewCOC => new COC(this);

	public virtual RGN NewRGN => new RGN(this);

	public virtual QCD NewQCD => new QCD(this);

	public virtual QCC NewQCC => new QCC(this);

	public virtual POC NewPOC => new POC(this);

	public virtual CRG NewCRG => new CRG(this);

	public virtual COM NewCOM
	{
		get
		{
			ncom++;
			return new COM(this);
		}
	}

	public virtual int NumCOM => ncom;

	public virtual string toStringMainHeader()
	{
		int csiz = sizValue.csiz;
		string text = sizValue?.ToString() ?? "";
		if (codValue["main"] != null)
		{
			text += (COD)codValue["main"];
		}
		for (int i = 0; i < csiz; i++)
		{
			if (cocValue["main_c" + i] != null)
			{
				text += (COC)cocValue["main_c" + i];
			}
		}
		if (qcdValue["main"] != null)
		{
			text += (QCD)qcdValue["main"];
		}
		for (int j = 0; j < csiz; j++)
		{
			if (qccValue["main_c" + j] != null)
			{
				text += (QCC)qccValue["main_c" + j];
			}
		}
		for (int k = 0; k < csiz; k++)
		{
			if (rgnValue["main_c" + k] != null)
			{
				text += (RGN)rgnValue["main_c" + k];
			}
		}
		if (pocValue["main"] != null)
		{
			text += (POC)pocValue["main"];
		}
		if (crgValue != null)
		{
			text += crgValue;
		}
		for (int l = 0; l < ncom; l++)
		{
			if (comValue["main_" + l] != null)
			{
				text += (COM)comValue["main_" + l];
			}
		}
		return text;
	}

	public virtual string toStringTileHeader(int t, int ntp)
	{
		int csiz = sizValue.csiz;
		string text = "";
		for (int i = 0; i < ntp; i++)
		{
			text = text + "Tile-part " + i + ", tile " + t + ":\n";
			text += (SOT)sotValue["t" + t + "_tp" + i];
		}
		if (codValue["t" + t] != null)
		{
			text += (COD)codValue["t" + t];
		}
		for (int j = 0; j < csiz; j++)
		{
			if (cocValue["t" + t + "_c" + j] != null)
			{
				text += (COC)cocValue["t" + t + "_c" + j];
			}
		}
		if (qcdValue["t" + t] != null)
		{
			text += (QCD)qcdValue["t" + t];
		}
		for (int k = 0; k < csiz; k++)
		{
			if (qccValue["t" + t + "_c" + k] != null)
			{
				text += (QCC)qccValue["t" + t + "_c" + k];
			}
		}
		for (int l = 0; l < csiz; l++)
		{
			if (rgnValue["t" + t + "_c" + l] != null)
			{
				text += (RGN)rgnValue["t" + t + "_c" + l];
			}
		}
		if (pocValue["t" + t] != null)
		{
			text += (POC)pocValue["t" + t];
		}
		return text;
	}

	public virtual string toStringThNoSOT(int t, int ntp)
	{
		int csiz = sizValue.csiz;
		string text = "";
		if (codValue["t" + t] != null)
		{
			text += (COD)codValue["t" + t];
		}
		for (int i = 0; i < csiz; i++)
		{
			if (cocValue["t" + t + "_c" + i] != null)
			{
				text += (COC)cocValue["t" + t + "_c" + i];
			}
		}
		if (qcdValue["t" + t] != null)
		{
			text += (QCD)qcdValue["t" + t];
		}
		for (int j = 0; j < csiz; j++)
		{
			if (qccValue["t" + t + "_c" + j] != null)
			{
				text += (QCC)qccValue["t" + t + "_c" + j];
			}
		}
		for (int k = 0; k < csiz; k++)
		{
			if (rgnValue["t" + t + "_c" + k] != null)
			{
				text += (RGN)rgnValue["t" + t + "_c" + k];
			}
		}
		if (pocValue["t" + t] != null)
		{
			text += (POC)pocValue["t" + t];
		}
		return text;
	}

	public virtual HeaderInformation getCopy(int nt)
	{
		HeaderInformation headerInformation = null;
		try
		{
			headerInformation = (HeaderInformation)Clone();
		}
		catch (Exception)
		{
		}
		headerInformation.sizValue = sizValue.Copy;
		if (codValue["main"] != null)
		{
			COD cOD = (COD)codValue["main"];
			headerInformation.codValue["main"] = cOD.Copy;
		}
		for (int i = 0; i < nt; i++)
		{
			if (codValue["t" + i] != null)
			{
				COD cOD2 = (COD)codValue["t" + i];
				headerInformation.codValue["t" + i] = cOD2.Copy;
			}
		}
		return headerInformation;
	}

	public virtual object Clone()
	{
		return null;
	}
}
