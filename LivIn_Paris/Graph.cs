
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

namespace Graph_Karaté
{
    public class Graph<T>
    {
        #region Attribut
        public int Ordre { get; set; }
        public int Taille { get; set; } 
        public List<Lien<T>> ListeLien = new List<Lien<T>>();
        public bool[,] MatriceAdjacence { get; set; }
        public List<Noeud<T>> ListeNoeud { get; set; }
        public Form form { get; set; }
        #endregion

        #region Construction du Graph

        /// <summary>
        ///  Construit le graph de 2 maniere differente : Listes d’adjacence et Matrice d’adjacence
        /// </summary>
        /// <param name="chaine"></param>
        public Graph(string chaine, Form form) // chaine doit etre sous la forme : "num Noeud, num Noeud , ... , num Neud"
        {
            ListeLien = new List<Lien<T>>();
            ListeNoeud = new List<Noeud<T>>();
            string[] elements = chaine.Split(',');

            #region Liste d'adjacence
            for (int i = 0; i < elements.Length; i += 2)
            {
                if (int.TryParse(elements[i], out int debutNum) && int.TryParse(elements[i + 1], out int finNum))
                {
                    Noeud debut = TrouverOuCreerNoeud(debutNum);
                    Noeud fin = TrouverOuCreerNoeud(finNum);

                    AjouterLien(debut, fin);
                }
            }
            #endregion
            
            #region Matrice d'adjacence
            CalculerOrdre();
            MatriceAdjacence = new bool[Ordre, Ordre];
            for (int i = 0; i < elements.Length; i += 2)
            {
                if (int.TryParse(elements[i], out int debutNum) && int.TryParse(elements[i + 1], out int finNum))
                {
                    MatriceAdjacence[debutNum - 1, finNum - 1] = true;
                    MatriceAdjacence[finNum - 1, debutNum - 1] = true;
                }
            }
            #endregion

            ListeNoeud.Sort((a, b) => a.Num.CompareTo(b.Num));
            this.form = form;
        }

        /// <summary>
        ///  Parcours la liste des Noeuds pour voir si le noeud recherché y figure et le créé si il ne le trouve pas
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Noeud<int> TrouverOuCreerNoeud(int num)
        {
            foreach (var noeud in ListeNoeud)
            {
                if (noeud.Num == num)
                    return noeud;
            }
            Noeud<int> nouveau = new Noeud<int>(num);
            ListeNoeud.Add(nouveau);
            return nouveau;
        }

        /// <summary>
        /// Créé un lien entre deux noeuds
        /// </summary>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        public void AjouterLien(Noeud debut, Noeud fin)
        {
            Lien lien = new Lien(debut, fin);
            ListeLien.Add(lien);
            lien = new Lien(fin, debut); // On créer aussi le lien inverse car le cas du graph "karate" represente des liens d'amitié et n'est donc pas orienté. 
            ListeLien.Add(lien);         // Le cas des stations de métros que l'on étudira par la suite ne le sera pas non plus.
            Taille++;
        }

        #endregion

        #region Propriété
        /// <summary>
        /// Calcule le nombre de noeud présent dans le graph
        /// </summary>
        public void CalculerOrdre()
        {
            Ordre = ListeNoeud.Count;
        }
        
        /// <summary>
        /// Determine si le graph est orienté ou non
        /// </summary>
        /// <returns></returns>
        public bool EstOriente()
        {
            foreach (Lien L in ListeLien)
            {
                Lien Linverse = new Lien(L.Fin, L.Debut);
                bool InverseTrouvé = false;
                foreach (Lien l in ListeLien)
                {
                    if (l == Linverse) InverseTrouvé = true;
                }
                if (!InverseTrouvé) return false;
            }

            return false;
        }

