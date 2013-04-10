using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSerializationFramework.GraphStreamFramework;
using QuickGraph;
using QuickGraph.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace GraphSerializationFramework.NETSerializer {

	/// <summary>
	/// Graph serialization reader using .NET Serialization 
	/// Based on the QuickGraph.NET library non-portable project by Diego Tosato
	/// https://sites.google.com/site/diegotosato/news-and-recent-highlights/quickgraph36standardnetclasslibrary
	/// </summary>
	/// <typeparam name="TVertex">Vertex type</typeparam>
	/// <typeparam name="TEdge">Edge type</typeparam>
	public class SerializationReader<TVertex, TEdge> : IGraphReader<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		protected bool disposed = false;
		protected string file;
		protected int bufferSize;
		private FileInfo fi;

		public SerializationReader(string file)
			: this(file, (int)Math.Pow(2, 16)) {
		}
		public SerializationReader(string file, int bufferSize) {					
			this.file = file;
			this.bufferSize = bufferSize;
			fi = new FileInfo(file);
		}

		~SerializationReader() {
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

		#endregion

		#region IGraphReader<TVertex,TEdge> Members


		public IVertexAndEdgeListGraph<TVertex, TEdge> ReadEntireGraph() {
			BinaryFormatter bf = new BinaryFormatter();
			using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize)) {
				return (IVertexAndEdgeListGraph<TVertex, TEdge>)bf.Deserialize(fs);
			}
		}

		public IUndirectedGraph<TVertex, TEdge> ReadEntireGraphAsUndirected() {
			BinaryFormatter bf = new BinaryFormatter();
			using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize)) {
				return (IUndirectedGraph<TVertex, TEdge>)bf.Deserialize(fs);
			}
		}

		public IVertexEdgeDictionary<TVertex, TEdge> ReadAdjecencyList() {
			throw new NotSupportedException();
		}

		public void ResetStream() {	}

		public long Position {
			get { return 0; }
			set { throw new NotSupportedException(); }
		}

		public long Length {
			get { return fi.Length; }
		}

		public void Calibrate() { } 

		public event ProgressChangedEventHandler ProgressChanged;

		#endregion
		
	}
}
