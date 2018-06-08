using ManagedFile = Craiel.UnityEssentials.Runtime.IO.ManagedFile;

namespace Craiel.GDX.AI.Sharp.Runtime.IO
{
    using System;
    using System.IO;

    public abstract class BaseFileProvider : IDisposable
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract Stream BeginWrite(ManagedFile file);
        public abstract Stream BeginRead(ManagedFile file);

        public virtual void Write(ManagedFile file, byte[] data)
        {
            using (var stream = this.BeginWrite(file))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }

        public virtual void Read(ManagedFile file, out byte[] data)
        {
            using (var stream = this.BeginRead(file))
            {
                if (stream == null)
                {
                    data = null;
                    return;
                }

                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        public abstract ManagedFile[] Find(string pattern);
        
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected abstract void Dispose(bool isDisposing);
    }
}
