using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressingUtilities
{

    public class ParametrizedEventArgs : EventArgs
    {
        private object _parameter;
        public object Parameter
        {
            get { return _parameter; }
        }
        private ParametrizedEventArgs() { }
        public ParametrizedEventArgs(object parameter) 
            : base() 
        { 
            _parameter = parameter; 
        }

    }


    public delegate void ProgressEventDelegate(string text, long currentProgress, long maxProgress);



    public interface IProgressing
    {
        void SuppressProgress();
        void UnsuppressProgress();
        void SuppressStatus();
        void UnsuppressStatus();

        event EventHandler ProgressStart;
        event ProgressEventDelegate ProgressTick;
        event EventHandler ProgressFinish;
        event EventHandler<ParametrizedEventArgs> StatusText;
        
    }

    public class Progressing : IProgressing, IDisposable
    {
        private bool ProgressSupressed = false;
        private bool StatusSuppressed = false;

        private bool progressStarted = false;

        #region IProgressing Members

        protected virtual void OnProgressStarted()
        {


            if (ProgressStart != null && !ProgressSupressed)
            {
                progressStarted = true;
                ProgressStart(this, EventArgs.Empty);
            }
        }


        protected virtual void OnProgressTick(string text, int currentProgress, int maxProgress)
        {
            OnProgressTick(text, (long)currentProgress, (long)maxProgress);
        }
        protected virtual void OnProgressTick(string text, long currentProgress, long maxProgress)
        {
            if (!progressStarted && currentProgress < maxProgress)
                OnProgressStarted();

            if (ProgressTick != null && !ProgressSupressed)
                ProgressTick(text, currentProgress, maxProgress);

            if (currentProgress == maxProgress)
                OnProgressDone();
        }
        protected virtual void OnProgressDone()
        {
            progressStarted = false;
            if (ProgressFinish != null && !ProgressSupressed)
                ProgressFinish(this, EventArgs.Empty);
        }

        protected virtual void OnStatusText(object statusObject)
        {
            if (StatusText != null && !StatusSuppressed)
                StatusText(this, new ParametrizedEventArgs(statusObject));
        }

        public event EventHandler ProgressStart;

        public event ProgressEventDelegate ProgressTick;

        public event EventHandler ProgressFinish;

        public event EventHandler<ParametrizedEventArgs> StatusText;

        

        public void SuppressProgress()
        {
            ProgressSupressed = true;
        }

        public void UnsuppressProgress()
        {
            ProgressSupressed = false;
        }

        public void SuppressStatus()
        {
            StatusSuppressed = true;
        }

        public void UnsuppressStatus()
        {
            StatusSuppressed = false;
        }

        #endregion

      

        public virtual void Dispose()
        {
            ProgressStart = null;
            ProgressTick = null;
            ProgressFinish = null;
        }

       
    }

}
