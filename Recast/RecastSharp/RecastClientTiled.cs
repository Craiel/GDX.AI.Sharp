namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System;
    using AI.Recast.Protocol;
    using NLog;
    
    public class RecastClientTiled : RecastClient
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClientTiled(uint area, uint layer)
            : base(area, layer)
        {
            _Initialize(area, layer, true);
        }
         
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool GetDebugNavMesh(out byte[] data)
        {
            IntPtr ptr;
            int size;
            if (!_GetDebugNavMesh(this.Slot.Area, this.Slot.Layer, out ptr, out size))
            {
                data = null;
                return false;
            }

            data = GetArrayFromPtr(ptr, size);
            return true;
        }
         
        public bool Load(byte[] data)
        {
            IntPtr ptr = GetArrayPtr(data);
            return _Load(this.Slot.Area, this.Slot.Layer, ptr, data.Length);
        }

        public bool Save(out byte[] data)
        {
            IntPtr ptr;
            int size;
            if (!_Save(this.Slot.Area, this.Slot.Layer, out ptr, out size))
            {
                data = null;
                return false;
            }

            data = GetArrayFromPtr(ptr, size);

            return true;
        }

        public bool Generate(string path)
        {
            IntPtr ptr = GetStringPtr(path);
            bool result = _Build(this.Slot.Area, this.Slot.Layer, ptr, path.Length);

            _LogBuildTimes(this.Slot.Area, this.Slot.Layer);

            IntPtr logPtr;
            int logSize;
            if (!_GetLog(this.Slot.Area, this.Slot.Layer, out logPtr, out logSize))
            {
                return result;
            }

            byte[] logData = GetArrayFromPtr(logPtr, logSize);
            var log = ProtoRecastLog.Parser.ParseFrom(logData);
            foreach (string message in log.Messages)
            {
                Logger.Info(message);
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void Dispose(bool isDisposing)
        {
            _Destroy(this.Slot.Area, this.Slot.Layer);
        }
    }
}
