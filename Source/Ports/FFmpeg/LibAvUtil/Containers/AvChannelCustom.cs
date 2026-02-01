/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// An AVChannelCustom defines a single channel within a custom order layout
	///
	/// Unlike most structures in FFmpeg, sizeof(AVChannelCustom) is a part of the
	/// public ABI.
	///
	/// No new fields may be added to it without a major version bump
	/// </summary>
	public class AvChannelCustom : IDeepCloneable<AvChannelCustom>
	{
		/// <summary>
		/// 
		/// </summary>
		public AvChannel Id;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name = new CPointer<char>(16);

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Opaque;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public AvChannelCustom MakeDeepClone()
		{
			AvChannelCustom clone = new AvChannelCustom
			{
				Id = Id,
				Opaque = Opaque
			};

			CMemory.memcpy(clone.Name, Name, 16);

			return clone;
		}
	}
}
