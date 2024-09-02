using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartFillImpl : ShapeFillImpl
{
	private ChartGelFrameRecord m_gel;

	private bool m_invertIfNegative;

	public override double TransparencyFrom
	{
		get
		{
			throw new NotSupportedException("This property doesnt support in chart fill format.");
		}
		set
		{
			throw new NotSupportedException("This property doesnt support in chart fill format.");
		}
	}

	public override double TransparencyTo
	{
		get
		{
			throw new NotSupportedException("This property doesnt support in chart fill format.");
		}
		set
		{
			throw new NotSupportedException("This property doesnt support in chart fill format.");
		}
	}

	public override ChartColor ForeColorObject => (base.Parent as IFillColor).ForeGroundColorObject;

	public override ChartColor BackColorObject => (base.Parent as IFillColor).BackGroundColorObject;

	public override bool Visible
	{
		get
		{
			return (base.Parent as IFillColor).Pattern != OfficePattern.None;
		}
		set
		{
			IFillColor fillColor = base.Parent as IFillColor;
			fillColor.IsAutomaticFormat = false;
			if (value)
			{
				if (fillColor.Pattern == OfficePattern.None)
				{
					fillColor.Pattern = OfficePattern.Solid;
				}
			}
			else
			{
				fillColor.Pattern = OfficePattern.None;
			}
		}
	}

	public bool InvertIfNegative
	{
		get
		{
			return m_invertIfNegative;
		}
		set
		{
			m_invertIfNegative = value;
		}
	}

	public ChartFillImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_gel = (ChartGelFrameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartGelFrame);
		m_bIsShapeFill = false;
	}

	[CLSCompliant(false)]
	public ChartFillImpl(IApplication application, object parent, ChartGelFrameRecord gel)
		: base(application, parent)
	{
		if (gel == null)
		{
			throw new ArgumentNullException("gel");
		}
		m_gel = gel;
		m_bIsShapeFill = false;
		Parse();
	}

	private void Parse()
	{
		IList optionList = m_gel.OptionList;
		int i = 0;
		for (int count = optionList.Count; i < count; i++)
		{
			ParseOption(optionList[i] as MsofbtOPT.FOPTE);
		}
		if (base.ParsePictureData != null)
		{
			ParsePictureOrUserDefinedTexture(m_fillType == OfficeFillType.Picture);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (!(base.Parent as IFillColor).IsAutomaticFormat && Visible)
		{
			m_gel.UpdateToSerialize();
			FopteOptionWrapper opt = new FopteOptionWrapper(m_gel.OptionList);
			Serialize(opt);
			List<BiffRecordRaw> list = ((ChartGelFrameRecord)m_gel.Clone()).UpdatesToAddInStream();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				records.Add(list[i]);
			}
		}
	}

	[CLSCompliant(false)]
	protected override IFopteOptionWrapper SetPicture(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		MemoryStream memoryStream = new MemoryStream();
		m_picture.Save(memoryStream, m_picture.RawFormat);
		byte[] array = memoryStream.ToArray();
		byte[] array2 = new byte[array.Length + 25];
		byte[] obj = new byte[4] { 160, 70, 29, 240 };
		array2[24] = byte.MaxValue;
		array.CopyTo(array2, 25);
		BitConverter.GetBytes(array.Length + 17).CopyTo(array2, 4);
		obj.CopyTo(array2, 0);
		ShapeImpl.SerializeForte(opt, MsoOptions.PatternTexture, 0, array2, isValid: true);
		return opt;
	}

	[CLSCompliant(false)]
	protected override IFopteOptionWrapper SerializeTransparency(IFopteOptionWrapper opt)
	{
		return opt;
	}

	internal override void ChangeVisible()
	{
		if ((base.Parent as IFillColor).IsAutomaticFormat || base.Parent is ChartFrameFormatImpl)
		{
			Visible = true;
		}
		if (base.Parent is ChartWallOrFloorImpl)
		{
			(base.Parent as ChartWallOrFloorImpl).HasShapeProperties = true;
		}
	}
}
