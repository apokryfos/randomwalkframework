using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using System.Runtime.Serialization;
using QuickGraph.Collections;
using System.ComponentModel;

namespace GraphSerializationFramework.GraphStreamFramework {

	public interface IGraphReader<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {
		
		event ProgressChangedEventHandler ProgressChanged;
		
		IVertexAndEdgeListGraph<TVertex, TEdge> ReadEntireGraph();		
		IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected();		
		IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList();
		void ResetStream();
		long Position { get; set; }
		long Length { get; }
		void Calibrate();
	}

	public interface IGraphWriter<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {
		
		event ProgressChangedEventHandler ProgressChanged;
		void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph);
		void WriteGraph(IUndirectedGraph<TVertex, TEdge> graph);		
		void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph);
	}
}
