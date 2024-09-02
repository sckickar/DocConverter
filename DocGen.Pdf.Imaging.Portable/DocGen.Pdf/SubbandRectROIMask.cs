namespace DocGen.Pdf;

internal class SubbandRectROIMask : SubbandROIMask
{
	public int[] ulxs;

	public int[] ulys;

	public int[] lrxs;

	public int[] lrys;

	public SubbandRectROIMask(Subband sb, int[] ulxs, int[] ulys, int[] lrxs, int[] lrys, int nr)
		: base(sb.ulx, sb.uly, sb.w, sb.h)
	{
		this.ulxs = ulxs;
		this.ulys = ulys;
		this.lrxs = lrxs;
		this.lrys = lrys;
		if (!sb.isNode)
		{
			return;
		}
		isNode = true;
		int num = sb.ulcx % 2;
		int num2 = sb.ulcy % 2;
		WaveletFilter horWFilter = sb.HorWFilter;
		WaveletFilter verWFilter = sb.VerWFilter;
		int synLowNegSupport = horWFilter.SynLowNegSupport;
		int synHighNegSupport = horWFilter.SynHighNegSupport;
		int synLowPosSupport = horWFilter.SynLowPosSupport;
		int synHighPosSupport = horWFilter.SynHighPosSupport;
		int synLowNegSupport2 = verWFilter.SynLowNegSupport;
		int synHighNegSupport2 = verWFilter.SynHighNegSupport;
		int synLowPosSupport2 = verWFilter.SynLowPosSupport;
		int synHighPosSupport2 = verWFilter.SynHighPosSupport;
		int[] array = new int[nr];
		int[] array2 = new int[nr];
		int[] array3 = new int[nr];
		int[] array4 = new int[nr];
		int[] array5 = new int[nr];
		int[] array6 = new int[nr];
		int[] array7 = new int[nr];
		int[] array8 = new int[nr];
		for (int num3 = nr - 1; num3 >= 0; num3--)
		{
			int num4 = ulxs[num3];
			if (num == 0)
			{
				array[num3] = (num4 + 1 - synLowNegSupport) / 2;
				array5[num3] = (num4 - synHighNegSupport) / 2;
			}
			else
			{
				array[num3] = (num4 - synLowNegSupport) / 2;
				array5[num3] = (num4 + 1 - synHighNegSupport) / 2;
			}
			int num5 = ulys[num3];
			if (num2 == 0)
			{
				array2[num3] = (num5 + 1 - synLowNegSupport2) / 2;
				array6[num3] = (num5 - synHighNegSupport2) / 2;
			}
			else
			{
				array2[num3] = (num5 - synLowNegSupport2) / 2;
				array6[num3] = (num5 + 1 - synHighNegSupport2) / 2;
			}
			num4 = lrxs[num3];
			if (num == 0)
			{
				array3[num3] = (num4 + synLowPosSupport) / 2;
				array7[num3] = (num4 - 1 + synHighPosSupport) / 2;
			}
			else
			{
				array3[num3] = (num4 - 1 + synLowPosSupport) / 2;
				array7[num3] = (num4 + synHighPosSupport) / 2;
			}
			num5 = lrys[num3];
			if (num2 == 0)
			{
				array4[num3] = (num5 + synLowPosSupport2) / 2;
				array8[num3] = (num5 - 1 + synHighPosSupport2) / 2;
			}
			else
			{
				array4[num3] = (num5 - 1 + synLowPosSupport2) / 2;
				array8[num3] = (num5 + synHighPosSupport2) / 2;
			}
		}
		hh = new SubbandRectROIMask(sb.HH, array5, array6, array7, array8, nr);
		lh = new SubbandRectROIMask(sb.LH, array, array6, array3, array8, nr);
		hl = new SubbandRectROIMask(sb.HL, array5, array2, array7, array4, nr);
		ll = new SubbandRectROIMask(sb.LL, array, array2, array3, array4, nr);
	}
}
