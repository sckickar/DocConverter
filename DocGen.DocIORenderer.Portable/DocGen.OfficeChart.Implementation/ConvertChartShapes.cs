using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation;

internal class ConvertChartShapes
{
	private static readonly PointF[] _arrowPoints = new PointF[4]
	{
		new PointF(0f, 0f),
		new PointF(-5f, -10f),
		new PointF(5f, -10f),
		new PointF(0f, 0f)
	};

	private static readonly PointF[] _arrowOpenPoints = new PointF[3]
	{
		new PointF(-5f, -11f),
		new PointF(0f, -2f),
		new PointF(5f, -11f)
	};

	private static readonly PointF[] _arrowStealthPoints = new PointF[5]
	{
		new PointF(0f, -6f),
		new PointF(5f, -10f),
		new PointF(0f, 0f),
		new PointF(-5f, -10f),
		new PointF(0f, -6f)
	};

	private static readonly PointF[] _arrowDiamondPoints = new PointF[5]
	{
		new PointF(-5f, 0f),
		new PointF(0f, -5f),
		new PointF(5f, 0f),
		new PointF(0f, 5f),
		new PointF(-5f, 0f)
	};

	private StringFormat _stringFormat;

	private Graphics m_graphics;

	private RectangleF _currentCellRect;

	private WorkbookImpl m_workbook;

	private ChartImpl m_chartImpl;

	private ChartRenderingOptions m_imageOptions;

