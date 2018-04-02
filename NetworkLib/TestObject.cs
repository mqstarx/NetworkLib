using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    [Serializable]
    public class TestObject
    {
        private int[] m_Array;
        public TestObject()
        {
            // Array = new int[1024*1024*10];
            Array = new int[10000];
            for (int i=0;i<m_Array.Length;i++)
            {
                m_Array[i] = new Random((int)DateTime.Now.Ticks).Next();
            }
        }

        public int[] Array
        {
            get
            {
                return m_Array;
            }

            set
            {
                m_Array = value;
            }
        }
        public override string ToString()
        {
            return "Массив:" + m_Array.Length.ToString();
        }
    }
}
