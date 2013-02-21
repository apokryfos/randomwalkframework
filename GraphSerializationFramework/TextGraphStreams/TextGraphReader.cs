using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph.Collections;
using GraphSerializationFramework.GenericGraphSerializers;
using System.ComponentModel;

namespace GraphSerializationFramework {
	
	class TextGraphReader<TVertex, TEdge> : GenericGraphReaderBase<TVertex, TEdge>, IGraphReader<TVertex, TEdge> 
		where TVertex : IConvertible
		where TEdge : IEdge<TVertex> {
		private StreamReader stream;
		private int bufferSize;
		private bool srcIsCSV = false;
		EdgeFactory<TVertex, TEdge> edgeFactory;
		

		public TextGraphReader(string file)
			: this(file, (int)Math.Pow(2, 11)) {

		}
		public TextGraphReader(string file, int bufferSize) {
			stream = new StreamReader(file, Encoding.ASCII, false, bufferSize);
			this.bufferSize = bufferSize;
			srcIsCSV = (Path.GetExtension(file) == ".csv");

			var ctor = typeof(TEdge).GetConstructor(new Type[] { typeof(TVertex), typeof(TVertex) });
			if (ctor == null) {
				throw new NotSupportedException("Edges must have a constructor which takes two TVertex arugments");
			}
			this.edgeFactory = (s, t) => (TEdge)Activator.CreateInstance(typeof(TEdge), s, t);

			

		}
		public TextGraphReader(string file, int bufferSize, EdgeFactory<TVertex, TEdge> edgeFactory, Func<string,TVertex> vertexParser) 
			: this(file, (int)Math.Pow(2, 11)) {
				this.edgeFactory = edgeFactory;			
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


		public override IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList() {
			if (stream.EndOfStream)
				return null;

			IVertexEdgeDictionary<TVertex, TEdge> adjList = new VertexEdgeDictionary<TVertex, TEdge>();
			IEdgeList<TVertex, TEdge> current;
			int count = 0;
			do {
				string s = stream.ReadLine();

				var parts = s.Split(' ', ',', '\t');
				TVertex source, target;

				try {
					source = (TVertex)Convert.ChangeType(parts[0], typeof(TVertex));
				} catch (InvalidCastException) {
					continue;
				}


				if (!adjList.TryGetValue(source, out current)) {
					current = new EdgeList<TVertex, TEdge>();
					adjList.Add(source, current);
					count++;
				}

				for (int i = 1; i < parts.Length; i++) {
					try {
						target = (TVertex)Convert.ChangeType(parts[i], typeof(TVertex));
						current.Add(edgeFactory(source,target));
						count++;
					} catch (NotSupportedException) {
						continue;
					}											
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
	

		#endregion
	}
}
