using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {

	public interface IRadixFunction<T,TRadix> {
		TRadix RadixCode(T item, int radix);		
	}

	


	public static class ListRadixSortExtension {

		
		private static SortedDictionary<TBucket, List<T>> BucketSortInternal<T, TBucket>(this List<T> l, Func<T, TBucket> BucketCode) {

			SortedDictionary<TBucket, List<T>> bucket = new SortedDictionary<TBucket, List<T>>();
			List<T> b;
			foreach (var t in l) {
				var code = BucketCode(t);
				if (!bucket.TryGetValue(code, out b)) {
					b = new List<T>();
					bucket.Add(code, b);
				}
				b.Add(t);
			}
			return bucket;
		}

		private static SortedDictionary<TRadix, List<T>> BucketSortInternal<T, TRadix>(this List<T> l, Func<T, int, TRadix> BucketCode, int BucketIndex) {

			SortedDictionary<TRadix, List<T>> bucket = new SortedDictionary<TRadix, List<T>>();
			List<T> b;
			foreach (var t in l) {
				var code = BucketCode(t, BucketIndex);
				if (!bucket.TryGetValue(code, out b)) {
					b = new List<T>();
					bucket.Add(code, b);
				}
				b.Add(t);
			}
			return bucket;
		}

		public static void RadixSort<T, TRadix>(this List<T> l, Func<T, int, TRadix> RadixCode, int NumRadixes) {
	
			if (NumRadixes == 0) {
				return;
			}			
			var bucket = l.BucketSortInternal(RadixCode, 0);			
			if (NumRadixes > 1) {			
				foreach (var kv in bucket) {
					kv.Value.RadixSort((t, i) => RadixCode(t, i + 1), NumRadixes - 1);
				}
			}

			l.MergeFromBuckets(bucket);

		}

		private static void MergeFromBuckets<T, TBucket>(this List<T> l, SortedDictionary<TBucket, List<T>> bucket) {
			
			int j = 0;
			foreach (var kv in bucket) {				
				foreach (var t in kv.Value) {
					l[j++] = t;
				}
			}
		}

		public static void BucketSort<T, TBucket>(this List<T> l, Func<T, TBucket> BucketCode) {
			SortedDictionary<TBucket, List<T>> bucket = l.BucketSortInternal(BucketCode);
			l.MergeFromBuckets(bucket);
		}


		
	}

}
