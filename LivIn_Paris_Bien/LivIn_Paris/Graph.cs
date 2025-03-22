using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.IO;
using OfficeOpenXml;
using static System.Windows.Forms.LinkLabel;
using System.Diagnostics;
using OfficeOpenXml.Export.HtmlExport.StyleCollectors.StyleContracts;
using System.Collections.Generic;


namespace LivIn_Paris
{
    public class Graph<T> where T : IConvertible
    {
        #region Attribut

        public int Taille { get; set; } 
        public List<Lien<T>> ListeLien = new List<Lien<T>>();
        public List<Noeud<T>> ListeNoeud { get; set; }

        bool AffichageCheminRouge;
        #endregion

        #region Construction du Graph

        /// <summary>
        /// Construit le graph sous la fomre d'une Listes d’adjacence
        /// </summary>
        public Graph(string filePath)
        {
            ListeLien = new List<Lien<T>>();
            ListeNoeud = new List<Noeud<T>>();
            
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {

                #region Creation des Noeud et des Liens

                #region Recuperation des données des Liens
                var worksheet = package.Workbook.Worksheets[1]; // Première feuille (index 0)
                int rowCount = worksheet.Dimension.Rows; // Nombre total de lignes

                for (int row = 2; row <= rowCount; row++) // Parcours des lignes
                {
                    T nom = (T)Convert.ChangeType(worksheet.Cells[row, 2].Text, typeof(T)); // Nom du noeud
                    Noeud<T> Station = TrouverOuCreerNoeud(nom); // Creation du Noeud

                    if (worksheet.Cells[row, 4].Text != "") //Si il y a un noeud suivany
                    {
                        T Suivant = (T)Convert.ChangeType(worksheet.Cells[row, 4].Text, typeof(T)); // nom du neud suivant
                        Noeud<T> StationSuivante = TrouverOuCreerNoeud(Suivant); // Creation du Noeud
                        int poid = Convert.ToInt32(worksheet.Cells[row, 5].Text); // poid du lien

                        bool LienOrienté = false;

                        string Ligne = worksheet.Cells[row, 5].Text;

                        if (worksheet.Cells[row, 6].Text == "orienté")
                        {
                            LienOrienté = true;
                        }
                        AjouterLien(Station, StationSuivante, poid, LienOrienté, Ligne); // Creation du lien
                    }
                }
                #endregion

                #region Recuperation des données des Stations 
                worksheet = package.Workbook.Worksheets[0]; 
                rowCount = worksheet.Dimension.Rows; 

                for (int row = 2; row <= rowCount; row++) 
                {
                    T nom = (T)Convert.ChangeType(worksheet.Cells[row, 3].Text, typeof(T));
                    Noeud<T> Station = TrouverOuCreerNoeud(nom);

                    if (!Station.Ligne.Contains(worksheet.Cells[row, 2].Text)) Station.Ligne.Add(worksheet.Cells[row, 2].Text);
                    Station.IdStation.Add(Convert.ToInt32(worksheet.Cells[row, 1].Text) - 1);
                    Station.Longitude = Convert.ToDouble(worksheet.Cells[row, 4].Text);
                    Station.Latitude = Convert.ToDouble(worksheet.Cells[row, 5].Text);
                    Station.CalculTempsChangement();
                }
                #endregion

                #endregion


            }

        }

        public Graph(List<Lien<T>> LL, List<Noeud<T>> LN, bool Rouge)
        {
            ListeLien = LL;
            ListeNoeud = new List<Noeud<T>>();

            foreach (Lien<T> L in LL) 
            {
                if (!ExistanceNoeud(L.Debut)) ListeNoeud.Add(L.Debut);
                if (!ExistanceNoeud(L.Fin)) ListeNoeud.Add(L.Fin);
            }
            AffichageCheminRouge = Rouge;
        }

        /// <summary>
        ///  Parcours la liste des Noeuds pour voir si le noeud recherché y figure et le créé si il ne le trouve pas
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public Noeud<T> TrouverOuCreerNoeud(T nom)
        {
            foreach (Noeud<T> noeud in ListeNoeud)
            {
                if (noeud.Nom.Equals(nom))
                   
                return noeud;
            }
            Noeud<T> nouveau = new Noeud<T>(nom);
            ListeNoeud.Add(nouveau);
            return nouveau;
        }

        public bool ExistanceNoeud(Noeud<T> NoeudRef)
        {
            foreach (Noeud<T> noeud in ListeNoeud)
            {
                if (noeud.Nom.Equals(NoeudRef))

                    return true;
            }
            return false;
        }

        /// <summary>
        /// Créé un lien entre deux noeuds
        /// </summary>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        public void AjouterLien(Noeud<T> debut, Noeud<T> fin, int poid, bool LienOrienté, string Ligne)
        {
            Lien<T> lien = new Lien<T>(debut, fin, LienOrienté, Ligne);
            ListeLien.Add(lien);
            if (!LienOrienté)
            {

                ListeLien.Add(lien.Inverse());     
            }
            Taille++;
        }
        #endregion


