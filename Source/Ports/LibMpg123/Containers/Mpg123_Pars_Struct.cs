/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Mpg123_Pars_Struct
	{
		/// <summary>
		/// Verbose level
		/// </summary>
		public c_int Verbose;
		/// <summary>
		/// Combination of above
		/// </summary>
		public Mpg123_Param_Flags Flags;
		/// <summary></summary>
		public c_long Force_Rate;
		/// <summary></summary>
		public c_int Down_Sample;
		/// <summary>
		/// (which) rva to do: 0: nothing, 1: radio/mix/track 2: album/audiophile
		/// </summary>
		public Mpg123_Param_Rva Rva;
		/// <summary></summary>
		public c_long HalfSpeed;
		/// <summary></summary>
		public c_long DoubleSpeed;
		/// <summary></summary>
		public c_long Timeout;
		/// <summary></summary>
		public readonly c_char[,,] Audio_Caps = new c_char[Constant.Num_Channels, Constant.Mpg123_Rates + 1, Constant.Mpg123_Encodings];
		/// <summary></summary>
		public c_long Icy_Interval;
		/// <summary></summary>
		public c_double OutScale;
		/// <summary></summary>
		public c_long Resync_Limit;
		/// <summary>
		/// Long, because: negative values have a meaning
		/// </summary>
		public c_long Index_Size;
		/// <summary></summary>
		public c_long PreFrames;
		/// <summary></summary>
		public c_long FeedPool;
		/// <summary></summary>
		public c_long FeedBuffer;
		/// <summary></summary>
		public c_long FreeFormat_FrameSize;

		/********************************************************************/
		/// <summary>
		/// Copy the current object into the given
		/// </summary>
		/********************************************************************/
		public void Copy(Mpg123_Pars destination)
		{
			destination.Verbose = Verbose;
			destination.Flags = Flags;
			destination.Force_Rate = Force_Rate;
			destination.Down_Sample = Down_Sample;
			destination.Rva = Rva;
			destination.HalfSpeed = HalfSpeed;
			destination.DoubleSpeed = DoubleSpeed;
			destination.Timeout = Timeout;
			destination.Icy_Interval = Icy_Interval;
			destination.OutScale = OutScale;
			destination.Resync_Limit = Resync_Limit;
			destination.Index_Size = Index_Size;
			destination.PreFrames = PreFrames;
			destination.FeedPool = FeedPool;
			destination.FeedBuffer = FeedBuffer;
			destination.FreeFormat_FrameSize = FreeFormat_FrameSize;

			Array.Copy(Audio_Caps, destination.Audio_Caps, Audio_Caps.Length);
		}
	}
}
