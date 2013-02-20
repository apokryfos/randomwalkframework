using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using ProgressingUtilities;
using QuickGraph.Collections;

namespace GraphSerializationFramework {
	class TextGraphWriter : GraphWriterBase<int, Edge<int>>, IGraphWriter<int, Edge<int>> {
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

		public override void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph) {
			foreach (var e in graph.Edges)
				stream.WriteLine(e.Source.ToString() + " " + e.Target.ToString());
		}

		public override void WriteNextPart(IVertexEdgeDictionary<int, Edge<int>> graph) {
			foreach (var kv in graph) {
				foreach (var v in kv.Value)
					stream.WriteLine(kv.Key.ToString() + " " + v.ToString());
			}
		}

		#endregion

		
	}

	class CSVGraphWriter : GraphWriterBase<int, Edge<int>>, IGraphWriter<int, Edge<int>> {
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

		public override void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph) {
			foreach (var v in graph.Vertices) {
				stream.Write(v.ToString());
				foreach (var e in graph.OutEdges(v))
					stream.Write("," + e.Target.ToString());
				stream.WriteLine();
			}
		}

		public override void WriteNextPart(IVertexEdgeDictionary<int, Edge<int>> graph) {
			foreach (var kv in graph) {
				stream.Write(kv.Key.ToString());
				foreach (var v in kv.Value)
					stream.Write("," + v.ToString());
				stream.WriteLine();
			}
		}

		#endregion
	}

	class TextGraphReader : GraphReaderBase<int, Edge<int>>, IGraphReader<int, Edge<int>> {
		private StreamReader stream;
		private int bufferSize;
		private bool srcIsCSV = false;


		public TextGraphReader(string file)
			: this(file, (int)Math.Pow(2, 11)) {

		}
		public TextGraphReader(string file, int bufferSize) {
			stream = new StreamReader(file, Encoding.ASCII, false, bufferSize);
			this.bufferSize = bufferSize;
			srcIsCSV = (Path.GetExtension(file) == ".csv");
		}

		~TextGraphReader() {
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

		#region IGraphReader Members


		public override IVertexEdgeDictionary<int, Edge<int>> ReadAdjecencyList() {
			if (stream.EndOfStream)
				return null;

			IVertexEdgeDictionary<int, Edge<int>> adjList = new VertexEdgeDictionary<int, Edge<int>>();
			IEdgeList<int, Edge<int>> current;
			int count = 0;
			do {
				string s = stream.ReadLine();

				var parts = s.Split(' ', ',', '\t');
				int source, target;
				if (!int.TryParse(parts[0], out source))
					continue;

				if (!adjList.TryGetValue(source, out current)) {
					current = new EdgeList<int, Edge<int>>();
					adjList.Add(source, current);
					count++;
				}

				for (int i = 1; i < parts.Length; i++) {
					if (int.TryParse(parts[i], out target)) {
						current.Add(new Edge<int>(source,target));
						count++;
					}
				}

			} while (!stream.EndOfStream && count < bufferSize);
			return adjList;
		}


		public override void ResetStream() {
			stream.BaseStream.Seek(0, SeekOrigin.Begin);
			stream.DiscardBufferedData();
		}

		public override long Position {
			get { return stream.BaseStream.Position; }
			set {
				stream.BaseStream.Position = Math.Min(value, Length);
				Calibrate();
			}
		}

		public override long Length {
			get { return stream.BaseStream.Length; }
		}


		public override void Calibrate() {
			if (Position < Length) {
				Position -= Environment.NewLine.Length;
				stream.ReadLine();
			}
		}
	

		#endregion
	}
}
