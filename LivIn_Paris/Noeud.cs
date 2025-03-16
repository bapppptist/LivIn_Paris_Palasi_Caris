using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Karaté
{
    public class Noeud<T>
    {
        T num;
        Noeud<T> pre;
       

        public Noeud(T a)
        {
            num = a;
            pre = null;
        }
            
        public T Num
        { 
            get { return num; }
            set { num = value; }
        }

        public Noeud<T> Pre
        {
            get { return pre; }
            set { pre = value; }
        }

    }
}
