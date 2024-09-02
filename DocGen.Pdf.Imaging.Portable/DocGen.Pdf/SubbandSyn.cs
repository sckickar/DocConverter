using System;

namespace DocGen.Pdf;

internal class SubbandSyn : Subband
{
	private SubbandSyn parent;

	private SubbandSyn subb_LL;

	private SubbandSyn subb_HL;

	private SubbandSyn subb_LH;

	private SubbandSyn subb_HH;

	public SynWTFilter hFilter;

	public SynWTFilter vFilter;

	public int magbits;

	public override Subband Parent => parent;

	public override Subband LL => subb_LL;

	public override Subband HL => subb_HL;

	public override Subband LH => subb_LH;

	public override Subband HH => subb_HH;

	internal override WaveletFilter HorWFilter => hFilter;

	internal override WaveletFilter VerWFilter => hFilter;

	public SubbandSyn()
	{
	}

	internal SubbandSyn(int w, int h, int ulcx, int ulcy, int lvls, WaveletFilter[] hfilters, WaveletFilter[] vfilters)
		: base(w, h, ulcx, ulcy, lvls, hfilters, vfilters)
	{
	}

	internal override Subband split(WaveletFilter hfilter, WaveletFilter vfilter)
	{
		if (isNode)
		{
			throw new ArgumentException();
		}
		isNode = true;
		hFilter = (SynWTFilter)hfilter;
		vFilter = (SynWTFilter)vfilter;
		subb_LL = new SubbandSyn();
		subb_LH = new SubbandSyn();
		subb_HL = new SubbandSyn();
		subb_HH = new SubbandSyn();
		subb_LL.parent = this;
		subb_HL.parent = this;
		subb_LH.parent = this;
		subb_HH.parent = this;
		initChilds();
		return subb_LL;
	}
}
