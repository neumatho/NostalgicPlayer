/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// MIDI event handling, event lists, ...
	/// </summary>
	internal static class MidiEvents
	{
		/********************************************************************/
		/// <summary>
		/// Get the length of a MIDI event in bytes
		/// </summary>
		/********************************************************************/
		public static uint8 GetEventLength(uint8 firstByte)//XX 104
		{
			uint8 msgSize = 3;

			switch (firstByte & 0xf0)
			{
				case 0xc0:
				case 0xd0:
				{
					msgSize = 2;
					break;
				}

				case 0xf0:
				{
					switch (firstByte)
					{
						case 0xf1:
						case 0xf3:
						{
							msgSize = 2;
							break;
						}

						case 0xf2:
						{
							msgSize = 3;
							break;
						}

						default:
						{
							msgSize = 1;
							break;
						}
					}

					break;
				}
			}

			return msgSize;
		}
	}
}
