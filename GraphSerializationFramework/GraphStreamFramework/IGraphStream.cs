using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using ProgressingUtilities;

using System.Runtime.Serialization;
using QuickGraph.Collections;

namespace GraphSerializationFramework.GraphStreamFramework {

	public delegate decimal WeightFunction<TGraph, TVertex, TEdge>(TGraph graph, TEdge edge)
		where TGraph : IGraph<TVertex, TEdge>
		where TEdge : IEdge<TVertex>;

	public interface IGraphReader<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {
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
		void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph);		
		void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph);
	}
}
