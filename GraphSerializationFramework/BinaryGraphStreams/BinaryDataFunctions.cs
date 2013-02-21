using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;
using QuickGraph.Collections;

namespace GraphSerializationFramework.BinaryGraphStreams {
	public static class BinaryDataFunctions {

		#region Data To Bytes

		public static byte[] MakeOutEdgesList(int src, IEdgeList<int, Edge<int>> OutEdges) {
			byte[] b = new byte[(OutEdges.Count + 2) * sizeof(int)];
			int i = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(src), 0, b, i, sizeof(int));
			i += sizeof(int);
			foreach (var v in OutEdges) {
				Buffer.BlockCopy(BitConverter.GetBytes(v.GetOtherVertex(src)), 0, b, i, sizeof(int));
				i += sizeof(int);
			}
			Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfLine), 0, b, i, sizeof(int));
			return b;
		}

		public static byte[] MakeOutEdgesList(int src, IEnumerable<int> OutEdges, int count) {
			byte[] b = new byte[(count + 2) * sizeof(int)];
			int i = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(src), 0, b, i, sizeof(int));
			i += sizeof(int);
			foreach (var v in OutEdges) {
				Buffer.BlockCopy(BitConverter.GetBytes(v), 0, b, i, sizeof(int));
				i += sizeof(int);
			}
			Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfLine), 0, b, i, sizeof(int));
			return b;
		}

		public static byte[] MakeOutEdgesList(int src, ICollection<int> OutEdges) {
			return MakeOutEdgesList(src, OutEdges, OutEdges.Count);
		}

		public static byte[] MakeOutEdgesList(int src, int OutDegree, IEnumerable<Edge<int>> OutEdges) {
			byte[] b = new byte[(OutDegree + 2) * sizeof(int)];
			int i = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(src), 0, b, i, sizeof(int));
			i += sizeof(int);
			foreach (var v in OutEdges) {
				Buffer.BlockCopy(BitConverter.GetBytes(v.Target), 0, b, i, sizeof(int));
				i += sizeof(int);
			}
			Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfLine), 0, b, i, sizeof(int));
			return b;
		}

		public static byte[] MakeInOutEdgesList(int src, int OutDegree, IEnumerable<Edge<int>> OutEdges, int InDegree, IEnumerable<Edge<int>> InEdges) {
			byte[] b = new byte[(OutDegree + InDegree + 3) * sizeof(int)];
			int i = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(src), 0, b, i, sizeof(int));
			i += sizeof(int);
			foreach (var v in OutEdges) {
				Buffer.BlockCopy(BitConverter.GetBytes(v.Target), 0, b, i, sizeof(int));
				i += sizeof(int);
			}
			Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfDirection), 0, b, i, sizeof(int));
			i += sizeof(int);
			foreach (var v in InEdges) {
				Buffer.BlockCopy(BitConverter.GetBytes(v.Source), 0, b, i, sizeof(int));
				i += sizeof(int);
			}
			Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfLine), 0, b, i, sizeof(int));
			i += sizeof(int);
			return b;
		}
		#endregion

		#region Bytes To Data
		public static int[] ReadInt32Buffer(BinaryReader br, int bufferSize) {
			var bytes = br.ReadBytes(bufferSize * sizeof(int));

			int[] ints = new int[bytes.Length / sizeof(int)];
			if (bytes.Length % sizeof(int) != 0 && br.BaseStream.Position != br.BaseStream.Length)
				br.BaseStream.Seek(-(bytes.Length % sizeof(int)), SeekOrigin.Current);

			for (int i = 0; i < bytes.Length / sizeof(int); i++)
				ints[i] = BitConverter.ToInt32(bytes, sizeof(int) * i);

			return ints;
		}

		#endregion
	}
}
