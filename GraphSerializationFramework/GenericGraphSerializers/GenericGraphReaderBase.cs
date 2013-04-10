using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph;
using QuickGraph.Collections;
using System.ComponentModel;

namespace GraphSerializationFramework.GenericGraphSerializers {
	public abstract class GenericGraphReaderBase<TVertex, TEdge> : IGraphReader<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {


		protected bool disposed = false;

		protected void Merge(IMutableVertexAndEdgeSet<TVertex, TEdge> graph, IVertexEdgeDictionary<TVertex, TEdge> part) {
			foreach (var kv in part) {
				graph.AddVerticesAndEdgeRange(kv.Value);
			}
		}

		protected static IMutableVertexAndEdgeSet<TVertex, TEdge> CreateInstance(bool directed) {
			if (directed) {
				return PrefferedGenericGraphTypes<TVertex, TEdge>.GetPrefferedGraphInstance();
			} else {
				return PrefferedGenericGraphTypes<TVertex, TEdge>.GetPrefferedUndirectedGraphInstance();
			}

		}

		#region IDisposable Members
		public void Dispose() {
			Dispose(true);			
			GC.SuppressFinalize(this);
		}
		protected abstract void Dispose(bool disposing);

		#endregion

		#region IGraphReader<TVertex,TEdge> Members

		protected IMutableVertexAndEdgeSet<TVertex, TEdge> ReadEntireGraph(bool directed) {
			var g = CreateInstance(directed);

			IVertexEdgeDictionary<TVertex, TEdge> adj = ReadAdjecencyList();
			OnProgressChanged(0, "Started");
			while (adj != null) {
				Merge(g, adj);
				adj = ReadAdjecencyList();
			}
			OnProgressChanged(100, "Finished");
			return g;

		}

		public IVertexAndEdgeListGraph<TVertex, TEdge> ReadEntireGraph() {
			return (IVertexAndEdgeListGraph<TVertex, TEdge>)ReadEntireGraph(true);
		}

		public IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected() {
			return (IUndirectedGraph<TVertex, TEdge>)ReadEntireGraph(false);
		}

		public abstract IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList();

		public abstract void ResetStream();

		public abstract long Position {
			get;
			set;
		}

		public abstract long Length {
			get;
		}

		public abstract void Calibrate();

		public event ProgressChangedEventHandler ProgressChanged;

		

		#endregion



		protected void OnProgressChanged(int percentage, object state) {
			if (ProgressChanged != null) {
				ProgressChanged(this, new ProgressChangedEventArgs(percentage, state));
			}
		}



	}
}
