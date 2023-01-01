/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod
{
	/// <summary>
	/// Helper methods to allocate and free different structures
	/// </summary>
	public static class MLoader
	{
		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold the positions
		/// </summary>
		/********************************************************************/
		public static bool AllocPositions(Module of, int total)
		{
			if (total == 0)
				return false;

			of.Positions = new ushort[total];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold the patterns
		/// </summary>
		/********************************************************************/
		public static bool AllocPatterns(Module of)
		{
			if ((of.NumPat == 0) || (of.NumChn == 0))
				return false;

			// Allocate track sequencing array
			of.Patterns = new ushort[(of.NumPat + 1) * of.NumChn];
			of.PattRows = new ushort[of.NumPat + 1];

			// Initialize the patterns with default values
			ushort tracks = 0;

			for (int t = 0; t < of.NumPat; t++)
			{
				of.PattRows[t] = 64;

				for (int s = 0; s < of.NumChn; s++)
					of.Patterns[(t * of.NumChn) + s] = tracks++;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold the tracks
		/// </summary>
		/********************************************************************/
		public static bool AllocTracks(Module of)
		{
			if (of.NumTrk == 0)
				return false;

			of.Tracks = new byte[of.NumTrk][];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold the instrument info
		/// </summary>
		/********************************************************************/
		public static bool AllocInstruments(Module of)
		{
			if (of.NumIns == 0)
				return false;

			of.Instruments = new Instrument[of.NumIns];

			for (int t = 0; t < of.NumIns; t++)
			{
				Instrument inst = new Instrument();

				for (int n = 0; n < SharedConstant.InstNotes; n++)
				{
					// Init note / sample lookup table
					inst.SampleNote[n] = (byte)n;
					inst.SampleNumber[n] = (byte)t;
				}

				inst.GlobVol = 64;

				of.Instruments[t] = inst;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold the sample info
		/// </summary>
		/********************************************************************/
		public static bool AllocSamples(Module of)
		{
			if (of.NumSmp == 0)
				return false;

			of.Samples = new Sample[of.NumSmp];

			for (short u = 0; u < of.NumSmp; u++)
			{
				Sample samp = new Sample();

				// Initialize the structure
				samp.SampleNumber = u;

				samp.Panning = 128;				// Center
				samp.Handle = null;
				samp.GlobVol = 64;
				samp.Volume = 64;

				of.Samples[u] = samp;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all memory allocated in the given module
		/// </summary>
		/********************************************************************/
		public static void FreeAll(Module of)
		{
			// Free the position structure
			of.Positions = null;

			// Free the patterns
			of.Patterns = null;
			of.PattRows = null;

			// Free the tracks
			of.Tracks = null;

			// Free the instruments
			of.Instruments = null;

			// Free the sample structures
			of.Samples = null;
		}
	}
}
