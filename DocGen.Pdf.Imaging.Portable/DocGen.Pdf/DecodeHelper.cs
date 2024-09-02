using System;

namespace DocGen.Pdf;

internal class DecodeHelper
{
	public ModuleSpec iccs;

	public MaxShiftSpec rois;

	public QuantTypeSpec qts;

	public QuantStepSizeSpec qsss;

	internal GuardBitsSpec gbs;

	public SynWTFilterSpec wfs;

	public IntegerSpec dls;

	public IntegerSpec nls;

	public IntegerSpec pos;

	public ModuleSpec ecopts;

	public CompTransfSpec cts;

	public ModuleSpec pcs;

	public ModuleSpec ers;

	public PrecinctSizeSpec pss;

	public ModuleSpec sops;

	public ModuleSpec ephs;

	public CBlkSizeSpec cblks;

	public ModuleSpec pphs;

	public virtual DecodeHelper Copy
	{
		get
		{
			DecodeHelper decodeHelper = null;
			try
			{
				decodeHelper = (DecodeHelper)Clone();
			}
			catch (Exception)
			{
			}
			decodeHelper.qts = (QuantTypeSpec)qts.Copy;
			decodeHelper.qsss = (QuantStepSizeSpec)qsss.Copy;
			decodeHelper.gbs = (GuardBitsSpec)gbs.Copy;
			decodeHelper.wfs = (SynWTFilterSpec)wfs.Copy;
			decodeHelper.dls = (IntegerSpec)dls.Copy;
			decodeHelper.cts = (CompTransfSpec)cts.Copy;
			if (rois != null)
			{
				decodeHelper.rois = (MaxShiftSpec)rois.Copy;
			}
			return decodeHelper;
		}
	}

	public DecodeHelper(int nt, int nc)
	{
		qts = new QuantTypeSpec(nt, nc, 2);
		qsss = new QuantStepSizeSpec(nt, nc, 2);
		gbs = new GuardBitsSpec(nt, nc, 2);
		wfs = new SynWTFilterSpec(nt, nc, 2);
		dls = new IntegerSpec(nt, nc, 2);
		cts = new CompTransfSpec(nt, nc, 2);
		ecopts = new ModuleSpec(nt, nc, 2);
		ers = new ModuleSpec(nt, nc, 2);
		cblks = new CBlkSizeSpec(nt, nc, 2);
		pss = new PrecinctSizeSpec(nt, nc, 2, dls);
		nls = new IntegerSpec(nt, nc, 1);
		pos = new IntegerSpec(nt, nc, 1);
		pcs = new ModuleSpec(nt, nc, 1);
		sops = new ModuleSpec(nt, nc, 1);
		ephs = new ModuleSpec(nt, nc, 1);
		pphs = new ModuleSpec(nt, nc, 1);
		iccs = new ModuleSpec(nt, nc, 1);
		pphs.setDefault(false);
	}

	public virtual object Clone()
	{
		return null;
	}
}
