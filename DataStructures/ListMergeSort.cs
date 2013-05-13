using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {
	
	
	public static class ListMergeSortExtensions {
		
		public static void MergeSort<T>(this List<T> l) {
			l.MergeSort(Comparer<T>.Default.Compare);
		}

		public static void MergeSort<T>(this List<T> l, IComparer<T> cmp) {
			l.MergeSort(cmp.Compare);
		}

		public static void MergeSort<T>(this List<T> l, Comparison<T> cmp) {
			if (l.Count == 1) {
				return;  
			}

			int middle = (l.Count / 2);
			List<T> left = l.GetRange(0, middle);
			List<T> right = l.GetRange(middle, l.Count - left.Count);
			


			left.MergeSort(cmp);
			right.MergeSort(cmp);

			int i =0, j = 0;
			
			for (int k = 0;k < l.Count;k++) {
				int cmpres = 0;
				if (i < left.Count && j < right.Count) {
					cmpres = cmp(left[i], right[j]);
				}
				if (cmpres > 0 || i >= left.Count) {
					l[k] = right[j++];
				} else if (cmpres < 0 || j >= right.Count || cmpres == 0) {
					l[k] = left[i++];
				} 
			}
		}

	}
}
