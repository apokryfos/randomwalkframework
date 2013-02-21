using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph;
using QuickGraph.Collections;
using System.ComponentModel;

namespace GraphSerializationFramework {
	public abstract class GenericGraphWriterBase<TVertex, TEdge> : IGraphWriter<TVertex, TEdge>
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

		public event ProgressChangedEventHandler ProgressChanged;

		#endregion

		protected void OnProgressChanged(int percentage, object state) {
			if (ProgressChanged != null) {
				ProgressChanged(this, new ProgressChangedEventArgs(percentage, state));
			}
		}



		#region IGraphWriter<TVertex,TEdge> Members

		public abstract void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph);
		public abstract void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph);		

		#endregion
	}
}
