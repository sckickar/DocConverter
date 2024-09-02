using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class CodecWithPredictorTagMethods : TiffTagMethods
{
	public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
	{
		CodecWithPredictor codecWithPredictor = tif.m_currentCodec as CodecWithPredictor;
		if (tag == TiffTag.PREDICTOR)
		{
			codecWithPredictor.SetPredictorValue((Predictor)ap[0].ToByte());
			tif.setFieldBit(66);
			tif.m_flags |= TiffFlags.DIRTYDIRECT;
			return true;
		}
		return codecWithPredictor.GetChildTagMethods()?.SetField(tif, tag, ap) ?? base.SetField(tif, tag, ap);
	}

	public override FieldValue[] GetField(Tiff tif, TiffTag tag)
	{
		CodecWithPredictor codecWithPredictor = tif.m_currentCodec as CodecWithPredictor;
		if (tag == TiffTag.PREDICTOR)
		{
			FieldValue[] array = new FieldValue[1];
			array[0].Set(codecWithPredictor.GetPredictorValue());
			return array;
		}
		TiffTagMethods childTagMethods = codecWithPredictor.GetChildTagMethods();
		if (childTagMethods != null)
		{
			return childTagMethods.GetField(tif, tag);
		}
		return base.GetField(tif, tag);
	}

	public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
	{
		CodecWithPredictor codecWithPredictor = tif.m_currentCodec as CodecWithPredictor;
		if (tif.fieldSet(66))
		{
			Tiff.fprintf(fd, "  Predictor: ");
			Predictor predictorValue = codecWithPredictor.GetPredictorValue();
			switch (predictorValue)
			{
			case Predictor.NONE:
				Tiff.fprintf(fd, "none ");
				break;
			case Predictor.HORIZONTAL:
				Tiff.fprintf(fd, "horizontal differencing ");
				break;
			case Predictor.FLOATINGPOINT:
				Tiff.fprintf(fd, "floating point predictor ");
				break;
			}
			Tiff.fprintf(fd, "{0} (0x{1:x})\r\n", predictorValue, predictorValue);
		}
		TiffTagMethods childTagMethods = codecWithPredictor.GetChildTagMethods();
		if (childTagMethods != null)
		{
			childTagMethods.PrintDir(tif, fd, flags);
		}
		else
		{
			base.PrintDir(tif, fd, flags);
		}
	}
}
