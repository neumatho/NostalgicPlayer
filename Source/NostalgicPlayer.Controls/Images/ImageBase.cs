/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.IO;
using System.Reflection;
using SkiaSharp;
using Svg.Skia;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Base class to all image classes with helper methods
	/// </summary>
	internal abstract class ImageBase
	{
		/********************************************************************/
		/// <summary>
		/// Return a bitmap
		/// </summary>
		/********************************************************************/
		protected Bitmap GetBitmap(string category, string resourceName)
		{
			using (Stream stream = GetResourceStream(category, resourceName))
			{
				return new Bitmap(stream);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load an SVG resource and convert it to bitmap
		/// </summary>
		/********************************************************************/
		protected Bitmap GetSvgBitmap(string category, string resourceName, Color color, int width, int height)
		{
			using (Stream stream = GetResourceStream(category, $"{resourceName}.svg"))
			{
				return ConvertToBitmap(stream, color, width, height);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream to a resource
		/// </summary>
		/********************************************************************/
		private Stream GetResourceStream(string category, string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(ImageBase).Namespace}.Resources.{category}.{resourceName}");
		}



		/********************************************************************/
		/// <summary>
		/// Convert an SVG to bitmap
		/// </summary>
		/********************************************************************/
		private Bitmap ConvertToBitmap(Stream svgStream, Color color, int width, int height)
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
