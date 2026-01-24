/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	internal class Far_Module_Extra : IModuleNewChannelExtras
	{
		public class Far_Module_Extra_Info : IModuleExtraInfo
		{
			public c_int Coarse_Tempo;
			public c_int Fine_Tempo;
			public c_int Tempo_Mode;
			public c_int Vib_Depth;			// Vibrato depth for all channels
		}

		// The time factor needed to directly use FAR tempos is a little unintuitive.
		//
		// Generally: FAR tries to run 32/[coarse tempo] rows per second, which
		// (usually, but not always) are subdivided into 4 "ticks". To achieve
		// this, it measures tempos in the number of ticks that should play per second
		// (see far_tempos below). Fine tempo is added or subtracted from this number.
		// To time these ticks, FAR uses the programmable interval timer (PIT) to run a
		// player interrupt.
		//
		// libxmp effectively uses a calculation of 10.0 * 0.25 / BPM to get the tick
		// duration in seconds. A base time factor of 4.0 makes this 1 / BPM, turning
		// BPM into the ticks/sec measure that FAR uses. This isn't completely
		// accurate to FAR, though.
		//
		// The x86 PIT runs at a rate of 1193182 Hz, but FAR does something strange
		// when calculating PIT divisors and uses a constant of 1197255 Hz instead.
		// This means FAR tempo is slightly slower by a factor of around:
		//
		// floor(1197255 / 32) / floor(1193182 / 32) ~= 1.003439
		//
		// This still isn't perfect, but it gets the playback rate fairly close
		public static readonly c_int[] Far_Tempos =
		[
			256, 128, 64, 42, 32, 25, 21, 18, 16, 14, 12, 11, 10, 9, 9, 8
		];

		public const c_int Far_Old_Tempo_Shift = 2;		// Power of multiplier for old tempo mode
		public const c_int Far_Gus_Channels = 17;

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		private Module_Data m;
		private Far_Module_Extra_Info extraInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Far_Module_Extra(LibXmp libXmp, Xmp_Context ctx, Module_Data m)
		{
			lib = libXmp;
			this.ctx = ctx;

			this.m = m;
			extraInfo = new Far_Module_Extra_Info();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IModuleExtraInfo Module_Extras => extraInfo;



		/********************************************************************/
		/// <summary>
		/// FAR tempo has some unusual requirements that don't really match
		/// any other format:
		/// 
		/// 1) The coarse tempo is roughly equivalent to speed, but a value
		///    of 0 is supported, and FAR doesn't actually have a concept of
		///    ticks: it translates this value to tempo.
		/// 
		/// 2) There is some very bizarre clamping behavior involving fine
		///    tempo slides that needs to be emulated.
		/// 
		/// 3) Tempos can range from 1 to 356(!). FAR uses a fixed row
		///    subdivision size of 16, so just shift the tempo by 4 and hope
		///    libxmp doesn't change it.
		/// 
		/// 4) There are two tempo modes, and they can be switched between
		///    arbitrarily...
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Far_Translate_Tempo(c_int mode, c_int fine_Change, c_int coarse, ref c_int fine, ref c_int _speed, ref c_int _bpm)
		{
			if ((coarse < 0) || (coarse > 15) || (mode < 0) || (mode > 1))
				return -1;

			c_int speed, bpm;

			// Compatibility for FAR's broken fine tempo "clamping"
			if ((fine_Change < 0) && ((Far_Tempos[coarse] + fine) <= 0))
				fine = 0;
			else if ((fine_Change > 0) && ((Far_Tempos[coarse] + fine) >= 100))
				fine = 100;

			if (mode == 1)
			{
				// "New" FAR tempo
				// Note that negative values are possible in Farandole Composer
				// via changing fine tempo and then slowing coarse tempo.
				// These result in very slow final tempos due to signed to
				// unsigned conversion. Zero should just be ignored entirely
				c_int tempo = Far_Tempos[coarse] + fine;
				if (tempo == 0)
					return -1;

				uint32 divisor = (uint32)(1197255 / tempo);

				// Coincidentally(?), the "new" FAR tempo algorithm actually
				// prevents the BPM from dropping too far under XMP_MIN_BPM,
				// which is what libxmp needs anyway
				speed = 0;

				while (divisor > 0xffff)
				{
					divisor >>= 1;
					tempo <<= 1;
					speed++;
				}

				if (speed >= 2)
					speed++;

				speed += 3;

				// Add an extra tick because the FAR replayer checks the tick
				// remaining count before decrementing it but after handling
				// each tick, i.e. a count of "3" executes 4 ticks
				speed++;
				bpm = tempo;
			}
			else
			{
				// "Old" FAR tempo
				// This runs into the XMP_MIN_BPM limit, but nothing uses it anyway.
				// Old tempo mode in the original FAR replayer has 32 ticks,
				// but ignores all except every 8th
				speed = 4 << Far_Old_Tempo_Shift;
				bpm = (Far_Tempos[coarse] + fine * 2) << Far_Old_Tempo_Shift;
			}

			if (bpm < Constants.Xmp_Min_Bpm)
				bpm = Constants.Xmp_Min_Bpm;

			_speed = speed;
			_bpm = bpm;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int New_Channel_Extras(Channel_Data xc)
		{
			xc.Extra = new Far_Channel_Extra(lib, ctx, xc);
			if (xc.Extra == null)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int LibXmp_Far_New_Module_Extras(LibXmp libXmp, Xmp_Context ctx, Module_Data m)
		{
			m.Extra = new Far_Module_Extra(libXmp, ctx, m);
			if (m.Extra == null)
				return -1;

			Far_Module_Extra_Info extras = (Far_Module_Extra_Info)m.Extra.Module_Extras;
			extras.Vib_Depth = 4;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Release_Module_Extras()
		{
			m.Extra = null;

			m = null;
			extraInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IModuleExtra MakeDeepClone()
		{
			Far_Module_Extra clone = (Far_Module_Extra)MemberwiseClone();

			clone.extraInfo = new Far_Module_Extra_Info
			{
				Coarse_Tempo = extraInfo.Coarse_Tempo,
				Fine_Tempo = extraInfo.Fine_Tempo,
				Tempo_Mode = extraInfo.Tempo_Mode,
				Vib_Depth = extraInfo.Vib_Depth
			};

			clone.m.Extra = clone;

			return clone;
		}
	}
}
