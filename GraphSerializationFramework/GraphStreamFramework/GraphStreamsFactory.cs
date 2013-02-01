﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using GraphSerializationFramework.BinaryGraphStreams;

namespace GraphSerializationFramework.GraphStreamFramework
{


    static class ReaderWriterTypes
    {
        private static string[] extensions = { ".bin", ".csv", ".txt", ".bel", ".sbel" };
        private static Type[] readers = { typeof(BinaryGraphReader), typeof(TextGraphReader), typeof(TextGraphReader), typeof(BinaryEdgeListReader), typeof(BinaryEdgeListReader) };
        private static Type[] writers = { typeof(BinaryGraphWriter), typeof(CSVGraphWriter), typeof(TextGraphWriter), typeof(BinaryEdgeListWriter), typeof(BinaryEdgeListWriter) };

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
        public static IGraphReader GetGraphReaderForExtension(string extension, string filename, int bufferSize)
        {
            var kv = ReaderWriterTypes.GetReaderAndWriterType(extension);
            if (kv.Equals(default(KeyValuePair<Type,Type>)))
                return null;
            if (kv.Key.GetConstructor(new Type[] { typeof(string), typeof(int) }) != null && kv.Key.GetInterface("IGraphReader") != null)
                return (IGraphReader)Activator.CreateInstance(kv.Key, filename, bufferSize);

            return null;
        }
        public static IGraphReader GetGraphReaderForExtension(string extension, string filename)
        {
            return GetGraphReaderForExtension(extension, filename, (int)Math.Pow(2, 16));
        }

        public static IGraphReader GetGraphReaderForExtension(string filename)
        {
            return GetGraphReaderForExtension(filename, (int)Math.Pow(2, 16));
        }

        public static IGraphReader GetGraphReaderForExtension(string filename, int bufferSize)
        {
            return GetGraphReaderForExtension(Path.GetExtension(filename), filename, bufferSize);

        }
        #endregion

    }


    public static class GraphWriterFactory
    {

     


        #region Writer Per Extention
        public static IGraphWriter GetGraphWriterForExtension(string extension, string filename, int bufferSize)
        {
            var kv = ReaderWriterTypes.GetReaderAndWriterType(extension);
            if (kv.Equals(default(KeyValuePair<Type,Type>)))
                return null;            
            if (kv.Value.GetConstructor(new Type[] { typeof(string), typeof(int) }) != null && kv.Value.GetInterface("IGraphWriter") != null)
                return (IGraphWriter)Activator.CreateInstance(kv.Value, filename, bufferSize);
            
            return null;
        }
        public static IGraphWriter GetGraphWriterForExtension(string extension, string filename)
        {
            return GetGraphWriterForExtension(extension, filename, (int)Math.Pow(2, 11));
        }

        public static IGraphWriter GetGraphWriterForExtension(string filename)
        {
            return GetGraphWriterForExtension(filename, (int)Math.Pow(2, 11));
        }

        public static IGraphWriter GetGraphWriterForExtension(string filename, int bufferSize)
        {
            return GetGraphWriterForExtension(Path.GetExtension(filename), filename, bufferSize);

        }

        #endregion
    }

}
