using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivIn_Paris
{
    public class Lien<T> where T : IConvertible
    {
        Noeud<T> debut;
        Noeud<T> fin;
        int poid;
        bool orienté;

        string ligne;

        public Lien(Noeud<T> d, Noeud<T> f, bool o, string l)
        {
            debut = d;
            Fin = f;
            CalculPoid();
            orienté = o;
            ligne = l;
        }

        public Lien(Noeud<T> d, Noeud<T> f, bool o, int p)
        {
            debut = d;
            Fin = f;
            poid = p;
            orienté = o;
        }
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

        public int Poid
        {
            get { return poid; }
            set { poid = value; }
        }

        public string Ligne
        {
            get { return ligne; }
            set { ligne = value; }
        }

        public bool Orienté
        {
            get { return orienté; }
            set { orienté = value; }
        }

        public Lien<T> Inverse()
        {
            return new Lien<T>(fin, debut, orienté, poid);
        }

        private void CalculPoid()
        {
            int R = 6371;

            poid = (int)(2 * Math.Asin( Math.Sqrt( Math.Pow(Math.Sin((Fin.Latitude - Debut.Latitude) / 2), 2) + Math.Cos(Debut.Latitude) * Math.Cos(Fin.Latitude) * Math.Pow(Math.Sin((Fin.Longitude - Debut.Longitude) / 2), 2)))/ 30);// le metro a une V moyenne de 600m/ minute
            if (poid < 0) poid = poid * (-1);
        }
    }
}