        #region Affichage du graphique
        /// <summary>
        /// Implemente le dessin du graph dans le panel panelAffichage 
        /// </summary>          
        public void AfficherGraphique(Panel panelAffichage)
        {
            panelAffichage.Paint += new PaintEventHandler(DessinerGraphique);
        }

        /// <summary>
        /// Dessine et place les noeuds et leurs liens en utilisant System.Drawing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DessinerGraphique(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black, 2);

            int TailleDesNoeuds = 25;

            Point center = new Point(1920 / 2-35, 1080 / 2-50);

            Dictionary<Noeud<T>, Point> positions = new Dictionary<Noeud<T>, Point>();
            for (int i = 0; i < ListeNoeud.Count; i++)
            {
                int x = (int)(center.X + ListeNoeud[i].Localisation.X*0.0093);
                int y = (int)(center.Y + ListeNoeud[i].Localisation.Y*- 0.0015);
                positions[ListeNoeud[i]] = new Point(x, y);
            }

            // Dessiner les liens
            foreach (var lien in ListeLien)
            {
                int tailleTrait = 10;
                
                
                string Ligne = "";
                foreach (string nomLigneDebut in lien.Debut.Ligne)
                {
                    foreach (string nomLigneFin in lien.Fin.Ligne)
                    if (nomLigneDebut == nomLigneFin) Ligne = nomLigneFin;
                }

                #region Reglage des couleurs
                if (AffichageCheminRouge) pen = new Pen(Color.Red, tailleTrait);
                else if (Ligne == "1") pen = new Pen(Color.Yellow, tailleTrait);
                else if (Ligne == "2") pen = new Pen(Color.DodgerBlue, tailleTrait);
                else if (Ligne == "3") pen = new Pen(Color.Olive, tailleTrait);
                else if (Ligne == "4") pen = new Pen(Color.MediumOrchid, tailleTrait);
                else if (Ligne == "5") pen = new Pen(Color.Coral, tailleTrait);
                else if (Ligne == "6" || Ligne == "7bis") pen = new Pen(Color.MediumAquamarine, tailleTrait);
                else if (Ligne == "7") pen = new Pen(Color.LightPink, tailleTrait);
                else if (Ligne == "8") pen = new Pen(Color.Plum, tailleTrait);
                else if (Ligne == "9") pen = new Pen(Color.YellowGreen, tailleTrait);
                else if (Ligne == "10") pen = new Pen(Color.Goldenrod, tailleTrait);
                else if (Ligne == "11") pen = new Pen(Color.Sienna, tailleTrait);
                else if (Ligne == "12") pen = new Pen(Color.ForestGreen, tailleTrait);
                else if (Ligne == "13" || Ligne == "3bis") pen = new Pen(Color.SkyBlue, tailleTrait);
                else if (Ligne == "14") pen = new Pen(Color.BlueViolet, tailleTrait);

                #endregion


                Point start = positions[lien.Debut];

                if (lien.Orienté)
                {
                    Pen fleche = new Pen(Color.Gray, 8);
                    fleche.CustomEndCap = new AdjustableArrowCap(3, 2);
                    //pen.EndCap = LineCap.ArrowAnchor;
                    Point endMilieu = new Point(positions[lien.Fin].X + (start.X-positions[lien.Fin].X)*2/5, positions[lien.Fin].Y + (start.Y - positions[lien.Fin].Y) * 2 / 5);
                    g.DrawLine(fleche, start, endMilieu);
                }
                
                pen.EndCap = LineCap.NoAnchor;

                
                Point end = positions[lien.Fin];
                g.DrawLine(pen, start, end);
            }


            Brush brush;
            Color couleur = new Color();
            System.Drawing.Font font = new System.Drawing.Font("Arial", 10);

