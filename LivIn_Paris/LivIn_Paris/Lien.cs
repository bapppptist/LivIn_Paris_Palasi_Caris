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
            orienté = o;
            ligne = l;
        }

        public Lien(Noeud<T> d, Noeud<T> f, bool o, int p,string l)
        {
            debut = d;
            Fin = f;
            poid = p;
            orienté = o;
            ligne = l;
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
            return new Lien<T>(fin, debut, orienté, poid, ligne);
        }

        public void CalculPoid()
        {
            int R = 6371000;

            double Sin2Latitude = Math.Sin((Fin.Latitude - Debut.Latitude) / 2) * Math.Sin((Fin.Latitude - Debut.Latitude) / 2);
            double cosLat1 = Math.Cos(Fin.Latitude);
            double cosLat2 = Math.Cos(Debut.Latitude);
            double Sin2Longitude = Math.Sin((Fin.Longitude - Debut.Longitude)) * Math.Sin((Fin.Longitude - Debut.Longitude));

            poid = (int)( 2 * R * Math.Asin(Math.Sqrt(Sin2Latitude + cosLat1 * cosLat2 * Sin2Longitude))/1000);


        }
    }
}
