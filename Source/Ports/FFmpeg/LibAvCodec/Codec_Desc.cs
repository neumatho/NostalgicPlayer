/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Codec_Desc
	{
		private static readonly AvCodecDescriptor[] codec_Descriptors =
		[
			new AvCodecDescriptor
			{
				Id = AvCodecId.WmaV1,
				Type = AvMediaType.Audio,
				Name = "wmav1".ToCharPointer(),
				Long_Name = "Windows Media Audio 1".ToCharPointer(),
				Props = AvCodecProp.Intra_Only | AvCodecProp.Lossy
			},
			new AvCodecDescriptor
			{
				Id = AvCodecId.WmaV2,
				Type = AvMediaType.Audio,
				Name = "wmav2".ToCharPointer(),
				Long_Name = "Windows Media Audio 2".ToCharPointer(),
				Props = AvCodecProp.Intra_Only | AvCodecProp.Lossy
			}
		];

		/********************************************************************/
		/// <summary>
		/// Return descriptor for given codec ID or NULL if no descriptor
		/// exists
		/// </summary>
		/********************************************************************/
		public static AvCodecDescriptor AvCodec_Descriptor_Get(AvCodecId id)//XX 3878
		{
			CPointer<AvCodecDescriptor> ret = CArray.bsearch<AvCodecDescriptor, AvCodecId>(id, codec_Descriptors, Macros.FF_Array_Elems(codec_Descriptors), Descriptor_Compare);

			if (ret.IsNull)
				return null;

			return ret[0];
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Descriptor_Compare(AvCodecId key, AvCodecDescriptor member)//XX 3870
		{
			AvCodecId id = key;
			AvCodecDescriptor desc = member;

			return id - desc.Id;
		}
		#endregion
	}
}
