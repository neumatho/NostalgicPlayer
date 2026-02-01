/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvDisposition
	{
		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// The stream should be chosen by default among other streams of the same type,
		/// unless the user has explicitly specified otherwise
		/// </summary>
		Default = 1 << 0,

		/// <summary>
		/// The stream is not in original language.
		///
		/// Note: AV_DISPOSITION_ORIGINAL is the inverse of this disposition. At most
		///       one of them should be set in properly tagged streams.
		/// Note: This disposition may apply to any stream type, not just audio
		/// </summary>
		Dub = 1 << 1,

		/// <summary>
		/// The stream is in original language
		///
		/// See the notes for AV_DISPOSITION_DUB
		/// </summary>
		Original = 1 << 2,

		/// <summary>
		/// The stream is a commentary track
		/// </summary>
		Comment = 1 << 3,

		/// <summary>
		/// The stream contains song lyrics
		/// </summary>
		Lyrics = 1 << 4,

		/// <summary>
		/// The stream contains karaoke audio
		/// </summary>
		Karaoke = 1 << 5,

		/// <summary>
		/// Track should be used during playback by default.
		/// Useful for subtitle track that should be displayed
		/// even when user did not explicitly ask for subtitles
		/// </summary>
		Forced = 1 << 6,

		/// <summary>
		/// The stream is intended for hearing impaired audiences
		/// </summary>
		Hearing_Impaired = 1 << 7,

		/// <summary>
		/// The stream is intended for visually impaired audiences
		/// </summary>
		Visual_Impaired = 1 << 8,

		/// <summary>
		/// The audio stream contains music and sound effects without voice
		/// </summary>
		Clean_Effects = 1 << 9,

		/// <summary>
		/// The stream is stored in the file as an attached picture/"cover art" (e.g.
		/// APIC frame in ID3v2). The first (usually only) packet associated with it
		/// will be returned among the first few packets read from the file unless
		/// seeking takes place. It can also be accessed at any time in
		/// AVStream.attached_pic
		/// </summary>
		Attached_Pic = 1 << 10,

		/// <summary>
		/// The stream is sparse, and contains thumbnail images, often corresponding
		/// to chapter markers. Only ever used with AV_DISPOSITION_ATTACHED_PIC
		/// </summary>
		Timed_Thumbnails = 1 << 11,

		/// <summary>
		/// The stream is intended to be mixed with a spatial audio track. For example,
		/// it could be used for narration or stereo music, and may remain unchanged by
		/// listener head rotation
		/// </summary>
		Non_Diegetic = 1 << 12,

		/// <summary>
		/// The subtitle stream contains captions, providing a transcription and possibly
		/// a translation of audio. Typically intended for hearing-impaired audiences
		/// </summary>
		Captions = 1 << 16,

		/// <summary>
		/// The subtitle stream contains a textual description of the video content.
		/// Typically intended for visually-impaired audiences or for the cases where the
		/// video cannot be seen
		/// </summary>
		Descriptions = 1 << 17,

		/// <summary>
		/// The subtitle stream contains time-aligned metadata that is not intended to be
		/// directly presented to the user
		/// </summary>
		Metadata = 1 << 18,

		/// <summary>
		/// The stream is intended to be mixed with another stream before presentation.
		/// Used for example to signal the stream contains an image part of a HEIF grid,
		/// or for mix_type=0 in mpegts
		/// </summary>
		Dependent = 1 << 19,

		/// <summary>
		/// The video stream contains still images
		/// </summary>
		Still_Image = 1 << 20,

		/// <summary>
		/// The video stream contains multiple layers, e.g. stereoscopic views (cf. H.264
		/// Annex G/H, or HEVC Annex F)
		/// </summary>
		Multilayer = 1 << 21
	}
}
