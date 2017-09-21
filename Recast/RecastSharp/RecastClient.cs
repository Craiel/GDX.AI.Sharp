namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using AI.Recast.Protocol;
    using Microsoft.Xna.Framework;

    public abstract class RecastClient : IDisposable
    {
        public const uint InvalidObstacleRef = 0;
        
        private static bool isConfigured;

        private static readonly IList<RecastSlot> InitializeCheck = new List<RecastSlot>();

        public struct RecastSlot
        {
            public RecastSlot(uint area, uint layer)
            {
                this.Area = area;
                this.Layer = layer;
            }

            public readonly uint Area;
            public readonly uint Layer;
        }

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClient(uint area, uint layer)
        {
            if (!isConfigured)
            {
                throw new ArgumentException("call RecastClient.Configure() first");
            }

            if (InitializeCheck.Any(x => x.Area == area && x.Layer == layer))
            {
                throw new ArgumentException("Recast area/layer is already occupied!");
            }

            this.Slot = new RecastSlot(area, layer);
            InitializeCheck.Add(this.Slot);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public RecastSlot Slot { get; private set; }

        public static bool Configure(uint totalAreas, uint totalLayers)
        {
            isConfigured = true;
            return _Configure(totalAreas, totalLayers);
        }

        public void LoadSettings(RecastClientSettings settings)
        {
            byte[] data = settings.GetData();
            _LoadSettings(this.Slot.Area, this.Slot.Layer, GetArrayPtr(data), data.Length);
        }

        public void Update(float delta)
        {
            _Update(this.Slot.Area, this.Slot.Layer, delta);
        }

        public bool FindRandomPointAroundCircle(uint startRef, Vector3 centerPosition, float maxRadius, out uint randomRef, out Vector3 randomPosition)
        {
            float x;
            float y;
            float z;
            if (!_FindRandomPointAroundCircle(this.Slot.Area, this.Slot.Layer, startRef, centerPosition.X, centerPosition.Y, centerPosition.Z, maxRadius,
                out randomRef, out x, out y, out z))
            {
                randomRef = 0;
                randomPosition = Vector3.Zero;
                return false;
            }

            randomPosition = new Vector3(x, y, z);
            return true;
        }

        public ProtoRecastPathInfo GetPath(uint startRef, uint endRef, Vector3 startPos, Vector3 endPos, bool detailed = false)
        {
            IntPtr ptr;
            int size;
            if (!_GetPath(this.Slot.Area, this.Slot.Layer, startRef, endRef, startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, detailed,
                out ptr, out size))
            {
                return null;
            }

            byte[] data = GetArrayFromPtr(ptr, size);
            return ProtoRecastPathInfo.Parser.ParseFrom(data);
        }
        
        public bool FindNearestPoly(Vector3 center, Vector3 extents, out uint nearestRef, out Vector3 nearestPoint)
        {
            float nearestX;
            float nearestY;
            float nearestZ;
            if (!_FindNearestPoly(this.Slot.Area, this.Slot.Layer, center.X, center.Y, center.Z,
                extents.X, extents.Y, extents.Z,
                out nearestRef,
                out nearestX,
                out nearestY,
                out nearestZ))
            {
                nearestRef = 0;
                nearestPoint = Vector3.Zero;
                return false;
            }
            
            nearestPoint = new Vector3(nearestX, nearestY, nearestZ);
            return true;
        }
        
        public int AddAgent(DetourCrowdAgentParameters parameters)
        {
            byte[] data = parameters.GetData();
            IntPtr ptr = GetArrayPtr(data);
            return _AddAgent(this.Slot.Area, this.Slot.Layer, ptr, data.Length);
        }

        public bool RemoveAgent(int index)
        {
            return _RemoveAgent(this.Slot.Area, this.Slot.Layer, index);
        }

        public bool UpdateAgent(int index, DetourCrowdAgentParameters parameters)
        {
            byte[] data = parameters.GetData();
            IntPtr ptr = GetArrayPtr(data);
            return _UpdateAgent(this.Slot.Area, this.Slot.Layer, index, ptr, data.Length);
        }

        public bool RequestMoveTarget(int agentIndex, uint polyRef, Vector3 position)
        {
            return _RequestMoveTarget(this.Slot.Area, this.Slot.Layer, agentIndex, polyRef, position.X, position.Y, position.Z);
        }

        public bool ResetMoveTarget(int agentIndex)
        {
            return _ResetMoveTarget(this.Slot.Area, this.Slot.Layer, agentIndex);
        }

        public ProtoCrowdAgentInfo GetAgentInfo(int agentIndex)
        {
            IntPtr ptr;
            int size;
            if (!_GetAgentInfo(this.Slot.Area, this.Slot.Layer, agentIndex, out ptr, out size))
            {
                return null;
            }

            byte[] data = GetArrayFromPtr(ptr, size);
            return ProtoCrowdAgentInfo.Parser.ParseFrom(data);
        }

        public bool AddObstacle(Vector3 position, float radius, float height, out uint obstacleRef)
        {
            return _AddObstacle(this.Slot.Area, this.Slot.Layer, position.X, position.Y, position.Z, radius, height, out obstacleRef);
        }

        public bool AddObstacleBox(Vector3 min, Vector3 max, out uint obstacleRef)
        {
            return _AddObstacleBox(this.Slot.Area, this.Slot.Layer, min.X, min.Y, min.Z, max.X, max.Y, max.Z, out obstacleRef);
        }

        public bool RemoveObstacle(uint obstacleRef)
        {
            return _RemoveObstacle(this.Slot.Area, this.Slot.Layer, obstacleRef);
        }

        public bool ClearObstacles()
        {
            return _ClearObstacles(this.Slot.Area, this.Slot.Layer);
        }

        public void Dispose()
        {
            InitializeCheck.Remove(this.Slot);

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected abstract void Dispose(bool isDisposing);

        protected IntPtr GetArrayPtr(byte[] array)
        {
            int size = Marshal.SizeOf(array[0]) * array.Length;

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the array to unmanaged memory.
                Marshal.Copy(array, 0, ptr, array.Length);
            }
            finally 
            {
                // Free the unmanaged memory.
                // Marshal.FreeHGlobal(pnt);
            }

            return ptr;
        }

        protected byte[] GetArrayFromPtr(IntPtr ptr, int length)
        {
            byte[] result = new byte[length];
            try
            {
                Marshal.Copy(ptr, result, 0, length);
            }
            finally
            { 
                _FreePointer(ptr);
            }

            return result; 
        }

        protected IntPtr GetStringPtr(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            return GetArrayPtr(bytes);
        }

        [DllImport("RecastCLI.dll", EntryPoint = "RecastConfigure")]
        protected static extern bool _Configure(uint totalAreas, uint totalLayers);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastInitialize")]
        protected static extern bool _Initialize(uint area, uint layer, bool isTiled);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastDestroy")]
        protected static extern bool _Destroy(uint area, uint layer);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastLoadSettings")]
        protected static extern bool _LoadSettings(uint area, uint layer, IntPtr data, int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastUpdate")]
        protected static extern bool _Update(uint area, uint layer, float delta);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastBuild")]
        protected static extern bool _Build(uint area, uint layer, IntPtr data, int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastLoad")]
        protected static extern bool _Load(uint area, uint layer, IntPtr data, int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastSave")]
        protected static extern bool _Save(uint area, uint layer, out IntPtr data, out int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastFreePointer")]
        protected static extern bool _FreePointer(IntPtr ptr);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastGetDebugNavMesh")]
        protected static extern bool _GetDebugNavMesh(uint area, uint layer, out IntPtr data, out int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastLogBuildTimes")]
        protected static extern bool _LogBuildTimes(uint area, uint layer);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastGetLog")]
        protected static extern bool _GetLog(uint area, uint layer, out IntPtr data, out int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastAddAgent")]
        protected static extern int _AddAgent(uint area, uint layer, IntPtr data, int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastUpdateAgent")]
        protected static extern bool _UpdateAgent(uint area, uint layer, int index, IntPtr data, int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastRemoveAgent")]
        protected static extern bool _RemoveAgent(uint area, uint layer, int index);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastRequestMoveTarget")]
        protected static extern bool _RequestMoveTarget(uint area, uint layer, int index, uint polyRef, float x, float y, float z);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastResetMoveTarget")]
        protected static extern bool _ResetMoveTarget(uint area, uint layer, int index);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastFindNearestPoly")]
        protected static extern bool _FindNearestPoly(uint area, uint layer, float centerX, float centerY, float centerZ, float extendX,
            float extendY, float extendZ, out uint nearestRef, out float nearestX, out float nearestY,
            out float nearestZ);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastGetAgentInfo")]
        protected static extern bool _GetAgentInfo(uint area, uint layer, int index, out IntPtr data, out int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastFindRandomPointAroundCircle")]
        protected static extern bool _FindRandomPointAroundCircle(uint area, uint layer, uint startRef, float centerX, float centerY,
            float centerZ, float maxRadius, out uint randomRef, out float randomPositionX, out float randomPositionY,
            out float randomPositionZ);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastGetPath")]
        protected static extern bool _GetPath(uint area, uint layer, uint startRef, uint endRef,
            float startX, float startY, float startZ,
            float endX, float endY, float endZ,
            bool detailed, out IntPtr data, out int size);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastAddObstacle")]
        protected static extern bool _AddObstacle(uint area, uint layer, float positionX, float positionY, float positionZ, float radius, float height, out uint obstacleRef);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastAddObstacleBox")]
        protected static extern bool _AddObstacleBox(uint area, uint layer, float minX, float minY, float minZ, float maxX, float maxY, float maxZ, out uint obstacleRef);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastRemoveObstacle")]
        protected static extern bool _RemoveObstacle(uint area, uint layer, uint obstacleRef);

        [DllImport("RecastCLI.dll", EntryPoint = "RecastClearObstacles")]
        protected static extern bool _ClearObstacles(uint area, uint layer);
    }
}
