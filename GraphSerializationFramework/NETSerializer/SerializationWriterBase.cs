using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph;
using QuickGraph.Collections;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GraphSerializationFramework.NETSerializer {

	/// <summary>
	/// Graph serialization writer using .NET Serialization 
	/// Based on the QuickGraph.NET library non-portable project by Diego Tosato
	/// https://sites.google.com/site/diegotosato/news-and-recent-highlights/quickgraph36standardnetclasslibrary
	/// </summary>
	/// <typeparam name="TVertex">Vertex type</typeparam>
	/// <typeparam name="TEdge">Edge type</typeparam>
	public class SerializationWriter<TVertex, TEdge> : IGraphWriter<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		protected bool disposed = false;
		protected string file;
		protected int bufferSize;
		

		public SerializationWriter(string file)
			: this(file, (int)Math.Pow(2, 16)) {
		}
		public SerializationWriter(string file, int bufferSize) {					
			this.file = file;
			this.bufferSize = bufferSize;			
		}

		~SerializationWriter() {
			Dispose(false);
		}

		
		#region IDisposable Members
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;
			}
		}

		public event ProgressChangedEventHandler ProgressChanged;

		#endregion

		#region IGraphWriter<TVertex,TEdge> Members

		public void WriteGraph(IUndirectedGraph<TVertex, TEdge> graph) {
			BinaryFormatter bf = new BinaryFormatter();
			using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize)) {
				bf.Serialize(fs, graph);
			}
		}
		public void WriteGraph(IVertexAndEdgeListGraph<TVertex, TEdge> graph) {
			BinaryFormatter bf = new BinaryFormatter();
			using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize)) {
				bf.Serialize(fs,graph);
			}

		}
		
		public void WriteNextPart(IVertexEdgeDictionary<TVertex, TEdge> graph) {
			throw new NotSupportedException();
		}


		#endregion
	}
}
