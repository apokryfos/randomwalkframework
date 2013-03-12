using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNG {
	/// <summary>
	/// Provides a centralized RNG to be used for all active walks.	
	/// </summary>
	public static class RNGProvider {
		//public static Random r = new ThreadSafeRandom(new NPack.MersenneTwister(new int[] { 123, 456, 789 }));		
		public static Random r = new ThreadSafeRandom(new NPack.MersenneTwister());		
	}
}
