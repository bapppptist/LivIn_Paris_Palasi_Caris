using System.Drawing.Drawing2D;
using OfficeOpenXml;

namespace LivIn_Paris
{
    public class Graph<T> where T : IConvertible
    {
        //test
        #region Attribut
        public int Taille { get; set; } 
        public List<Lien<T>> ListeLien = new List<Lien<T>>();
        public List<Noeud<T>> ListeNoeud { get; set; }
        string  Couleur;
        #endregion

        #region Construction du Graph
        /// <summary>
        /// Construit le graph sous la forme d'une Listes d’adjacence à partir d'un fichier Exel.
        /// </summary>
        /// <param name="filePath"></param>
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
                    Noeud<T> Station = TrouverOuCréerNœud(nom); // Creation du Noeud

                    if (worksheet.Cells[row, 4].Text != "") //Si il y a un noeud suivany
                    {
                        T Suivant = (T)Convert.ChangeType(worksheet.Cells[row, 4].Text, typeof(T)); // nom du neud suivant
                        Noeud<T> StationSuivante = TrouverOuCréerNœud(Suivant); // Creation du Noeud

                        bool LienOrienté = false;

                        string Ligne = worksheet.Cells[row, 5].Text;

                        if (worksheet.Cells[row, 6].Text == "orienté")
                        {
                            LienOrienté = true;
                        }
                        AjouterLien(Station, StationSuivante, LienOrienté, Ligne); // Creation du lien
                    }
                }
                #endregion

                #region Recuperation des données des Stations 
                worksheet = package.Workbook.Worksheets[0]; 
                rowCount = worksheet.Dimension.Rows; 

                for (int row = 2; row <= rowCount; row++) 
                {
                    T nom = (T)Convert.ChangeType(worksheet.Cells[row, 3].Text, typeof(T));
                    Noeud<T> Station = TrouverOuCréerNœud(nom);

                    if (!Station.Ligne.Contains(worksheet.Cells[row, 2].Text)) Station.Ligne.Add(worksheet.Cells[row, 2].Text);
                    Station.Longitude = Convert.ToDouble(worksheet.Cells[row, 4].Text);
                    Station.Latitude = Convert.ToDouble(worksheet.Cells[row, 5].Text);
                    Station.CalculTempsChangement();
                }
                #endregion
                #endregion

                foreach(Lien<T> L in ListeLien)
                {
                    L.CalculPoid();
                }

