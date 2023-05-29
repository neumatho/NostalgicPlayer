/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mpg123_Pars_Struct
	{
		public c_int Verbose;			// Verbose level
		public Mpg123_Param_Flags Flags;// Combination of above
		public c_long Force_Rate;
		public c_int Down_Sample;
		public c_int Rva;				// (which) rva to do: 0: nothing, 1: radio/mix/track 2: album/audiophile
		public c_long HalfSpeed;
		public c_long DoubleSpeed;
		public c_long Timeout;
		public readonly c_char[,,] Audio_Caps = new c_char[Constant.Num_Channels, Constant.Mpg123_Rates + 1, Constant.Mpg123_Encodings];
		public c_long Icy_Interval;
		public c_double OutScale;
		public c_long Resync_Limit;
		public c_long Index_Size;		// Long, because: negative values have a meaning
		public c_long PreFrames;
		public c_long FeedPool;
		public c_long FeedBuffer;
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