        /// <summary>
        /// retourne le nombre de cycle (ou circuits) présents dans le graph
        /// </summary>
        /// <returns></returns>
        public List<Noeud> Cycle()
        {
            List<Noeud> ListeNoeudCycle = new List<Noeud>();
            List<Noeud> DFS = RechercheDFS(1);
            for (int i = 0; i < Ordre; i++)
            {
                for (int j = Ordre - 1; j >= 0; j--)
                {
                    if (i != j)
                    {
                        for (int compteur = 0; compteur < Successeur(DFS[j], true).Count; compteur++)
                        {
                            if (DFS[i] == Successeur(DFS[j], true)[compteur] && DFS[i] != DFS[j].Pre)
                            {

                                ListeNoeudCycle.Add(Successeur(DFS[j], true)[compteur]);
                                while (DFS[j] != null)
                                {
                                    ListeNoeudCycle.Add(DFS[j]);
                                    DFS[j] = DFS[j].Pre;
                                }
                                ListeNoeudCycle.Add(DFS[i]);
                                if (ListeNoeudCycle.Count > 3)

                                    return ListeNoeudCycle;
                                ListeNoeudCycle = new List<Noeud>();


                            }
                        }

                    }
                }

            }
            return ListeNoeudCycle;


        }
        #endregion

        #region Affichage
        /// <summary>
        /// Ecrit chaque lien et les renvoie sous la forme d'une liste.
        /// </summary>
        /// <returns></returns>
        public List<string> AfficherListeAdj()
        {
            List<string> txt = new List<string>();          
            foreach (Lien L in ListeLien)
            {
                if (!txt.Contains("Lien : " + L.Fin.Num + " - " + L.Debut.Num + "\n"))
                {
                    txt.Add("Lien : "+L.Debut.Num+" - "+L.Fin.Num+"\n");
                }
            }
            return txt;
        }
        
        /// <summary>
        /// Affiche la Matrice d'adjacence et la renvoie sous forme d'un string
        /// </summary>
        /// <returns></returns>
        public string AfficherMatAdj()
        {
            string txt = "  ";
            for (int i = 0; i < Ordre; i++)
            { 
                if (i<10) txt += " ";
                txt += i+" "; 
            }
            txt += "\n";
            for (int i = 0; i < Ordre; i++)
            {
                if (i < 10) txt += " ";
                txt += i ;
                
                for (int j = 0; j < Ordre; j++)
                {
                    if (MatriceAdjacence[i, j])
                        txt+= " 1 ";
                    else txt += "   ";
                }
                txt += "\n";
                
            }
            return txt;
        }

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
            Pen pen = new Pen(System.Drawing.Color.Black, 1);
            int nodeSize = 30;

            double angleStep = 360.0 / Ordre;
            int radius = 400;
            Point center = new Point((1400 * 5 / 6 - 100) / 2, 900 / 2);

            Dictionary<Noeud, Point> positions = new Dictionary<Noeud, Point>();
            for (int i = 0; i < ListeNoeud.Count; i++)
            {
                double angle = i * angleStep;
                int x = (int)(center.X + radius * Math.Cos(angle * Math.PI / 180));
                int y = (int)(center.Y + radius * Math.Sin(angle * Math.PI / 180));
                positions[ListeNoeud[i]] = new Point(x, y);
            }

            // Dessiner les liens
            foreach (var lien in ListeLien)
            {
                Point start = positions[lien.Debut];
                Point end = positions[lien.Fin];
                g.DrawLine(pen, start, end);
            }

            // Dessiner les nœuds et ajouter leurs numéros
            Brush brush = Brushes.Gray;
            System.Drawing.Font font = new System.Drawing.Font("Arial", 10);

            for (int i = 0; i< ListeNoeud.Count;i++)
            {
                Point p = positions[ListeNoeud[i]];
                
                g.FillEllipse(brush, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize);

                // Dessiner le numéro du sommet à côté du nœud
                string numero = ListeNoeud[i].Num.ToString();
                SizeF stringSize = g.MeasureString(numero, font);
                PointF textPosition = new PointF(p.X - stringSize.Width / 2, p.Y - stringSize.Height / 2);
                g.DrawString(numero, font, Brushes.Black, textPosition);

                
                

            }
        }

