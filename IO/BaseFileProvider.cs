namespace GDX.AI.Sharp.IO
{
    using System;
    using System.IO;

    using CarbonCore.Utils.IO;

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

        public abstract Stream BeginWrite(CarbonFile file);
        public abstract Stream BeginRead(CarbonFile file);

        public virtual void Write(CarbonFile file, byte[] data)
        {
            using (var stream = this.BeginWrite(file))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public virtual void Read(CarbonFile file, out byte[] data)
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
        
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected abstract void Dispose(bool isDisposing);
    }
}
