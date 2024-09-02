using System;
using System.Text;
using DocGen.DocIO.ReaderWriter.DataStreamParser;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class TextBoxPropertiesConverter
{
	public static void Export(TextBoxProps txbxProps, WTextBoxFormat txbxFormat)
	{
		txbxFormat.HorizontalPosition = (float)txbxProps.XaLeft / 20f;
		txbxFormat.VerticalPosition = (float)txbxProps.YaTop / 20f;
		txbxFormat.Width = (float)txbxProps.Width / 20f;
		txbxFormat.Height = (float)txbxProps.Height / 20f;
		if (txbxProps.LeftMargin != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Left = (float)txbxProps.LeftMargin / 12700f;
		}
		if (txbxProps.RightMargin != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Right = (float)txbxProps.RightMargin / 12700f;
		}
		if (txbxProps.TopMargin != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Top = (float)txbxProps.TopMargin / 12700f;
		}
		if (txbxProps.BottomMargin != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Bottom = (float)txbxProps.BottomMargin / 12700f;
		}
		txbxFormat.HorizontalAlignment = txbxProps.HorizontalAlignment;
		txbxFormat.VerticalAlignment = txbxProps.VerticalAlignment;
		txbxFormat.HorizontalOrigin = txbxProps.RelHrzPos;
		txbxFormat.VerticalOrigin = txbxProps.RelVrtPos;
		txbxFormat.FillColor = txbxProps.FillColor;
		txbxFormat.LineColor = txbxProps.LineColor;
		txbxFormat.LineDashing = txbxProps.LineDashing;
		txbxFormat.LineStyle = txbxProps.LineStyle;
		txbxFormat.LineWidth = txbxProps.TxbxLineWidth;
		txbxFormat.NoLine = txbxProps.NoLine;
		txbxFormat.AutoFit = txbxProps.FitShapeToText;
		txbxFormat.SetTextWrappingStyleValue(txbxProps.TextWrappingStyle);
		txbxFormat.TextWrappingType = txbxProps.TextWrappingType;
		txbxFormat.WrappingMode = txbxProps.WrapText;
		txbxFormat.IsBelowText = txbxProps.IsBelowText;
		txbxFormat.TextBoxIdentificator = txbxProps.TXID;
		txbxFormat.IsHeaderTextBox = txbxProps.IsHeaderShape;
		txbxFormat.TextBoxShapeID = txbxProps.Spid;
	}

	public static void Import(WTextBoxFormat txbxFormat, TextBoxProps txbxProps)
	{
		txbxProps.XaLeft = (int)Math.Round(txbxFormat.HorizontalPosition * 20f);
		txbxProps.YaTop = (int)Math.Round(txbxFormat.VerticalPosition * 20f);
		txbxProps.Width = (int)Math.Round(txbxFormat.Width * 20f);
		txbxProps.Height = (int)Math.Round(txbxFormat.Height * 20f);
		if (txbxFormat.HorizontalOrigin == HorizontalOrigin.LeftMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.RightMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.InsideMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			txbxProps.RelHrzPos = HorizontalOrigin.Margin;
		}
		else
		{
			txbxProps.RelHrzPos = txbxFormat.HorizontalOrigin;
		}
		if (txbxFormat.VerticalOrigin == VerticalOrigin.TopMargin || txbxFormat.VerticalOrigin == VerticalOrigin.BottomMargin || txbxFormat.VerticalOrigin == VerticalOrigin.InsideMargin || txbxFormat.VerticalOrigin == VerticalOrigin.OutsideMargin)
		{
			txbxProps.RelVrtPos = VerticalOrigin.Page;
		}
		else
		{
			txbxProps.RelVrtPos = txbxFormat.VerticalOrigin;
		}
		txbxProps.HorizontalAlignment = txbxFormat.HorizontalAlignment;
		txbxProps.VerticalAlignment = txbxFormat.VerticalAlignment;
		if (txbxFormat.InternalMargin.Left != 7.087f)
		{
			txbxProps.LeftMargin = (uint)Math.Round(txbxFormat.InternalMargin.Left * 12700f);
		}
		if (txbxFormat.InternalMargin.Right != 7.087f)
		{
			txbxProps.RightMargin = (uint)Math.Round(txbxFormat.InternalMargin.Right * 12700f);
		}
		if (txbxFormat.InternalMargin.Top != 3.685f)
		{
			txbxProps.TopMargin = (uint)Math.Round(txbxFormat.InternalMargin.Top * 12700f);
		}
		if (txbxFormat.InternalMargin.Bottom != 3.685f)
		{
			txbxProps.BottomMargin = (uint)Math.Round(txbxFormat.InternalMargin.Bottom * 12700f);
		}
		txbxProps.FillColor = txbxFormat.FillColor;
		txbxProps.LineColor = txbxFormat.LineColor;
		txbxProps.LineDashing = txbxFormat.LineDashing;
		txbxProps.LineStyle = txbxFormat.LineStyle;
		txbxProps.TxbxLineWidth = txbxFormat.LineWidth;
		txbxProps.NoLine = txbxFormat.NoLine;
		txbxProps.FitShapeToText = txbxFormat.AutoFit;
		txbxProps.TextWrappingStyle = txbxFormat.TextWrappingStyle;
		txbxProps.TextWrappingType = txbxFormat.TextWrappingType;
		txbxProps.WrapText = txbxFormat.WrappingMode;
		txbxProps.IsBelowText = txbxFormat.IsBelowText;
		txbxProps.Spid = txbxFormat.TextBoxShapeID;
		txbxProps.TXID = txbxFormat.TextBoxIdentificator;
	}

	internal static void Export(MsofbtSpContainer txbxContainer, FileShapeAddress fspa, WTextBoxFormat txbxFormat, bool skipPositionOrigins)
	{
		txbxFormat.HorizontalPosition = (float)fspa.XaLeft / 20f;
		txbxFormat.VerticalPosition = (float)fspa.YaTop / 20f;
		if (!skipPositionOrigins)
		{
			txbxFormat.HorizontalOrigin = fspa.RelHrzPos;
			txbxFormat.VerticalOrigin = fspa.RelVrtPos;
		}
		txbxFormat.Width = (float)fspa.Width / 20f;
		txbxFormat.Height = (float)fspa.Height / 20f;
		txbxFormat.SetTextWrappingStyleValue(fspa.TextWrappingStyle);
		txbxFormat.TextWrappingType = fspa.TextWrappingType;
		txbxFormat.IsHeaderTextBox = fspa.IsHeaderShape;
		txbxFormat.TextBoxShapeID = fspa.Spid;
		byte[] complexPropValue = txbxContainer.GetComplexPropValue(896);
		if (complexPropValue != null)
		{
			string text = null;
			text = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
			if (text.Contains("\0"))
			{
				text = text.TrimEnd(new char[1]);
			}
			txbxFormat.Name = text;
		}
		if ((txbxFormat.TextWrappingStyle == TextWrappingStyle.Tight || txbxFormat.TextWrappingStyle == TextWrappingStyle.Through) && txbxContainer.ShapeOptions.Properties.Contains(899))
		{
			txbxFormat.WrapPolygon = new WrapPolygon();
			txbxFormat.WrapPolygon.Edited = false;
			for (int i = 0; i < txbxContainer.ShapeOptions.WrapPolygonVertices.Coords.Count; i++)
			{
				txbxFormat.WrapPolygon.Vertices.Add(txbxContainer.ShapeOptions.WrapPolygonVertices.Coords[i]);
			}
		}
		if (txbxContainer.ShapePosition != null && txbxContainer.ShapePosition.Properties.ContainsKey(959))
		{
			txbxFormat.AllowInCell = txbxContainer.ShapePosition.AllowInTableCell;
		}
		else if (txbxContainer.ShapeOptions != null && txbxContainer.ShapeOptions.Properties.ContainsKey(959))
		{
			txbxFormat.AllowInCell = txbxContainer.ShapeOptions.AllowInTableCell;
		}
		txbxFormat.UpdateFillEffects(txbxContainer, txbxFormat.Document);
		uint propertyValue = txbxContainer.GetPropertyValue(459);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.LineWidth = (float)propertyValue / 12700f;
		}
		propertyValue = txbxContainer.GetPropertyValue(461);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.LineStyle = (TextBoxLineStyle)propertyValue;
		}
		propertyValue = txbxContainer.GetPropertyValue(462);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.LineDashing = (LineDashing)propertyValue;
		}
		propertyValue = txbxContainer.GetPropertyValue(133);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.WrappingMode = (WrapMode)propertyValue;
		}
		switch (txbxContainer.GetPropertyValue(136))
		{
		case 1u:
			txbxFormat.TextDirection = TextDirection.VerticalFarEast;
			break;
		case 3u:
			txbxFormat.TextDirection = TextDirection.VerticalTopToBottom;
			break;
		case 5u:
			txbxFormat.TextDirection = TextDirection.Vertical;
			break;
		case 4u:
			txbxFormat.TextDirection = TextDirection.HorizontalFarEast;
			break;
		case 2u:
			txbxFormat.TextDirection = TextDirection.VerticalBottomToTop;
			break;
		default:
			txbxFormat.TextDirection = TextDirection.Horizontal;
			break;
		case uint.MaxValue:
			break;
		}
		propertyValue = txbxContainer.GetPropertyValue(385);
		if (propertyValue != uint.MaxValue && txbxFormat.FillEfects.Type == BackgroundType.NoBackground)
		{
			txbxFormat.FillColor = WordColor.ConvertRGBToColor(propertyValue);
		}
		propertyValue = txbxContainer.GetPropertyValue(448);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.LineColor = WordColor.ConvertRGBToColor(propertyValue);
		}
		propertyValue = txbxContainer.GetPropertyValue(447);
		if ((propertyValue & 0x10) != 16)
		{
			txbxFormat.FillColor = Color.Empty;
		}
		propertyValue = txbxContainer.GetPropertyValue(191);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.AutoFit = (propertyValue & 2) != 0;
		}
		propertyValue = txbxContainer.GetPropertyValue(511);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.NoLine = (propertyValue & 8) == 0;
		}
		txbxFormat.TextBoxIdentificator = txbxContainer.GetPropertyValue(128);
		propertyValue = txbxContainer.GetPropertyValue(959);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.IsBelowText = (propertyValue & 0x20) == 32;
		}
		else
		{
			txbxFormat.IsBelowText = false;
		}
		if (txbxContainer.ShapePosition != null)
		{
			ExportPosition(txbxContainer, txbxFormat);
		}
		ExportIntMargin(txbxContainer, txbxFormat);
	}

	internal static void Import(FileShapeAddress fspa, WTextBoxFormat txbxFormat)
	{
		fspa.XaLeft = (int)Math.Round(txbxFormat.HorizontalPosition * 20f);
		fspa.YaTop = (int)Math.Round(txbxFormat.VerticalPosition * 20f);
		fspa.Width = (int)Math.Round(txbxFormat.Width * 20f);
		fspa.Height = (int)Math.Round(txbxFormat.Height * 20f);
		if (txbxFormat.HorizontalOrigin == HorizontalOrigin.LeftMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.RightMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.InsideMargin || txbxFormat.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			fspa.RelHrzPos = HorizontalOrigin.Margin;
		}
		else
		{
			fspa.RelHrzPos = txbxFormat.HorizontalOrigin;
		}
		if (txbxFormat.VerticalOrigin == VerticalOrigin.TopMargin || txbxFormat.VerticalOrigin == VerticalOrigin.BottomMargin || txbxFormat.VerticalOrigin == VerticalOrigin.InsideMargin || txbxFormat.VerticalOrigin == VerticalOrigin.OutsideMargin)
		{
			fspa.RelVrtPos = VerticalOrigin.Page;
			if (txbxFormat.VerticalPosition < 0f)
			{
				fspa.YaTop = 0;
			}
		}
		else
		{
			fspa.RelVrtPos = txbxFormat.VerticalOrigin;
		}
		fspa.TextWrappingStyle = txbxFormat.TextWrappingStyle;
		fspa.TextWrappingType = txbxFormat.TextWrappingType;
		fspa.IsBelowText = txbxFormat.IsBelowText;
		fspa.Spid = txbxFormat.TextBoxShapeID;
	}

	private static void ExportPosition(MsofbtSpContainer txbxContainer, WTextBoxFormat txbxFormat)
	{
		if (txbxContainer.ShapePosition.XAlign != uint.MaxValue)
		{
			txbxFormat.HorizontalAlignment = (ShapeHorizontalAlignment)txbxContainer.ShapePosition.XAlign;
		}
		if (txbxContainer.ShapePosition.YAlign != uint.MaxValue)
		{
			txbxFormat.VerticalAlignment = (ShapeVerticalAlignment)txbxContainer.ShapePosition.YAlign;
		}
		if (txbxContainer.ShapePosition.XRelTo != uint.MaxValue)
		{
			txbxFormat.HorizontalOrigin = (HorizontalOrigin)txbxContainer.ShapePosition.XRelTo;
		}
		if (txbxContainer.ShapePosition.YRelTo != uint.MaxValue)
		{
			txbxFormat.VerticalOrigin = (VerticalOrigin)txbxContainer.ShapePosition.YRelTo;
		}
		if (txbxContainer.ShapeOptions.DistanceFromRight != uint.MaxValue)
		{
			txbxFormat.WrapDistanceRight = (float)txbxContainer.ShapeOptions.DistanceFromRight / 12700f;
		}
		if (txbxContainer.ShapeOptions.DistanceFromLeft != uint.MaxValue)
		{
			txbxFormat.WrapDistanceLeft = (float)txbxContainer.ShapeOptions.DistanceFromLeft / 12700f;
		}
		if (txbxContainer.ShapeOptions.DistanceFromBottom != uint.MaxValue)
		{
			txbxFormat.WrapDistanceBottom = (float)txbxContainer.ShapeOptions.DistanceFromBottom / 12700f;
		}
		if (txbxContainer.ShapeOptions.DistanceFromTop != uint.MaxValue)
		{
			txbxFormat.WrapDistanceTop = (float)txbxContainer.ShapeOptions.DistanceFromTop / 12700f;
		}
	}

	private static void ExportIntMargin(MsofbtSpContainer txbxContainer, WTextBoxFormat txbxFormat)
	{
		uint propertyValue = txbxContainer.GetPropertyValue(129);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Left = (float)propertyValue / 12700f;
		}
		propertyValue = txbxContainer.GetPropertyValue(131);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Right = (float)propertyValue / 12700f;
		}
		propertyValue = txbxContainer.GetPropertyValue(130);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Top = (float)propertyValue / 12700f;
		}
		propertyValue = txbxContainer.GetPropertyValue(132);
		if (propertyValue != uint.MaxValue)
		{
			txbxFormat.InternalMargin.Bottom = (float)propertyValue / 12700f;
		}
	}
}
