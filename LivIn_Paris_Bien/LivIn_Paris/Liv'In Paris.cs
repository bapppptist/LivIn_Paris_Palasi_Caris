using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LivIn_Paris
{
    public partial class Liv_In_Paris : Form
    {


        

        public Liv_In_Paris()
        {
            InitializeComponent();

            int WpanelAffichage = 1720;
            int HpanelAffichage = 880;
            Panel panelAffichage = new Panel { Width = WpanelAffichage, Height = HpanelAffichage, Location = new Point((1920 - WpanelAffichage) /2, (1080- HpanelAffichage) /2), BorderStyle = BorderStyle.FixedSingle};
            
            typeof(Panel).InvokeMember("DoubleBuffered",System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,null, panelAffichage, new object[] { true });

            Width = 1920;
            Height = 1080;

            Label Titre = new Label { Text = "Liv'In Paris\nPalasi & Caris", AutoSize = true, Font = new Font("Arial", 25, FontStyle.Bold) };
            Titre.Location = new Point((Width - Titre.Width) / 2, 20);
            Controls.Add(Titre);
            #region Graph
            Graph<string> g = new Graph<string>(@"MetroParis1.xlsx");

            g.AfficherGraphique(panelAffichage);

            #region Boutons pour les Noeuds de Départ et d'Arrivé
            System.Windows.Forms.TextBox TextBoxNoeudA = new System.Windows.Forms.TextBox { Location = new Point(120,50) , Width = 120, Height = 30 };
            System.Windows.Forms.TextBox TextNoeudA = new System.Windows.Forms.TextBox { Location = new Point(TextBoxNoeudA.Location.X - 100, TextBoxNoeudA.Location.Y+3), AutoSize = true, Text = "Station de Départ", ReadOnly = true, BorderStyle = BorderStyle.None};
            System.Windows.Forms.TextBox TextBoxNoeudB = new System.Windows.Forms.TextBox { Location = new Point(TextBoxNoeudA.Location.X, TextBoxNoeudA.Location.Y + 30), Width = TextBoxNoeudA.Width, Height = 30};
            System.Windows.Forms.TextBox TextNoeudB = new System.Windows.Forms.TextBox { Location = new Point(TextBoxNoeudB.Location.X - 100, TextBoxNoeudB.Location.Y+3), AutoSize = true, Text = "Station d'Arrivé", ReadOnly = true, BorderStyle = BorderStyle.None };

            #region Creation d'une liste d'option pour les boutons 
            AutoCompleteStringCollection options = new AutoCompleteStringCollection();
            string[] listeStations = new string[g.ListeNoeud.Count];

            int compteur = 0;
            foreach (Noeud<string> N in  g.ListeNoeud)
            {
                listeStations[compteur] = N.Nom;
                compteur++;
            }
            Random rnd = new Random(); 

            TextBoxNoeudA.Text = listeStations[rnd.Next(listeStations.Length)];
            TextBoxNoeudB.Text = listeStations[rnd.Next(listeStations.Length)];

            options.AddRange(listeStations);

            TextBoxNoeudA.AutoCompleteCustomSource = options;
            TextBoxNoeudA.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            TextBoxNoeudA.AutoCompleteSource = AutoCompleteSource.CustomSource;

            TextBoxNoeudB.AutoCompleteCustomSource = options;
            TextBoxNoeudB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            TextBoxNoeudB.AutoCompleteSource = AutoCompleteSource.CustomSource;
            #endregion

            panelAffichage.Controls.Add(TextBoxNoeudA);
            panelAffichage.Controls.Add(TextBoxNoeudB);
            panelAffichage.Controls.Add(TextNoeudA);
            panelAffichage.Controls.Add(TextNoeudB);
            #endregion

            bool AfficherDijkstra = false;
            bool AfficherBellmanFord = false;

            #region Dijkstra
            Button btnDijkstra = new System.Windows.Forms.Button { Text = "Dijkstra", AutoSize = true};

            bool AfficherDijkstra = false;
            string stationA = "";
            string stationB = "";

            btnDijkstra.Click += (s, e) =>
            {
                if (AfficherBellmanFord)
                {
                    Graph<string> chemin = g.BellmanFord(stationA, stationB, "");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherBellmanFord = false;
                }

                string LectureNœudA = TextBoxNoeudA.Text;
                string LectureNœudB = TextBoxNoeudB.Text;

                if (listeStations.Contains(LectureNœudA) && listeStations.Contains(LectureNœudB));
                {
                    panelAffichage.Invalidate();
                    Graph<string> chemin = g.Dijkstra(stationA, stationB, "");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherDijkstra = !AfficherDijkstra;

                    if (AfficherDijkstra || LectureNœudA != stationA || stationB != LectureNœudB)
                {
                        stationA = LectureNœudA;
                        stationB = LectureNœudB;

                        AfficherDijkstra = true;
                        chemin = g.Dijkstra(LectureNœudA, LectureNœudB, "Rouge");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    }

                }
            };

            panelAffichage.Controls.Add(btnDijkstra);
            #endregion

            #region BellmanFord
            System.Windows.Forms.Button btnBellmanFord = new System.Windows.Forms.Button { Text = "BellmanFord", AutoSize = true , Location = new Point(btnDijkstra.Location.X + btnDijkstra.Width+10, btnDijkstra.Location.Y) };

            bool AfficherBellmanFord = false;


            btnBellmanFord.Click += (s, e) =>
            {
                if (AfficherDijkstra)
                {
                    Graph<string> chemin = g.Dijkstra(stationA, stationB, "");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherDijkstra = false;
                }

                string LectureNœudA = TextBoxNoeudA.Text;
                string LectureNœudB = TextBoxNoeudB.Text;
                if (listeStations.Contains(LectureNœudA) && listeStations.Contains(LectureNœudB)) ;
                {
                    panelAffichage.Invalidate();

                    Graph<string> chemin = g.BellmanFord(stationA, stationB, "");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherBellmanFord = !AfficherBellmanFord;

                    if (AfficherBellmanFord || LectureNœudA != stationA || stationB != LectureNœudB)
                {


                        stationA = LectureNœudA;
                        stationB = LectureNœudB;

                        chemin = g.BellmanFord(LectureNœudA, LectureNœudB, "Bleu");
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                        AfficherBellmanFord = true;
                    }
                }
            };

            panelAffichage.Controls.Add(btnBellmanFord);
            #endregion

            #region Rendre Ligne inutilisable
            Label btnRendreLigneInutilisable = new Label { Text = "Rendre la ligne inutilisable", AutoSize = true, Location = new Point(panelAffichage.Right-250, btnDijkstra.Location.Y) };
            
            string[] LigneExistantes = { "1", "2", "3", "3bis", "4", "5", "6", "7", "8", "9", "10", "7bis", "11", "12", "13", "14" };

            #region Création des Cases à Cohcé
            for (int i = 0; i < LigneExistantes.Length; i++)
            {
                int nomLigne = i;
                bool coché = false;
                CheckBox CheckBox = new CheckBox { Text = LigneExistantes[i], Location = new Point(panelAffichage.Right - 160, i * 20+20) };
                CheckBox.CheckedChanged += (s, e) =>
                {
                    #region Reintianilisation de la carte si besoin
                    if (AfficherDijkstra)
                    {
                        panelAffichage.Invalidate();
                        Graph<string> chemin = g.Dijkstra(stationA, stationB, "");
                        if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    }
                    if (AfficherBellmanFord)
                    {
                        panelAffichage.Invalidate();
                        Graph<string> chemin = g.BellmanFord(stationA, stationB, "");
                        if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    }
            #endregion

                    #region Modification des poids de Liens et du temps de changement des Noeuds
                    if (!coché)
                    {
                        foreach (Noeud<string> N in g.ListeNoeud)
                        {
                            if (N.Ligne.Contains(LigneExistantes[nomLigne])) N.Changement = 1000;
                        }
                        foreach (Lien<string> L in g.ListeLien)
                        {
                            if (L.Ligne == LigneExistantes[nomLigne]) L.Poid = 1000;
                        }
                        coché = true;
                    }
                    else
                    {
                        foreach (Noeud<string> N in g.ListeNoeud)
                        {
                            if (N.Ligne.Contains(LigneExistantes[nomLigne])) N.CalculTempsChangement();
                        }
                        foreach (Lien<string> L in g.ListeLien)
                        {
                            if (L.Ligne == LigneExistantes[nomLigne]) L.CalculPoid();
                        }
                        coché = false;
                    }
                    #endregion

                    #region Affichage du nouveau chemin
                    if (AfficherDijkstra)
                    {
                        panelAffichage.Invalidate();
                        Graph<string> chemin = g.Dijkstra(stationA, stationB, "Rouge");
                        if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    }
                    if (AfficherBellmanFord)
                    {
                        panelAffichage.Invalidate();
                        Graph<string> chemin = g.BellmanFord(stationA, stationB, "Bleu");
                        if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    }
                    #endregion
                };

                panelAffichage.Controls.Add(CheckBox);
        }
            #endregion
            panelAffichage.Controls.Add(btnRendreLigneInutilisable);
           
            #endregion

            Controls.Add(panelAffichage);
            #endregion
        }
    }
}
