using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using GraphSerializationFramework.BinaryGraphStreams;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using GraphSerializationFramework.TextGraphStreams;
using GraphSerializationFramework.NETSerializer;

namespace GraphSerializationFramework.GenericGraphSerializers {


	static class ReaderWriterTypes<TVertex, TEdge> 
		where TEdge : IEdge<TVertex> 		
	{		
		private static string[] extensions = { ".nbin", ".bin", ".csv", ".txt" };
		private static Type[] readers = { typeof(SerializationReader<TVertex,TEdge>), typeof(BinaryGraphReader), typeof(TextGraphReader<TVertex, TEdge>), typeof(TextGraphReader<TVertex, TEdge>) };
		private static Type[] writers = { typeof(SerializationWriter<TVertex,TEdge>), typeof(BinaryGraphWriter), typeof(CSVGraphWriter<TVertex, TEdge>), typeof(TextGraphWriter<TVertex, TEdge>) };

		public static KeyValuePair<Type, Type> GetReaderAndWriterType(string extention) {
			
			for (int i = 0; i < extensions.Length; i++) {
				if (extensions[i] == extention) {
					return new KeyValuePair<Type, Type>(readers[i], writers[i]);
				}
			}
			 
			return default(KeyValuePair<Type, Type>);
		}
	}


	public static class GenericGraphReaderFactory<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		#region Reader Per Extention
		public static IGraphReader<TVertex, TEdge> GetGraphReaderForExtension(string extension, string filename, int bufferSize) {
			var kv = ReaderWriterTypes<TVertex, TEdge>.GetReaderAndWriterType(extension);
			if (kv.Equals(default(KeyValuePair<Type, Type>)))
				return null;
			var ctor = kv.Key.GetConstructor(new Type[] { typeof(string), typeof(int) });
			var gwinterface = kv.Key.GetInterface(typeof(IGraphReader<TVertex, TEdge>).Name);

			if (gwinterface == null) {
				throw new InvalidOperationException();
			}
			if (ctor == null) {
				ctor = kv.Key.GetConstructor(new Type[] { typeof(string) });
				if (ctor != null) {
					return (IGraphReader<TVertex, TEdge>)Activator.CreateInstance(kv.Key, filename);
				}
			} else {
				return (IGraphReader<TVertex, TEdge>)Activator.CreateInstance(kv.Key, filename, bufferSize);
			}

			return null;
		}
		public static IGraphReader<TVertex, TEdge> GetGraphReaderForExtension(string extension, string filename) {
			return GetGraphReaderForExtension(extension, filename, (int)Math.Pow(2, 16));
		}

		public static IGraphReader<TVertex, TEdge> GetGraphReaderForExtension(string filename) {
			return GetGraphReaderForExtension(filename, (int)Math.Pow(2, 16));
		}

		public static IGraphReader<TVertex, TEdge> GetGraphReaderForExtension(string filename, int bufferSize) {
			return GetGraphReaderForExtension(Path.GetExtension(filename), filename, bufferSize);

		}
		#endregion

	}


	public static class GenericGraphWriterFactory<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {




		#region Writer Per Extention
		public static IGraphWriter<TVertex, TEdge> GetGraphWriterForExtension(string extension, string filename, int bufferSize) {
			var kv = ReaderWriterTypes<TVertex, TEdge>.GetReaderAndWriterType(extension);
			if (kv.Equals(default(KeyValuePair<Type, Type>)))
				return null;

			var ctor = kv.Value.GetConstructor(new Type[] { typeof(string), typeof(int) });
			var gwinterface = kv.Value.GetInterface(typeof(IGraphWriter<int, Edge<int>>).Name);

			if (gwinterface == null)
				throw new InvalidOperationException();
			if (ctor != null) {
				return (IGraphWriter<TVertex, TEdge>)Activator.CreateInstance(kv.Value, filename, bufferSize);
			} else {
				ctor = kv.Value.GetConstructor(new Type[] { typeof(string) });
				if (ctor != null) {
					return (IGraphWriter<TVertex, TEdge>)Activator.CreateInstance(kv.Value, filename);
				}
			}
			return null;
		}
		public static IGraphWriter<TVertex, TEdge> GetGraphWriterForExtension(string extension, string filename) {
			return GetGraphWriterForExtension(extension, filename, (int)Math.Pow(2, 11));
		}

		public static IGraphWriter<TVertex, TEdge> GetGraphWriterForExtension(string filename) {
			return GetGraphWriterForExtension(filename, (int)Math.Pow(2, 11));
		}

		public static IGraphWriter<TVertex, TEdge> GetGraphWriterForExtension(string filename, int bufferSize) {
			return GetGraphWriterForExtension(Path.GetExtension(filename), filename, bufferSize);

		}

		#endregion
	}

}
