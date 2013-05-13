using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {
	public static class ListShuffling {
		
		
		public static void Swap<T>(this List<T> l, int index1, int index2) {
			T tmp = l[index1];
			l[index1] = l[index2];
			l[index2] = tmp;
		}

		public static void FYShuffle<T>(this List<T> l) {
			var r = RNG.RNGProvider.r;

			for (int i = l.Count - 1; i > 0; i--) {
				var j = r.Next(i);
				l.Swap(i, j);
			}

		}

	}
}
