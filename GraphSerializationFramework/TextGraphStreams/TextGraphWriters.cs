using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using System.IO;
using QuickGraph.Collections;

namespace GraphSerializationFramework.TextGraphStreams {

	class TextGraphWriter<TVertex, TEdge> : GenericGraphWriterBase<TVertex, TEdge>, IGraphWriter<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		private StreamWriter stream;


		public TextGraphWriter(string file)
			: this(file, (int)Math.Pow(2, 11)) {
		}
		public TextGraphWriter(string file, int bufferSize) {
			stream = new StreamWriter(file, false, Encoding.ASCII, bufferSize);

		}


		~TextGraphWriter() {
			Dispose(false);
		}


		protected override void Dispose(bool disposing) {
			if (!base.disposed) {
				if (disposing) {
					stream.Dispose();
				}
				base.disposed = true;
			}
		}


		#region IGraphWriter Members

		public override void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");
			foreach (var e in graph.Edges) {
				stream.WriteLine(e.Source.ToString() + " " + e.Target.ToString());
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.EdgeCount) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");
		}

		public override void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");
			foreach (var kv in graph) {
				foreach (var v in kv.Value) {
					stream.WriteLine(kv.Key.ToString() + " " + v.ToString());
				}
				i++;
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.Count) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");
		}

		#endregion


	}

	class CSVGraphWriter<TVertex, TEdge> : GenericGraphWriterBase<TVertex, TEdge>, IGraphWriter<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		private StreamWriter stream;

		public CSVGraphWriter(string file)
			: this(file, (int)Math.Pow(2, 11)) {

		}
		public CSVGraphWriter(string file, int bufferSize) {
			stream = new StreamWriter(file, false, Encoding.ASCII, bufferSize);
		}

		~CSVGraphWriter() {
			Dispose(false);
		}


		protected override void Dispose(bool disposing) {
			if (!base.disposed) {
				if (disposing) {
					stream.Dispose();
				}
				base.disposed = true;
			}
		}

		#region IGraphWriter Members

		public override void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");
			foreach (var v in graph.Vertices) {
				stream.Write(v.ToString());
				foreach (var e in graph.OutEdges(v))
					stream.Write("," + e.Target.ToString());
				stream.WriteLine();
				i++;
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.VertexCount) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");
		}

		public override void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph) {
			int i = 0;
			OnProgressChanged(0, "Started");
			foreach (var kv in graph) {
				stream.Write(kv.Key.ToString());
				foreach (var v in kv.Value)
					stream.Write("," + v.ToString());
				stream.WriteLine();
				i++;
				if (i % 100 == 0) {
					OnProgressChanged((int)(((double)i / (double)graph.Count) * 100.0), "Working");
				}
			}
			OnProgressChanged(100, "Finished");
		}

		#endregion
	}

}
