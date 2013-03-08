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

		public void WriteGraph(IUndirectedGraph<TVertex, TEdge> graph) {
			AdjacencyGraph<TVertex, TEdge> g = new AdjacencyGraph<TVertex, TEdge>(true,graph.VertexCount);
			EdgeEquality eeq = new EdgeEquality();
			g.AddVertexRange(graph.Vertices);
			HashSet<TEdge> added = new HashSet<TEdge>(eeq);
			foreach (var e in graph.Edges) {
				if (added.Add(e)) {
					g.AddEdge(e);
				}
			}
			WriteGraph(g);


		}
		public abstract void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph);
		public abstract void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph);

		private class EdgeEquality : IEqualityComparer<TEdge>	{		
			private EdgeEqualityComparer<TVertex, TEdge> defaultC = 
				(
					(e, s, t) 
						=> 
					((e.Source.Equals(s) && e.Target.Equals(t)) || (e.Source.Equals(t) && e.Target.Equals(s)))
				);

			public EdgeEqualityComparer<TVertex, TEdge> Default {
				get { return defaultC; }
				set { defaultC = value; }
			}



			#region IEqualityComparer<TEdge> Members

			public bool Equals(TEdge x, TEdge y) {
				return Default(x, y.Source, y.Target);
			}

			public int GetHashCode(TEdge obj) {
				return obj.GetHashCode();
			}

			#endregion
		}


		#endregion
	}
}
