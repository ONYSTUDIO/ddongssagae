using System.Collections;
using System;

namespace DoubleuGames.GameRGD
{
    public class CPriorityByteArray 
    {
        public byte[] ByteArray {get; private set;}
        public int Priority { get; private set; }
        public int CreateTick { get; private set; }
        
        public CPriorityByteArray(byte[] bytes, int priority = 0)
        {
            ByteArray = new byte[bytes.Length];
            Array.Copy (bytes, 0, ByteArray, 0, bytes.Length);
            bytes = null;

            Priority = priority;

            CreateTick = CMiscFunc.SafeGetTimerSecond();
        }
    }

    public class CPriorityByteArrayCompareAsce : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            CPriorityByteArray value1 = (CPriorityByteArray)x;
            CPriorityByteArray value2 = (CPriorityByteArray)y;
            
            if (value1.Priority > value2.Priority)
            {
                return 1;
            }
            
            if (value1.Priority < value2.Priority)
            {
                return -1;
            }

            if (value1.CreateTick > value2.CreateTick)
            {
                return 1;
            }

            if (value1.CreateTick < value2.CreateTick)
            {
                return -1;
            }
            
            return 0;
        }
    }
    
    public class CPriorityByteArrayCompareDesc : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            CPriorityByteArray value1 = (CPriorityByteArray)x;
            CPriorityByteArray value2 = (CPriorityByteArray)y;
            
            if (value1.Priority < value2.Priority)
            {
                return 1;
            }
            
            if (value1.Priority > value2.Priority)
            {
                return -1;
            }

            if (value1.CreateTick < value2.CreateTick)
            {
                return 1;
            }
            
            if (value1.CreateTick > value2.CreateTick)
            {
                return -1;
            }
            
            return 0;
        }
    }
}