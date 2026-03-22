/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// Tests some strange properties of FT2's tremor effect.
		/// Left=right, relies on instrument fadeout being 0 for both
		/// channels to be equivalent.
		///
		/// 00-03: noins + volume overwrites the last tremor volume until
		///        tremor updates again.
		/// 04-07: keyoff + noins + volume does the same thing.
		/// 08-0B: instrument without keyoff resets the tremor state.
		/// 0C-0F: instrument with keyoff does not reset the tremor state,
		///        but does overwrite the tremor volume until it updates
		///        again.
		///
		/// 12-15: noins + volume + K00 behaves identically to noins + volume
		///        + keyoff.
		///        (Tremor continues one row later due to using K00.)
		/// 16-19: ins# + volume + K00 behaves identically to ins# + volume +
		///        keyoff.
		///        (Tremor continues one row later due to using K00.)
		///
		/// 1C-1F: noins + Cxx behaves identically to noins + volume.
		///        (Tremor continues one row later due to using Cxx.)
		/// 20-23: noins + Cxx + keyoff behaves identically to noins + volume
		///        + keyoff.
		///        (Tremor continues one row later due to using Cxx.)
		/// 24-27: ins# + Cxx behaves identically to ins# + volume.
		///        (Tremor continues one row later due to using Cxx.)
		/// 28-2B: ins# + Cxx + keyoff behaves identically to ins# + volume +
		///        keyoff.
		///        (Tremor continues one row later due to using Cxx.)
		///
		/// 2E-31: volume + delay resets the tremor state.
		///        (Tremor continues one row later due to using EDx.)
		/// 32-35: volume + delay + Keyoff resets the tremor state.
		///        (Tremor continues one row later due to using EDx.)
		/// 36-39: ins# + delay resets the tremor state.
		///        (Tremor continues one row later due to using EDx.)
		/// 3A-3D: ins# + delay + keyoff resets the tremor state.
		///        (Tremor continues one row later due to using EDx.)
		/// 3E-43: keyoff + noins + novc + delay resets the tremor state.
		///        (Tremor continues one row later due to using EDx.)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Tremor_Reset()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Tremor_Reset.xm", "Ft2_Tremor_Reset.data");
		}
	}
}
