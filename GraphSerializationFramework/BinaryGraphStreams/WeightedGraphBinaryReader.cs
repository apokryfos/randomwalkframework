using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph.Collections;
using GraphSerializationFramework.GenericGraphSerializers;
using GraphSerializationFramework.GraphStreamFramework;
using System.IO;
using QuickGraph;

namespace GraphSerializationFramework.BinaryGraphStreams {
	
	
	
	public class WeightedGraphBinaryReader<TVertex, TEdge, TTag> : GenericGraphReaderBase<TVertex, TEdge>, IGraphReader<TVertex, TEdge> 
		where TEdge : IEdge<TVertex>, ITagged<TTag> {
		protected FileStream baseStream;
		protected BinaryReader stream;
		private string file;
		BlockToEdgeConverter bytesToEdges;
		int blockSize;
		int bufferSize;

		public delegate int BlockToEdgeConverter(byte[] block, int start, out TEdge edge);
		
	

		public WeightedGraphBinaryReader(string file, BlockToEdgeConverter bytesToEdges, int blockSize)
			: this(file, bytesToEdges, blockSize, (int)Math.Pow(2, 16)) {
		}
		public WeightedGraphBinaryReader(string file, BlockToEdgeConverter bytesToEdges, int blockSize, int bufferSize) {
			baseStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
			stream = new BinaryReader(baseStream);
			this.file = file;
			this.bytesToEdges = bytesToEdges;
			this.blockSize = blockSize;
			this.bufferSize = bufferSize;
		}

		~WeightedGraphBinaryReader() {
			Dispose(false);
		}


		#region IGraphReader Members

		public override IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList() {
			if (baseStream.Position == baseStream.Length) {
				return null;
			}

			bufferSize -= bufferSize % blockSize;
			byte[] buffer;
			var adjList = new VertexEdgeDictionary<TVertex, TEdge>();
			IEdgeList<TVertex, TEdge> current = null;
			do {
				TEdge e;
				int i = 0;
				buffer = stream.ReadBytes(bufferSize);
				while (i < buffer.Length) {
					var used = bytesToEdges(buffer, i, out e);
					if (used > 0) {
						if (!adjList.TryGetValue(e.Source, out current)) {
							current = new EdgeList<TVertex, TEdge>();
							adjList.Add(e.Source, current);
						}
						current.Add(e);
						i += used;
					} else {
						i += blockSize;
					}
				}
				



			} while (baseStream.Position < baseStream.Length);
			OnProgressChanged((int)(((double)Position / (double)Length) * 100.0), "Working");
			return adjList;
		}



		public override void ResetStream() {
			baseStream.Seek(0, SeekOrigin.Begin);
		}



		public override long Position {
			get {
				return baseStream.Position;
			}
			set {
				baseStream.Position = Math.Min(value, Length);
				Calibrate();
			}
		}

		public override long Length {
			get { return baseStream.Length; }
		}

		public override void Calibrate() {
			baseStream.Position -= baseStream.Position % blockSize;
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing) {
			if (!base.disposed) {
				if (disposing) {
					stream.Dispose();
				}
				base.disposed = true;
			}
		}
		#endregion
	}
}
