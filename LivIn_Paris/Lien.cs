using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Karaté
{
    public class Lien<T>
    {
        Noeud<T> debut;
        Noeud<T> fin;

        public Lien(Noeud<T> d, Noeud<T> f)
        {
            debut = d;
            Fin = f;
        }

        public Noeud<T> Debut
        {
            get { return debut; }
            set { debut = value; }
        }

        public Noeud<T> Fin
        {
            get { return fin; }
            set { fin = value; }
        }
        public string toString() //Utile Uniquement pour le test Unitaire
        {
            return this.debut + "->" + this.fin;
        }


    }
}
