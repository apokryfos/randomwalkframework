using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using GraphSerializationFramework.BinaryGraphStreams;
using QuickGraph.Collections;
using GraphSerializationFramework.GenericGraphSerializers;

namespace GraphSerializationFramework {

	public struct BinaryGraphFileConstants {
		public const int StartOfGraph = -6;
		public const int StartOfMetaData = -5;
		public const int EndOfDirection = -4;
		public const int EndOfLine = -2;
		public const int Empty = -3;
	}

	public class BinaryGraphReader : GenericGraphReaderBase<int, Edge<int>>, IGraphReader<int, Edge<int>> {
		protected FileStream baseStream;
		protected BinaryReader stream;
		private string file;



		public BinaryGraphReader(string file)
			: this(file, (int)Math.Pow(2, 16)) {
		}
		public BinaryGraphReader(string file, int bufferSize) {
			baseStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
			stream = new BinaryReader(baseStream);
			this.file = file;
		}

		~BinaryGraphReader() {
			Dispose(false);
		}


		#region IGraphReader Members
		
		public override IVertexEdgeDictionary<int, Edge<int>> ReadAdjecencyList() {
			if (baseStream.Position == baseStream.Length) {
				return null;
			}

			int bufferSize = (int)Math.Pow(2, 15);
			int[] buffer;

			bool isSource = true;
			bool outdirection = true;
			int src = -1, pass = 0;
			var adjlist = new VertexEdgeDictionary<int, Edge<int>>();

			IEdgeList<int, Edge<int>> current = null;
			do {

				long virtualPos = baseStream.Position;
				buffer = BinaryDataFunctions.ReadInt32Buffer(stream, bufferSize);
				for (int i = 0; i < buffer.Length; i++) {
					virtualPos += sizeof(int);

					if (isSource) {
						src = buffer[i];
						isSource = false;
						if (!adjlist.TryGetValue(src, out current)) {
							current = new EdgeList<int, Edge<int>>();
							adjlist.Add(src, current);
						}
					} else if (buffer[i] == BinaryGraphFileConstants.Empty)
						continue;
					else if (buffer[i] == BinaryGraphFileConstants.EndOfDirection) {
						outdirection = false;
						//current.Add(BinaryGraphFileConstants.EndOfDirection);
					} else if (buffer[i] == BinaryGraphFileConstants.EndOfLine) {
						isSource = true;
						outdirection = true;
						if (pass > 0) {
							baseStream.Seek(virtualPos - baseStream.Position, SeekOrigin.Current);
							break;
						}
					} else if (outdirection == true)
						current.Add(new Edge<int>(src, buffer[i]));
				}
				pass++;
			} while (!isSource && baseStream.Position < baseStream.Length);
			OnProgressChanged((int)(((double)Position / (double)Length) * 100.0), "Working");
			return adjlist;
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
			if (Position < Length && Position >= sizeof(int)) {
				baseStream.Position -= sizeof(int);

				int next = stream.ReadInt32();
				while (next != BinaryGraphFileConstants.EndOfLine && Position < Length)
					next = stream.ReadInt32();
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing) {
			if (!base.disposed) {
				if (disposing) {
					stream.Dispose();
				}
				base.disposed = true;
			}		}
		#endregion

	}

	public class BinaryGraphWriter : GenericGraphWriterBase<int, Edge<int>>, IGraphWriter<int, Edge<int>> {
		private FileStream baseStream;
		private BinaryWriter stream;


		public BinaryGraphWriter(string file)
			: this(file, 1024) {
		}
		public BinaryGraphWriter(string file, int bufferSize) {
			baseStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
			stream = new BinaryWriter(baseStream);
		}

		~BinaryGraphWriter() {

			Dispose(false);
		}

		#region IGraphWriter Members

		public override void WriteNextPart(IVertexEdgeDictionary<int, Edge<int>> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");			
			foreach (var kv in graph) {
				var bytes = BinaryDataFunctions.MakeOutEdgesList(kv.Key, kv.Value);
				stream.Write(bytes);
				i++;
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.Count) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");

		}

		public override void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");			
			foreach (var v in graph.Vertices) {
				var bytes = BinaryDataFunctions.MakeOutEdgesList(v, graph.OutEdges(v).Select(e => e.GetOtherVertex(v)),graph.OutDegree(v));
				stream.Write(bytes);
				i++;
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.VertexCount) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");
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
