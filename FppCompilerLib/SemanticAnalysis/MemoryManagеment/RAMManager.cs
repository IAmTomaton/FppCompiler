namespace FppCompilerLib.SemanticAnalysis.MemoryManagement
{
    internal class RAMManager
    {
        private readonly int memoryOffset;
        private readonly SortedList<int, MemorySegment> memory = new();
        private int UpperLimit => memory.Count > 0 ? memory.Last().Value.EndSegment : memoryOffset;

        public RAMManager(int memoryOffset)
        {
            this.memoryOffset = memoryOffset;
        }

        public int Allocate(int size)
        {
            var address = FindFreeSegment(size);
            memory.Add(address, new MemorySegment { address = address, size = size });
            return address;
        }

        public void FreeUp(int address)
        {
            memory.Remove(address);
        }

        public RAMManager GetChild()
        {
            return new RAMManager(UpperLimit);
        }

        private int FindFreeSegment(int size)
        {
            if (memory.Count == 0) return UpperLimit;
            return memory.Values
                .Zip(memory.Values.Skip(1))
                .FirstOrDefault(t => t.First.EndSegment + size <= t.Second.address,
                    (First: memory.Values.Last(), Second: memory.Values.Last()))
                .First.EndSegment;
        }

        private struct MemorySegment
        {
            public int address;
            public int size;
            public readonly int EndSegment => address + size;
        }
    }
}
