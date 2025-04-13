/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Synth
{
	/// <summary>
	/// 
	/// </summary>
	internal class SynthSound : Sample
	{
		// Common commands (in both vol table and waveform table)
		public const byte CmdSpd = 0xf0;
		public const byte CmdWai = 0xf1;
		public const byte CmdChd = 0xf2;
		public const byte CmdChu = 0xf3;
		public const byte CmdRes = 0xf6;
		public const byte CmdHlt = 0xfb;
		public const byte CmdJmp = 0xfe;
		public const byte CmdEnd = 0xff;

		// Volume commands (in vol table)
		public const byte VolCmdEn1 = 0xf4;
		public const byte VolCmdEn2 = 0xf5;
		public const byte VolCmdJws = 0xfa;

		// Waveform commands (in waveform table)
		public const byte WfCmdVbd = 0xf4;
		public const byte WfCmdVbs = 0xf5;
		public const byte WfCmdVwf = 0xf7;
		public const byte WfCmdJvs = 0xfa;
		public const byte WfCmdArp = 0xfc;
		public const byte WfCmdAre = 0xfd;

		private uint volTableLen;
		private uint wfTableLen;
		private uint volSpeed;
		private uint wfSpeed;
		private readonly byte[] volTable = new byte[128];
		private readonly byte[] wfTable = new byte[128];

		private readonly List<SynthWf> list = new List<SynthWf>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SynthSound(OctaMedWorker worker) : base(worker)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetVolSpeed(uint spd)
		{
			volSpeed = spd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetWfSpeed(uint spd)
		{
			wfSpeed = spd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetVolTableLen(uint tbl)
		{
			volTableLen = tbl;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetWfTableLen(uint tbl)
		{
			wfTableLen = tbl;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetVolData(uint pos, byte data)
		{
			volTable[pos] = data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetWfData(uint pos, byte data)
		{
			wfTable[pos] = data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint GetVolSpeed()
		{
			return volSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint GetWfSpeed()
		{
			return wfSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte GetVolData(uint pos)
		{
			return volTable[pos & 0x7f];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte GetWfData(uint pos)
		{
			return wfTable[pos & 0x7f];
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IsSynthSound()
		{
			return true;
		}
		#endregion

		#region List implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Add(SynthWf item)
		{
			list.Add(item);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Count => list.Count;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public SynthWf this[uint index]
		{
			get
			{
				return list[(int)index];
			}
		}
		#endregion
	}
}
