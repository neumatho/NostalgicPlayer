﻿/******************************************************************************/
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
		/// When a tone portamento effect is performed, we expect it to
		/// always reset the envelope if the previous envelope already ended.
		/// If it still didn't end, reset envelope in XM (and IT compatible
		/// GXX mode) but not in standard IT mode
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_Portamento_Envelope_Reset()
		{
			Compare_Mixer_Data(dataDirectory, "Xm_Portamento_Envelope_Reset.xm", "Xm_Portamento_Envelope_Reset.data");
		}
	}
}
