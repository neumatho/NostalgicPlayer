/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// MPT IT Instrument Extension
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 554 + 120)]
	internal struct ItInstrumentEx
	{
		public ItInstrument Iti;
		public Array120 KeyboardHi;

		/********************************************************************/
		/// <summary>
		/// Convert an ITInstrumentEx to OpenMPT's internal instrument
		/// representation. Returns size of the instrument data that has
		/// been read
		/// </summary>
		/********************************************************************/
		public uint32 ConvertToMpt(ModInstrument mptIns, ModType fromType)
		{
			uint32 insSize = Iti.ConvertToMpt(mptIns, fromType);

			// Is this actually an extended instrument?
			// Note: OpenMPT 1.20 - 1.22 accidentally wrote "MPTX" here (since revision 1203), while previous versions wrote the reversed version, "XTPM"
			if ((insSize == 0) || ((CMemory.memcmp(Iti.Dummy.ToArray().data(), "MPTX", 4) != 0) && (CMemory.memcmp(Iti.Dummy.ToArray().data(), "XTPM", 4) != 0)))
				return insSize;

			// Olivier's MPT Instrument Extension
			array<uint8> keyboardHi = KeyboardHi.ToArray();

			for (c_int i = 0; i < 120; i++)
				mptIns.Keyboard[i] |= (SampleIndex)(keyboardHi[i] << 8);

			return (uint32)Marshal.SizeOf<ItInstrumentEx>();
		}
	}
}
