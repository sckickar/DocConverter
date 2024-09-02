using System;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtDgContainer : BaseContainer
{
	private ShapeDocType m_shapeDocType;

	internal MsofbtDg Dg => FindContainerByType(typeof(MsofbtDg)) as MsofbtDg;

	internal MsofbtSpgrContainer PatriarchGroupContainer => FindContainerByType(typeof(MsofbtSpgrContainer)) as MsofbtSpgrContainer;

	internal ShapeDocType ShapeDocType
	{
		get
		{
			return m_shapeDocType;
		}
		set
		{
			m_shapeDocType = value;
		}
	}

	internal MsofbtDgContainer(WordDocument doc)
		: base(MSOFBT.msofbtDgContainer, doc)
	{
	}

	private static void GetShapeCountAndMaxSpid(BaseContainer baseContainer, ref int shapeCount, ref int spidMax)
	{
		for (int i = 0; i < baseContainer.Children.Count; i++)
		{
			BaseEscherRecord baseEscherRecord = baseContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSp)
			{
				MsofbtSp msofbtSp = baseEscherRecord as MsofbtSp;
				spidMax = Math.Max(spidMax, msofbtSp.ShapeId);
				shapeCount++;
			}
			if (baseEscherRecord is BaseContainer)
			{
				GetShapeCountAndMaxSpid(baseEscherRecord as BaseContainer, ref shapeCount, ref spidMax);
			}
		}
	}

	internal void InitWriting()
	{
		int shapeCount = 0;
		int spidMax = 0;
		GetShapeCountAndMaxSpid(PatriarchGroupContainer, ref shapeCount, ref spidMax);
		Dg.ShapeCount = shapeCount;
		Dg.SpidLast = spidMax;
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtDgContainer obj = (MsofbtDgContainer)base.Clone();
		obj.m_shapeDocType = m_shapeDocType;
		obj.m_doc = m_doc;
		return obj;
	}
}
