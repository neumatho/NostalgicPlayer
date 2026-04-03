/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.IO;
using System.Reflection;

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
		/// Return a stream to a resource
		/// </summary>
		/********************************************************************/
		private Stream GetResourceStream(string category, string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(ImageBase).Namespace}.Resources.{category}.{resourceName}");
		}
	}
}