                foreach (Noeud<T> N in ListeNoeud)
                {
                    N.CalculTempsChangement();
                }
            }

        }

        /// <summary>
        /// Construit le Graph à partir d'une liste de Liens et d'un booléen déterminant la forme d'affichage.
        /// </summary>
        /// <param name="LL"></param>
        /// <param name="Rouge"></param>
        public Graph(List<Lien<T>> LL, string couleur)
        {
            ListeLien = LL;
            ListeNoeud = new List<Noeud<T>>();

            foreach (Lien<T> L in LL) 
            {
                if (!ExistenceNœud(L.Debut)) ListeNoeud.Add(L.Debut);
                if (!ExistenceNœud(L.Fin)) ListeNoeud.Add(L.Fin);
            }
            Couleur = couleur;
        }

        /// <summary>
        ///  Parcours la liste des Nœuds pour voir si le nœud recherché y figure et le créé s'il ne le trouve pas.
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public Noeud<T> TrouverOuCréerNœud(T nom)
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

        /// <summary>
        /// Détermine si un nœud existe dans ListeNœud.
        /// </summary>
        /// <param name="NœudRef"></param>
        /// <returns></returns>
        public bool ExistenceNœud(Noeud<T> NœudRef)
        {
            foreach (Noeud<T> noeud in ListeNoeud)
            {
                if (noeud.Nom.Equals(NœudRef))

                    return true;
            }
            return false;
        }

        /// <summary>
        /// Créé un lien entre deux nœuds.
        /// </summary>
        /// <param name="début"></param>
        /// <param name="fin"></param>
        public void AjouterLien(Noeud<T> début, Noeud<T> fin, bool LienOrienté, string Ligne)
        {
            Lien<T> lien = new Lien<T>(début, fin, LienOrienté, Ligne);
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
        /// Implémente le dessin du graph dans le panel panelAffichage .
        /// </summary>          
        public void AfficherGraphique(Panel panelAffichage)
        {
            panelAffichage.Paint += new PaintEventHandler(DessinerGraphique);
        }

        /// <summary>
        /// Dessine et place les nœuds et leurs liens en utilisant System.Drawing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DessinerGraphique(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            int TailleDesNoeuds = 25;

            Point center = new Point(1920 / 2-35, 1080 / 2-50);

            Dictionary<Noeud<T>, Point> positions = new Dictionary<Noeud<T>, Point>();
            foreach (Noeud<T> N in ListeNoeud)
            {
                int x = (int)(center.X + N.Localisation.X*0.0093);
                int y = (int)(center.Y + N.Localisation.Y*- 0.0015);
                positions[N] = new Point(x, y);
            }

            #region Affichage des Liens
            foreach (Lien<T> lien in ListeLien)
            {
                int tailleTrait = 10;
                Pen pen = new Pen(Color.Transparent, tailleTrait);

                string Ligne = lien.Ligne;

                #region Reglage des couleurs
                if (Couleur == "Rouge") pen = new Pen(Color.Red, tailleTrait);
                else if (Couleur == "Bleu") pen = new Pen(Color.Blue, tailleTrait);
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

                if (lien.Orienté )
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
            #endregion

            #region Affichage des Noeuds
            Brush brush;

            System.Drawing.Font font = new System.Drawing.Font("Arial", 10);

            foreach (Noeud<T> N in ListeNoeud)
            {
                #region Reglage des couleurs
                if (Couleur == "Rouge") brush = Brushes.Red;
                else if (Couleur == "Bleu") brush = Brushes.Blue;
                else if (N.Ligne == null) brush = Brushes.Red;
                else if (N.Ligne.Count > 1) brush = Brushes.LightGray;
                else if (N.Ligne.Contains( "1")) brush = Brushes.Yellow;
                else if (N.Ligne.Contains("2")) brush = Brushes.DodgerBlue;
                else if (N.Ligne.Contains("3")) brush = Brushes.Olive;
                else if (N.Ligne.Contains("4")) brush = Brushes.MediumOrchid;
                else if (N.Ligne.Contains("5")) brush = Brushes.Coral;
                else if (N.Ligne.Contains("6") || N.Ligne.Contains("7bis")) brush = Brushes.MediumAquamarine;
                else if (N.Ligne.Contains("7")) brush = Brushes.LightPink;
                else if (N.Ligne.Contains("8")) brush = Brushes.Plum;
                else if (N.Ligne.Contains("9")) brush = Brushes.YellowGreen;
                else if (N.Ligne.Contains("10")) brush = Brushes.Goldenrod;
                else if (N.Ligne.Contains("11")) brush = Brushes.Sienna;
                else if (N.Ligne.Contains("12")) brush = Brushes.ForestGreen;
                else if (N.Ligne.Contains("13") || N.Ligne.Contains("3bis")) brush = Brushes.SkyBlue; 
                else if (N.Ligne.Contains("14")) brush = Brushes.BlueViolet;
                else brush = Brushes.Transparent;
                #endregion

                Point p = positions[N];
                
                g.FillEllipse(brush, p.X - TailleDesNoeuds / 2, p.Y - TailleDesNoeuds / 2, TailleDesNoeuds, TailleDesNoeuds);

                string[] nomAbrégé = N.Nom.ToString().Split(' ', '\'');
                List<string> nomAbrégéListe = new List<string>();
                string txt = "";
                foreach (string s in nomAbrégé)
                {
                    if (s == "Porte") nomAbrégéListe.Add("P");
                    else if (s.Length > 2) nomAbrégéListe.Add(s);
                }
                foreach (string s in nomAbrégéListe)
                {
                    if (s.Length > 4) txt += s.Substring(0, 4);
                    else txt += s.Substring(0, s.Length);
                    txt += " ";
                }
                    
                SizeF stringSize = g.MeasureString(txt, font);
                PointF textePosition = new PointF(p.X - stringSize.Width / 2, p.Y - stringSize.Height / 2);
                g.DrawString(txt, font, Brushes.Black, textePosition);
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Trouve le chemin le plus court entre un Nœud A et un Nœud B du graph en suivant l'algorithme de Dijkstra.
        /// </summary>
        /// <param name="NœudA"></param>
        /// <param name="NœudB"></param>
        /// <param name="CouleurRouge"></param>
        /// <returns></returns>
        public Graph<T> Dijkstra(T NœudA, T NœudB, string couleur)
        {
            #region Initialisation
            Dictionary<Noeud<T>, int> Distance = new Dictionary<Noeud<T>, int>();
            Dictionary<Noeud<T>, Noeud<T>> Précèdent = new Dictionary<Noeud<T>, Noeud<T>>();
            Dictionary<List<Lien<T>>, int> TempsChemin = new Dictionary<List<Lien<T>>, int>();
            List<List<Lien<T>>> CheminsTrouvé = new List<List<Lien<T>>>();
            List<Noeud<T>> ListePriorité = new List<Noeud<T>>();

            foreach (Noeud<T> N in ListeNoeud)
            {
                Distance[N] = int.MaxValue;
                Précèdent[N] = null;
            }

            Noeud<T> Départ = TrouverOuCréerNœud(NœudA);
            Noeud<T> Fin = TrouverOuCréerNœud(NœudB);

            if (Départ.Longitude == 0 || Fin.Longitude == 0) return null;

            Distance[Départ] = 0; 
            ListePriorité.Add(Départ);
            #endregion

            while (ListePriorité.Count > 0)
            {
                #region Recuperation du Noeud Aillant le chemin le moins long
                ListePriorité.Sort((a, b) => Distance[a].CompareTo(Distance[b]));
                Noeud<T> NœudCourant = ListePriorité[0];
                ListePriorité.RemoveAt(0);
                #endregion

                #region Constitution du graph representant un chemin lorsque le noeud d'arrivé est atteind
                if (NœudCourant.Equals(Fin))
                {
                    int Temps = Distance[Fin];
                    List<Lien<T>> Chemin = new List<Lien<T>>();
                    Noeud<T> Actuel = Fin;

                    while (Actuel != null && !Actuel.Equals(Départ))
                    {
                        Noeud<T> Prec = Précèdent[Actuel];
                        Lien<T> lien = new Lien<T>(Prec, Actuel); 
                        foreach (string ligne in Prec.Ligne)
                        {
                            if (Actuel.Ligne.Contains(ligne))lien.Ligne = ligne;
                        }
                        Chemin.Add(lien);

                        Actuel = Prec;
                    }

                    #region Prise en compte des changment de ligne sur le chemin trouvé
                    for (int i = 0; i < Chemin.Count - 1; i++)
                    {
                        if (Chemin[i].Ligne != Chemin[i + 1].Ligne) Temps += Chemin[i].Fin.Changement;
                    }
                    #endregion

                    //Chemin.Reverse(); à changer si l'on veut afficher le chemin qq part
                    TempsChemin[Chemin] = Temps;
                    CheminsTrouvé.Add(Chemin);
                }
                #endregion

                #region Alogithme de Dijkstra
                foreach (Lien<T> L in ListeLien)
                {
                    int NouvelleDistance = Distance[NœudCourant] + L.Poid;
                    if (L.Debut.Equals(NœudCourant) && NouvelleDistance < Distance[L.Fin])
                    {
                            Distance[L.Fin] = NouvelleDistance;
                            Précèdent[L.Fin] = NœudCourant;
                            ListePriorité.Add(L.Fin);
                    }
                }
                #endregion
            }

            if (CheminsTrouvé == null ) return null;

            if (TempsChemin.MinBy(kvp => kvp.Value).Value > 1000) return null;
            return new Graph<T>(TempsChemin.MinBy(kvp => kvp.Value).Key, couleur); // on choisit le chemin le plus court parmi tous les chemins trouvé
        }

        /// <summary>
        /// Trouve le chemin le plus court entre un Nœud A et un Nœud B du graph en suivant l'algorithme de BellmanFord.
        /// </summary>
        /// <param name="NœudA"></param>
        /// <param name="NœudB"></param>
        /// <param name="CouleurRouge"></param>
        /// <returns></returns>
        public Graph<T> BellmanFord(T NœudA, T NœudB, string couleur)
        {
            #region Initialisation
            Dictionary<Noeud<T>, int> Distance = new Dictionary<Noeud<T>, int>();
            Dictionary<Noeud<T>, Noeud<T>> Précédent = new Dictionary<Noeud<T>, Noeud<T>>();
            Noeud<T> Départ = TrouverOuCréerNœud(NœudA);
            Noeud<T> Fin = TrouverOuCréerNœud(NœudB);

            foreach (Noeud<T> N in ListeNoeud)
            {
                Distance[N] = int.MaxValue;
                Précédent[N] = null;
            }

            Distance[Départ] = 0;
            #endregion

            #region Algorithme de BellmanFord
            for (int i = 0; i < ListeNoeud.Count -1; i++)
            {
                foreach(Noeud<T> N in ListeNoeud)
                {
                    foreach(Lien<T> L in ListeLien)
                    {
                        if (L.Debut == N && Distance[L.Debut]!= int.MaxValue)
                        {
                            int NouvelleDistance = Distance[N] + L.Poid;

                            if (Précédent[N] != null)
                            {
                                bool MêmeLigne = false;
                                foreach (string ligne in Précédent[N].Ligne)
                                {
                                    if (L.Fin.Ligne.Contains(ligne)) MêmeLigne = true;
                                }
                                if (!MêmeLigne) NouvelleDistance += N.Changement;
                            }

                            if (NouvelleDistance < Distance[L.Fin])
                            {
                                
                                Distance[L.Fin] = NouvelleDistance;
                                Précédent[L.Fin] = N;
                            }
                        }
                    }
                }
            }
            #endregion

            Noeud<T> NœudCourant = Fin;
            List<Lien<T>> Chemin = new List<Lien<T>>();
            while (Précédent[NœudCourant] != null && NœudCourant != Départ && Précédent[NœudCourant]!= NœudCourant)
            {
                

                Lien<T> lien = new Lien<T>(Précédent[NœudCourant], NœudCourant);
                foreach (string ligne in Précédent[NœudCourant].Ligne)
                {
                    if (NœudCourant.Ligne.Contains(ligne)) lien.Ligne = ligne;
                }
                Chemin.Add(lien);

                NœudCourant = Précédent[NœudCourant];
            }

            if (Chemin != null) return new Graph<T>(Chemin, couleur);
            
            return null;
        }
    }
}