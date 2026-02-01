/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This struct describes the properties of a single codec described by an
	/// AVCodecID.
	/// See avcodec_descriptor_get()
	/// </summary>
	public class AvCodecDescriptor
	{
		/// <summary>
		/// 
		/// </summary>
		public AvCodecId Id;

		/// <summary>
		/// 
		/// </summary>
		public AvMediaType Type;

		/// <summary>
		/// Name of the codec described by this descriptor. It is non-empty and
		/// unique for each codec descriptor. It should contain alphanumeric
		/// characters and '_' only
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// A more descriptive name for this codec. May be NULL
		/// </summary>
		public CPointer<char> Long_Name;

		/// <summary>
		/// Codec properties, a combination of AV_CODEC_PROP_* flags
		/// </summary>
		public AvCodecProp Props;

		/// <summary>
		/// MIME type(s) associated with the codec.
		/// May be NULL; if not, a NULL-terminated array of MIME types.
		/// The first item is always non-NULL and is the preferred MIME type
		/// </summary>
		public CPointer<CPointer<char>> Mime_Types;

		/// <summary>
		/// If non-NULL, an array of profiles recognized for this codec.
		/// Terminated with AV_PROFILE_UNKNOWN
		/// </summary>
		public CPointer<AvProfile> Profiles;
	}
}
