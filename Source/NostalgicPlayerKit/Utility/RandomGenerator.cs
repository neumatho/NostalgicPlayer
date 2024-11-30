/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Security.Cryptography;
using System.Threading;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// PCG random number generator
	///
	/// Based on code by Melissa O'Neill. Converted to C# by Thomas Neumann
	///
	/// Copyright 2014 Melissa O'Neill [oneill@pcg-random.org]
	///
	/// Licensed under the Apache License, Version 2.0 (the "License");
	/// you may not use this file except in compliance with the License.
	/// You may obtain a copy of the License at
	///
	///     http://www.apache.org/licenses/LICENSE-2.0
	///
	/// Unless required by applicable law or agreed to in writing, software
	/// distributed under the License is distributed on an "AS IS" BASIS,
	/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	/// See the License for the specific language governing permissions and
	/// limitations under the License.
	///
	/// For additional information about the PCG random number generation scheme,
	/// including its license and other licensing options, visit
	///
	///      http://www.pcg-random.org
	/// </summary>
	public static class RandomGenerator
	{
		private static readonly Lock lockObject;

		private static ulong state;			// RNG state. All values are possible
		private static readonly ulong inc;	// Controls which RNG sequence (stream) is selected. Must *always* be odd

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static RandomGenerator()
		{
			lockObject = new Lock();

			byte[] number = RandomNumberGenerator.GetBytes(8);
			ulong initState = BitConverter.ToUInt64(number, 0);

			number = RandomNumberGenerator.GetBytes(8);
			ulong initSequence = BitConverter.ToUInt64(number, 0);

			state = 0;
			inc = (initSequence << 1) | 1;
			GetRandomNumber();
			state += initState;
			GetRandomNumber();
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between 0 and int.MaxValue
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber()
		{
			lock (lockObject)
			{
				ulong oldState = state;
				state = oldState * 6364136223846793005 + inc;

				uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
				uint rot = (uint)(oldState >> 59);

				return Math.Abs((int)((xorShifted >> (int)rot) | (xorShifted << ((-(int)rot) & 31))));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between 0 and maxValue (exclusive)
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber(int maxValue)
		{
			return GetRandomNumber() % maxValue;
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between minValue (inclusive) and
		/// maxValue (exclusive)
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber(int minValue, int maxValue)
		{
			return GetRandomNumber(maxValue - minValue) + minValue;
		}
	}
}
