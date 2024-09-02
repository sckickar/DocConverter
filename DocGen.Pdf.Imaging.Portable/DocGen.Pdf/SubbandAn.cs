using System;

namespace DocGen.Pdf;

internal class SubbandAn : Subband
{
	public SubbandAn parentband;

	public SubbandAn subb_LL;

	public SubbandAn subb_HL;

	public SubbandAn subb_LH;

	public SubbandAn subb_HH;

	public AnWTFilter hFilter;

	public AnWTFilter vFilter;

	public float l2Norm = -1f;

	public float stepWMSE;

	public override Subband Parent => parentband;

	public override Subband LL => subb_LL;

	public override Subband HL => subb_HL;

	public override Subband LH => subb_LH;

	public override Subband HH => subb_HH;

	internal override WaveletFilter HorWFilter => hFilter;

	internal override WaveletFilter VerWFilter => hFilter;

	public SubbandAn()
	{
	}

	internal SubbandAn(int w, int h, int ulcx, int ulcy, int lvls, WaveletFilter[] hfilters, WaveletFilter[] vfilters)
		: base(w, h, ulcx, ulcy, lvls, hfilters, vfilters)
	{
		calcL2Norms();
	}

	internal override Subband split(WaveletFilter hfilter, WaveletFilter vfilter)
	{
		if (isNode)
		{
			throw new ArgumentException();
		}
		isNode = true;
		hFilter = (AnWTFilter)hfilter;
		vFilter = (AnWTFilter)vfilter;
		subb_LL = new SubbandAn();
		subb_LH = new SubbandAn();
		subb_HL = new SubbandAn();
		subb_HH = new SubbandAn();
		subb_LL.parentband = this;
		subb_HL.parentband = this;
		subb_LH.parentband = this;
		subb_HH.parentband = this;
		initChilds();
		return subb_LL;
	}

	private void calcBasisWaveForms(float[][] wfs)
	{
		if (!(l2Norm < 0f))
		{
			return;
		}
		if (isNode)
		{
			if (subb_LL.l2Norm < 0f)
			{
				subb_LL.calcBasisWaveForms(wfs);
				wfs[0] = hFilter.getLPSynWaveForm(wfs[0], null);
				wfs[1] = vFilter.getLPSynWaveForm(wfs[1], null);
			}
			else if (subb_HL.l2Norm < 0f)
			{
				subb_HL.calcBasisWaveForms(wfs);
				wfs[0] = hFilter.getHPSynWaveForm(wfs[0], null);
				wfs[1] = vFilter.getLPSynWaveForm(wfs[1], null);
			}
			else if (subb_LH.l2Norm < 0f)
			{
				subb_LH.calcBasisWaveForms(wfs);
				wfs[0] = hFilter.getLPSynWaveForm(wfs[0], null);
				wfs[1] = vFilter.getHPSynWaveForm(wfs[1], null);
			}
			else if (subb_HH.l2Norm < 0f)
			{
				subb_HH.calcBasisWaveForms(wfs);
				wfs[0] = hFilter.getHPSynWaveForm(wfs[0], null);
				wfs[1] = vFilter.getHPSynWaveForm(wfs[1], null);
			}
		}
		else
		{
			wfs[0] = new float[1];
			wfs[0][0] = 1f;
			wfs[1] = new float[1];
			wfs[1][0] = 1f;
		}
	}

	private void assignL2Norm(float l2n)
	{
		if (!(l2Norm < 0f))
		{
			return;
		}
		if (isNode)
		{
			if (subb_LL.l2Norm < 0f)
			{
				subb_LL.assignL2Norm(l2n);
			}
			else if (subb_HL.l2Norm < 0f)
			{
				subb_HL.assignL2Norm(l2n);
			}
			else if (subb_LH.l2Norm < 0f)
			{
				subb_LH.assignL2Norm(l2n);
			}
			else if (subb_HH.l2Norm < 0f)
			{
				subb_HH.assignL2Norm(l2n);
				if (subb_HH.l2Norm >= 0f)
				{
					l2Norm = 0f;
				}
			}
		}
		else
		{
			l2Norm = l2n;
		}
	}

	private void calcL2Norms()
	{
		float[][] array = new float[2][];
		while (l2Norm < 0f)
		{
			calcBasisWaveForms(array);
			double num = 0.0;
			for (int num2 = array[0].Length - 1; num2 >= 0; num2--)
			{
				num += (double)(array[0][num2] * array[0][num2]);
			}
			float num3 = (float)Math.Sqrt(num);
			num = 0.0;
			for (int num2 = array[1].Length - 1; num2 >= 0; num2--)
			{
				num += (double)(array[1][num2] * array[1][num2]);
			}
			num3 *= (float)Math.Sqrt(num);
			array[0] = null;
			array[1] = null;
			assignL2Norm(num3);
		}
	}
}
