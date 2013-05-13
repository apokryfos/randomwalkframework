using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;

namespace RandomWalkAnalysis {

	public delegate void LoggingEvent<TVertex, TEdge>(RandomWalkLogger<TVertex, TEdge> obj, string logPath)
			where TEdge : IEdge<TVertex>
			;


	public interface IRandomWalkLogger<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {

		void OpenLogger();
		void CloseLogger();
		void LogLine(IEnumerable<object> line);

		event EventHandler StartedLogging;
		event LoggingEvent<TVertex, TEdge> FinishedLogging;
	}

	public class RandomWalkBinaryLogger<TVertex, TEdge> : RandomWalkLogger<TVertex, TEdge>, IRandomWalkLogger<TVertex, TEdge>
		 where TEdge : IEdge<TVertex> {

		private new BinaryWriter log;

		public RandomWalkBinaryLogger(string logPath)
			: base(logPath) {

		}

		public override void OpenLogger() {
			log = new BinaryWriter(new FileStream(logging_path, FileMode.Create, FileAccess.Write, FileShare.None));
		}

		public override void CloseLogger() {
			log.Close();
		}

		public override void LogLine(IEnumerable<object> parameters) {
			foreach (var p in parameters) {

				if (p.GetType() == typeof(int) || p.GetType() == typeof(Int32))
					log.Write((int)p);
				else if (p.GetType() == typeof(double) || p.GetType() == typeof(Double))
					log.Write((double)p);
				else if (p.GetType() == typeof(decimal) || p.GetType() == typeof(Decimal))
					log.Write((decimal)p);
				else if (p.GetType() == typeof(short) || p.GetType() == typeof(Int16))
					log.Write((short)p);
				else if (p.GetType() == typeof(long) || p.GetType() == typeof(Int64))
					log.Write((long)p);
				else if (p.GetType() == typeof(byte) || p.GetType() == typeof(Byte))
					log.Write((byte)p);
				else if (p.GetType() == typeof(sbyte) || p.GetType() == typeof(SByte))
					log.Write((sbyte)p);
				else if (p.GetType() == typeof(ushort) || p.GetType() == typeof(UInt16))
					log.Write((ushort)p);
				else if (p.GetType() == typeof(uint) || p.GetType() == typeof(UInt32))
					log.Write((uint)p);
				else if (p.GetType() == typeof(ulong) || p.GetType() == typeof(UInt64))
					log.Write((ulong)p);
				else if (p.GetType() == typeof(bool) || p.GetType() == typeof(Boolean))
					log.Write((bool)p);
				else if (p.GetType() == typeof(float) || p.GetType() == typeof(Single))
					log.Write((float)p);
				else if (p.GetType() == typeof(char) || p.GetType() == typeof(Char))
					log.Write((char)p);
				else
					log.Write(p.ToString());
			}
		}

		public override void Dispose() {
			if (log != null)
				CloseLogger();
		}

	}

	public class RandomWalkLogger<TVertex, TEdge> : IRandomWalkLogger<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		protected StreamWriter log = null;
		public string logging_path { get; protected set; }
		protected RandomWalkObserver<TVertex, TEdge> obs;


		public event EventHandler StartedLogging;
		public event LoggingEvent<TVertex, TEdge> FinishedLogging;




		public RandomWalkLogger(string logPath) {
			if (logPath == null)
				throw new ArgumentNullException();

			this.logging_path = logPath;
		}


		public virtual void OpenLogger() {
			log = new StreamWriter(new FileStream(logging_path, FileMode.Create, FileAccess.Write, FileShare.None));
			if (StartedLogging != null)
				StartedLogging(this, EventArgs.Empty);

		}

		public virtual void LogLine(IEnumerable<object> parameters) {
			string toWrite = "";
			foreach (var o in parameters)
				toWrite += (o != null ? o.ToString() + "," : "");

			toWrite = toWrite.TrimEnd(',');
			log.WriteLine(toWrite);

		}

		public virtual void CloseLogger() {
			if (log != null) {
				try {
					log.Close();
					log = null;
				} catch {
				}
			}
			if (FinishedLogging != null)
				FinishedLogging(this, logging_path);


		}




		#region IDisposable Members

		public virtual void Dispose() {
			if (log != null)
				CloseLogger();
		}

		#endregion
	}
}
