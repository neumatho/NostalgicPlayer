/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Sane_Random_Device : IEngine_Traits<c_uint>
	{
		private readonly Lock m = new Lock();
		private readonly StdString token = new StdString();
		private MptMt19937 rd_Fallback;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sane_Random_Device()
		{
			Init_Fallback();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Result_Bits => sizeof(c_uint) * 8;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_uint min()
		{
			return c_uint.MinValue;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_uint max()
		{
			return c_uint.MaxValue;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_uint Invoke()
		{
			lock (m)
			{
				c_uint result = 0;

				result ^= Random.Random_<c_uint, uint32_t>(rd_Fallback);

				return result;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Init_Fallback()
		{
			if (rd_Fallback == null)
			{
				if (token.length() > 0)
				{
					uint64 seed_Val = new Prng_Random_Device_Time_Seeder().Generate_Seed<uint64>();

					vector<c_uint> seeds = new vector<c_uint>();
					seeds.push_back((uint32)(seed_Val >> 32));
					seeds.push_back((uint32)(seed_Val >> 0));

					for (size_t i = 0; i < token.length(); ++i)
						seeds.push_back((c_uchar)token[i]);

					Seed_Seq seed = new Seed_Seq(seeds.begin(), seeds.end());
					rd_Fallback = (MptMt19937)MptMt19937.Create(seed);
				}
				else
				{
					uint64 seed_Val = new Prng_Random_Device_Time_Seeder().Generate_Seed<uint64>();

					CPointer<c_uint> seeds = new CPointer<c_uint>(2);
					seeds[0] = (uint32)(seed_Val >> 32);
					seeds[1] = (uint32)(seed_Val >> 0);

					Seed_Seq seed = new Seed_Seq(seeds + 0, seeds + 2);
					rd_Fallback = (MptMt19937)MptMt19937.Create(seed);
				}
			}
		}
		#endregion
	}
}
