using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using GraphSerializationFramework.BinaryGraphStreams;
using QuickGraph;

namespace GraphSerializationFramework.GraphStreamFramework
{


    static class ReaderWriterTypes
    {
        private static string[] extensions = { ".bin", ".csv", ".txt" };
        private static Type[] readers = { typeof(BinaryGraphReader), typeof(TextGraphReader), typeof(TextGraphReader) };
        private static Type[] writers = { typeof(BinaryGraphWriter), typeof(CSVGraphWriter), typeof(TextGraphWriter) };

        public static KeyValuePair<Type, Type> GetReaderAndWriterType(string extention)
        {
            for (int i = 0; i < extensions.Length; i++)
                if (extensions[i] == extention)
                    return new KeyValuePair<Type, Type>(readers[i], writers[i]);
            return default(KeyValuePair<Type,Type>);
        }
    }

    public static class GraphReaderFactory
    {

        #region Reader Per Extention
        public static IGraphReader<int, Edge<int>>  GetGraphReaderForExtension(string extension, string filename, int bufferSize)
        {
            var kv = ReaderWriterTypes.GetReaderAndWriterType(extension);
            if (kv.Equals(default(KeyValuePair<Type,Type>)))
                return null;
			var ctor = kv.Key.GetConstructor(new Type[] { typeof(string), typeof(int) });
			var gwinterface = kv.Key.GetInterface(typeof(IGraphReader<int, Edge<int>>).Name);

			if (gwinterface == null) {
				throw new InvalidOperationException();
			}
			if (ctor == null) {
				ctor = kv.Key.GetConstructor(new Type[] { typeof(string) });
				if (ctor != null) {
					return (IGraphReader<int, Edge<int>>)Activator.CreateInstance(kv.Key, filename);
				}
			} else {
				return (IGraphReader<int, Edge<int>>)Activator.CreateInstance(kv.Key, filename,bufferSize);
			}

            return null;
        }
		public static IGraphReader<int, Edge<int>> GetGraphReaderForExtension(string extension, string filename)
        {
            return GetGraphReaderForExtension(extension, filename, (int)Math.Pow(2, 16));
        }

		public static IGraphReader<int, Edge<int>> GetGraphReaderForExtension(string filename)
        {
            return GetGraphReaderForExtension(filename, (int)Math.Pow(2, 16));
        }

		public static IGraphReader<int, Edge<int>> GetGraphReaderForExtension(string filename, int bufferSize)
        {
            return GetGraphReaderForExtension(Path.GetExtension(filename), filename, bufferSize);

        }
        #endregion

    }


    public static class GraphWriterFactory
    {

     


        #region Writer Per Extention
		public static IGraphWriter<int, Edge<int>> GetGraphWriterForExtension(string extension, string filename, int bufferSize)
        {
            var kv = ReaderWriterTypes.GetReaderAndWriterType(extension);
            if (kv.Equals(default(KeyValuePair<Type,Type>)))
                return null;            

			var ctor = kv.Value.GetConstructor(new Type[] { typeof(string), typeof(int) });
			var gwinterface = kv.Value.GetInterface(typeof(IGraphWriter<int, Edge<int>>).Name);

			if (gwinterface == null)
				throw new InvalidOperationException();
			if (ctor != null) {
				return (IGraphWriter<int, Edge<int>>)Activator.CreateInstance(kv.Value, filename, bufferSize);
			} else {
				ctor = kv.Value.GetConstructor(new Type[] { typeof(string) });
				if (ctor != null) {
					return (IGraphWriter<int, Edge<int>>)Activator.CreateInstance(kv.Value, filename);
				}
			}
            return null;
        }
		public static IGraphWriter<int, Edge<int>> GetGraphWriterForExtension(string extension, string filename)
        {
            return GetGraphWriterForExtension(extension, filename, (int)Math.Pow(2, 11));
        }

		public static IGraphWriter<int, Edge<int>> GetGraphWriterForExtension(string filename)
        {
            return GetGraphWriterForExtension(filename, (int)Math.Pow(2, 11));
        }

		public static IGraphWriter<int, Edge<int>> GetGraphWriterForExtension(string filename, int bufferSize)
        {
            return GetGraphWriterForExtension(Path.GetExtension(filename), filename, bufferSize);

        }

        #endregion
    }

}
