using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph;
using GraphSerializationFramework.GenericGraphSerializers;
using QuickGraph.Collections;
using System.IO;

namespace GraphSerializationFramework.TextGraphStreams {

	
	public class WeigtedGraphLineParser<TVertex, TEdge, TTag> : GenericGraphReaderBase<TVertex, TEdge>, IGraphReader<TVertex, TEdge> 
		where TEdge : IEdge<TVertex>, ITagged<TTag> {

		private StreamReader stream;
		private int bufferSize;
		private LineToEdgesConverter edgeFactory;


		public delegate TEdge[] LineToEdgesConverter(string line);


		private WeigtedGraphLineParser() { }

		public WeigtedGraphLineParser(string file, LineToEdgesConverter edgeFactoryFromLine)
			: this(file, edgeFactoryFromLine, (int)Math.Pow(2, 14)) {
		}

		public WeigtedGraphLineParser(string file, LineToEdgesConverter edgeFactoryFromLine, int bufferSize)
			: base() {
			edgeFactory = edgeFactoryFromLine;
			this.bufferSize = bufferSize;
			stream = new StreamReader(file,Encoding.ASCII, false, bufferSize);
				 
		}

		protected override void Dispose(bool disposing) {
			if (!base.disposed) {
				if (disposing) {
					stream.Dispose();
				}
				base.disposed = true;
			}
		}

		public override IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList() {
			if (stream.EndOfStream)
				return null;

			IVertexEdgeDictionary<TVertex, TEdge> adjList = new VertexEdgeDictionary<TVertex, TEdge>();
			IEdgeList<TVertex, TEdge> current;
			int count = 0;
			do {
				string s = stream.ReadLine();				
				var edges = edgeFactory(s);
				foreach (var e in edges) {
					if (!adjList.TryGetValue(e.Source, out current)) {
						current = new EdgeList<TVertex, TEdge>();
						adjList.Add(e.Source, current);
						count++;
					}
					current.Add(e);
					count++;
				}
				
			} while (!stream.EndOfStream && count < bufferSize);
			OnProgressChanged((int)(((double)Position / (double)Length) * 100.0), "Working");
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
	}
}
