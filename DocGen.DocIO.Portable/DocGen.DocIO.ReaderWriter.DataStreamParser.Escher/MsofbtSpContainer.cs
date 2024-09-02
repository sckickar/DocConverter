using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSpContainer : BaseContainer
{
	public const int DEF_TXID_INCREMENT = 65536;

	public const string DEF_PICTMARK_STRING = "WordPictureWatermark";

	public const string DEF_TEXTMARK_STRING = "PowerPlusWaterMarkObject";

	public const string DEF_NULL_STRING = "\0";

	private const uint DEF_NOTALLOWINCELL = 2147483648u;

	private MsofbtBSE m_bse;

	private byte m_bFlags;

	internal MsofbtBSE Bse
	{
		get
		{
			return m_bse;
		}
		set
		{
			m_bse = value;
		}
	}

	internal int Pib
	{
		get
		{
			if (ShapeOptions != null && ShapeOptions.Pib != null)
			{
				return (int)ShapeOptions.Pib.Value;
			}
			return -1;
		}
		set
		{
			ShapeOptions.Pib.Value = (uint)value;
		}
	}

	internal MsofbtSp Shape
	{
		get
		{
			return FindContainerByType(typeof(MsofbtSp)) as MsofbtSp;
		}
		set
		{
			_ = (MsofbtSp)FindContainerByType(typeof(MsofbtSp));
		}
	}

	internal MsofbtOPT ShapeOptions => FindContainerByType(typeof(MsofbtOPT)) as MsofbtOPT;

	internal int Txid
	{
		get
		{
			if (ShapeOptions != null && ShapeOptions.Txid != null)
			{
				return (int)ShapeOptions.Txid.Value;
			}
			return -1;
		}
		set
		{
			if (ShapeOptions == null)
			{
				throw new ArgumentNullException("Shape options are null.");
			}
			((ShapeOptions.Properties[128] as FOPTEBid) ?? throw new ArgumentException("Txid property does not exist.")).Value = (uint)value;
		}
	}

	internal MsofbtTertiaryFOPT ShapePosition => FindContainerByType(typeof(MsofbtTertiaryFOPT)) as MsofbtTertiaryFOPT;

	internal bool IsWatermark
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal MsofbtSpContainer(WordDocument doc)
		: base(MSOFBT.msofbtSpContainer, doc)
	{
	}

	public uint GetPropertyValue(int key)
	{
		return (uint)(((int?)ShapeOptions?.GetPropertyValue(key)) ?? (-1));
	}

	public byte[] GetComplexPropValue(int key)
	{
		return ShapeOptions?.GetComplexPropValue(key);
	}

	internal bool HasFillEffect()
	{
		bool result = false;
		if (ShapeOptions == null)
		{
			return false;
		}
		if (!ShapeOptions.Properties.ContainsKey(447))
		{
			return true;
		}
		uint propertyValue = GetPropertyValue(447);
		if (propertyValue != uint.MaxValue)
		{
			result = (propertyValue & 0x10) == 16;
		}
		return result;
	}

	internal BackgroundFillType GetBackgroundFillType()
	{
		uint propertyValue = GetPropertyValue(384);
		if (propertyValue != uint.MaxValue)
		{
			return (BackgroundFillType)propertyValue;
		}
		return BackgroundFillType.msofillSolid;
	}

	internal BackgroundType GetBackgroundType()
	{
		return GetPropertyValue(384) switch
		{
			2u => BackgroundType.Texture, 
			3u => BackgroundType.Picture, 
			1u => BackgroundType.NoBackground, 
			uint.MaxValue => BackgroundType.NoBackground, 
			_ => BackgroundType.Gradient, 
		};
	}

	internal ImageRecord GetBackgroundImage(EscherClass escher)
	{
		uint propertyValue = GetPropertyValue(390);
		if (propertyValue != uint.MaxValue && propertyValue - 1 < escher.m_msofbtDggContainer.BstoreContainer.Children.Count)
		{
			MsofbtBSE msofbtBSE = (MsofbtBSE)escher.m_msofbtDggContainer.BstoreContainer.Children[(int)(propertyValue - 1)];
			if (msofbtBSE.Blip != null)
			{
				try
				{
					return msofbtBSE.Blip.ImageRecord;
				}
				catch (Exception)
				{
				}
			}
		}
		return null;
	}

	internal byte[] GetBackgroundImBytes(EscherClass escher)
	{
		uint propertyValue = GetPropertyValue(390);
		if (propertyValue != uint.MaxValue && propertyValue - 1 < escher.m_msofbtDggContainer.BstoreContainer.Children.Count)
		{
			MsofbtBSE msofbtBSE = (MsofbtBSE)escher.m_msofbtDggContainer.BstoreContainer.Children[(int)(propertyValue - 1)];
			if (msofbtBSE.Blip != null)
			{
				try
				{
					return msofbtBSE.Blip.ImageBytes;
				}
				catch (Exception)
				{
				}
			}
		}
		return null;
	}

	internal Color GetBackgroundColor(bool isPictureBackground)
	{
		Color result = Color.White;
		int key = (isPictureBackground ? 387 : 385);
		uint propertyValue = GetPropertyValue(key);
		if (propertyValue != uint.MaxValue)
		{
			result = WordColor.ConvertRGBToColor(propertyValue);
		}
		return result;
	}

	internal GradientShadingStyle GetGradientShadingStyle(BackgroundFillType fillType)
	{
		return fillType switch
		{
			BackgroundFillType.msofillShadeCenter => GradientShadingStyle.FromCorner, 
			BackgroundFillType.msofillShadeShape => GradientShadingStyle.FromCenter, 
			_ => GetPropertyValue(395) switch
			{
				4289069056u => GradientShadingStyle.Vertical, 
				4286119936u => GradientShadingStyle.DiagonalUp, 
				4292018176u => GradientShadingStyle.DiagonalDown, 
				_ => GradientShadingStyle.Horizontal, 
			}, 
		};
	}

	internal GradientShadingVariant GetGradientShadingVariant(GradientShadingStyle shadingStyle)
	{
		if (shadingStyle != GradientShadingStyle.FromCorner)
		{
			return GetPropertyValue(396) switch
			{
				4294967246u => GradientShadingVariant.ShadingOut, 
				50u => GradientShadingVariant.ShadingMiddle, 
				uint.MaxValue => GradientShadingVariant.ShadingDown, 
				_ => GradientShadingVariant.ShadingUp, 
			};
		}
		return GetCornerStyleVariant();
	}

	internal MsofbtSpContainer CreateRectangleContainer()
	{
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptRectangle;
		msofbtSp.HasAnchor = true;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.IsBackground = true;
		msofbtSp.ShapeId = 1025;
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		msofbtOPT.Properties.Add(new FOPTEBid(447, isBid: false, 1048576u));
		msofbtOPT.Properties.Add(new FOPTEBid(459, isBid: false, 0u));
		msofbtOPT.Properties.Add(new FOPTEBid(511, isBid: false, 524288u));
		msofbtOPT.Properties.Add(new FOPTEBid(772, isBid: false, 9u));
		msofbtOPT.Properties.Add(new FOPTEBid(831, isBid: false, 65537u));
		msofbtOPT.Properties.Add(new FOPTEBid(191, isBid: false, 131072u));
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		MsofbtClientData msofbtClientData = new MsofbtClientData(m_doc);
		msofbtClientData.Data = new byte[4] { 1, 0, 0, 0 };
		MsofbtTertiaryFOPT msofbtTertiaryFOPT = new MsofbtTertiaryFOPT(m_doc);
		msofbtTertiaryFOPT.Unknown1 = 6291520u;
		base.Children.Add(msofbtSp);
		base.Children.Add(msofbtOPT);
		base.Children.Add(msofbtTertiaryFOPT);
		base.Children.Add(msofbtClientData);
		return this;
	}

	internal void UpdateBackground(WordDocument doc, Background background)
	{
		if (background.Type == BackgroundType.NoBackground)
		{
			return;
		}
		CheckEscher(doc);
		EscherClass escher = doc.Escher;
		switch (background.Type)
		{
		case BackgroundType.Picture:
		case BackgroundType.Texture:
			if (background.ImageRecord != null && background.ImageRecord.m_imageBytes != null)
			{
				m_bse = new MsofbtBSE(m_doc);
				m_bse.Initialize(background.ImageRecord);
				ShapeOptions.Properties.Remove(390);
				ShapeOptions.Properties.Remove(384);
				escher.m_msofbtDggContainer.BstoreContainer.Children.Add(m_bse);
				int count = escher.m_msofbtDggContainer.BstoreContainer.Children.Count;
				UpdateFillPicture(background, count);
			}
			break;
		case BackgroundType.Color:
			UpdateFillColor(background.Color);
			break;
		case BackgroundType.Gradient:
			UpdateFillGradient(background.Gradient);
			break;
		}
		if (background.Type != BackgroundType.Color)
		{
			SetShapeOption(ShapeOptions.Properties, 1310740u, 447, isBid: false);
		}
	}

	internal void UpdateFillGradient(BackgroundGradient gradient)
	{
		if (gradient.Color1 != Color.White)
		{
			uint curValue = WordColor.ConvertColorToRGB(gradient.Color1);
			SetShapeOption(ShapeOptions.Properties, curValue, 385, isBid: false);
		}
		if (gradient.Color2 != Color.White)
		{
			uint curValue2 = WordColor.ConvertColorToRGB(gradient.Color2);
			SetShapeOption(ShapeOptions.Properties, curValue2, 387, isBid: false);
		}
		AddGradientFillAngle(gradient.ShadingStyle);
		AddGradientFillType(gradient.ShadingStyle);
		AddFillProperties(gradient.ShadingStyle, gradient.ShadingVariant);
		AddGradientFocusFopte(gradient.ShadingStyle, gradient.ShadingVariant);
	}

	internal void UpdateFillPicture(Background background, int fillBlipIndex)
	{
		if (background.Type == BackgroundType.Picture)
		{
			SetShapeOption(ShapeOptions.Properties, 3u, 384, isBid: false);
		}
		else
		{
			SetShapeOption(ShapeOptions.Properties, 2u, 384, isBid: false);
		}
		if (background.PictureBackColor != Color.White)
		{
			uint curValue = WordColor.ConvertColorToRGB(background.PictureBackColor);
			SetShapeOption(ShapeOptions.Properties, curValue, 387, isBid: false);
		}
		SetShapeOption(ShapeOptions.Properties, (uint)fillBlipIndex, 390, isBid: true);
		SetShapeOption(ShapeOptions.Properties, 2u, 392, isBid: false);
	}

	internal void UpdateFillColor(Color color)
	{
		uint num = WordColor.ConvertColorToRGB(color);
		if (num == 4278190080u)
		{
			SetBoolShapeOption(ShapeOptions.Properties, 447, 16, 4, 0, 1048576u);
			return;
		}
		uint num2 = WordColor.ConvertColorToRGB(Color.White, ignoreAlpha: true);
		SetBoolShapeOption(ShapeOptions.Properties, 447, 16, 4, 1, 1048592u);
		if (num != num2)
		{
			SetShapeOption(ShapeOptions.Properties, num, 385, isBid: false);
		}
		uint opacity = GetOpacity(color.A);
		if (opacity != uint.MaxValue)
		{
			SetShapeOption(ShapeOptions.Properties, opacity, 386, isBid: false);
		}
	}

	internal void CreateTextWatermarkContainer(int watermarkNum, TextWatermark textWatermark)
	{
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptTextPlainText;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		if (textWatermark.Layout == WatermarkLayout.Diagonal)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(4, isBid: false, 20643840u));
		}
		textWatermark.Text = ((textWatermark.Text == null) ? string.Empty : textWatermark.Text);
		byte[] bytes = Encoding.Unicode.GetBytes(textWatermark.Text.EndsWith("\0", StringComparison.Ordinal) ? textWatermark.Text : (textWatermark.Text + "\0"));
		FOPTEComplex fOPTEComplex = new FOPTEComplex(192, isBid: false, bytes.Length);
		fOPTEComplex.Value = bytes;
		msofbtOPT.Properties.Add(fOPTEComplex);
		uint value = (uint)((int)textWatermark.Size << 16);
		msofbtOPT.Properties.Add(new FOPTEBid(195, isBid: false, value));
		byte[] bytes2 = Encoding.Unicode.GetBytes(textWatermark.FontName.EndsWith("\0", StringComparison.Ordinal) ? textWatermark.FontName : (textWatermark.FontName + "\0"));
		FOPTEComplex fOPTEComplex2 = new FOPTEComplex(197, isBid: false, bytes2.Length);
		fOPTEComplex2.Value = bytes2;
		msofbtOPT.Properties.Add(fOPTEComplex2);
		uint value2 = WordColor.ConvertColorToRGB(textWatermark.Color);
		msofbtOPT.Properties.Add(new FOPTEBid(385, isBid: false, value2));
		if (textWatermark.Semitransparent)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(386, isBid: false, 32768u));
		}
		msofbtOPT.Properties.Add(new FOPTEBid(447, isBid: false, 1048529u));
		msofbtOPT.Properties.Add(new FOPTEBid(511, isBid: false, 524288u));
		msofbtOPT.Properties.Add(new FOPTEBid(959, isBid: false, 2097184u));
		string s = "PowerPlusWaterMarkObject" + watermarkNum + "\0";
		byte[] bytes3 = Encoding.Unicode.GetBytes(s);
		FOPTEComplex fOPTEComplex3 = new FOPTEComplex(896, isBid: false, bytes3.Length);
		fOPTEComplex3.Value = bytes3;
		msofbtOPT.Properties.Add(fOPTEComplex3);
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		MsofbtTertiaryFOPT msofbtTertiaryFOPT = new MsofbtTertiaryFOPT(m_doc);
		msofbtTertiaryFOPT.XAlign = (uint)textWatermark.HorizontalAlignment;
		msofbtTertiaryFOPT.YAlign = (uint)textWatermark.VerticalAlignment;
		msofbtTertiaryFOPT.XRelTo = (uint)textWatermark.HorizontalOrigin;
		msofbtTertiaryFOPT.YRelTo = (uint)textWatermark.VerticalOrigin;
		msofbtTertiaryFOPT.AllowInTableCell = false;
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4] { 2, 0, 0, 0 };
		MsofbtClientData msofbtClientData = new MsofbtClientData(m_doc);
		msofbtClientData.Data = new byte[4] { 1, 0, 0, 0 };
		base.Children.Add(msofbtSp);
		base.Children.Add(msofbtOPT);
		base.Children.Add(msofbtTertiaryFOPT);
		base.Children.Add(msofbtClientAnchor);
		base.Children.Add(msofbtClientData);
	}

	internal void CreatePictWatermarkContainer(int watermarkNum, PictureWatermark pictWatermark)
	{
		_Blip blip = null;
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptPictureFrame;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		msofbtOPT.Properties.Add(new FOPTEBid(260, isBid: true, 1u));
		msofbtOPT.Properties.Add(new FOPTEBid(262, isBid: false, 2u));
		if (pictWatermark.Washout)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(264, isBid: false, 19661u));
			msofbtOPT.Properties.Add(new FOPTEBid(265, isBid: false, 22938u));
		}
		msofbtOPT.Properties.Add(new FOPTEBid(511, isBid: false, 524288u));
		msofbtOPT.Properties.Add(new FOPTEBid(959, isBid: false, 2097184u));
		string s = "WordPictureWatermark" + watermarkNum + "\0";
		byte[] bytes = Encoding.Unicode.GetBytes(s);
		FOPTEComplex fOPTEComplex = new FOPTEComplex(896, isBid: false, bytes.Length);
		fOPTEComplex.Value = bytes;
		msofbtOPT.Properties.Add(fOPTEComplex);
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		if (pictWatermark.OriginalPib == -1)
		{
			if (pictWatermark.WordPicture.IsMetaFile)
			{
				blip = new MsofbtMetaFile(pictWatermark.WordPicture.ImageRecord, m_doc);
			}
			else
			{
				bool isBitmap = IsBitmap(pictWatermark.Picture.RawFormat);
				if (pictWatermark.WordPicture.ImageRecord != null)
				{
					blip = new MsofbtImage(pictWatermark.WordPicture.ImageRecord, isBitmap, m_doc);
				}
			}
			Guid uid = blip.Uid;
			MsofbtBSE msofbtBSE = new MsofbtBSE(m_doc);
			msofbtBSE.Header.Instance = (int)blip.Type;
			msofbtBSE.Fbse.m_btWin32 = (int)blip.Type;
			msofbtBSE.Fbse.m_btMacOS = (int)blip.Type;
			msofbtBSE.Fbse.m_rgbUid = uid.ToByteArray();
			msofbtBSE.Fbse.m_tag = 255;
			msofbtBSE.Fbse.m_cRef = 1;
			msofbtBSE.Blip = blip;
			Bse = msofbtBSE;
		}
		MsofbtTertiaryFOPT msofbtTertiaryFOPT = new MsofbtTertiaryFOPT(m_doc);
		ApplyPictureProperties(msofbtTertiaryFOPT, pictWatermark.WordPicture);
		msofbtTertiaryFOPT.AllowInTableCell = false;
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4] { 2, 0, 0, 0 };
		MsofbtClientData msofbtClientData = new MsofbtClientData(m_doc);
		msofbtClientData.Data = new byte[4] { 1, 0, 0, 0 };
		base.Children.Add(msofbtSp);
		base.Children.Add(msofbtOPT);
		base.Children.Add(msofbtClientAnchor);
		base.Children.Add(msofbtClientData);
		base.Children.Add(msofbtTertiaryFOPT);
	}

	private void ApplyPictureProperties(MsofbtTertiaryFOPT msofbtShapePosition, WPicture picture)
	{
		if (picture.HorizontalAlignment != 0)
		{
			msofbtShapePosition.XAlign = (uint)picture.HorizontalAlignment;
		}
		if (picture.VerticalAlignment != 0)
		{
			msofbtShapePosition.YAlign = (uint)picture.VerticalAlignment;
		}
		if (picture.HorizontalOrigin == HorizontalOrigin.LeftMargin || picture.HorizontalOrigin == HorizontalOrigin.RightMargin || picture.HorizontalOrigin == HorizontalOrigin.InsideMargin || picture.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			msofbtShapePosition.XRelTo = 0u;
		}
		else
		{
			msofbtShapePosition.XRelTo = (uint)picture.HorizontalOrigin;
		}
		msofbtShapePosition.YRelTo = (uint)picture.VerticalOrigin;
	}

	public MsofbtSpContainer CreateImageContainer(WPicture pict, PictureShapeProps pictProps)
	{
		_Blip blip;
		if (pict.ImageRecord != null && pict.ImageRecord.IsMetafile)
		{
			blip = new MsofbtMetaFile(pict.ImageRecord, m_doc);
		}
		else
		{
			bool flag = false;
			blip = new MsofbtImage(isBitmap: (!WordDocument.EnablePartialTrustCode) ? IsBitmap(pict.Image.RawFormat) : IsBitmapForPartialTrustMode(pict.ImageForPartialTrustMode.Format), imageRecord: pict.ImageRecord, doc: m_doc);
		}
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptPictureFrame;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		base.Children.Add(msofbtSp);
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		if (pict.PictureShape.ShapeContainer != null && pict.PictureShape.ShapeContainer.ShapeOptions != null)
		{
			msofbtOPT = pict.PictureShape.ShapeContainer.ShapeOptions.Clone() as MsofbtOPT;
		}
		base.Children.Add(msofbtOPT);
		WritePictureOptions(pictProps, pict);
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		MsofbtBSE msofbtBSE = new MsofbtBSE(m_doc);
		msofbtBSE.Header.Instance = (int)blip.Type;
		msofbtBSE.Fbse.m_btWin32 = (int)blip.Type;
		msofbtBSE.Fbse.m_btMacOS = (int)blip.Type;
		msofbtBSE.Fbse.m_rgbUid = blip.Uid.ToByteArray();
		msofbtBSE.Fbse.m_tag = 255;
		msofbtBSE.Fbse.m_cRef = 1;
		msofbtBSE.Blip = blip;
		Bse = msofbtBSE;
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4];
		MsofbtClientData msofbtClientData = new MsofbtClientData(m_doc);
		msofbtClientData.Data = new byte[4] { 1, 0, 0, 0 };
		base.Children.Add(msofbtClientAnchor);
		base.Children.Add(msofbtClientData);
		Shape.ShapeId = pictProps.Spid;
		MsofbtTertiaryFOPT item = new MsofbtTertiaryFOPT(m_doc);
		base.Children.Add(item);
		ShapePosition.XAlign = (uint)pictProps.HorizontalAlignment;
		ShapePosition.XRelTo = (uint)pictProps.RelHrzPos;
		ShapePosition.YAlign = (uint)pictProps.VerticalAlignment;
		ShapePosition.YRelTo = (uint)pictProps.RelVrtPos;
		return this;
	}

	public MsofbtSpContainer CreateInlineImageContainer(WPicture pict)
	{
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptPictureFrame;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		msofbtOPT.Properties.Add(new FOPTEBid(260, isBid: true, 1u));
		msofbtOPT.Properties.Add(new FOPTEBid(262, isBid: false, 2u));
		if (!string.IsNullOrEmpty(pict.AlternativeText))
		{
			if (msofbtOPT.Properties.ContainsKey(897))
			{
				msofbtOPT.Properties.Remove(897);
			}
			byte[] bytes = Encoding.Unicode.GetBytes(pict.AlternativeText + "\0");
			FOPTEComplex fOPTEComplex = new FOPTEComplex(897, isBid: false, bytes.Length);
			fOPTEComplex.Value = bytes;
			msofbtOPT.Properties.Add(fOPTEComplex);
		}
		if (!string.IsNullOrEmpty(pict.Name))
		{
			if (msofbtOPT.Properties.ContainsKey(896))
			{
				msofbtOPT.Properties.Remove(896);
			}
			byte[] bytes2 = Encoding.Unicode.GetBytes(pict.Name + "\0");
			FOPTEComplex fOPTEComplex2 = new FOPTEComplex(896, isBid: false, bytes2.Length);
			fOPTEComplex2.Value = bytes2;
			msofbtOPT.Properties.Add(fOPTEComplex2);
		}
		if ((double)pict.FillRectangle.BottomOffset != 0.0)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(257, isBid: false, SetPictureCropValue(pict.FillRectangle.BottomOffset)));
		}
		if ((double)pict.FillRectangle.RightOffset != 0.0)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(259, isBid: false, SetPictureCropValue(pict.FillRectangle.RightOffset)));
		}
		if ((double)pict.FillRectangle.LeftOffset != 0.0)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(258, isBid: false, SetPictureCropValue(pict.FillRectangle.LeftOffset)));
		}
		if ((double)pict.FillRectangle.TopOffset != 0.0)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(256, isBid: false, SetPictureCropValue(pict.FillRectangle.TopOffset)));
		}
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		_Blip blip;
		if (pict.ImageRecord != null && pict.ImageRecord.IsMetafile)
		{
			blip = new MsofbtMetaFile(pict, m_doc);
		}
		else
		{
			bool isBitmap = IsBitmap(pict.ImageRecord.ImageFormat);
			blip = new MsofbtImage(pict.ImageRecord, isBitmap, m_doc);
		}
		if (pict.Rotation != 0f && pict.TextWrappingStyle != 0)
		{
			msofbtOPT.Properties.Add(new FOPTEBid(4, isBid: false, SetPictureRotationValue(pict.Rotation)));
		}
		Guid uid = blip.Uid;
		MsofbtBSE msofbtBSE = new MsofbtBSE(m_doc);
		msofbtBSE.Header.Instance = (int)blip.Type;
		msofbtBSE.Fbse.m_btWin32 = (int)blip.Type;
		msofbtBSE.Fbse.m_btMacOS = (int)blip.Type;
		msofbtBSE.Fbse.m_rgbUid = uid.ToByteArray();
		msofbtBSE.Fbse.m_tag = 255;
		msofbtBSE.Fbse.m_cRef = 1;
		msofbtBSE.IsInlineBlip = true;
		msofbtBSE.Blip = blip;
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4] { 0, 0, 0, 128 };
		base.Children.Add(msofbtSp);
		base.Children.Add(msofbtOPT);
		base.Children.Add(msofbtClientAnchor);
		if (pict.PictureShape.ShapeContainer != null && pict.PictureShape.ShapeContainer.ShapePosition != null)
		{
			base.Children.Add(pict.PictureShape.ShapeContainer.ShapePosition);
		}
		Bse = msofbtBSE;
		return this;
	}

	public MsofbtSpContainer CreateTextBoxContainer(bool visible, WTextBoxFormat txbxFormat)
	{
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptTextBox;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		base.Children.Add(msofbtSp);
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		base.Children.Add(msofbtOPT);
		WriteTextBoxOptions(visible, txbxFormat);
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		if (!string.IsNullOrEmpty(txbxFormat.Name))
		{
			if (msofbtOPT.Properties.ContainsKey(896))
			{
				msofbtOPT.Properties.Remove(896);
			}
			byte[] bytes = Encoding.Unicode.GetBytes(txbxFormat.Name + "\0");
			FOPTEComplex fOPTEComplex = new FOPTEComplex(896, isBid: false, bytes.Length);
			fOPTEComplex.Value = bytes;
			msofbtOPT.Properties.Add(fOPTEComplex);
		}
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4];
		MsofbtClientData msofbtClientData = new MsofbtClientData(m_doc);
		msofbtClientData.Data = new byte[4] { 1, 0, 0, 0 };
		MsofbtClientTextbox msofbtClientTextbox = new MsofbtClientTextbox(m_doc);
		msofbtClientTextbox.Txid = (int)txbxFormat.TextBoxIdentificator;
		base.Children.Add(msofbtClientAnchor);
		base.Children.Add(msofbtClientData);
		base.Children.Add(msofbtClientTextbox);
		MsofbtTertiaryFOPT item = new MsofbtTertiaryFOPT(m_doc);
		base.Children.Add(item);
		ShapePosition.XAlign = (uint)txbxFormat.HorizontalAlignment;
		ShapePosition.YAlign = (uint)txbxFormat.VerticalAlignment;
		if (txbxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			ShapePosition.XRelTo = 3u;
			ShapePosition.YRelTo = 3u;
			ShapePosition.Unknown1 = 6291456u;
			ShapePosition.Unknown2 = 65537u;
		}
		else
		{
			if (txbxFormat.HorizontalOrigin == HorizontalOrigin.LeftMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.RightMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.InsideMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
			{
				ShapePosition.XRelTo = 0u;
			}
			else
			{
				ShapePosition.XRelTo = (uint)txbxFormat.HorizontalOrigin;
			}
			if (txbxFormat.VerticalOrigin == VerticalOrigin.TopMargin || txbxFormat.VerticalOrigin == VerticalOrigin.BottomMargin || txbxFormat.VerticalOrigin == VerticalOrigin.InsideMargin || txbxFormat.VerticalOrigin == VerticalOrigin.OutsideMargin)
			{
				ShapePosition.YRelTo = 1u;
			}
			else
			{
				ShapePosition.YRelTo = (uint)txbxFormat.VerticalOrigin;
			}
		}
		ShapePosition.AllowInTableCell = txbxFormat.AllowInCell;
		if (ShapePosition != null)
		{
			ShapePosition.AllowOverlap = txbxFormat.AllowOverlap;
		}
		else if (ShapeOptions != null)
		{
			ShapeOptions.AllowOverlap = txbxFormat.AllowOverlap;
		}
		Shape.ShapeId = txbxFormat.TextBoxShapeID;
		ShapeOptions.DistanceFromTop = (uint)(txbxFormat.WrapDistanceTop * 12700f);
		ShapeOptions.DistanceFromBottom = (uint)(txbxFormat.WrapDistanceBottom * 12700f);
		ShapeOptions.DistanceFromLeft = (uint)(txbxFormat.WrapDistanceLeft * 12700f);
		ShapeOptions.DistanceFromRight = (uint)(txbxFormat.WrapDistanceRight * 12700f);
		return this;
	}

	public bool IsMetafile(DocGen.DocIO.DLS.Entities.ImageFormat imageFormat)
	{
		if (!imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Emf))
		{
			return imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Wmf);
		}
		return true;
	}

	private bool IsBitmap(DocGen.DocIO.DLS.Entities.ImageFormat imageFormat)
	{
		if (!imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Png) && !imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Bmp))
		{
			return imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.MemoryBmp);
		}
		return true;
	}

	private bool IsBitmapForPartialTrustMode(DocGen.DocIO.DLS.Entities.ImageFormat imageFormat)
	{
		if (!imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Png) && !imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Bmp))
		{
			return imageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.MemoryBmp);
		}
		return true;
	}

	public void WriteContainer(Stream stream)
	{
		WriteMsofbhWithRecord(stream);
		if (Bse != null)
		{
			Bse.WriteMsofbhWithRecord(stream);
		}
	}

	internal MsofbtSpContainer CreateInlineTxbxImageCont()
	{
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeType = EscherShapeType.msosptPictureFrame;
		msofbtSp.HasShapeTypeProperty = true;
		msofbtSp.HasAnchor = true;
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		msofbtOPT.Properties.Add(new FOPTEBid(127, isBid: false, 20971840u));
		msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
		MsofbtClientAnchor msofbtClientAnchor = new MsofbtClientAnchor(m_doc);
		msofbtClientAnchor.Data = new byte[4] { 0, 0, 0, 128 };
		MsofbtTertiaryFOPT msofbtTertiaryFOPT = new MsofbtTertiaryFOPT(m_doc);
		msofbtTertiaryFOPT.Unknown2 = 65537u;
		base.Children.Add(msofbtSp);
		base.Children.Add(msofbtOPT);
		base.Children.Add(msofbtTertiaryFOPT);
		base.Children.Add(msofbtClientAnchor);
		return this;
	}

	internal void CheckOptContainer()
	{
		if (ShapeOptions == null)
		{
			MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
			msofbtOPT.Properties.Add(new FOPTEBid(260, isBid: true, 1u));
			msofbtOPT.Properties.Add(new FOPTEBid(262, isBid: false, 2u));
			msofbtOPT.Header.Instance = msofbtOPT.Properties.Count;
			base.Children.Add(msofbtOPT);
		}
	}

	public static MsofbtSpContainer ReadInlineImageContainers(int length, Stream stream, WordDocument doc)
	{
		ContainerCollection containerCollection = new ContainerCollection(doc);
		containerCollection.Read(stream, length);
		MsofbtSpContainer msofbtSpContainer = containerCollection[0] as MsofbtSpContainer;
		if (msofbtSpContainer != null && containerCollection.Count > 1)
		{
			msofbtSpContainer.Bse = containerCollection[1] as MsofbtBSE;
		}
		return msofbtSpContainer;
	}

	public static _Blip GetBlipFromShapeContainer(BaseEscherRecord escherRecord)
	{
		if (escherRecord == null)
		{
			throw new NullReferenceException("Container is null");
		}
		if (!(escherRecord is MsofbtSpContainer msofbtSpContainer))
		{
			throw new ArgumentException("Container is not a shape container.");
		}
		if (msofbtSpContainer.Shape.ShapeType != EscherShapeType.msosptPictureFrame || msofbtSpContainer.Bse == null)
		{
			return null;
		}
		return msofbtSpContainer.Bse.Blip;
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtSpContainer msofbtSpContainer = (MsofbtSpContainer)base.Clone();
		if (m_bse != null)
		{
			msofbtSpContainer.Bse = (MsofbtBSE)m_bse.Clone();
		}
		msofbtSpContainer.m_doc = m_doc;
		return msofbtSpContainer;
	}

	internal override void CloneRelationsTo(WordDocument doc)
	{
		m_doc = doc;
		base.Header.m_doc = doc;
		foreach (BaseEscherRecord child in base.Children)
		{
			child.m_doc = doc;
			child.Header.m_doc = doc;
		}
		if (Bse == null)
		{
			return;
		}
		Bse.m_doc = doc;
		Bse.Header.m_doc = doc;
		if (Bse.Blip != null)
		{
			Bse.Blip.m_doc = doc;
			Bse.Blip.Header.m_doc = doc;
			Size size = Bse.Blip.ImageRecord.Size;
			DocGen.DocIO.DLS.Entities.ImageFormat imageFormat = Bse.Blip.ImageRecord.ImageFormat;
			if (Bse.Blip is MsofbtMetaFile)
			{
				(Bse.Blip as MsofbtMetaFile).ImageRecord = doc.Images.LoadMetaFileImage(Bse.Blip.ImageRecord.m_imageBytes, isCompressed: true);
			}
			else
			{
				Bse.Blip.ImageRecord = doc.Images.LoadImage(Bse.Blip.ImageRecord.ImageBytes);
			}
			Bse.Blip.ImageRecord.Size = size;
			Bse.Blip.ImageRecord.ImageFormat = imageFormat;
		}
	}

	internal bool Compare(MsofbtSpContainer shapeContainer)
	{
		if ((Bse == null && shapeContainer.Bse != null) || (Bse != null && shapeContainer.Bse == null) || (Shape == null && shapeContainer.Shape != null) || (Shape != null && shapeContainer.Shape == null) || (ShapeOptions == null && shapeContainer.ShapeOptions != null) || (ShapeOptions != null && shapeContainer.ShapeOptions == null) || (ShapePosition == null && shapeContainer.ShapePosition != null) || (ShapePosition != null && shapeContainer.ShapePosition == null))
		{
			return false;
		}
		if (Bse != null && shapeContainer.Bse != null && !Bse.Compare(shapeContainer.Bse))
		{
			return false;
		}
		if (Shape != null && shapeContainer.Shape != null && !Shape.Compare(shapeContainer.Shape))
		{
			return false;
		}
		if (ShapeOptions != null && shapeContainer.ShapeOptions != null && !ShapeOptions.Compare(shapeContainer.ShapeOptions))
		{
			return false;
		}
		if (ShapePosition != null && shapeContainer.ShapePosition != null && !ShapePosition.Compare(shapeContainer.ShapePosition))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Bse != null)
		{
			stringBuilder.Append(Bse.GetAsString());
		}
		if (Shape != null)
		{
			stringBuilder.Append(Shape.GetAsString());
		}
		if (ShapeOptions != null)
		{
			stringBuilder.Append(ShapeOptions.GetAsString());
		}
		if (ShapePosition != null)
		{
			stringBuilder.Append(ShapePosition.GetAsString());
		}
		return stringBuilder;
	}

	internal void RemoveSpContainerOle()
	{
		Shape.IsOle = false;
		if (ShapeOptions.Properties.ContainsKey(267))
		{
			ShapeOptions.Properties.Remove(267);
		}
		FOPTEBid fOPTEBid = (FOPTEBid)ShapeOptions.Properties[319];
		if (fOPTEBid != null)
		{
			fOPTEBid.Value = (uint)BaseWordRecord.SetBitsByMask((int)fOPTEBid.Value, 1, 1, 0);
		}
	}

	internal void WriteTextBoxOptions(bool visible, WTextBoxFormat txbxFormat)
	{
		msofbtRGFOPTE properties = ShapeOptions.Properties;
		if (txbxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			SetBoolShapeOption(properties, 127, 64, 6, 0, 20971840u);
		}
		if (txbxFormat.TextWrappingStyle == TextWrappingStyle.Through || txbxFormat.TextWrappingStyle == TextWrappingStyle.Tight)
		{
			PointF[] array = txbxFormat.WrapPolygon.Vertices.ToArray();
			byte[] array2 = new byte[6 + array.Length * 8];
			byte[] bytes;
			byte[] sourceArray = (bytes = BitConverter.GetBytes((short)array.Length));
			byte[] bytes2 = BitConverter.GetBytes((short)8);
			int num = 0;
			Array.Copy(sourceArray, 0, array2, num, 2);
			num += 2;
			Array.Copy(bytes, 0, array2, num, 2);
			num += 2;
			Array.Copy(bytes2, 0, array2, num, 2);
			num += 2;
			for (int i = 0; i < array.Length; i++)
			{
				byte[] bytes3 = BitConverter.GetBytes((int)array[i].X);
				Array.Copy(bytes3, 0, array2, num, bytes3.Length);
				num += bytes3.Length;
				byte[] bytes4 = BitConverter.GetBytes((int)array[i].Y);
				Array.Copy(bytes4, 0, array2, num, bytes4.Length);
				num += bytes4.Length;
			}
			if (properties.ContainsKey(899))
			{
				properties.Remove(899);
			}
			FOPTEComplex fOPTEComplex = new FOPTEComplex(899, isBid: false, array2.Length);
			fOPTEComplex.Value = array2;
			properties.Add(fOPTEComplex);
		}
		if (properties.ContainsKey(128))
		{
			properties.Remove(128);
		}
		properties.Add(new FOPTEBid(128, isBid: false, (uint)txbxFormat.TextBoxIdentificator));
		if (properties.ContainsKey(138))
		{
			properties.Remove(138);
			properties.Add(new FOPTEBid(138, isBid: false, (uint)txbxFormat.TextBoxShapeID));
		}
		float num2 = txbxFormat.LineWidth * 12700f;
		if (txbxFormat.LineWidth != 0.75f)
		{
			SetShapeOption(properties, (uint)num2, 459, isBid: false);
		}
		if (txbxFormat.LineDashing != 0)
		{
			SetShapeOption(properties, (uint)txbxFormat.LineDashing, 462, isBid: false);
		}
		if (txbxFormat.LineStyle != 0)
		{
			SetShapeOption(properties, (uint)txbxFormat.LineStyle, 461, isBid: false);
		}
		if (txbxFormat.TextDirection != 0)
		{
			if (properties.ContainsKey(136))
			{
				properties.Remove(136);
			}
			uint num3 = 0u;
			properties.Add(new FOPTEBid(136, isBid: false, txbxFormat.TextDirection switch
			{
				TextDirection.VerticalFarEast => 1u, 
				TextDirection.VerticalBottomToTop => 2u, 
				TextDirection.VerticalTopToBottom => 3u, 
				TextDirection.HorizontalFarEast => 4u, 
				TextDirection.Vertical => 5u, 
				_ => 0u, 
			}));
		}
		uint num4 = WordColor.ConvertColorToRGB(txbxFormat.LineColor);
		uint num5 = WordColor.ConvertColorToRGB(Color.Black);
		if (num4 != num5)
		{
			num4 = WordColor.ConvertColorToRGB(txbxFormat.LineColor, ignoreAlpha: true);
			SetShapeOption(properties, num4, 448, isBid: false);
			uint opacity = GetOpacity(txbxFormat.LineColor.A);
			if (opacity != uint.MaxValue)
			{
				SetShapeOption(properties, opacity, 449, isBid: false);
			}
		}
		if (txbxFormat.NoLine)
		{
			SetBoolShapeOption(properties, 511, 8, 3, 0, 524288u);
		}
		if (txbxFormat.AutoFit)
		{
			SetBoolShapeOption(properties, 191, 2, 1, 1, 131074u);
		}
		if (!txbxFormat.AllowInCell)
		{
			SetBoolShapeOption(properties, 959, int.MaxValue, 31, 1, 2147483648u);
		}
		if (ShapePosition != null)
		{
			ShapePosition.AllowOverlap = txbxFormat.AllowOverlap;
		}
		else if (ShapeOptions != null)
		{
			ShapeOptions.AllowOverlap = txbxFormat.AllowOverlap;
		}
		if (txbxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			SetBoolShapeOption(properties, 959, 1, 0, 0, 2097152u);
		}
		else if (txbxFormat.IsBelowText)
		{
			SetBoolShapeOption(properties, 959, 32, 5, 1, 2097184u);
		}
		uint num6 = 0u;
		if (txbxFormat.InternalMargin.Left != 7.087f)
		{
			num6 = (uint)Math.Round(txbxFormat.InternalMargin.Left * 12700f);
			SetShapeOption(properties, num6, 129, isBid: false);
		}
		if (txbxFormat.InternalMargin.Right != 7.087f)
		{
			num6 = (uint)Math.Round(txbxFormat.InternalMargin.Right * 12700f);
			SetShapeOption(properties, num6, 131, isBid: false);
		}
		if (txbxFormat.InternalMargin.Top != 3.685f)
		{
			num6 = (uint)Math.Round(txbxFormat.InternalMargin.Top * 12700f);
			SetShapeOption(properties, num6, 130, isBid: false);
		}
		if (txbxFormat.InternalMargin.Bottom != 3.685f)
		{
			num6 = (uint)Math.Round(txbxFormat.InternalMargin.Bottom * 12700f);
			SetShapeOption(properties, num6, 132, isBid: false);
		}
		if (txbxFormat.Document != null)
		{
			UpdateBackground(txbxFormat.Document, txbxFormat.FillEfects);
		}
		ShapeOptions.Visible = visible;
	}

	internal void WritePictureOptions(PictureShapeProps pictProps, WPicture pic)
	{
		msofbtRGFOPTE properties = ShapeOptions.Properties;
		if (pic.TextWrappingStyle != 0)
		{
			ShapeOptions.DistanceFromBottom = (uint)(pic.DistanceFromBottom * 12700f);
			ShapeOptions.DistanceFromLeft = (uint)(pic.DistanceFromLeft * 12700f);
			ShapeOptions.DistanceFromRight = (uint)(pic.DistanceFromRight * 12700f);
			ShapeOptions.DistanceFromTop = (uint)(pic.DistanceFromTop * 12700f);
		}
		if (pic.Rotation != 0f && pic.TextWrappingStyle != 0)
		{
			ShapeOptions.Roation = (uint)(pic.Rotation * 65536f);
		}
		if ((double)pic.FillRectangle.BottomOffset != 0.0)
		{
			SetShapeOption(properties, SetPictureCropValue(pic.FillRectangle.BottomOffset), 257, isBid: false);
		}
		if ((double)pic.FillRectangle.RightOffset != 0.0)
		{
			SetShapeOption(properties, SetPictureCropValue(pic.FillRectangle.RightOffset), 259, isBid: false);
		}
		if ((double)pic.FillRectangle.LeftOffset != 0.0)
		{
			SetShapeOption(properties, SetPictureCropValue(pic.FillRectangle.LeftOffset), 258, isBid: false);
		}
		if ((double)pic.FillRectangle.TopOffset != 0.0)
		{
			SetShapeOption(properties, SetPictureCropValue(pic.FillRectangle.TopOffset), 256, isBid: false);
		}
		if (properties.ContainsKey(263))
		{
			ShapeOptions.SetPropertyValue(263, WordColor.ConvertColorToRGB(pic.ChromaKeyColor));
		}
		if (pic.TextWrappingStyle == TextWrappingStyle.Through || pic.TextWrappingStyle == TextWrappingStyle.Tight)
		{
			PointF[] array = pic.WrapPolygon.Vertices.ToArray();
			byte[] array2 = new byte[6 + array.Length * 8];
			byte[] bytes;
			byte[] sourceArray = (bytes = BitConverter.GetBytes((short)array.Length));
			byte[] bytes2 = BitConverter.GetBytes((short)8);
			int num = 0;
			Array.Copy(sourceArray, 0, array2, num, 2);
			num += 2;
			Array.Copy(bytes, 0, array2, num, 2);
			num += 2;
			Array.Copy(bytes2, 0, array2, num, 2);
			num += 2;
			for (int i = 0; i < array.Length; i++)
			{
				byte[] bytes3 = BitConverter.GetBytes((int)array[i].X);
				Array.Copy(bytes3, 0, array2, num, bytes3.Length);
				num += bytes3.Length;
				byte[] bytes4 = BitConverter.GetBytes((int)array[i].Y);
				Array.Copy(bytes4, 0, array2, num, bytes4.Length);
				num += bytes4.Length;
			}
			if (properties.ContainsKey(899))
			{
				properties.Remove(899);
			}
			FOPTEComplex fOPTEComplex = new FOPTEComplex(899, isBid: false, array2.Length);
			fOPTEComplex.Value = array2;
			properties.Add(fOPTEComplex);
		}
		if (!properties.ContainsKey(260))
		{
			SetShapeOption(properties, 1u, 260, isBid: true);
		}
		if (!properties.ContainsKey(262))
		{
			SetShapeOption(properties, 2u, 262, isBid: false);
		}
		if (!string.IsNullOrEmpty(pictProps.AlternativeText))
		{
			if (properties.ContainsKey(897))
			{
				properties.Remove(897);
			}
			byte[] bytes5 = Encoding.Unicode.GetBytes(pictProps.AlternativeText + "\0");
			FOPTEComplex fOPTEComplex2 = new FOPTEComplex(897, isBid: false, bytes5.Length);
			fOPTEComplex2.Value = bytes5;
			properties.Add(fOPTEComplex2);
		}
		if (!string.IsNullOrEmpty(pictProps.Name))
		{
			if (properties.ContainsKey(896))
			{
				properties.Remove(896);
			}
			byte[] bytes6 = Encoding.Unicode.GetBytes(pictProps.Name + "\0");
			FOPTEComplex fOPTEComplex3 = new FOPTEComplex(896, isBid: false, bytes6.Length);
			fOPTEComplex3.Value = bytes6;
			properties.Add(fOPTEComplex3);
		}
		if (pictProps.IsBelowText)
		{
			SetBoolShapeOption(properties, 959, 32, 5, 1, 2097184u);
		}
		ShapeOptions.Visible = pic.Visible;
	}

	private uint SetPictureCropValue(float offset)
	{
		return (uint)((double)offset / 1.5259 * 1000.0);
	}

	private uint SetPictureRotationValue(float rotation)
	{
		return (uint)(rotation * 65536f);
	}

	private void SetShapeOption(msofbtRGFOPTE shapeProps, uint curValue, int fopteKey, bool isBid)
	{
		if (shapeProps.ContainsKey(fopteKey))
		{
			FOPTEBid fOPTEBid = (FOPTEBid)shapeProps[fopteKey];
			if (fOPTEBid.Value != curValue)
			{
				fOPTEBid.Value = curValue;
			}
		}
		else
		{
			shapeProps.Add(new FOPTEBid(fopteKey, isBid, curValue));
		}
	}

	private void SetBoolShapeOption(msofbtRGFOPTE shapeProps, int fopteKey, int bitMask, int startBit, int value, uint defValue)
	{
		if (shapeProps.ContainsKey(fopteKey))
		{
			FOPTEBid fOPTEBid = (FOPTEBid)shapeProps[fopteKey];
			if (BaseWordRecord.GetBitsByMask(fOPTEBid.Value, bitMask, startBit) != (uint)value)
			{
				fOPTEBid.Value = (uint)BaseWordRecord.SetBitsByMask((int)fOPTEBid.Value, bitMask, startBit, value);
			}
		}
		else
		{
			shapeProps.Add(new FOPTEBid(fopteKey, isBid: false, defValue));
		}
	}

	private uint GetOpacity(byte opacity)
	{
		float num = (float)(100.0 - (double)(float)(int)opacity / 2.55);
		if (num != 0f)
		{
			if (num != 0f)
			{
				return (uint)Math.Round((double)(100f - num) * 655.35);
			}
			return 0u;
		}
		return uint.MaxValue;
	}

	private void CheckEscher(WordDocument doc)
	{
		if (doc != null)
		{
			EscherClass escher = doc.Escher;
			if ((escher != null && escher.m_dgContainers.Count == 0) || escher == null)
			{
				escher = new EscherClass(doc);
				escher.CreateDgForSubDocuments();
				doc.Escher = escher;
			}
			else if (escher.m_msofbtDggContainer.BstoreContainer == null)
			{
				escher.m_msofbtDggContainer.Children.Add(new MsofbtBstoreContainer(m_doc));
			}
		}
	}

	private GradientShadingVariant GetCornerStyleVariant()
	{
		uint propertyValue = GetPropertyValue(397);
		uint propertyValue2 = GetPropertyValue(400);
		if (propertyValue == uint.MaxValue && propertyValue2 == uint.MaxValue)
		{
			return GradientShadingVariant.ShadingUp;
		}
		if (propertyValue != uint.MaxValue && propertyValue2 != uint.MaxValue)
		{
			return GradientShadingVariant.ShadingMiddle;
		}
		if (propertyValue != uint.MaxValue)
		{
			return GradientShadingVariant.ShadingDown;
		}
		return GradientShadingVariant.ShadingOut;
	}

	private void AddGradientFillAngle(GradientShadingStyle shadingStyle)
	{
		if (shadingStyle != 0)
		{
			uint num = 0u;
			SetShapeOption(ShapeOptions.Properties, shadingStyle switch
			{
				GradientShadingStyle.Vertical => 4289069056u, 
				GradientShadingStyle.DiagonalUp => 4286119936u, 
				_ => 4292018176u, 
			}, 395, isBid: false);
		}
	}

	private void AddGradientFocusFopte(GradientShadingStyle shadingStyle, GradientShadingVariant shadingVariant)
	{
		uint num = 0u;
		if (shadingStyle != GradientShadingStyle.FromCorner)
		{
			switch (shadingVariant)
			{
			case GradientShadingVariant.ShadingUp:
				num = 100u;
				break;
			case GradientShadingVariant.ShadingOut:
				num = 4294967246u;
				break;
			case GradientShadingVariant.ShadingMiddle:
				num = 50u;
				break;
			}
		}
		else
		{
			num = 100u;
		}
		if (num != 0)
		{
			SetShapeOption(ShapeOptions.Properties, num, 396, isBid: false);
		}
	}

	private void AddGradientFillType(GradientShadingStyle shadingStyle)
	{
		uint num = 0u;
		SetShapeOption(ShapeOptions.Properties, shadingStyle switch
		{
			GradientShadingStyle.FromCorner => 5u, 
			GradientShadingStyle.FromCenter => 6u, 
			_ => 7u, 
		}, 384, isBid: false);
	}

	private void AddFillProperties(GradientShadingStyle shadingStyle, GradientShadingVariant shadingVariant)
	{
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		uint num4 = 0u;
		if (shadingStyle == GradientShadingStyle.FromCorner)
		{
			if (shadingVariant == GradientShadingVariant.ShadingDown)
			{
				num = (num2 = 65536u);
			}
			if (shadingVariant == GradientShadingVariant.ShadingOut)
			{
				num3 = (num4 = 65536u);
			}
			if (shadingVariant == GradientShadingVariant.ShadingMiddle)
			{
				num3 = (num4 = (num = (num2 = 65536u)));
			}
		}
		else
		{
			num3 = (num4 = (num = (num2 = 32768u)));
		}
		if (num != 0)
		{
			SetShapeOption(ShapeOptions.Properties, num, 397, isBid: false);
		}
		if (num2 != 0)
		{
			SetShapeOption(ShapeOptions.Properties, num2, 399, isBid: false);
		}
		if (num3 != 0)
		{
			SetShapeOption(ShapeOptions.Properties, num3, 398, isBid: false);
		}
		if (num4 != 0)
		{
			SetShapeOption(ShapeOptions.Properties, num4, 400, isBid: false);
		}
	}
}
