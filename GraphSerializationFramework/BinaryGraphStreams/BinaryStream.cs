using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using ProgressingUtilities;
using GraphSerializationFramework.BinaryGraphStreams;

namespace GraphSerializationFramework
{

    public struct BinaryGraphFileConstants
    {
        public const int StartOfGraph = -6;
        public const int StartOfMetaData = -5;
        public const int EndOfDirection = -4;
        public const int EndOfLine = -2;
        public const int Empty = -3;
    }

    public class BinaryGraphReader : GraphStreamBase, IGraphReader
    {
        protected FileStream baseStream;
        protected BinaryReader stream;
        private string file;



        public BinaryGraphReader(string file)
            : this(file, (int)Math.Pow(2,16))
        {
        }
        public BinaryGraphReader(string file, int bufferSize)            
        {
            baseStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read,bufferSize);
            stream = new BinaryReader(baseStream);
            this.file = file;
            caps = GraphStreamCaps.AdjecencyStreamable|GraphStreamCaps.EdgeStreamable;
        }

        ~BinaryGraphReader()
        {
            Dispose();
        }

        public override string[] SupportedExtensions
        {
            get { return new string[] { ".bin" }; }
        }

        protected void ReadAndMergeGraph(IMutableVertexAndEdgeListGraph<int, Edge<int>> g)
        {

            var adjLst = ReadAdjacencyList();
            foreach (var kv in adjLst)
                g.AddVerticesAndEdgeRange(kv.Value.Select<int, Edge<int>>(v => new Edge<int>(kv.Key, v)));

        }

        protected void ReadAndMergeGraph(IMutableUndirectedGraph<int, Edge<int>> g)
        {

            var adjLst = ReadAdjacencyList();
            foreach (var kv in adjLst)
                g.AddVerticesAndEdgeRange(kv.Value.Select<int, Edge<int>>(v => new Edge<int>(kv.Key, v)));

        }    
       

        #region IGraphReader Members

        public virtual IDictionary<int, ICollection<int>> ReadAdjacencyList()
        {

            if (baseStream.Position == 0)
                OnProgressStarted();
            if (baseStream.Position == baseStream.Length)
            {
                OnProgressDone();
                return null;
            }

            int bufferSize = (int)Math.Pow(2, 15);
            int[] buffer;

            bool isSource = true;
            bool outdirection = true;
            int src = -1, pass = 0;
            var adjlist = PrefferedDataTypes.GetAdjecencyListInstance();

            ICollection<int> current=null;
            do
            {
               
                long virtualPos = baseStream.Position;
                buffer = BinaryDataFunctions.ReadInt32Buffer(stream, bufferSize);
                for (int i = 0; i < buffer.Length; i++)
                {
                    virtualPos += sizeof(int);

                    if (isSource)
                    {
                        src = buffer[i];
                        isSource = false;
                        if (!adjlist.TryGetValue(src, out current))
                        {
                            current = PrefferedDataTypes.GetCollectionInstance();
                            adjlist.Add(src, current);
                        }
                    }
                    else if (buffer[i] == BinaryGraphFileConstants.Empty)
                        continue;
                    else if (buffer[i] == BinaryGraphFileConstants.EndOfDirection)
                    {
                        outdirection = false;
                        //current.Add(BinaryGraphFileConstants.EndOfDirection);
                    }
                    else if (buffer[i] == BinaryGraphFileConstants.EndOfLine)
                    {
                        isSource = true;
                        outdirection = true;
                        if (pass > 0)
                        {
                            baseStream.Seek(virtualPos - baseStream.Position, SeekOrigin.Current);
                            break;
                        }
                    }
                    else if (outdirection == true) 
                        current.Add(buffer[i]);
                }
                pass++;
            } while (!isSource && baseStream.Position < baseStream.Length);

            OnProgressTick("Reading graph...", baseStream.Position, baseStream.Length);
            return adjlist;
        }


        public virtual IVertexAndEdgeListGraph<int, Edge<int>> ReadPartialGraph()
        {
            if (baseStream.Position == 0)
                OnProgressStarted();
            if (baseStream.Position == baseStream.Length)
            {
                OnProgressDone();
                return null;
            }


            IMutableVertexAndEdgeListGraph<int, Edge<int>> g;           
            g = new AdjacencyGraph<int, Edge<int>>();
            ReadAndMergeGraph(g);
            
            return g;

        }

        public virtual IBidirectionalGraph<int, Edge<int>> ReadPartialGraphAsBidirectional()
        {
            if (baseStream.Position == 0)
                OnProgressStarted();
            if (baseStream.Position == baseStream.Length)
            {
                OnProgressDone();
                return null;
            }

            IMutableBidirectionalGraph<int, Edge<int>> g;
            g = new BidirectionalGraph<int, Edge<int>>();
            ReadAndMergeGraph(g);
            return g;
        }

        public virtual void ResetStream()
        {
            baseStream.Seek(0, SeekOrigin.Begin);
        }




        public virtual IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph()
        {
            ResetStream();
            IMutableVertexAndEdgeListGraph<int, Edge<int>> g = new AdjacencyGraph<int, Edge<int>>();
            OnProgressStarted();
            while (baseStream.Position < baseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", baseStream.Position, baseStream.Length);
            }
            OnProgressDone();
            return g;
        }

