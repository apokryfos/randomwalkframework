using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {

	public class TriangleEqualityComparer : IEqualityComparer<Triangle> {
		#region IEqualityComparer<Triangle> Members

		public bool Equals(Triangle x, Triangle y) {
			return x.Equals(y);
		}

		public int GetHashCode(Triangle obj) {
			return (((long)obj.A + (long)obj.B + (long)obj.C) / 3).GetHashCode();
		}

		#endregion
	}

	public class Triangle : TriangleGeneric<int> {
		public Triangle(int a, int b, int c)
			: base(a, b, c) { }

		public byte[] ToBinary() {
			byte[] bin = new byte[3 * sizeof(int)];
			Buffer.BlockCopy(BitConverter.GetBytes(A), 0, bin, 0, sizeof(int));
			Buffer.BlockCopy(BitConverter.GetBytes(B), 0, bin, sizeof(int), sizeof(int));
			Buffer.BlockCopy(BitConverter.GetBytes(C), 0, bin, 2 * sizeof(int), sizeof(int));
			return bin;
		}

	}

	public class TriangleGeneric<TVertex> : IEquatable<TriangleGeneric<TVertex>> {
		private TVertex a, b, c;

		private TriangleGeneric() { }
		public TriangleGeneric(TVertex a, TVertex b, TVertex c) {
			this.a = a;
			this.b = b;
			this.c = c;
		}


		public TVertex A { get { return a; } }
		public TVertex B { get { return b; } }
		public TVertex C { get { return c; } }

		public TVertex Vertex(int index) {
			index = index % 3;
			if (index == 0)
				return A;
			else if (index == 1)
				return B;
			else
				return C;
		}

		public bool HasVertex(TVertex vertex) {
			return (A.Equals(vertex) || B.Equals(vertex) || C.Equals(vertex));
		}


		#region IEquatable<TriangleGeneric<TVertex>> Members

		public bool Equals(TriangleGeneric<TVertex> other) {
			return (other.HasVertex(A) && other.HasVertex(B) && other.HasVertex(C));
		}

		public bool IsValid {
			get { return (!A.Equals(B) && !B.Equals(C) && !C.Equals(A)); }
		}

		public override string ToString() {
			return A + "<->" + B + "<->" + C;
		}

		public override int GetHashCode() {
			return (int)(((long)A.GetHashCode() + (long)B.GetHashCode() + (long)C.GetHashCode()) / 3L);
		}



		#endregion

	}
}