        #endregion
         #endregion

            #region Parcours du graph
            /// <summary>
            /// Cherche dans la ListeLien tous les Noeuds relié au Noeud étudié "N"
            /// </summary>
            /// <param name="N"></param>
            /// <param name="Ocroissant"></param>
            /// <returns></returns>
            public List<Noeud> Successeur(Noeud N, bool Ocroissant)
        {
            List<Noeud> successeur = new List<Noeud>();
            foreach (Lien l in ListeLien)
            {
                if (l.Debut == N)
                {
                    successeur.Add(l.Fin);
                }
            }

            if (Ocroissant) successeur.Sort((a, b) => a.Num.CompareTo(b.Num));
            else successeur.Sort((a, b) => b.Num.CompareTo(a.Num));
            return successeur;
        }

        /// <summary>
        /// Methode de Recherche en largeur BFS 
        /// </summary>
        /// <returns></returns>
        public List<Noeud> RechercheBFS(int numNoeudDebut)
        {
            for (int i = 0; i < Ordre; i++)
            {
                ListeNoeud[i].Pre = null;
            }

            Queue<Noeud> fileBFS = new Queue<Noeud>();
            List<Noeud> NoeudAppercu = new List<Noeud>();
            List<Noeud> ListeRechercheBFS = new List<Noeud>();

            for (int i = 0; i < Ordre; i++)
            {
                if (!NoeudAppercu.Contains(ListeNoeud[i]))
                {
                    if (ListeRechercheBFS.Count > 0) fileBFS.Enqueue(ListeNoeud[i]);
                    else fileBFS.Enqueue(ListeNoeud[numNoeudDebut-1]);

                    while (fileBFS.Count > 0)
                    {
                        Noeud NoeudSuivant = fileBFS.Dequeue();
                        NoeudAppercu.Add(NoeudSuivant);
                        ListeRechercheBFS.Add(NoeudSuivant);

                        foreach (Noeud SuccesseurN in Successeur(NoeudSuivant, true))
                        {
                            if (!NoeudAppercu.Contains(SuccesseurN))
                            {
                                SuccesseurN.Pre = NoeudSuivant;
                                fileBFS.Enqueue(SuccesseurN);
                                NoeudAppercu.Add(SuccesseurN);
                            }
                        }
                    }
                }

            }
            return ListeRechercheBFS;
        }

        /// <summary>
        /// Methode de Recherche en profondeur DFS 
        /// </summary>
        /// <returns></returns>
        public List<Noeud> RechercheDFS(int numNoeudDebut)
        {
            for (int i = 0; i<Ordre;i++)
            {
                ListeNoeud[i].Pre = null;
            }


            Stack<Noeud> PileDFS = new Stack<Noeud>();
            List<Noeud> NoeudAppercu = new List<Noeud>();
            List<Noeud> ListeRechercheDFS = new List<Noeud>();

            for (int i = 0; i < Ordre; i++)
            {
                if (!NoeudAppercu.Contains(ListeNoeud[i]))
                {
                    if (PileDFS.Count > 0) PileDFS.Push(ListeNoeud[i]);
                    else PileDFS.Push(ListeNoeud[numNoeudDebut - 1]);

                    while (PileDFS.Count > 0)
                    {
                        Noeud NoeudSuivant = PileDFS.Pop();
                        if (!NoeudAppercu.Contains(NoeudSuivant)) NoeudAppercu.Add(NoeudSuivant);
                        ListeRechercheDFS.Add(NoeudSuivant);

                        foreach (Noeud SuccesseurN in Successeur(NoeudSuivant, false))
                        {
                            if (!NoeudAppercu.Contains(SuccesseurN))
                            {
                                SuccesseurN.Pre = NoeudSuivant;
                                PileDFS.Push(SuccesseurN);
                                NoeudAppercu.Add(SuccesseurN);
                            }
                        }
                    }
                }
            }
            return ListeRechercheDFS;
        }

        
        #endregion




    }
}
