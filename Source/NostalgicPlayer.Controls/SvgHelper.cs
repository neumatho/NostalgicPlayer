/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.IO;
using SkiaSharp;
using Svg.Skia;

namespace Polycode.NostalgicPlayer.Controls
{
	/// <summary>
	/// Helper class to handle SVGs
	/// </summary>
	public static class SvgHelper
	{
		/********************************************************************/
		/// <summary>
		/// Convert an SVG to bitmap
		/// </summary>
		/********************************************************************/
		public static Bitmap ConvertToBitmap(Stream svgStream, Color color, int width, int height)
		{
			using (SKSvg svg = new SKSvg())
			{
				svg.Load(svgStream);

				SKRect rect = svg.Picture.CullRect;
				float scaleX = width / rect.Width;
				float scaleY = height / rect.Height;

				using (SKBitmap bitmap = new SKBitmap(width, height))
				{
					using (SKCanvas canvas = new SKCanvas(bitmap))
					{
						canvas.Clear(SKColors.Transparent);
						canvas.Scale(scaleX, scaleY);

						using (SKPaint paint = new SKPaint())
						{
							paint.ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(color.R, color.G, color.B, color.A), SKBlendMode.SrcIn);

							canvas.DrawPicture(svg.Picture, paint);
							canvas.Flush();
						}

						using (SKImage image = SKImage.FromBitmap(bitmap))
						{
							using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
							{
								using (MemoryStream ms = new MemoryStream(data.ToArray()))
								{
									return new Bitmap(ms);
								}
							}
						}
					}
				}
			}
		}
	}
}