	internal StringFormat StringFormt
	{
		get
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags &= ~StringFormatFlags.LineLimit;
			stringFormat.FormatFlags |= StringFormatFlags.NoClip;
			return stringFormat;
		}
	}

	internal ChartRenderingOptions ImageOptions
	{
		get
		{
			if (m_imageOptions == null)
			{
				m_imageOptions = new ChartRenderingOptions();
			}
			return m_imageOptions;
		}
		set
		{
			m_imageOptions = value;
		}
	}

	internal ConvertChartShapes(WorkbookImpl workbook, ChartImpl chartImpl)
	{
		m_workbook = workbook;
		m_chartImpl = chartImpl;
	}

	internal void DrawChartShapes(Stream imageAsStream, int width, int height)
	{
		width = width * 96 / 72;
		height = height * 96 / 72;
		if (m_chartImpl.Shapes.Count > 0)
		{
			IShape[] array = new IShape[m_chartImpl.Shapes.Count];
			for (int i = 0; i < m_chartImpl.Shapes.Count; i++)
			{
				array[i] = m_chartImpl.Shapes[i];
			}
			DocGen.Drawing.SkiaSharpHelper.Image image = DocGen.Drawing.SkiaSharpHelper.Image.FromStream(imageAsStream as MemoryStream);
			int width2 = image.Width;
			int height2 = image.Height;
			double scaleWidth = (double)width2 / (double)width;
			double scaleHeight = (double)height2 / (double)height;
			Bitmap bitmap = new Bitmap(image.Width, image.Height);
			bitmap.SetResolution(96f, 96f);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.PageUnit = GraphicsUnit.Pixel;
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
			DrawShapesCollection(array, graphics, scaleWidth, scaleHeight);
			imageAsStream.Position = 0L;
			imageAsStream.SetLength(0L);
			graphics.SkSurface.Snapshot().Encode(GetImageFormat(ImageOptions.ImageFormat), 100).SaveTo(imageAsStream);
		}
	}

	internal SKEncodedImageFormat GetImageFormat(ExportImageFormat imageFormat)
	{
		return imageFormat switch
		{
			ExportImageFormat.Jpeg => SKEncodedImageFormat.Jpeg, 
			ExportImageFormat.Png => SKEncodedImageFormat.Png, 
			_ => SKEncodedImageFormat.Png, 
		};
	}

	internal void DrawShapesCollection(IShape[] shapes, Graphics graphics, double scaleWidth, double scaleHeight)
	{
		if (shapes == null)
		{
			return;
		}
		foreach (IShape shape in shapes)
		{
			if (!shape.IsShapeVisible)
			{
				continue;
			}
			switch (shape.ShapeType)
			{
			case OfficeShapeType.Picture:
				DrawImage(shape as ShapeImpl, DocGen.Drawing.SkiaSharpHelper.Image.FromImage((shape as IPictureShape).Picture), graphics, scaleWidth, scaleHeight);
				break;
			case OfficeShapeType.AutoShape:
			case OfficeShapeType.TextBox:
				DrawShape(shape as ShapeImpl, graphics, scaleWidth, scaleHeight);
				break;
			case OfficeShapeType.Group:
				if ((shape as GroupShapeImpl).Group == null)
				{
					(shape as GroupShapeImpl).LayoutGroupShape(isAll: true);
				}
				DrawGroupShape(shape as IGroupShape, graphics, scaleWidth, scaleHeight);
				break;
			}
		}
	}

	internal void DrawGroupShape(IGroupShape groupShape, Graphics graphics, double scaledWidth, double scaledHeight)
	{
		DrawShapesCollection(groupShape.Items, graphics, scaledWidth, scaledHeight);
	}

	internal void DrawImage(ShapeImpl shape, DocGen.Drawing.SkiaSharpHelper.Image image, Graphics graphics, double scaleWidth, double scaleHeight)
	{
		float x;
		float y;
		float width;
		float height;
		if (shape.GroupFrame != null)
		{
			x = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetX, MeasureUnits.EMU) * scaleWidth);
			y = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetY, MeasureUnits.EMU) * scaleHeight);
			width = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetCX, MeasureUnits.EMU) * scaleWidth);
			height = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetCY, MeasureUnits.EMU) * scaleHeight);
		}
		else
		{
			x = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeX * scaleWidth, MeasureUnits.Pixel);
			y = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeY * scaleHeight, MeasureUnits.Pixel);
			width = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeWidth * scaleWidth, MeasureUnits.Pixel);
			height = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeHeight * scaleHeight, MeasureUnits.Pixel);
		}
		RectangleF rectangleF = new RectangleF(x, y, width, height);
		if (rectangleF.Width == 0f)
		{
			rectangleF.Width = 0.1f;
		}
		if (rectangleF.Height == 0f)
		{
			rectangleF.Height = 0.1f;
		}
		graphics.ResetTransform();
		ImageAttributes imageAttributes = null;
		if (shape is BitmapShapeImpl)
		{
			BitmapShapeImpl bitmapShapeImpl = shape as BitmapShapeImpl;
			if (bitmapShapeImpl.GrayScale || bitmapShapeImpl.Threshold > 0)
			{
				image = ApplyRecolor(bitmapShapeImpl, image);
			}
			if (bitmapShapeImpl.ColorChange != null && bitmapShapeImpl.ColorChange.Count == 2)
			{
				if (bitmapShapeImpl.ColorChange[1].GetRGB(m_workbook).A == 0)
				{
					Bitmap bitmap = new Bitmap(image);
					Color pixel = bitmap.GetPixel(1, 1);
					bitmap.MakeTransparent(pixel);
					MemoryStream memoryStream = new MemoryStream();
					bitmap.Save(memoryStream, ImageFormat.Png);
					image = DocGen.Drawing.SkiaSharpHelper.Image.FromStream(memoryStream);
				}
				else
				{
					imageAttributes = new ImageAttributes();
					imageAttributes = ColorChange(bitmapShapeImpl, imageAttributes);
				}
			}
			if (bitmapShapeImpl.DuoTone != null && bitmapShapeImpl.DuoTone.Count == 2)
			{
				image = ApplyDuoTone(image, bitmapShapeImpl.DuoTone);
			}
			double num = 1.0;
			if (bitmapShapeImpl.Amount / 100000 < 1)
			{
				if (num < 0.0)
				{
					num = 0.0;
				}
				if (imageAttributes == null)
				{
					imageAttributes = new ImageAttributes();
				}
				ApplyImageTransparency(imageAttributes, (float)num);
			}
			if (imageAttributes != null)
			{
				Bitmap bitmap2 = new Bitmap(image.Width, image.Height);
				Graphics graphics2 = Graphics.FromImage(bitmap2);
				graphics2.DrawImage(image, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), 0f, 0f, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				image = bitmap2;
				graphics2.Dispose();
			}
		}
		MemoryStream stream = new MemoryStream();
		if (shape is BitmapShapeImpl)
		{
			BitmapShapeImpl bitmapShapeImpl2 = shape as BitmapShapeImpl;
			double leftOffset = (double)bitmapShapeImpl2.CropLeftOffset / 1000.0;
			double topOffset = bitmapShapeImpl2.CropTopOffset / 1000;
			double rightOffset = bitmapShapeImpl2.CropRightOffset / 1000;
			double bottomOffset = bitmapShapeImpl2.CropBottomOffset / 1000;
			bool flag = image.RawFormat.Equals(ImageFormat.Png);
			if (rectangleF.Height < (float)image.Size.Height && rectangleF.Width < (float)image.Width && bitmapShapeImpl2.CropLeftOffset > 0 && bitmapShapeImpl2.CropTopOffset > 0 && bitmapShapeImpl2.CropRightOffset > 0 && bitmapShapeImpl2.CropLeftOffset > 0)
			{
				image = CropHFImage(image, leftOffset, topOffset, rightOffset, bottomOffset, bitmapShapeImpl2.HasTransparency);
			}
			if (bitmapShapeImpl2.HasTransparency || flag)
			{
				image.Save(stream, ImageFormat.Png);
			}
		}
		graphics.ResetTransform();
		if (shape is BitmapShapeImpl)
		{
			Rotate(graphics, shape as BitmapShapeImpl, rectangleF);
		}
		graphics.DrawImage(image, rectangleF);
		if (shape is BitmapShapeImpl && shape.Line.Visible)
		{
			Pen pen = CreatePen(shape, scaleWidth);
			graphics.DrawRectangle(pen, rectangleF.X - pen.Width / 2f, rectangleF.Y - pen.Width / 2f, rectangleF.Width + pen.Width, rectangleF.Height + pen.Width);
		}
		graphics.ResetTransform();
	}

	public static DocGen.Drawing.SkiaSharpHelper.Image CropHFImage(DocGen.Drawing.SkiaSharpHelper.Image cropableImage, double leftOffset, double topOffset, double rightOffset, double bottomOffset, bool isTransparent)
	{
		double num = cropableImage.Width;
		double num2 = cropableImage.Height;
		leftOffset = num * (leftOffset / 100.0);
		topOffset = num2 * (topOffset / 100.0);
		rightOffset = num * (rightOffset / 100.0);
		bottomOffset = num2 * (bottomOffset / 100.0);
		RectangleF rectangle = new RectangleF((float)(0.0 - leftOffset), (float)(0.0 - topOffset), (float)num, (float)num2);
		Bitmap bitmap = new Bitmap((int)(num - leftOffset - rightOffset), (int)(num2 - topOffset - bottomOffset));
		bitmap.SetResolution(cropableImage.VerticalResolution, cropableImage.HorizontalResolution);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.FromArgb(0, 255, 255, 255));
		if (isTransparent)
		{
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		}
		graphics.DrawImage(cropableImage, rectangle);
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		Bitmap result = DocGen.Drawing.SkiaSharpHelper.Image.FromStream(memoryStream) as Bitmap;
		memoryStream.Dispose();
		return result;
	}

	private ImageAttributes ColorChange(BitmapShapeImpl pictureImpl, ImageAttributes imageAttributes)
	{
		List<ChartColor> colorChange = pictureImpl.ColorChange;
		ChartColor chartColor = colorChange[0];
		ChartColor chartColor2 = colorChange[1];
		ColorMap[] array = new ColorMap[1]
		{
			new ColorMap()
		};
		array[0].OldColor = chartColor.GetRGB(m_workbook);
		if (pictureImpl.IsUseAlpha)
		{
			array[0].NewColor = chartColor2.GetRGB(m_workbook);
		}
		else
		{
			array[0].NewColor = Color.FromArgb(chartColor2.GetRGB(m_workbook).R, chartColor2.GetRGB(m_workbook).G, chartColor2.GetRGB(m_workbook).B);
		}
		imageAttributes.SetRemapTable(array);
		return imageAttributes;
	}

	private DocGen.Drawing.SkiaSharpHelper.Image ApplyDuoTone(DocGen.Drawing.SkiaSharpHelper.Image image, List<ChartColor> duotone)
	{
		if (duotone.Count != 2)
		{
			return image;
		}
		ChartColor chartColor = new ChartColor(ColorExtension.Empty);
		ChartColor chartColor2 = new ChartColor(ColorExtension.Empty);
		Bitmap bitmap = image as Bitmap;
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
		chartColor = duotone[1];
		chartColor2 = duotone[0];
		Color color = Color.FromArgb(255 - chartColor2.GetRGB(m_workbook).A, chartColor2.GetRGB(m_workbook));
		Color color2 = Color.FromArgb(255 - chartColor.GetRGB(m_workbook).A, chartColor.GetRGB(m_workbook));
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		BitmapData bitmapData2 = bitmap2.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		int num = Math.Abs(bitmapData.Stride) * bitmap.Height;
		byte[] array = new byte[num];
		Marshal.Copy(bitmapData.Scan0, array, 0, num);
		num = Math.Abs(bitmapData2.Stride) * bitmap2.Height;
		byte[] array2 = new byte[num];
		Marshal.Copy(bitmapData2.Scan0, array2, 0, num);
		for (int i = 0; i < num; i += 4)
		{
			Color inputPixelColor = Color.FromArgb(array[i + 3], array[i + 2], array[i + 1], array[i]);
			float num2 = (float)Math.Sqrt(0.299 * (double)(int)inputPixelColor.R * (double)(int)inputPixelColor.R + 0.587 * (double)(int)inputPixelColor.G * (double)(int)inputPixelColor.G + 0.114 * (double)(int)inputPixelColor.B * (double)(int)inputPixelColor.B);
			float factor = num2 / 255f;
			Color empty = Color.Empty;
			empty = ((num2 != 255f && num2 != 0f) ? ExecuteLinearInterpolation(color, color2, inputPixelColor, factor) : ((num2 != 255f) ? Color.FromArgb(inputPixelColor.A, color) : Color.FromArgb(inputPixelColor.A, color2)));
			array2[i] = empty.B;
			array2[i + 1] = empty.G;
			array2[i + 2] = empty.R;
			array2[i + 3] = empty.A;
		}
		Marshal.Copy(array2, 0, bitmapData2.Scan0, num);
		bitmap.UnlockBits(bitmapData);
		bitmap2.UnlockBits(bitmapData2);
		bitmap.Dispose();
		return bitmap2;
	}

	private Color ExecuteLinearInterpolation(Color firstColor, Color secondColor, Color inputPixelColor, float factor)
	{
		int red = (int)((1f - factor) * (float)(int)firstColor.R + factor * (float)(int)secondColor.R);
		int green = (int)((1f - factor) * (float)(int)firstColor.G + factor * (float)(int)secondColor.G);
		int blue = (int)((1f - factor) * (float)(int)firstColor.B + factor * (float)(int)secondColor.B);
		return Color.FromArgb(inputPixelColor.A, red, green, blue);
	}

	private Bitmap CreateNonIndexedImage(DocGen.Drawing.SkiaSharpHelper.Image sourceImage)
	{
		return sourceImage as Bitmap;
	}

	private void ApplyImageTransparency(ImageAttributes imgAttribute, float transparency)
	{
		ColorMatrix colorMatrix = new ColorMatrix();
		colorMatrix.Matrix33 = transparency;
		imgAttribute.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
	}

	private Bitmap ApplyRecolor(BitmapShapeImpl picture, DocGen.Drawing.SkiaSharpHelper.Image image)
	{
		Bitmap bitmap = null;
		bitmap = (image.PixelFormat.ToString().ToLower().Contains("rgb") ? ((Bitmap)image) : CreateNonIndexedImage(image));
		for (int i = 0; i < bitmap.Width; i++)
		{
			for (int j = 0; j < bitmap.Height; j++)
			{
				Color pixel = bitmap.GetPixel(i, j);
				if (picture.GrayScale)
				{
					byte b = (byte)(0.299 * (double)(int)pixel.R + 0.587 * (double)(int)pixel.G + 0.114 * (double)(int)pixel.B);
					bitmap.SetPixel(i, j, Color.FromArgb(pixel.A, b, b, b));
				}
				else
				{
					int num = (((0.299 * (double)(int)pixel.R + 0.587 * (double)(int)pixel.G + 0.114 * (double)(int)pixel.B) / 2.5 >= (double)(picture.Threshold / 1000)) ? 255 : 0);
					bitmap.SetPixel(i, j, Color.FromArgb(pixel.A, num, num, num));
				}
			}
		}
		return bitmap;
	}

	internal void DrawShape(ShapeImpl shape, Graphics graphics, double scaleWidth, double scaleHeight)
	{
		if (shape.IsShapeVisible)
		{
			float x;
			float y;
			float width;
			float height;
			if (shape.GroupFrame != null)
			{
				x = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetX, MeasureUnits.EMU) * scaleWidth);
				y = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetY, MeasureUnits.EMU) * scaleHeight);
				width = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetCX, MeasureUnits.EMU) * scaleWidth);
				height = (float)(ApplicationImpl.ConvertToPixels(shape.GroupFrame.OffsetCY, MeasureUnits.EMU) * scaleHeight);
			}
			else
			{
				x = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeX * scaleWidth, MeasureUnits.Pixel);
				y = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeY * scaleHeight, MeasureUnits.Pixel);
				width = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeWidth * scaleWidth, MeasureUnits.Pixel);
				height = (float)ApplicationImpl.ConvertFromPixel(shape.ChartShapeHeight * scaleHeight, MeasureUnits.Pixel);
			}
			RectangleF rect = new RectangleF(x, y, width, height);
			if (rect.Width == 0f)
			{
				rect.Width = 0.1f;
			}
			if (rect.Height == 0f)
			{
				rect.Height = 0.1f;
			}
			graphics.ResetTransform();
			rect = shape.UpdateShapeBounds(rect, shape.GetShapeRotation());
			Rotate(graphics, shape, rect);
			Pen pen = CreatePen(shape, shape.Line, scaleWidth);
			GraphicsPath graphicsPath = null;
			if (shape is TextBoxShapeImpl)
			{
				graphicsPath = new GraphicsPath();
				graphicsPath.AddRectangle(rect);
			}
			else
			{
				m_graphics = graphics;
				graphicsPath = GetGraphicsPath(rect, ref pen, graphics, shape as AutoShapeImpl);
			}
			DrawShapeFillAndLine(graphicsPath, shape, pen, graphics, rect);
			IRichTextString richText = null;
			if (shape is TextBoxShapeImpl)
			{
				richText = (shape as TextBoxShapeImpl).RichText;
			}
			else if (shape is AutoShapeImpl)
			{
				richText = (shape as AutoShapeImpl).TextFrame.TextRange.RichText;
			}
			graphics.CompositingMode = CompositingMode.SourceOver;
			DrawShapeRTFText(richText, shape, rect, graphics, scaleWidth, scaleHeight);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.ResetTransform();
		}
	}

	internal static void RotateText(RectangleF bounds, TextDirection textDirectionType, Graphics graphics)
	{
		switch (textDirectionType)
		{
		case TextDirection.RotateAllText90:
			graphics.TranslateTransform(bounds.X + bounds.Y + bounds.Height, bounds.Y - bounds.X);
			graphics.RotateTransform(90f);
			break;
		case TextDirection.RotateAllText270:
			graphics.TranslateTransform(bounds.X - bounds.Y, bounds.X + bounds.Y + bounds.Width);
			graphics.RotateTransform(270f);
			break;
		}
	}

	internal static void ApplyRotation(ShapeImpl shape, RectangleF bounds, float rotationAngle, Graphics graphics)
	{
		bool flag = false;
		bool flag2 = false;
		if (shape is AutoShapeImpl)
		{
			flag = (shape as AutoShapeImpl).ShapeExt.FlipVertical;
			flag2 = (shape as AutoShapeImpl).ShapeExt.FlipHorizontal;
		}
		else if (shape is TextBoxShapeImpl)
		{
			flag = (shape as TextBoxShapeImpl).FlipVertical;
			flag2 = (shape as TextBoxShapeImpl).FlipHorizontal;
		}
		if (shape.Group != null && (IsGroupFlipH(shape.Group) || IsGroupFlipV(shape.Group)))
		{
			int flipVCount = GetFlipVCount(shape.Group, flag ? 1 : 0);
			int flipHCount = GetFlipHCount(shape.Group, flag2 ? 1 : 0);
			rotationAngle = shape.GetShapeRotation();
			if (flipVCount % 2 != 0)
			{
				graphics.Transform = GetTransformMatrix(bounds, rotationAngle, flipV: true, flipH: true);
			}
			else if (flipHCount % 2 != 0)
			{
				graphics.Transform = GetTransformMatrix(bounds, rotationAngle, flipV: false, flipH: false);
			}
		}
		else if (flag)
		{
			graphics.Transform = GetTransformMatrix(bounds, rotationAngle, flipV: true, flipH: true);
		}
		else if (flag2)
		{
			graphics.Transform = GetTransformMatrix(bounds, rotationAngle, flipV: false, flipH: false);
		}
	}

	internal static void UpdateShapeBoundsToLayoutTextBody(ref RectangleF layoutRect, RectangleF shapeBounds, ShapeImpl shape, double scaledWidth, double scaledHeight)
	{
		float num = 7.2f;
		float num2 = 7.2f;
		float num3 = 3.6f;
		float num4 = 3.6f;
		if (shape is AutoShapeImpl)
		{
			layoutRect.Height -= layoutRect.Y;
			layoutRect.Y += shapeBounds.Y;
			layoutRect.Width -= layoutRect.X;
			layoutRect.X += shapeBounds.X;
			num = (float)((shape.TextFrame as TextFrame).TextBodyProperties.LeftMarginPt * scaledWidth);
			num2 = (float)((shape.TextFrame as TextFrame).TextBodyProperties.RightMarginPt * scaledWidth);
			num3 = (float)((shape.TextFrame as TextFrame).TextBodyProperties.TopMarginPt * scaledHeight);
			num4 = (float)((shape.TextFrame as TextFrame).TextBodyProperties.BottomMarginPt * scaledHeight);
		}
		else if (shape is TextBoxShapeImpl)
		{
			num = (float)((shape as TextBoxShapeImpl).TextBodyPropertiesHolder.LeftMarginPt * scaledWidth);
			num2 = (float)((shape as TextBoxShapeImpl).TextBodyPropertiesHolder.RightMarginPt * scaledWidth);
			num3 = (float)((shape as TextBoxShapeImpl).TextBodyPropertiesHolder.TopMarginPt * scaledHeight);
			num4 = (float)((shape as TextBoxShapeImpl).TextBodyPropertiesHolder.BottomMarginPt * scaledHeight);
		}
		else
		{
			num = (float)((double)num * scaledWidth);
			num2 = (float)((double)num2 * scaledWidth);
			num3 = (float)((double)num3 * scaledHeight);
			num4 = (float)((double)num4 * scaledHeight);
		}
		layoutRect.X += num;
		layoutRect.Y += num3;
		layoutRect.Width -= num + num2;
		layoutRect.Height -= num3 + num4;
	}

	internal static StringAlignment GetVerticalAlignmentFromShape(IShape shape)
	{
		StringAlignment result = StringAlignment.Center;
		if (shape is AutoShapeImpl)
		{
			switch (shape.TextFrame.VerticalAlignment)
			{
			case OfficeVerticalAlignment.Middle:
			case OfficeVerticalAlignment.MiddleCentered:
				result = StringAlignment.Center;
				break;
			case OfficeVerticalAlignment.Bottom:
			case OfficeVerticalAlignment.BottomCentered:
				result = StringAlignment.Far;
				break;
			default:
				result = StringAlignment.Near;
				break;
			}
		}
		else if (shape is TextBoxShapeImpl)
		{
			result = (shape as TextBoxShapeImpl).VAlignment switch
			{
				OfficeCommentVAlign.Center => StringAlignment.Center, 
				OfficeCommentVAlign.Bottom => StringAlignment.Far, 
				_ => StringAlignment.Near, 
			};
		}
		return result;
	}

	internal static float GetRotationAngle(TextDirection textDirection)
	{
		float result = 0f;
		switch (textDirection)
		{
		case TextDirection.RotateAllText90:
			result = 90f;
			break;
		case TextDirection.RotateAllText270:
			result = 270f;
			break;
		}
		return result;
	}

	internal static StringAlignment GetTextAlignmentFromShape(IShape shape)
	{
		StringAlignment stringAlignment = StringAlignment.Near;
		if (shape is AutoShapeImpl)
		{
			switch (shape.TextFrame.HorizontalAlignment)
			{
			case OfficeHorizontalAlignment.Center:
				stringAlignment = StringAlignment.Center;
				break;
			case OfficeHorizontalAlignment.Left:
				stringAlignment = StringAlignment.Near;
				break;
			case OfficeHorizontalAlignment.Right:
				stringAlignment = StringAlignment.Far;
				break;
			}
			if (GetVerticalAnchorPosition((shape as AutoShapeImpl).TextFrame.TextDirection, (shape as AutoShapeImpl).TextFrame.VerticalAlignment, (shape as AutoShapeImpl).TextFrame.HorizontalAlignment) && stringAlignment != StringAlignment.Center)
			{
				stringAlignment = StringAlignment.Center;
			}
		}
		else if (shape is TextBoxShapeImpl)
		{
			switch ((shape as TextBoxShapeImpl).HAlignment)
			{
			case OfficeCommentHAlign.Center:
				stringAlignment = StringAlignment.Center;
				break;
			case OfficeCommentHAlign.Left:
				stringAlignment = StringAlignment.Near;
				break;
			case OfficeCommentHAlign.Right:
				stringAlignment = StringAlignment.Far;
				break;
			}
		}
		return stringAlignment;
	}

	private static bool GetVerticalAnchorPosition(TextDirection textDirection, OfficeVerticalAlignment verticalAlignment, OfficeHorizontalAlignment horizontalAlignment)
	{
		switch (textDirection)
		{
		case TextDirection.Horizontal:
			switch (verticalAlignment)
			{
			case OfficeVerticalAlignment.Top:
			case OfficeVerticalAlignment.Middle:
			case OfficeVerticalAlignment.Bottom:
				return false;
			case OfficeVerticalAlignment.TopCentered:
			case OfficeVerticalAlignment.MiddleCentered:
			case OfficeVerticalAlignment.BottomCentered:
				return true;
			}
			break;
		case TextDirection.RotateAllText90:
		case TextDirection.StackedRightToLeft:
			switch (horizontalAlignment)
			{
			case OfficeHorizontalAlignment.Left:
			case OfficeHorizontalAlignment.Center:
			case OfficeHorizontalAlignment.Right:
				return false;
			case OfficeHorizontalAlignment.LeftMiddle:
			case OfficeHorizontalAlignment.CenterMiddle:
			case OfficeHorizontalAlignment.RightMiddle:
				return true;
			}
			break;
		case TextDirection.RotateAllText270:
		case TextDirection.StackedLeftToRight:
			switch (horizontalAlignment)
			{
			case OfficeHorizontalAlignment.Left:
			case OfficeHorizontalAlignment.Center:
			case OfficeHorizontalAlignment.Right:
				return false;
			case OfficeHorizontalAlignment.LeftMiddle:
			case OfficeHorizontalAlignment.CenterMiddle:
			case OfficeHorizontalAlignment.RightMiddle:
				return true;
			}
			break;
		}
		return false;
	}

	private void DrawShapeRTFText(IRichTextString richText, ShapeImpl shape, RectangleF rect, Graphics graphics, double scaledWidth, double scaledHeight)
	{
		bool flag = true;
		bool isVerticalTextOverflow = false;
		bool isHorizontalTextOverflow = false;
		if (string.IsNullOrEmpty(richText.Text))
		{
			return;
		}
		_currentCellRect = GetBoundsToLayoutShapeTextBody(shape as AutoShapeImpl, rect);
		UpdateShapeBoundsToLayoutTextBody(ref _currentCellRect, rect, shape, scaledWidth, scaledHeight);
		if (shape is TextBoxShapeImpl)
		{
			string textDirection = ((Excel2007TextRotation)(shape as TextBoxShapeImpl).TextRotation).ToString();
			(shape as TextBoxShapeImpl).TextBodyPropertiesHolder.TextDirection = Helper.SetTextDirection(textDirection);
		}
		TextDirection textDirection2 = ((shape is AutoShapeImpl) ? shape.TextFrame.TextDirection : (shape as TextBoxShapeImpl).TextBodyPropertiesHolder.TextDirection);
		if ((uint)(textDirection2 - 1) <= 1u)
		{
			float width = _currentCellRect.Width;
			_currentCellRect.Width = _currentCellRect.Height;
			_currentCellRect.Height = width;
		}
		float rotationAngle = shape.ShapeRotation;
		ApplyRotation(shape, rect, rotationAngle, graphics);
		RotateText(_currentCellRect, textDirection2, graphics);
		IOfficeFont font = richText.GetFont(0);
		StringFormat stringFormt = StringFormt;
		stringFormt.Alignment = GetTextAlignmentFromShape(shape);
		GetFont(font, font.FontName, (int)font.Size);
		stringFormt.LineAlignment = GetVerticalAlignmentFromShape(shape);
		if (GetRotationAngle(textDirection2) > 0f)
		{
			UpdateAlignment(stringFormt, (int)GetRotationAngle(textDirection2), shape);
		}
		stringFormt.Trimming = StringTrimming.Word;
		stringFormt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		_stringFormat = stringFormt;
		List<IOfficeFont> richTextFonts = new List<IOfficeFont>();
		List<string> drawString = m_workbook.GetDrawString(richText.Text, richText as RichTextString, out richTextFonts, richText.GetFont(0));
		if (drawString.Count > 1 && drawString[drawString.Count - 1].Equals("\n"))
		{
			drawString.RemoveAt(drawString.Count - 1);
		}
		if (shape is AutoShapeImpl)
		{
			for (int i = 0; i < richTextFonts.Count; i++)
			{
				if (FontColorNeedsUpdation(richTextFonts[i]))
				{
					if (richTextFonts[i] is FontWrapper fontWrapper)
					{
						FontWrapper fontWrapper2 = new FontWrapper(fontWrapper.Font.Clone(fontWrapper), bReadOnly: false, bRaiseEvents: false);
						fontWrapper2.RGBColor = shape.GetDefaultColor(PreservedFlag.RichText, "fontRef");
						richTextFonts[i] = fontWrapper2;
					}
					else
					{
						FontImpl fontImpl = (richTextFonts[i] as FontImpl).Clone() as FontImpl;
						fontImpl.RGBColor = shape.GetDefaultColor(PreservedFlag.RichText, "fontRef");
						richTextFonts[i] = fontImpl;
					}
				}
			}
		}
		if (shape is AutoShapeImpl)
		{
			AutoShapeImpl autoShapeImpl = shape as AutoShapeImpl;
			if (autoShapeImpl.TextFrameInternal != null && autoShapeImpl.TextFrameInternal.TextBodyProperties != null)
			{
				flag = autoShapeImpl.TextFrameInternal.TextBodyProperties.WrapTextInShape;
				if (!flag)
				{
					flag = false;
				}
				if (autoShapeImpl.TextFrameInternal.TextBodyProperties.TextVertOverflowType == TextVertOverflowType.OverFlow)
				{
					isVerticalTextOverflow = true;
				}
				if (autoShapeImpl.TextFrameInternal.TextBodyProperties.TextHorzOverflowType == TextHorzOverflowType.OverFlow)
				{
					isHorizontalTextOverflow = true;
				}
			}
		}
		else
		{
			string value = string.Empty;
			TextBoxShapeBase textBoxShapeBase = shape as TextBoxShapeBase;
			if (textBoxShapeBase.UnknownBodyProperties != null)
			{
				textBoxShapeBase.UnknownBodyProperties.TryGetValue("wrap", out value);
				if (value == "none")
				{
					flag = false;
				}
				textBoxShapeBase.UnknownBodyProperties.TryGetValue("vertOverflow", out value);
				if (value == "overflow" || string.IsNullOrEmpty(value))
				{
					isVerticalTextOverflow = true;
				}
				textBoxShapeBase.UnknownBodyProperties.TryGetValue("horzOverflow", out value);
				if (value == "overflow" || string.IsNullOrEmpty(value))
				{
					isHorizontalTextOverflow = true;
				}
			}
		}
		List<IOfficeFont> list = new List<IOfficeFont>();
		if (richTextFonts.Count > 0)
		{
			foreach (IOfficeFont item in richTextFonts)
			{
				double size = item.Size * scaledWidth;
				FontImpl fontImpl2 = new FontImpl(item);
				fontImpl2.Size = size;
				list.Add(fontImpl2);
			}
		}
		DrawRTFText(_currentCellRect, _currentCellRect, graphics, list, drawString, isShape: true, flag, isHorizontalTextOverflow, isVerticalTextOverflow);
	}

	internal static StringFormat UpdateAlignment(StringFormat format, int rotationAngle, ShapeImpl shape)
	{
		OfficeHorizontalAlignment officeHorizontalAlignment = OfficeHorizontalAlignment.Left;
		OfficeVerticalAlignment officeVerticalAlignment = OfficeVerticalAlignment.Bottom;
		TextBoxShapeImpl textBoxShapeImpl = shape as TextBoxShapeImpl;
		if (shape is AutoShapeImpl || (textBoxShapeImpl != null && textBoxShapeImpl.IsCreated))
		{
			if (rotationAngle == 270)
			{
				switch (format.Alignment)
				{
				case StringAlignment.Near:
					officeVerticalAlignment = OfficeVerticalAlignment.Top;
					break;
				case StringAlignment.Center:
					officeVerticalAlignment = OfficeVerticalAlignment.Middle;
					break;
				case StringAlignment.Far:
					officeVerticalAlignment = OfficeVerticalAlignment.Bottom;
					break;
				}
				switch (format.LineAlignment)
				{
				case StringAlignment.Near:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Left;
					break;
				case StringAlignment.Center:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Center;
					break;
				case StringAlignment.Far:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Right;
					break;
				}
			}
			else
			{
				switch (format.Alignment)
				{
				case StringAlignment.Near:
					officeVerticalAlignment = OfficeVerticalAlignment.Bottom;
					break;
				case StringAlignment.Center:
					officeVerticalAlignment = OfficeVerticalAlignment.Middle;
					break;
				case StringAlignment.Far:
					officeVerticalAlignment = OfficeVerticalAlignment.Top;
					break;
				}
				switch (format.LineAlignment)
				{
				case StringAlignment.Near:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Right;
					break;
				case StringAlignment.Center:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Center;
					break;
				case StringAlignment.Far:
					officeHorizontalAlignment = OfficeHorizontalAlignment.Left;
					break;
				}
			}
		}
		format.Alignment = officeHorizontalAlignment switch
		{
			OfficeHorizontalAlignment.Right => StringAlignment.Far, 
			OfficeHorizontalAlignment.Center => StringAlignment.Center, 
			OfficeHorizontalAlignment.Left => StringAlignment.Near, 
			_ => format.Alignment, 
		};
		format.LineAlignment = officeVerticalAlignment switch
		{
			OfficeVerticalAlignment.Bottom => StringAlignment.Far, 
			OfficeVerticalAlignment.Middle => StringAlignment.Center, 
			OfficeVerticalAlignment.Top => StringAlignment.Near, 
			_ => format.LineAlignment, 
		};
		return format;
	}

	private void DrawRTFText(RectangleF cellRect, RectangleF adjacentRect, Graphics graphics, List<IOfficeFont> richTextFont, List<string> drawString, bool isShape, bool isWrapText, bool isHorizontalTextOverflow, bool isVerticalTextOverflow)
	{
		new GDIRenderer(graphics, _stringFormat, richTextFont, drawString, m_workbook, 1).DrawRTFText(cellRect, adjacentRect, isShape, isWrapText, isHorizontalTextOverflow, isVerticalTextOverflow, isChartShape: true, isHeaderFooter: false);
	}

	internal static Font GetFont(IOfficeFont font, string fontName, int size)
	{
		new Font(fontName, size);
		FontStyle fontStyle = FontStyle.Regular;
		fontStyle = (font.Bold ? FontStyle.Bold : FontStyle.Regular);
		FontStyle fontStyle2 = FontStyle.Regular;
		if (font.Italic)
		{
			fontStyle2 = FontStyle.Italic;
		}
		FontStyle fontStyle3 = FontStyle.Regular;
		if (font.Underline.ToString() == "Single")
		{
			fontStyle3 = FontStyle.Underline;
		}
		return new Font(fontName, size, fontStyle | fontStyle2 | fontStyle3);
	}

	internal static bool FontColorNeedsUpdation(IOfficeFont font)
	{
		if (font.Color.ToString() == "32767" || (font is FontImpl && (font as FontImpl).ColorObject.Value == 8))
		{
			return true;
		}
		return false;
	}

	internal static RectangleF GetBoundsToLayoutShapeTextBody(AutoShapeImpl shapeImpl, RectangleF bounds)
	{
		if (shapeImpl == null)
		{
			return bounds;
		}
		Dictionary<string, float> dictionary = new GDIShapePath(bounds, shapeImpl.ShapeExt.ShapeGuide).ParseShapeFormula(shapeImpl.ShapeExt.AutoShapeType);
		switch (shapeImpl.ShapeExt.AutoShapeType)
		{
		case AutoShapeType.Oval:
		case AutoShapeType.Donut:
		case AutoShapeType.BlockArc:
		case AutoShapeType.Arc:
		case AutoShapeType.CircularArrow:
		case AutoShapeType.FlowChartConnector:
		case AutoShapeType.FlowChartSequentialAccessStorage:
		case AutoShapeType.DoubleWave:
		case AutoShapeType.CloudCallout:
		case AutoShapeType.Chord:
		case AutoShapeType.Cloud:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.DoubleBracket:
		case AutoShapeType.DoubleBrace:
		case AutoShapeType.FlowChartAlternateProcess:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.BentUpArrow:
			return new RectangleF(0f, dictionary["y2"], dictionary["x4"], bounds.Height);
		case AutoShapeType.Bevel:
			return new RectangleF(dictionary["x1"], dictionary["x1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.Can:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, dictionary["y3"]);
		case AutoShapeType.L_Shape:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.FlowChartDelay:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.Cube:
			return new RectangleF(0f, dictionary["y1"], dictionary["x4"], bounds.Height);
		case AutoShapeType.Decagon:
			return new RectangleF(dictionary["x1"], dictionary["y2"], dictionary["x4"], dictionary["y3"]);
		case AutoShapeType.DiagonalStripe:
			return new RectangleF(0f, 0f, dictionary["x3"], dictionary["y3"]);
		case AutoShapeType.Diamond:
		case AutoShapeType.FlowChartDecision:
		case AutoShapeType.FlowChartCollate:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FlowChartDisplay:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.Dodecagon:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x4"], dictionary["y4"]);
		case AutoShapeType.DownArrow:
			return new RectangleF(dictionary["x1"], 0f, dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.DownArrowCallout:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y2"]);
		case AutoShapeType.FlowChartDocument:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y1"]);
		case AutoShapeType.FlowChartExtract:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 2f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartData:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x5"], bounds.Height);
		case AutoShapeType.FlowChartInternalStorage:
			return new RectangleF(bounds.Width / 8f, bounds.Height / 8f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartMagneticDisk:
			return new RectangleF(0f, bounds.Height / 3f, bounds.Width, dictionary["y3"]);
		case AutoShapeType.FlowChartDirectAccessStorage:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartManualInput:
		case AutoShapeType.FlowChartCard:
			return new RectangleF(0f, bounds.Height / 5f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartManualOperation:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x3"], bounds.Height);
		case AutoShapeType.FlowChartMerge:
			return new RectangleF(bounds.Width / 4f, 0f, dictionary["x2"], bounds.Height / 2f);
		case AutoShapeType.FlowChartMultiDocument:
			return new RectangleF(0f, dictionary["y2"], dictionary["x5"], dictionary["y8"]);
		case AutoShapeType.FlowChartOffPageConnector:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y1"]);
		case AutoShapeType.FlowChartStoredData:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.Parallelogram:
		case AutoShapeType.Hexagon:
		case AutoShapeType.Cross:
		case AutoShapeType.SmileyFace:
		case AutoShapeType.NoSymbol:
		case AutoShapeType.FlowChartTerminator:
		case AutoShapeType.FlowChartSummingJunction:
		case AutoShapeType.FlowChartOr:
		case AutoShapeType.Star16Point:
		case AutoShapeType.Star24Point:
		case AutoShapeType.Star32Point:
		case AutoShapeType.Wave:
		case AutoShapeType.OvalCallout:
		case AutoShapeType.SnipSameSideCornerRectangle:
		case AutoShapeType.Teardrop:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FlowChartPredefinedProcess:
			return new RectangleF(bounds.Width / 8f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartPreparation:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.UTurnArrow:
		case AutoShapeType.FlowChartProcess:
		case AutoShapeType.RectangularCallout:
		case AutoShapeType.StraightConnector:
			return new RectangleF(0f, 0f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartPunchedTape:
			return new RectangleF(0f, bounds.Height / 5f, bounds.Width, dictionary["ib"]);
		case AutoShapeType.FlowChartSort:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FoldedCorner:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y2"]);
		case AutoShapeType.Frame:
			return new RectangleF(dictionary["x1"], dictionary["x1"], dictionary["x4"], dictionary["y4"]);
		case AutoShapeType.Heart:
			return new RectangleF(dictionary["il"], bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.Heptagon:
			return new RectangleF(dictionary["x2"], dictionary["y1"], dictionary["x5"], dictionary["ib"]);
		case AutoShapeType.Pentagon:
		case AutoShapeType.RoundSingleCornerRectangle:
			return new RectangleF(0f, 0f, dictionary["ir"], bounds.Height);
		case AutoShapeType.HorizontalScroll:
			return new RectangleF(dictionary["ch"], dictionary["ch"], dictionary["x4"], dictionary["y6"]);
		case AutoShapeType.Explosion1:
			return new RectangleF(dictionary["x5"], dictionary["y3"], dictionary["x21"], dictionary["y9"]);
		case AutoShapeType.Explosion2:
			return new RectangleF(dictionary["x5"], dictionary["y3"], dictionary["x19"], dictionary["y17"]);
		case AutoShapeType.LeftArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], bounds.Width, dictionary["y2"]);
		case AutoShapeType.LeftArrowCallout:
			return new RectangleF(dictionary["x2"], 0f, bounds.Width, bounds.Height);
		case AutoShapeType.LeftBracket:
		case AutoShapeType.LeftBrace:
			return new RectangleF(dictionary["il"], dictionary["it"], bounds.Width, dictionary["ib"]);
		case AutoShapeType.LeftRightArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x4"], dictionary["y2"]);
		case AutoShapeType.LeftRightArrowCallout:
			return new RectangleF(dictionary["x2"], 0f, dictionary["x3"], bounds.Height);
		case AutoShapeType.LeftRightUpArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["ir"], dictionary["y5"]);
		case AutoShapeType.LeftUpArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["x4"], dictionary["y5"]);
		case AutoShapeType.LightningBolt:
			return new RectangleF(dictionary["x4"], dictionary["y4"], dictionary["x9"], dictionary["y10"]);
		case AutoShapeType.MathDivision:
			return new RectangleF(dictionary["x1"], dictionary["y3"], dictionary["x3"], dictionary["y4"]);
		case AutoShapeType.UpArrow:
		case AutoShapeType.MathEqual:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], bounds.Height);
		case AutoShapeType.UpDownArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], dictionary["y4"]);
		case AutoShapeType.MathMinus:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.MathMultiply:
			return new RectangleF(dictionary["xA"], dictionary["yB"], dictionary["xE"], dictionary["yH"]);
		case AutoShapeType.MathNotEqual:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x8"], dictionary["y4"]);
		case AutoShapeType.MathPlus:
			return new RectangleF(dictionary["x1"], dictionary["y2"], dictionary["x4"], dictionary["y3"]);
		case AutoShapeType.Moon:
			return new RectangleF(dictionary["g12w"], dictionary["g15h"], dictionary["g0w"], dictionary["g16h"]);
		case AutoShapeType.Trapezoid:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.NotchedRightArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x3"], dictionary["y2"]);
		case AutoShapeType.RoundedRectangle:
		case AutoShapeType.Octagon:
		case AutoShapeType.Plaque:
		case AutoShapeType.RoundedRectangularCallout:
		case AutoShapeType.SnipDiagonalCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RegularPentagon:
			return new RectangleF(dictionary["x2"], dictionary["it"], dictionary["x3"], dictionary["y2"]);
		case AutoShapeType.Pie:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.QuadArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["ir"], dictionary["y4"]);
		case AutoShapeType.QuadArrowCallout:
			return new RectangleF(dictionary["x2"], dictionary["y2"], dictionary["x7"], dictionary["y7"]);
		case AutoShapeType.DownRibbon:
			return new RectangleF(dictionary["x2"], dictionary["y2"], dictionary["x9"], bounds.Height);
		case AutoShapeType.UpRibbon:
			return new RectangleF(dictionary["x2"], 0f, dictionary["x9"], dictionary["y2"]);
		case AutoShapeType.RightArrow:
			return new RectangleF(0f, dictionary["y1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.RightArrowCallout:
			return new RectangleF(0f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.RightBracket:
		case AutoShapeType.RightBrace:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RoundDiagonalCornerRectangle:
			return new RectangleF(dictionary["dx"], dictionary["dx"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RoundSameSideCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["tdx"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RightTriangle:
			return new RectangleF(bounds.Width / 12f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.SnipSingleCornerRectangle:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.SnipAndRoundSingleCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], bounds.Height);
		case AutoShapeType.Star10Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy2"], dictionary["sx5"], dictionary["sy3"]);
		case AutoShapeType.Star12Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy2"], dictionary["sx5"], dictionary["sy5"]);
		case AutoShapeType.Star4Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx2"], dictionary["sy2"]);
		case AutoShapeType.Star5Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy3"]);
		case AutoShapeType.Star6Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy2"]);
		case AutoShapeType.Star7Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy1"], dictionary["sx5"], dictionary["sy3"]);
		case AutoShapeType.Star8Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy4"]);
		case AutoShapeType.StripedRightArrow:
			return new RectangleF(dictionary["x4"], dictionary["y1"], dictionary["x6"], dictionary["y2"]);
		case AutoShapeType.Sun:
			return new RectangleF(dictionary["x9"], dictionary["y9"], dictionary["x8"], dictionary["y8"]);
		case AutoShapeType.IsoscelesTriangle:
			return new RectangleF(dictionary["x1"], bounds.Height / 2f, dictionary["x3"], bounds.Height);
		case AutoShapeType.UpArrowCallout:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, bounds.Height);
		case AutoShapeType.UpDownArrowCallout:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, dictionary["y3"]);
		case AutoShapeType.VerticalScroll:
			return new RectangleF(dictionary["ch"], dictionary["ch"], dictionary["x6"], dictionary["y4"]);
		case AutoShapeType.CurvedUpRibbon:
			return new RectangleF(dictionary["x2"], dictionary["y6"], dictionary["x5"], dictionary["rh"]);
		case AutoShapeType.CurvedDownRibbon:
			return new RectangleF(dictionary["x2"], dictionary["q1"], dictionary["x5"], dictionary["y6"]);
		case AutoShapeType.Chevron:
			return new RectangleF(dictionary["il"], 0f, dictionary["ir"], bounds.Height);
		default:
			return new RectangleF(0f, 0f, bounds.Width, bounds.Height);
		}
	}

	private void DrawShapeFillAndLine(GraphicsPath graphicsPath, ShapeImpl shape, Pen pen, Graphics graphics, RectangleF bounds)
	{
		AutoShapeImpl autoShapeImpl = shape as AutoShapeImpl;
		if (graphicsPath.PointCount > 0)
		{
			if ((shape.Fill.Visible || (shape is AutoShapeImpl && (shape as AutoShapeImpl).ShapeExt.AutoShapeType == AutoShapeType.Unknown)) && IsShapeNeedToBeFill(shape))
			{
				IOfficeFill fill = shape.Fill;
				FillBackground(graphics, shape, graphicsPath, fill);
			}
			if (shape.Line.Visible || (shape is AutoShapeImpl && ((shape as AutoShapeImpl).ShapeExt.AutoShapeType == AutoShapeType.Unknown || (autoShapeImpl.ShapeExt.IsCreated && autoShapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line) && autoShapeImpl.Line.Visible) || (autoShapeImpl.ShapeExt.IsCreated && !autoShapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line)))))
			{
				graphics.DrawPath(pen, graphicsPath);
			}
		}
	}

	internal void FillBackground(Graphics pdfGraphics, ShapeImpl shape, GraphicsPath path, IOfficeFill format)
	{
		if (format.Visible && format.FillType == OfficeFillType.SolidColor)
		{
			Color color = NormalizeColor(shape.GetFillColor());
			pdfGraphics.FillPath(new SolidBrush(color), path);
		}
	}

	internal DocGen.Drawing.SkiaSharpHelper.Image CreateImage(RectangleF bounds, MemoryStream stream)
	{
		using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
		{
			bounds.Width = (int)((double)bounds.Width / 96.0 * (double)graphics.DpiX);
			bounds.Height = (int)((double)bounds.Height / 96.0 * (double)graphics.DpiY);
		}
		DocGen.Drawing.SkiaSharpHelper.Image image = null;
		if (bounds.Width == 0f)
		{
			bounds.Width = 1f;
		}
		if (bounds.Height == 0f)
		{
			bounds.Height = 1f;
		}
		using Bitmap bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
		using Graphics graphics2 = Graphics.FromImage(bitmap);
		bitmap.SetResolution(graphics2.DpiX, graphics2.DpiY);
		nint hdc = graphics2.GetHdc();
		Rectangle rect = new Rectangle(0, 0, (int)bounds.Width, (int)bounds.Height);
		image = new Metafile(stream, hdc, rect, DocGen.Drawing.SkiaSharpHelper.MetafileFrameUnit.Pixel, EmfType.EmfPlusOnly);
		graphics2.ReleaseHdc();
		return image;
	}

	private bool IsLine(ShapeImpl shape)
	{
		return false;
	}

	internal static bool IsShapeNeedToBeFill(ShapeImpl shape)
	{
		if (shape is AutoShapeImpl)
		{
			AutoShapeType autoShapeType = (shape as AutoShapeImpl).ShapeExt.AutoShapeType;
			if ((uint)(autoShapeType - 224) <= 1u || (uint)(autoShapeType - 227) <= 7u)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	internal static void Rotate(Graphics graphics, ShapeImpl shapeImpl, RectangleF rectangleF)
	{
		float num = shapeImpl.ShapeRotation;
		if (num > 360f)
		{
			num %= 360f;
		}
		bool flag = false;
		bool flag2 = false;
		if (shapeImpl is AutoShapeImpl)
		{
			flag = (shapeImpl as AutoShapeImpl).ShapeExt.FlipVertical;
			flag2 = (shapeImpl as AutoShapeImpl).ShapeExt.FlipHorizontal;
		}
		else if (shapeImpl is TextBoxShapeImpl)
		{
			flag = (shapeImpl as TextBoxShapeImpl).FlipVertical;
			flag2 = (shapeImpl as TextBoxShapeImpl).FlipHorizontal;
		}
		else if (shapeImpl is BitmapShapeImpl)
		{
			flag = (shapeImpl as BitmapShapeImpl).FlipVertical;
			flag2 = (shapeImpl as BitmapShapeImpl).FlipHorizontal;
		}
		if (shapeImpl.Group != null)
		{
			num = shapeImpl.GetShapeRotation();
			if (IsGroupFlipH(shapeImpl.Group) || IsGroupFlipV(shapeImpl.Group))
			{
				int flipVCount = GetFlipVCount(shapeImpl.Group, flag ? 1 : 0);
				int flipHCount = GetFlipHCount(shapeImpl.Group, flag2 ? 1 : 0);
				graphics.Transform = GetTransformMatrix(rectangleF, num, flipVCount % 2 != 0, flipHCount % 2 != 0);
			}
			else if (num != 0f || flag || flag2)
			{
				graphics.Transform = GetTransformMatrix(rectangleF, num, flag, flag2);
			}
		}
		else if (num != 0f || flag || flag2)
		{
			graphics.Transform = GetTransformMatrix(rectangleF, num, flag, flag2);
		}
		if (shapeImpl is AutoShapeImpl && (shapeImpl as AutoShapeImpl).ShapeExt.PreservedElements.ContainsKey("Scene3d"))
		{
			flag2 = false;
			flag = false;
			num = GetLatFromScene3D(shapeImpl as AutoShapeImpl, out flag2);
			if (num != 0f)
			{
				graphics.Transform = GetTransformMatrix(rectangleF, num, flag, flag2);
			}
		}
	}

	private static float GetLatFromScene3D(AutoShapeImpl shapeImpl, out bool flip)
	{
		float result = 0f;
		int num = 0;
		XmlReader xmlReader = UtilityMethods.CreateReader(shapeImpl.ShapeExt.PreservedElements["Scene3d"], "rot");
		if (xmlReader.MoveToAttribute("lat"))
		{
			result = (int)(Convert.ToInt64(xmlReader.Value) / 60000);
		}
		if (xmlReader.MoveToAttribute("lon"))
		{
			num = (int)(Convert.ToInt64(xmlReader.Value) / 60000);
		}
		if (num == 180)
		{
			flip = false;
		}
		else
		{
			flip = true;
		}
		return result;
	}

	private static bool IsGroupFlipV(GroupShapeImpl group)
	{
		while (group != null)
		{
			if (group.FlipVertical)
			{
				return true;
			}
			group = group.Group;
		}
		return false;
	}

	private static bool IsGroupFlipH(GroupShapeImpl group)
	{
		while (group != null)
		{
			if (group.FlipHorizontal)
			{
				return true;
			}
			group = group.Group;
		}
		return false;
	}

	private static int GetFlipHCount(GroupShapeImpl group, int count)
	{
		while (group != null)
		{
			if (group.FlipHorizontal)
			{
				count++;
			}
			group = group.Group;
		}
		return count;
	}

	private static int GetFlipVCount(GroupShapeImpl group, int count)
	{
		while (group != null)
		{
			if (group.FlipVertical)
			{
				count++;
			}
			group = group.Group;
		}
		return count;
	}

	internal static GraphicsPath GetGraphicsPath(RectangleF bounds, ref Pen pen, Graphics graphics, AutoShapeImpl shapeImpl)
	{
		GDIShapePath gDIShapePath = new GDIShapePath(bounds, shapeImpl.ShapeExt.ShapeGuide);
		GraphicsPath graphicsPath = new GraphicsPath();
		Color empty = Color.Empty;
		switch (shapeImpl.ShapeExt.AutoShapeType)
		{
		case AutoShapeType.Rectangle:
		case AutoShapeType.FlowChartProcess:
			graphicsPath.AddRectangle(bounds);
			return graphicsPath;
		case AutoShapeType.RoundedRectangle:
			return gDIShapePath.GetRoundedRectanglePath();
		case AutoShapeType.SnipSingleCornerRectangle:
			return gDIShapePath.GetSnipSingleCornerRectanglePath();
		case AutoShapeType.SnipSameSideCornerRectangle:
			return gDIShapePath.GetSnipSameSideCornerRectanglePath();
		case AutoShapeType.SnipDiagonalCornerRectangle:
			return gDIShapePath.GetSnipDiagonalCornerRectanglePath();
		case AutoShapeType.SnipAndRoundSingleCornerRectangle:
			return gDIShapePath.GetSnipAndRoundSingleCornerRectanglePath();
		case AutoShapeType.RoundSingleCornerRectangle:
			return gDIShapePath.GetRoundSingleCornerRectanglePath();
		case AutoShapeType.RoundSameSideCornerRectangle:
			return gDIShapePath.GetRoundSameSideCornerRectanglePath();
		case AutoShapeType.RoundDiagonalCornerRectangle:
			return gDIShapePath.GetRoundDiagonalCornerRectanglePath();
		case AutoShapeType.Line:
			graphicsPath.AddLine(bounds.X, bounds.Y, bounds.Right, bounds.Bottom);
			return graphicsPath;
		case AutoShapeType.StraightConnector:
			graphicsPath.AddLine(bounds.X, bounds.Y, bounds.Right, bounds.Bottom);
			return graphicsPath;
		case AutoShapeType.ElbowConnector:
			return gDIShapePath.GetBentConnectorPath();
		case AutoShapeType.CurvedConnector:
			return gDIShapePath.GetCurvedConnectorPath();
		case AutoShapeType.CurvedConnector2:
			return gDIShapePath.GetCurvedConnector2Path();
		case AutoShapeType.CurvedConnector4:
			return gDIShapePath.GetCurvedConnector4Path();
		case AutoShapeType.CurvedConnector5:
			return gDIShapePath.GetCurvedConnector5Path();
		case AutoShapeType.BentConnector2:
			return gDIShapePath.GetBentConnector2Path();
		case AutoShapeType.BentConnector4:
			return gDIShapePath.GetBentConnector4Path();
		case AutoShapeType.BentConnector5:
			return gDIShapePath.GetBentConnector5Path();
		case AutoShapeType.Oval:
			graphicsPath.AddEllipse(bounds);
			return graphicsPath;
		case AutoShapeType.RightTriangle:
			graphicsPath.AddLines(new PointF[3]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X, bounds.Y),
				new PointF(bounds.Right, bounds.Bottom)
			});
			graphicsPath.CloseFigure();
			return graphicsPath;
		case AutoShapeType.IsoscelesTriangle:
			return gDIShapePath.GetTrianglePath();
		case AutoShapeType.Parallelogram:
		case AutoShapeType.FlowChartData:
			return gDIShapePath.GetParallelogramPath();
		case AutoShapeType.Trapezoid:
			return gDIShapePath.GetTrapezoidPath();
		case AutoShapeType.Diamond:
		case AutoShapeType.FlowChartDecision:
			graphicsPath.AddLines(new PointF[4]
			{
				new PointF(bounds.X, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
			});
			graphicsPath.CloseFigure();
			break;
		case AutoShapeType.RegularPentagon:
			return gDIShapePath.GetRegularPentagonPath();
		case AutoShapeType.Hexagon:
			return gDIShapePath.GetHexagonPath();
		case AutoShapeType.Heptagon:
			return gDIShapePath.GetHeptagonPath();
		case AutoShapeType.Octagon:
			return gDIShapePath.GetOctagonPath();
		case AutoShapeType.Decagon:
			return gDIShapePath.GetDecagonPath();
		case AutoShapeType.Dodecagon:
			return gDIShapePath.GetDodecagonPath();
		case AutoShapeType.Pie:
			return gDIShapePath.GetPiePath();
		case AutoShapeType.Chord:
			return gDIShapePath.GetChordPath();
		case AutoShapeType.Teardrop:
			return gDIShapePath.GetTearDropPath();
		case AutoShapeType.Frame:
			return gDIShapePath.GetFramePath();
		case AutoShapeType.HalfFrame:
			return gDIShapePath.GetHalfFramePath();
		case AutoShapeType.L_Shape:
			return gDIShapePath.GetL_ShapePath();
		case AutoShapeType.DiagonalStripe:
			return gDIShapePath.GetDiagonalStripePath();
		case AutoShapeType.Cross:
			return gDIShapePath.GetCrossPath();
		case AutoShapeType.Plaque:
			return gDIShapePath.GetPlaquePath();
		case AutoShapeType.Can:
			return gDIShapePath.GetCanPath();
		case AutoShapeType.Cube:
			return gDIShapePath.GetCubePath();
		case AutoShapeType.Bevel:
			return gDIShapePath.GetBevelPath();
		case AutoShapeType.Donut:
			return gDIShapePath.GetDonutPath();
		case AutoShapeType.NoSymbol:
			return gDIShapePath.GetNoSymbolPath();
		case AutoShapeType.BlockArc:
			return gDIShapePath.GetBlockArcPath();
		case AutoShapeType.FoldedCorner:
			return gDIShapePath.GetFoldedCornerPath();
		case AutoShapeType.SmileyFace:
		{
			for (int j = 0; j < 2; j++)
			{
				bool flag = j == 1;
				GraphicsPath[] horizontalScroll = gDIShapePath.GetSmileyFacePath(flag);
				IOfficeFill fill = shapeImpl.Fill;
				empty = Color.Empty;
				if (fill.FillType == OfficeFillType.SolidColor)
				{
					empty = NormalizeColor(shapeImpl.GetFillColor());
				}
				if (fill.FillType == OfficeFillType.Gradient)
				{
					empty = NormalizeColor(fill.ForeColor);
				}
				if (flag)
				{
					empty = GetDarkerColor(empty, 80f);
				}
				Brush brush = new SolidBrush(empty);
				GraphicsPath[] array = horizontalScroll;
				foreach (GraphicsPath graphicsPath2 in array)
				{
					if (empty != Color.Empty)
					{
						graphics.FillPath(brush, graphicsPath2);
					}
					if ((shapeImpl.ShapeExt.IsCreated && !shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line)) || (shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line) && shapeImpl.ShapeExt.Line.Visible) || (!shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Line.Visible))
					{
						graphics.DrawPath(pen, graphicsPath2);
					}
				}
			}
			break;
		}
		case AutoShapeType.Heart:
			return gDIShapePath.GetHeartPath();
		case AutoShapeType.LightningBolt:
			return gDIShapePath.GetLightningBoltPath();
		case AutoShapeType.Sun:
			return gDIShapePath.GetSunPath();
		case AutoShapeType.Moon:
			return gDIShapePath.GetMoonPath();
		case AutoShapeType.Cloud:
			return gDIShapePath.GetCloudPath();
		case AutoShapeType.Arc:
		{
			GraphicsPath[] horizontalScroll = gDIShapePath.GetArcPath();
			IOfficeFill fill = shapeImpl.Fill;
			Color color = ColorExtension.Empty;
			empty = Color.Empty;
			if (fill.FillType == OfficeFillType.SolidColor)
			{
				color = shapeImpl.GetFillColor();
				empty = NormalizeColor(color);
			}
			Brush brush = new SolidBrush(empty);
			if (empty != Color.Empty && color != ColorExtension.Empty)
			{
				graphics.FillPath(brush, horizontalScroll[1]);
			}
			if ((shapeImpl.ShapeExt.IsCreated && !shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line)) || (shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line) && shapeImpl.ShapeExt.Line.Visible) || (!shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Line.Visible))
			{
				graphics.DrawPath(pen, horizontalScroll[0]);
			}
			break;
		}
		case AutoShapeType.DoubleBracket:
			return gDIShapePath.GetDoubleBracketPath();
		case AutoShapeType.DoubleBrace:
			return gDIShapePath.GetDoubleBracePath();
		case AutoShapeType.LeftBracket:
			return gDIShapePath.GetLeftBracketPath();
		case AutoShapeType.RightBracket:
			return gDIShapePath.GetRightBracketPath();
		case AutoShapeType.LeftBrace:
			return gDIShapePath.GetLeftBracePath();
		case AutoShapeType.RightBrace:
			return gDIShapePath.GetRightBracePath();
		case AutoShapeType.RightArrow:
			return gDIShapePath.GetRightArrowPath();
		case AutoShapeType.LeftArrow:
			return gDIShapePath.GetLeftArrowPath();
		case AutoShapeType.UpArrow:
			return gDIShapePath.GetUpArrowPath();
		case AutoShapeType.DownArrow:
			return gDIShapePath.GetDownArrowPath();
		case AutoShapeType.LeftRightArrow:
			return gDIShapePath.GetLeftRightArrowPath();
		case AutoShapeType.UpDownArrow:
			return gDIShapePath.GetUpDownArrowPath();
		case AutoShapeType.QuadArrow:
			return gDIShapePath.GetQuadArrowPath();
		case AutoShapeType.BentArrow:
			return gDIShapePath.GetBentArrowPath();
		case AutoShapeType.LeftRightUpArrow:
			return gDIShapePath.GetLeftRightUpArrowPath();
		case AutoShapeType.UTurnArrow:
			return gDIShapePath.GetUTrunArrowPath();
		case AutoShapeType.LeftUpArrow:
			return gDIShapePath.GetLeftUpArrowPath();
		case AutoShapeType.BentUpArrow:
			return gDIShapePath.GetBentUpArrowPath();
		case AutoShapeType.CurvedRightArrow:
			return gDIShapePath.GetCurvedRightArrowPath();
		case AutoShapeType.CurvedLeftArrow:
			return gDIShapePath.GetCurvedLeftArrowPath();
		case AutoShapeType.CurvedDownArrow:
			return gDIShapePath.GetCurvedDownArrowPath();
		case AutoShapeType.CurvedUpArrow:
			return gDIShapePath.GetCurvedUpArrowPath();
		case AutoShapeType.StripedRightArrow:
			return gDIShapePath.GetStripedRightArrowPath();
		case AutoShapeType.NotchedRightArrow:
			return gDIShapePath.GetNotchedRightArrowPath();
		case AutoShapeType.Pentagon:
			return gDIShapePath.GetPentagonPath();
		case AutoShapeType.Chevron:
			return gDIShapePath.GetChevronPath();
		case AutoShapeType.RightArrowCallout:
			return gDIShapePath.GetRightArrowCalloutPath();
		case AutoShapeType.DownArrowCallout:
			return gDIShapePath.GetDownArrowCalloutPath();
		case AutoShapeType.LeftArrowCallout:
			return gDIShapePath.GetLeftArrowCalloutPath();
		case AutoShapeType.UpArrowCallout:
			return gDIShapePath.GetUpArrowCalloutPath();
		case AutoShapeType.LeftRightArrowCallout:
			return gDIShapePath.GetLeftRightArrowCalloutPath();
		case AutoShapeType.QuadArrowCallout:
			return gDIShapePath.GetQuadArrowCalloutPath();
		case AutoShapeType.CircularArrow:
			return gDIShapePath.GetCircularArrowPath();
		case AutoShapeType.MathPlus:
			return gDIShapePath.GetMathPlusPath();
		case AutoShapeType.MathMinus:
			return gDIShapePath.GetMathMinusPath();
		case AutoShapeType.MathMultiply:
			return gDIShapePath.GetMathMultiplyPath();
		case AutoShapeType.MathDivision:
			return gDIShapePath.GetMathDivisionPath();
		case AutoShapeType.MathEqual:
			return gDIShapePath.GetMathEqualPath();
		case AutoShapeType.MathNotEqual:
			return gDIShapePath.GetMathNotEqualPath();
		case AutoShapeType.FlowChartAlternateProcess:
			return gDIShapePath.GetFlowChartAlternateProcessPath();
		case AutoShapeType.FlowChartPredefinedProcess:
			return gDIShapePath.GetFlowChartPredefinedProcessPath();
		case AutoShapeType.FlowChartInternalStorage:
			return gDIShapePath.GetFlowChartInternalStoragePath();
		case AutoShapeType.FlowChartDocument:
			return gDIShapePath.GetFlowChartDocumentPath();
		case AutoShapeType.FlowChartMultiDocument:
			return gDIShapePath.GetFlowChartMultiDocumentPath();
		case AutoShapeType.FlowChartPreparation:
			return gDIShapePath.GetFlowChartPreparationPath();
		case AutoShapeType.FlowChartManualInput:
			return gDIShapePath.GetFlowChartManualInputPath();
		case AutoShapeType.FlowChartManualOperation:
			return gDIShapePath.GetFlowChartManualOperationPath();
		case AutoShapeType.FlowChartConnector:
			return gDIShapePath.GetFlowChartConnectorPath();
		case AutoShapeType.FlowChartOffPageConnector:
			return gDIShapePath.GetFlowChartOffPageConnectorPath();
		case AutoShapeType.FlowChartCard:
			return gDIShapePath.GetFlowChartCardPath();
		case AutoShapeType.FlowChartTerminator:
			return gDIShapePath.GetFlowChartTerminatorPath();
		case AutoShapeType.FlowChartPunchedTape:
			return gDIShapePath.GetFlowChartPunchedTapePath();
		case AutoShapeType.FlowChartSummingJunction:
			return gDIShapePath.GetFlowChartSummingJunctionPath();
		case AutoShapeType.FlowChartOr:
			return gDIShapePath.GetFlowChartOrPath();
		case AutoShapeType.FlowChartCollate:
			return gDIShapePath.GetFlowChartCollatePath();
		case AutoShapeType.FlowChartSort:
			return gDIShapePath.GetFlowChartSortPath();
		case AutoShapeType.FlowChartExtract:
			return gDIShapePath.GetFlowChartExtractPath();
		case AutoShapeType.FlowChartMerge:
			return gDIShapePath.GetFlowChartMergePath();
		case AutoShapeType.FlowChartStoredData:
			return gDIShapePath.GetFlowChartOnlineStoragePath();
		case AutoShapeType.FlowChartDelay:
			return gDIShapePath.GetFlowChartDelayPath();
		case AutoShapeType.FlowChartSequentialAccessStorage:
			return gDIShapePath.GetFlowChartSequentialAccessStoragePath();
		case AutoShapeType.FlowChartMagneticDisk:
			return gDIShapePath.GetFlowChartMagneticDiskPath();
		case AutoShapeType.FlowChartDirectAccessStorage:
			return gDIShapePath.GetFlowChartDirectAccessStoragePath();
		case AutoShapeType.FlowChartDisplay:
			return gDIShapePath.GetFlowChartDisplayPath();
		case AutoShapeType.RectangularCallout:
			return gDIShapePath.GetRectangularCalloutPath();
		case AutoShapeType.RoundedRectangularCallout:
			return gDIShapePath.GetRoundedRectangularCalloutPath();
		case AutoShapeType.OvalCallout:
			return gDIShapePath.GetOvalCalloutPath();
		case AutoShapeType.CloudCallout:
			return gDIShapePath.GetCloudCalloutPath();
		case AutoShapeType.LineCallout1:
		case AutoShapeType.LineCallout1NoBorder:
			return gDIShapePath.GetLineCallout1Path();
		case AutoShapeType.LineCallout2:
		case AutoShapeType.LineCallout2NoBorder:
			return gDIShapePath.GetLineCallout2Path();
		case AutoShapeType.LineCallout3:
		case AutoShapeType.LineCallout3NoBorder:
			return gDIShapePath.GetLineCallout3Path();
		case AutoShapeType.LineCallout1AccentBar:
		case AutoShapeType.LineCallout1BorderAndAccentBar:
			return gDIShapePath.GetLineCallout1AccentBarPath();
		case AutoShapeType.LineCallout2AccentBar:
		case AutoShapeType.LineCallout2BorderAndAccentBar:
			return gDIShapePath.GetLineCallout2AccentBarPath();
		case AutoShapeType.LineCallout3AccentBar:
		case AutoShapeType.LineCallout3BorderAndAccentBar:
			return gDIShapePath.GetLineCallout3AccentBarPath();
		case AutoShapeType.Explosion1:
			return gDIShapePath.GetExplosion1();
		case AutoShapeType.Explosion2:
			return gDIShapePath.GetExplosion2();
		case AutoShapeType.Star4Point:
			return gDIShapePath.GetStar4Point();
		case AutoShapeType.Star5Point:
			return gDIShapePath.GetStar5Point();
		case AutoShapeType.Star6Point:
			return gDIShapePath.GetStar6Point();
		case AutoShapeType.Star7Point:
			return gDIShapePath.GetStar7Point();
		case AutoShapeType.Star8Point:
			return gDIShapePath.GetStar8Point();
		case AutoShapeType.Star10Point:
			return gDIShapePath.GetStar10Point();
		case AutoShapeType.Star12Point:
			return gDIShapePath.GetStar12Point();
		case AutoShapeType.Star16Point:
			return gDIShapePath.GetStar16Point();
		case AutoShapeType.Star24Point:
			return gDIShapePath.GetStar24Point();
		case AutoShapeType.Star32Point:
			return gDIShapePath.GetStar32Point();
		case AutoShapeType.UpRibbon:
			return gDIShapePath.GetUpRibbon();
		case AutoShapeType.DownRibbon:
			return gDIShapePath.GetDownRibbon();
		case AutoShapeType.CurvedUpRibbon:
			return gDIShapePath.GetCurvedUpRibbon();
		case AutoShapeType.CurvedDownRibbon:
			return gDIShapePath.GetCurvedDownRibbon();
		case AutoShapeType.VerticalScroll:
			return gDIShapePath.GetVerticalScroll();
		case AutoShapeType.HorizontalScroll:
		{
			GraphicsPath[] horizontalScroll = gDIShapePath.GetHorizontalScroll();
			empty = Color.Empty;
			IOfficeFill fill = shapeImpl.Fill;
			if (fill.FillType == OfficeFillType.SolidColor)
			{
				empty = NormalizeColor(shapeImpl.GetFillColor());
			}
			if (fill.FillType == OfficeFillType.Gradient)
			{
				empty = NormalizeColor(fill.ForeColor);
			}
			Brush brush = new SolidBrush(empty);
			GraphicsPath[] array = horizontalScroll;
			foreach (GraphicsPath path in array)
			{
				if (empty != Color.Empty)
				{
					graphics.FillPath(brush, graphicsPath);
				}
				if ((shapeImpl.ShapeExt.IsCreated && !shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line)) || (shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line) && shapeImpl.ShapeExt.Line.Visible) || (!shapeImpl.ShapeExt.IsCreated && shapeImpl.ShapeExt.Line.Visible))
				{
					graphics.DrawPath(pen, path);
				}
			}
			break;
		}
		case AutoShapeType.Wave:
			return gDIShapePath.GetWave();
		case AutoShapeType.DoubleWave:
			return gDIShapePath.GetDoubleWave();
		default:
			if (shapeImpl.ShapeExt.IsCustomGeometry)
			{
				return GetCustomGeomentryPath(bounds, graphicsPath, shapeImpl);
			}
			break;
		}
		return graphicsPath;
	}

	private static GraphicsPath GetCustomGeomentryPath(RectangleF bounds, GraphicsPath path, ShapeImpl shapeImpl)
	{
		foreach (Path2D path2D in (shapeImpl as AutoShapeImpl).ShapeExt.Path2DList)
		{
			double width = path2D.Width;
			double height = path2D.Height;
			GetGeomentryPath(path, path2D.PathElements, width, height, bounds);
		}
		return path;
	}

	private static void GetGeomentryPath(GraphicsPath path, List<double> pathElements, double pathWidth, double pathHeight, RectangleF bounds)
	{
		PointF pointF = PointF.Empty;
		double num = 0.0;
		int num2;
		for (num2 = 0; num2 < pathElements.Count; num2++)
		{
			switch ((Path2D.Path2DElements)(ushort)pathElements[num2])
			{
			case Path2D.Path2DElements.LineTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF pointF2 = new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds));
				path.AddLine(pointF, pointF2);
				pointF = pointF2;
				break;
			}
			case Path2D.Path2DElements.MoveTo:
				path.CloseFigure();
				num = pathElements[num2 + 1] * 2.0;
				pointF = new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds));
				break;
			case Path2D.Path2DElements.QuadBezTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF[] array2 = new PointF[3]
				{
					pointF,
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 4], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 5], bounds))
				};
				path.AddBeziers(array2);
				pointF = array2[2];
				break;
			}
			case Path2D.Path2DElements.CubicBezTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF[] array = new PointF[4]
				{
					pointF,
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 4], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 5], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 6], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 7], bounds))
				};
				path.AddBeziers(array);
				pointF = array[3];
				break;
			}
			case Path2D.Path2DElements.ArcTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				RectangleF rect = default(RectangleF);
				rect.X = bounds.X;
				rect.Y = bounds.Y;
				rect.Width = EmuToPoint((int)pathElements[num2 + 2]) * 2f;
				rect.Height = EmuToPoint((int)pathElements[num2 + 3]) * 2f;
				float startAngle = (float)pathElements[num2 + 4] / 60000f;
				float sweepAngle = (float)pathElements[num2 + 5] / 60000f;
				path.AddArc(rect, startAngle, sweepAngle);
				pointF = path.PathPoints[path.PathPoints.Length - 1];
				break;
			}
			case Path2D.Path2DElements.Close:
				path.CloseFigure();
				pointF = PointF.Empty;
				num = 0.0;
				break;
			}
			num2 += (int)num + 1;
		}
	}

	private static float EmuToPoint(int emu)
	{
		return (float)Convert.ToDouble((double)emu / 12700.0);
	}

	private static float GetGeomentryPathXValue(double pathWidth, double x, RectangleF bounds)
	{
		if (pathWidth != 0.0)
		{
			double num = x * 100.0 / pathWidth;
			return (float)((double)bounds.Width * num / 100.0) + bounds.X;
		}
		return bounds.X + EmuToPoint((int)x);
	}

	private static float GetGeomentryPathYValue(double pathHeight, double y, RectangleF bounds)
	{
		if (pathHeight != 0.0)
		{
			double num = y * 100.0 / pathHeight;
			return (float)((double)bounds.Height * num / 100.0) + bounds.Y;
		}
		return bounds.Y + EmuToPoint((int)y);
	}

	internal static Matrix GetTransformMatrix(RectangleF bounds, float ang, bool flipV, bool flipH)
	{
		Matrix matrix = new Matrix();
		Matrix target = new Matrix(1f, 0f, 0f, -1f, 0f, 0f);
		Matrix target2 = new Matrix(-1f, 0f, 0f, 1f, 0f, 0f);
		PointF pointF = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
		if (flipV)
		{
			matrix.Multiply(target, MatrixOrder.Append);
			matrix.Translate(0f, pointF.Y * 2f, MatrixOrder.Append);
		}
		if (flipH)
		{
			matrix.Multiply(target2, MatrixOrder.Append);
			matrix.Translate(pointF.X * 2f, 0f, MatrixOrder.Append);
		}
		matrix.RotateAt(ang, pointF, MatrixOrder.Append);
		return matrix;
	}

	private static Color GetDarkerColor(Color color, float correctionfactory)
	{
		return Color.FromArgb((int)((float)(int)color.R / 100f * correctionfactory), (int)((float)(int)color.G / 100f * correctionfactory), (int)((float)(int)color.B / 100f * correctionfactory));
	}

	private Pen CreatePen(ShapeImpl shape, double scaledWidth)
	{
		if (shape == null)
		{
			return null;
		}
		Color textColor = shape.Line.ForeColor;
		if (shape.Line.ForeColor.A == 0)
		{
			textColor = Color.FromArgb(255, textColor.R, textColor.G, textColor.B);
		}
		return new Pen(textColor, (float)(shape.GetBorderThickness() * scaledWidth))
		{
			DashStyle = GetDashStyle(shape.Line)
		};
	}

	private static Color NormalizeColor(Color color)
	{
		if (color.A == 0)
		{
			return Color.FromArgb(255, color.R, color.G, color.B);
		}
		return color;
	}

	internal static Pen CreatePen(ShapeImpl shape, IShapeLineFormat lineFormat, double scaledWidth)
	{
		Pen pen = new Pen(NormalizeColor(shape.GetBorderColor()), (float)(shape.GetBorderThickness() * scaledWidth));
		switch (lineFormat.DashStyle)
		{
		case OfficeShapeDashLineStyle.Dotted:
			pen.DashStyle = DashStyle.Dot;
			break;
		case OfficeShapeDashLineStyle.Dashed:
		case OfficeShapeDashLineStyle.Medium_Dashed:
			pen.DashStyle = DashStyle.Dash;
			break;
		case OfficeShapeDashLineStyle.Dotted_Round:
		case OfficeShapeDashLineStyle.Dash_Dot:
			pen.DashStyle = DashStyle.DashDot;
			break;
		case OfficeShapeDashLineStyle.Dash_Dot_Dot:
			pen.DashStyle = DashStyle.DashDotDot;
			break;
		case OfficeShapeDashLineStyle.Solid:
			pen.DashStyle = DashStyle.Solid;
			break;
		case OfficeShapeDashLineStyle.Medium_Dash_Dot:
			pen.DashPattern = new float[2] { 1f, 0.5f };
			break;
		}
		switch (lineFormat.Style)
		{
		case OfficeShapeLineStyle.Line_Thin_Thin:
			pen.CompoundArray = new float[4]
			{
				0f,
				0.3333333f,
				2f / 3f,
				1f
			};
			break;
		case OfficeShapeLineStyle.Line_Thin_Thick:
			pen.CompoundArray = new float[4] { 0f, 0.16666f, 0.3f, 1f };
			break;
		case OfficeShapeLineStyle.Line_Thick_Thin:
			pen.CompoundArray = new float[4] { 0f, 0.6f, 0.73333f, 1f };
			break;
		case OfficeShapeLineStyle.Line_Thick_Between_Thin:
			pen.CompoundArray = new float[6]
			{
				0f,
				0.1666667f,
				0.3333333f,
				2f / 3f,
				5f / 6f,
				1f
			};
			break;
		}
		return pen;
	}

	private DashStyle GetDashStyle(IShapeLineFormat lineFormat)
	{
		DashStyle result = DashStyle.Solid;
		switch (lineFormat.DashStyle)
		{
		case OfficeShapeDashLineStyle.Dash_Dot:
		case OfficeShapeDashLineStyle.Medium_Dash_Dot:
			result = DashStyle.DashDot;
			break;
		case OfficeShapeDashLineStyle.Dotted:
		case OfficeShapeDashLineStyle.Dotted_Round:
			result = DashStyle.Dot;
			break;
		case OfficeShapeDashLineStyle.Dash_Dot_Dot:
			result = DashStyle.DashDotDot;
			break;
		case OfficeShapeDashLineStyle.Dashed:
		case OfficeShapeDashLineStyle.Medium_Dashed:
			result = DashStyle.Dash;
			break;
		case OfficeShapeDashLineStyle.Solid:
			result = DashStyle.Solid;
			break;
		}
		return result;
	}
}
