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
		IBidirectionalGraph<TVertex, TEdge> ReadEntireGraphAsBidirectional();
		IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected();
		IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected(bool allowParallelEdges);
		void ResetStream();
		IVertexAndEdgeListGraph<TVertex, TEdge> ReadPartialGraph();
		IBidirectionalGraph<TVertex, TEdge> ReadPartialGraphAsBidirectional();
		IUndirectedGraph<TVertex, TEdge> ReadPartialGraphAsUndirected();
		IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList();
		IDictionary<TVertex, ICollection<TVertex>> ReadAdjacencyList();
		IEnumerable<Edge<TVertex>> StreamAllEdges();
		long Position { get; set; }
		long Length { get; }
		void Calibrate();
	}

	public interface IGraphWriter<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {
		void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph);
		void WriteGraph(IBidirectionalGraph<TVertex, TEdge> graph);
		void WriteNextPart(IVertexAndEdgeListGraph<TVertex, TEdge> graph);
		void WriteNextPart(IBidirectionalGraph<TVertex, TEdge> graph);
		void WriteNextPart(IDictionary<TVertex, ICollection<TVertex>> graph);
		void WriteNextEdges(IEdgeSet<TVertex, TEdge> edges);
		void WriteNextEdges(IEnumerable<TEdge> edges);
	}

	public interface IWeightedGraphWriter<TVertex, TEdge> : IGraphWriter<TVertex, TEdge>
	where TEdge : IEdge<TVertex> {
		void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph);

	}
}
