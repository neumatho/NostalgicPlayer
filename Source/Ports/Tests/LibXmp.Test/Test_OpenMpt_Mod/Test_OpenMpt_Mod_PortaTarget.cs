﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Mod
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Mod
	{
		/********************************************************************/
		/// <summary>
		/// ProTracker’s portamento behaviour is somewhere between FT2 and
		/// IT:
		///
		/// - A new note (with no portamento command next to it) does not
		///   reset the portamento target. That is, if a previous portamento
		///   has not finished yet, calling 3xx or 5xx after the new note
		///   will slide it towards the old target.
		/// - Once the portamento target period is reached, the target is
		///   reset. This means that if the period is modified by another
		///   slide (e.g. 1xx or 2xx), a following 3xx will not slide back to
		///   the original target
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PortaTarget()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PortaTarget.mod", "PortaTarget.data");
		}
	}
}
