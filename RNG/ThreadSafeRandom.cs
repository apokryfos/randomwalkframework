using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNG {
	
	public sealed class ThreadSafeRandom : Random {
		private object SyncRoot = new object();
		private Random r;

		public ThreadSafeRandom() : base() { r = this; }
		public ThreadSafeRandom(int seed) : base(seed) { r = this; }
		public ThreadSafeRandom(Random enclosed) { r = enclosed; }

		
		public override int Next() {
			lock (SyncRoot) {
				return r.Next();
			}
		}
		public override int Next(int maxValue) {
			lock (SyncRoot) {
				return r.Next(maxValue);
			}
		}
		public override int Next(int minValue, int maxValue) {
			lock (SyncRoot) {
				return r.Next(minValue, maxValue);
			}
		}
		public override void NextBytes(byte[] buffer) {
			lock (SyncRoot) {
				r.NextBytes(buffer);
			}
		}
		public override double NextDouble() {
			lock (SyncRoot) {
				return r.NextDouble();
			}
		}
	}
}