            for (int i = 0; i< ListeNoeud.Count;i++)
            {
                #region Reglage des couleurs
                if (AffichageCheminRouge) brush = Brushes.Red;
                else if (ListeNoeud[i].Ligne.Count > 1) brush = Brushes.LightGray;
                else if (ListeNoeud[i].Ligne[0] == "1") brush = Brushes.Yellow;
                else if (ListeNoeud[i].Ligne[0] == "2") brush = Brushes.DodgerBlue;
                else if (ListeNoeud[i].Ligne[0] == "3") brush = Brushes.Olive;
                else if (ListeNoeud[i].Ligne[0] == "4") brush = Brushes.MediumOrchid;
                else if (ListeNoeud[i].Ligne[0] == "5") brush = Brushes.Coral;
                else if (ListeNoeud[i].Ligne[0] == "6" || ListeNoeud[i].Ligne[0] == "7bis") brush = Brushes.MediumAquamarine;
                else if (ListeNoeud[i].Ligne[0] == "7") brush = Brushes.LightPink;
                else if (ListeNoeud[i].Ligne[0] == "8") brush = Brushes.Plum;
                else if (ListeNoeud[i].Ligne[0] == "9") brush = Brushes.YellowGreen;
                else if (ListeNoeud[i].Ligne[0] == "10") brush = Brushes.Goldenrod;
                else if (ListeNoeud[i].Ligne[0] == "11") brush = Brushes.Sienna;
                else if (ListeNoeud[i].Ligne[0] == "12") brush = Brushes.ForestGreen;
                else if (ListeNoeud[i].Ligne[0] == "13" || ListeNoeud[i].Ligne[0] == "3bis") brush = Brushes.SkyBlue; // test
                else if (ListeNoeud[i].Ligne[0] == "14") brush = Brushes.BlueViolet ;
                else brush = Brushes.LightGreen;
                #endregion

                Point p = positions[ListeNoeud[i]];
                
                g.FillEllipse(brush, p.X - TailleDesNoeuds / 2, p.Y - TailleDesNoeuds / 2, TailleDesNoeuds, TailleDesNoeuds);

                // Dessiner le numéro du sommet à côté du nœud
                string nomStation = ListeNoeud[i].Nom.ToString();
                string[] nomAbreger = nomStation.Split(' ', '\'');
                List<string> nomAbregerListe = new List<string>();
                string txt = "";
                foreach (string s in nomAbreger)
                {
                    if (s == "Porte") nomAbregerListe.Add("P");
                    else if (s.Length > 2) nomAbregerListe.Add(s);
                }
                foreach (string s in nomAbregerListe)
                {
                    if (s.Length > 4) txt += s.Substring(0, 4);
                    else txt += s.Substring(0, s.Length);
                    txt += " ";
                }
                    
                SizeF stringSize = g.MeasureString(txt, font);
                PointF textPosition = new PointF(p.X - stringSize.Width / 2, p.Y - stringSize.Height / 2);
                g.DrawString(txt, font, Brushes.Black, textPosition);
            }
        }

        #endregion
       
        public Graph<T> Dijkstra(T NoeudA, T NoeudB, bool CouleurRouge)
        {
            Dictionary<Noeud<T>, int> Distance = new Dictionary<Noeud<T>, int>();
            Dictionary<Noeud<T>, Noeud<T>> Precedent = new Dictionary<Noeud<T>, Noeud<T>>();
            Dictionary<Noeud<T>, bool> DejaEtudié = new Dictionary<Noeud<T>, bool>();



            foreach (Noeud<T> N in ListeNoeud)
            {
                Distance[N] = int.MaxValue;
                Precedent[N] = null;
                DejaEtudié[N] = false;
            }

            Noeud<T> Depart = null;
            Noeud<T> Fin = null;


            List<Lien<T>> Chemin = new List<Lien<T>>();
            foreach (Noeud<T> N in ListeNoeud)
            {
                if (N.Nom.Equals(NoeudA)) Depart = N;
                if (N.Nom.Equals(NoeudB)) Fin = N;
            }

            if (Depart!=null && Fin!= null)
            {
                Distance[Depart] = 0;

                List<Noeud<T>> ListePriorité = new List<Noeud<T>>();
                ListePriorité.Add(Depart);

                while (ListePriorité.Count > 0)
                {
                    // Recuperation du Noeud ayant la distance la plus courte
                    ListePriorité.Sort((a, b) => Distance[a].CompareTo(Distance[b]));
                    Noeud<T> NoeudCourant = ListePriorité[0];
                    ListePriorité.RemoveAt(0);
                    DejaEtudié[NoeudCourant] = true;

                    if (NoeudCourant.Equals(Fin))
                    {
                        Noeud<T> Actuel = Fin;
                        Chemin = new List<Lien<T>>();
                        while (Actuel != null && !Actuel.Equals(Depart))
                        {
                            Noeud<T> Prec = Precedent[Actuel];
                            Chemin.Add(new Lien<T>(Prec, Actuel)); 
                            Actuel = Prec;
                        }

                        Chemin.Reverse();
                    }



                    foreach (Lien<T> L in ListeLien)
                    {

                        if (L.Debut.Equals(NoeudCourant) && L.Fin != Precedent[NoeudCourant]  )
                        {
                            int NouvelleDistance = Distance[NoeudCourant] + L.Poid;
                            if (Precedent[NoeudCourant] != null && Precedent[Precedent[NoeudCourant]] != null)
                            {
                                bool memeLigne = false;
                                foreach (string LigneA in NoeudCourant.Ligne)
                                {
                                    foreach (string LigneB in Precedent[Precedent[NoeudCourant]].Ligne)
                                    {
                                        if (LigneA == LigneB) memeLigne = true;
                                    }
                                }
                                if (!memeLigne) NouvelleDistance += Precedent[NoeudCourant].Changement;
                            }

                            if (NouvelleDistance <= Distance[L.Fin])
                            {
                                Distance[L.Fin] = NouvelleDistance;
                                Precedent[L.Fin] = NoeudCourant;
                                if (!ListePriorité.Contains(L.Fin)) ListePriorité.Add(L.Fin);
                            }
                        }

                    }
                }
            }
            if (Chemin != null) return new Graph<T>(Chemin, ListeNoeud, CouleurRouge);
            return null;
        }







    }
}