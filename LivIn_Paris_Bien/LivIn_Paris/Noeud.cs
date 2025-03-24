using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivIn_Paris
{
    public class Noeud<T> where T : IConvertible
    {
        T nom;
        Noeud<T> pre;
        double longitude;
        double latitude;
        List<string> ligne;
        List<int> idStation;
        Point localisation;
        int changement;

        public Noeud(T n)
        {
            nom = n;
            pre = null;
            longitude = 0;
            latitude = 0;
            ligne= new List<string>();
            idStation= new List<int>();
            
        }
        

         public T Nom
        { 
            get { return nom; }
            set { nom = value; }
        }

        public Noeud<T> Pre
        {
            get { return pre; }
            set { pre = value; }
        }


        public double Latitude
        {
            set 
            { 
                latitude = value;
                localisation = Convertion();
            }
            get { return latitude; }
        }

        public double Longitude
        {
            set 
            { 
                longitude = value;
                localisation = Convertion();
            }
            get { return longitude; }
        }

        public Point Localisation
        { get { return localisation; } }

        public List<string> Ligne
        {
            set {  ligne = value; }
            get { return ligne; } 
        }

        public List<int> IdStation
        {
            set { idStation = value; }
            get { return idStation; } 
        }

        public int Changement
        {
            get { return changement; }
            set { changement = value; } 
        }

        public Point Convertion()
        {
            double R = 6371000;
            double LatiReference = 48.85695346;
            double LongiReference = 2.348160991;
            int x = Convert.ToInt32((longitude - LongiReference) * Math.Cos(LatiReference) * R);
            if (nom.ToString() == "Château de Vincennes") x -= 15000; 
            int y = Convert.ToInt32((latitude - LatiReference) * R);

            return new Point(x,y);
        }

        public void CalculTempsChangement()
        {

            if (nom.ToString().Length > 4 && nom.ToString().Substring(0, 4) == "Gare") changement = 15;
            else if (ligne.Count != 1)
            {
                changement = 5 * ligne.Count;
            }
            else changement = 0;
        }

    }
}
