using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using System.ComponentModel;
using System.Diagnostics.Contracts;



namespace GraphSerializationFramework.GenericGraphSerializers {



	public class GraphWriter<TVertex, TEdge> : IGraphWriter<TVertex, TEdge>
		where TVertex : IConvertible	
		where  TEdge : IEdge<TVertex> {
		private IGraphWriter<TVertex, TEdge> baseWriter;

		public GraphWriter(string file)
			: this(file, 1024) {			

		}
		public GraphWriter(string file, int bufferSize) {
			baseWriter = GenericGraphWriterFactory<TVertex,TEdge>.GetGraphWriterForExtension(file, bufferSize);
		}

		~GraphWriter() {
			Dispose();
		}

		public void Dispose() {
			baseWriter.Dispose();
		}



		#region IGraphWriter<int,Edge<int>> Members

		public void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph) {
			baseWriter.WriteGraph(graph);
		}

		public void WriteNextPart(QuickGraph.Collections.IVertexEdgeDictionary<TVertex, TEdge> graph) {
			baseWriter.WriteNextPart(graph);
		}

		public event ProgressChangedEventHandler ProgressChanged {
			add { baseWriter.ProgressChanged += value; }
			remove { baseWriter.ProgressChanged -= value; }
		}

		#endregion

	}


	public class GraphReader<TVertex, TEdge> : IGraphReader<TVertex, TEdge>
		where TVertex : IConvertible	
		where  TEdge : IEdge<TVertex>
	{
		private IGraphReader<TVertex, TEdge> baseReader;

		public GraphReader(string file, int bufferSize) {
			
			baseReader = GenericGraphReaderFactory<TVertex, TEdge>.GetGraphReaderForExtension(file, bufferSize);
		}

		public GraphReader(string file)
			: this(file, 1024) {

		}

		~GraphReader() { Dispose(); }

		public void Dispose() {
			baseReader.Dispose();
		}

		#region IGraphReader Members


		public IVertexAndEdgeListGraph<TVertex, TEdge> ReadEntireGraph() {
			return baseReader.ReadEntireGraph();
		}
		public void ResetStream() {
			baseReader.ResetStream();
		}
		public long Position {
			get {
				return baseReader.Position;
			}
			set { baseReader.Position = value; }
		}
		public long Length {
			get { return baseReader.Length; }
		}
		public void Calibrate() {
			baseReader.Calibrate();
		}
		public IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected() {
			return baseReader.ReadEntireGraphAsUndirected();
		}
		public IUndirectedGraph<TVertex, TEdge> ReadPartialGraphAsUndirected() {
			return baseReader.ReadEntireGraphAsUndirected();
		}
		public QuickGraph.Collections.IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList() {
			return baseReader.ReadAdjecencyList();
		}

		public event ProgressChangedEventHandler ProgressChanged {
			add { baseReader.ProgressChanged += value; }
			remove { baseReader.ProgressChanged -= value; }
		}

		#endregion
	}

}
