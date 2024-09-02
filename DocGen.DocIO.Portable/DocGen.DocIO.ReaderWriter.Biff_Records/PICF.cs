using System;
using System.IO;
using System.Text;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class PICF
{
	internal const int DEF_PICF_LENGTH = 68;

	internal const int DEF_SCALING_FACTOR = 1000;

	internal int lcb;

	internal short cbHeader;

	internal short mm;

	internal short xExt;

	internal short yExt;

	internal short hMf;

	internal short bm_rcWinMF;

	internal short bm_rcWinMF1;

	internal short bm_rcWinMF2;

	internal short bm_rcWinMF3;

	internal short bm_rcWinMF4;

	internal short bm_rcWinMF5;

	internal short bm_rcWinMF6;

	internal short dxaGoal;

	internal short dyaGoal;

	internal ushort mx;

	internal ushort my;

	internal short dyaCropLeft;

	internal short dyaCropTop;

	internal short dxaCropRight;

	internal short dyaCropBottom;

	internal short brcl;

	internal bool fFrameEmpty;

	internal bool fBitmap;

	internal bool fDrawHatch;

	internal bool fError;

	internal short bpp;

	internal BorderCode brcTop = new BorderCode();

	internal BorderCode brcLeft = new BorderCode();

	internal BorderCode brcBottom = new BorderCode();

	internal BorderCode brcRight = new BorderCode();

	internal short dxaOrigin;

	internal short dyaOrigin;

	internal short cProps;

	internal int ScaleHeight => (int)((double)dyaGoal * ScaleY);

	internal int ScaleWidth => (int)((double)dxaGoal * ScaleX);

	internal double ScaleX => (double)(int)mx / 1000.0;

	internal double ScaleY => (double)(int)my / 1000.0;

	internal BorderCode BorderTop => brcTop;

	internal BorderCode BorderLeft => brcLeft;

	internal BorderCode BorderRight => brcRight;

	internal BorderCode BorderBottom => brcBottom;

	internal PICF()
	{
		cbHeader = 68;
		mm = 100;
		mx = 1000;
		my = 1000;
	}

	internal PICF(BinaryReader reader)
	{
		Read(reader);
	}

	internal void Read(BinaryReader reader)
	{
		int num = (int)reader.BaseStream.Position;
		lcb = reader.ReadInt32();
		cbHeader = reader.ReadInt16();
		mm = reader.ReadInt16();
		xExt = reader.ReadInt16();
		yExt = reader.ReadInt16();
		hMf = reader.ReadInt16();
		bm_rcWinMF = reader.ReadInt16();
		bm_rcWinMF1 = reader.ReadInt16();
		bm_rcWinMF2 = reader.ReadInt16();
		bm_rcWinMF3 = reader.ReadInt16();
		bm_rcWinMF4 = reader.ReadInt16();
		bm_rcWinMF5 = reader.ReadInt16();
		bm_rcWinMF6 = reader.ReadInt16();
		dxaGoal = reader.ReadInt16();
		dyaGoal = reader.ReadInt16();
		mx = (ushort)(((mx = reader.ReadUInt16()) == 0) ? 1000 : mx);
		my = (ushort)(((my = reader.ReadUInt16()) == 0) ? 1000 : my);
		dyaCropLeft = reader.ReadInt16();
		dyaCropTop = reader.ReadInt16();
		dxaCropRight = reader.ReadInt16();
		dyaCropBottom = reader.ReadInt16();
		int num2 = reader.ReadInt16();
		brcl = (short)(num2 & 0xF);
		fFrameEmpty = (num2 & 0x10) != 0;
		fBitmap = (num2 & 0x20) != 0;
		fDrawHatch = (num2 & 0x40) != 0;
		fError = (num2 & 0x80) != 0;
		bpp = (short)((num2 & 0xFF00) >> 8);
		brcTop.Read(reader);
		brcLeft.Read(reader);
		brcBottom.Read(reader);
		brcRight.Read(reader);
		dxaOrigin = reader.ReadInt16();
		dyaOrigin = reader.ReadInt16();
		cProps = reader.ReadInt16();
		reader.BaseStream.Position = num + cbHeader;
	}

	internal void Read(Stream stream)
	{
		BinaryReader reader = new BinaryReader(stream);
		Read(reader);
	}

	internal void Write(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(lcb);
		binaryWriter.Write(cbHeader);
		binaryWriter.Write(mm);
		binaryWriter.Write(xExt);
		binaryWriter.Write(yExt);
		binaryWriter.Write(hMf);
		binaryWriter.Write(bm_rcWinMF);
		binaryWriter.Write(bm_rcWinMF1);
		binaryWriter.Write(bm_rcWinMF2);
		binaryWriter.Write(bm_rcWinMF3);
		binaryWriter.Write(bm_rcWinMF4);
		binaryWriter.Write(bm_rcWinMF5);
		binaryWriter.Write(bm_rcWinMF6);
		binaryWriter.Write(dxaGoal);
		binaryWriter.Write(dyaGoal);
		binaryWriter.Write((short)mx);
		binaryWriter.Write((short)my);
		binaryWriter.Write(dyaCropLeft);
		binaryWriter.Write(dyaCropTop);
		binaryWriter.Write(dxaCropRight);
		binaryWriter.Write(dyaCropBottom);
		int num = brcl;
		num |= (fFrameEmpty ? 16 : 0);
		num |= (fBitmap ? 32 : 0);
		num |= (fDrawHatch ? 64 : 0);
		num |= (fError ? 128 : 0);
		num |= bpp << 8;
		binaryWriter.Write((short)num);
		brcTop.Write(stream);
		brcLeft.Write(stream);
		brcBottom.Write(stream);
		brcRight.Write(stream);
		binaryWriter.Write(dxaOrigin);
		binaryWriter.Write(dyaOrigin);
		binaryWriter.Write(cProps);
	}

	internal PICF Clone()
	{
		PICF obj = MemberwiseClone() as PICF;
		obj.brcBottom = brcBottom.Clone();
		obj.brcLeft = brcLeft.Clone();
		obj.brcRight = brcRight.Clone();
		obj.brcTop = brcTop.Clone();
		return obj;
	}

	internal void SetBasePictureOptions(int height, int width, float heightScale, float widthScale)
	{
		if (width > 32767)
		{
			dxaGoal = short.MaxValue;
		}
		else
		{
			dxaGoal = (short)width;
		}
		if (height > 32767)
		{
			dyaGoal = short.MaxValue;
		}
		else
		{
			dyaGoal = (short)height;
		}
		mx = (ushort)Math.Round(widthScale * 10f);
		my = (ushort)Math.Round(heightScale * 10f);
	}

	internal bool Compare(PICF pictureDescriptor)
	{
		if ((brcTop != null && pictureDescriptor.brcTop == null) || (brcTop == null && pictureDescriptor.brcTop != null) || (brcLeft != null && pictureDescriptor.brcLeft == null) || (brcLeft == null && pictureDescriptor.brcLeft != null) || (brcBottom != null && pictureDescriptor.brcBottom == null) || (brcBottom == null && pictureDescriptor.brcBottom != null) || (brcRight != null && pictureDescriptor.brcRight == null) || (brcRight == null && pictureDescriptor.brcRight != null))
		{
			return false;
		}
		if (brcTop != null && !brcTop.Compare(pictureDescriptor.brcTop))
		{
			return false;
		}
		if (brcLeft != null && !brcLeft.Compare(pictureDescriptor.brcLeft))
		{
			return false;
		}
		if (brcBottom != null && !brcBottom.Compare(pictureDescriptor.brcBottom))
		{
			return false;
		}
		if (brcRight != null && !brcRight.Compare(pictureDescriptor.brcRight))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (brcTop != null)
		{
			stringBuilder.Append(brcTop.GetAsString()?.ToString() + ";");
		}
		if (brcRight != null)
		{
			stringBuilder.Append(brcRight.GetAsString()?.ToString() + ";");
		}
		if (brcLeft != null)
		{
			stringBuilder.Append(brcLeft.GetAsString()?.ToString() + ";");
		}
		if (brcBottom != null)
		{
			stringBuilder.Append(brcBottom.GetAsString()?.ToString() + ";");
		}
		return stringBuilder;
	}
}
