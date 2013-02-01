using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using ProgressingUtilities;

using System.Runtime.Serialization;

namespace GraphSerializationFramework.GraphStreamFramework
{


    [Flags]
    public enum GraphStreamCaps 
    { 
        Simple = 0x0,
        AdjecencyStreamable=0x1,
        RandomAccess = 0x2,        
        VertexSet = 0x4, 
        VertexMetadata = 0x8, 
        EdgeStreamable = 0x10,
        EdgeMetaData = 0x20,
        GraphMetaData = 0x40
    }


    public interface IGraphStream : IDisposable, IProgressing
    {
        string[] SupportedExtensions { get; }
        GraphStreamCaps Capabilities { get; }
        bool HasCapability(GraphStreamCaps cap);
    }

    public abstract class GraphStreamBase : Progressing, IGraphStream
    {
        protected GraphStreamCaps caps;

        #region IGraphStream Members

        public abstract string[] SupportedExtensions
        {
            get;
        }

        public GraphStreamCaps Capabilities
        {
            get { return caps; }
        }

        public bool HasCapability(GraphStreamCaps cap)
        {
            return ((caps & cap) == cap); 
        }

        #endregion
    }

    public class VertexMetaData 
    {
        
        public int MetaDataLength 
        {
            get { return metalength; }
        }

        private int vertexId;
        private int outdegree;
        private int indegree;
        private long edgelistoffset;
        private int metalength;


        public int VertexId 
        {
            get { return vertexId; }
            set 
            {
                vertexId = value; 
            }
        }

        public int OutDegree
        {
            get 
            {
                return outdegree;
            }
            set
            {
                outdegree = value;
            }
        }


        public int InDegree
        {
            get { return indegree; }
            set { indegree = value; ; }
        }


        public long EdgeListOffset
        {
            get { return edgelistoffset; }
            set { edgelistoffset = value; }
        }
      
        public byte[] BinaryData
        {
            get 
            {
                byte[] metadata = new byte[metalength];
                Buffer.BlockCopy(BitConverter.GetBytes(vertexId), 0, metadata, 0, sizeof(int));
                Buffer.BlockCopy(BitConverter.GetBytes(outdegree), 0, metadata, sizeof(int), sizeof(int));
                Buffer.BlockCopy(BitConverter.GetBytes(indegree), 0, metadata, 2* sizeof(int), sizeof(int));
                Buffer.BlockCopy(BitConverter.GetBytes(edgelistoffset), 0, metadata, 3 * sizeof(int), sizeof(long));
                return metadata; 
            }
            private set
            {
                metalength = value.Length;
                vertexId = BitConverter.ToInt32(value, 0);
                outdegree = BitConverter.ToInt32(value, sizeof(int));
                indegree = BitConverter.ToInt32(value, 2*sizeof(int));
                edgelistoffset = BitConverter.ToInt64(value, 3*sizeof(int));
            }
        }

        public VertexMetaData(byte[] data)
        {
            BinaryData = data;
        }

        public VertexMetaData(int metadataLen, int vertex_id, int inDegree, int outDegree, long edgeListOffset)
        {
            metalength = metadataLen;
            VertexId = vertex_id;
            InDegree = inDegree;
            OutDegree = outDegree;
            EdgeListOffset = edgeListOffset;
        }


    
    }

    public class GraphMetaData
    {
      
        public int MetaDataLength
        {
            get { return metadata.Length; }
        }

        public long VertexCount 
        {
            get { return BitConverter.ToInt64(metadata, 0); }
            set { Buffer.BlockCopy(BitConverter.GetBytes(value), 0, metadata, 0, sizeof(Int64)); }
        }
        public long EdgeCount
        {
            get { return BitConverter.ToInt64(metadata, sizeof(Int64)); }
            set { Buffer.BlockCopy(BitConverter.GetBytes(value), 0, metadata, sizeof(Int64), sizeof(Int64)); }
        }

        public int VertexMetaDataLength
        {
            get { return BitConverter.ToInt32(metadata, 2*sizeof(Int64)); }
            set { Buffer.BlockCopy(BitConverter.GetBytes(value), 0, metadata, 2*sizeof(Int64), sizeof(Int32)); }
        }

        public long GetEdgeListOffset()
        {
            return (sizeof(int) + MetaDataLength + VertexCount * VertexMetaDataLength);
        }

        private byte[] metadata;

        public byte[] BinaryData
        {
            get { return metadata; }
        }

        public GraphMetaData(byte[] data)
        {
            metadata = new byte[data.Length];
            Buffer.BlockCopy(data, 0, metadata, 0, data.Length);
        }

        public GraphMetaData(int length, long vertexCount, long edgeCount,int vertexMetaLen)
        {

            metadata = new byte[length];
            VertexCount = vertexCount;
            EdgeCount = edgeCount;
            VertexMetaDataLength = vertexMetaLen;
        }

    }


    public interface IGraphReader : IGraphStream
    {
            
        IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph();
        IBidirectionalGraph<int, Edge<int>> ReadEntireGraphAsBidirectional();
        IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected();
        IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected(bool allowParallelEdges);


        void ResetStream();
        IVertexAndEdgeListGraph<int, Edge<int>> ReadPartialGraph();
        IBidirectionalGraph<int, Edge<int>> ReadPartialGraphAsBidirectional();
        IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected();
        IDictionary<int, ICollection<int>> ReadAdjacencyList();
        IEnumerable<Edge<int>> StreamAllEdges();

        long Position
        {
            get;
            set;
        }
        long Length
        {
            get;
        }


        void Calibrate();
    }

    public interface IGraphWriter : IGraphStream
    {
        void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph);
        void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph);        
        void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph);
        void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph);
        void WriteNextPart(IDictionary<int, ICollection<int>> graph);
        void WriteNextEdges(IEdgeSet<int, Edge<int>> edges);
        void WriteNextEdges(IEnumerable<Edge<int>> edges);

    }

}