        public virtual IBidirectionalGraph<int, Edge<int>> ReadEntireGraphAsBidirectional()
        {
            ResetStream();
            IMutableBidirectionalGraph<int, Edge<int>> g = new BidirectionalGraph<int, Edge<int>>();
            OnProgressStarted();
            while (baseStream.Position < baseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", baseStream.Position, baseStream.Length);
            }
            OnProgressDone();
            return g;
        }

      
        public long Position
        {
            get
            {
                return baseStream.Position;
            }
            set 
            {
                baseStream.Position = Math.Min(value, Length); 
                Calibrate(); 
            }
        }

        public long Length
        {
            get { return baseStream.Length; }
        }

        public void Calibrate()
        {
            if (Position < Length && Position >= sizeof(int))
            {
                baseStream.Position -= sizeof(int);

                int next = stream.ReadInt32();
                while (next != BinaryGraphFileConstants.EndOfLine && Position < Length)
                    next = stream.ReadInt32();
            }
        }

        public IEnumerable<Edge<int>> StreamAllEdges()
        {
            ResetStream();
            var adj = ReadAdjacencyList();
            while (adj != null)
            {
                foreach (var kv in adj)
                {
                    foreach (var v in kv.Value)
                        yield return new Edge<int>(kv.Key, v);
                }
                adj = ReadAdjacencyList();
            }
            yield break;
        }


        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected(bool ape)
        {
            ResetStream();
            IMutableUndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>(ape);
            OnProgressStarted();
            while (baseStream.Position < baseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", baseStream.Position, baseStream.Length);
            }
            OnProgressDone();
            return g;
        }

        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected()
        {
            return ReadEntireGraphAsUndirected(true);
        }


        public IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected()
        {
            if (baseStream.Position == 0)
                OnProgressStarted();
            if (baseStream.Position == baseStream.Length)
            {
                OnProgressDone();
                return null;
            }
            IMutableUndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>();            
            ReadAndMergeGraph(g);

            return g;
        }

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                baseStream.Close();
                baseStream.Dispose();
                stream = null;
            }
            base.Dispose();
        }

        #endregion

    }

    public class BinaryGraphWriter : GraphStreamBase, IGraphWriter
    {
        private FileStream baseStream;
        private BinaryWriter stream;
   

        public BinaryGraphWriter(string file)
            : this(file, 1024)
        {
        }
        public BinaryGraphWriter(string file, int bufferSize)
        {
            baseStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
            stream = new BinaryWriter(baseStream);
            caps = GraphStreamCaps.AdjecencyStreamable;
        }

        ~BinaryGraphWriter()
        {
            
            Dispose();
        }
        
        #region IGraphWriter Members

        private HashSet<int> hiddenVertices = new HashSet<int>();

        public override string[] SupportedExtensions
        {
            get { return new string[] { ".bin" }; }
        }

        public void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            foreach (var v in graph.Vertices)
            {
                if (graph.OutDegree(v) == 0)
                {
                    hiddenVertices.Add(v);
                    continue;
                }
                if (hiddenVertices.Contains(v))
                    hiddenVertices.Remove(v);

                var bytes = BinaryDataFunctions.MakeOutEdgesList(v, graph.OutDegree(v), graph.OutEdges(v));
                stream.Write(bytes);
            }
        }

        public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);
            WriteIsolatedVertices();
        }

        protected void WriteIsolatedVertices()
        {
            byte[] bytes = new byte[sizeof(int) * (hiddenVertices.Count * 2)];
            byte[] hvb = new byte[sizeof(int)*2];
            Buffer.BlockCopy(BitConverter.GetBytes(BinaryGraphFileConstants.EndOfLine),0,hvb,sizeof(int),sizeof(int));
            int i = 0;
            foreach (var v in hiddenVertices)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(v), 0, hvb, 0, sizeof(int));
                Buffer.BlockCopy(hvb, 0, bytes, i, sizeof(int) * 2);
                i += 2 * sizeof(int);
            }
            stream.Write(bytes);
        }



        public void WriteNextPart(IDictionary<int, ICollection<int>> graph)
        {
            foreach (var kv in graph)
            {   
                var bytes = BinaryDataFunctions.MakeOutEdgesList(kv.Key, kv.Value);
                stream.Write(bytes);
            }
        }
     
        public void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph)
        {
            foreach (var v in graph.Vertices)
            {
                if (graph.Degree(v) == 0)
                {
                    hiddenVertices.Add(v);
                    continue;
                }
                if (hiddenVertices.Contains(v))
                    hiddenVertices.Remove(v);

                var bytes = BinaryDataFunctions.MakeOutEdgesList(v, graph.OutDegree(v), graph.OutEdges(v));
                stream.Write(bytes);
            }
        }



     
        public void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);
            WriteIsolatedVertices();
        }

        public void WriteNextEdges(IEdgeSet<int, Edge<int>> edges)
        {
            throw new NotSupportedException();
        }


        public void WriteNextEdges(IEnumerable<Edge<int>> edges)
        {
            throw new NotSupportedException();
        }


        #endregion

        #region IDisposable Memebers

        public override void Dispose()
        {
            
            if (stream != null)
            {
                WriteIsolatedVertices();
                stream.Close();
                stream = null;
            }
            base.Dispose();
        }

        
        #endregion


    }
}
