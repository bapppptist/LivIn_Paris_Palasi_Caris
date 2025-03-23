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

            #region Dijkstra
            System.Windows.Forms.Button btnDijkstra = new System.Windows.Forms.Button { Text = "Dijkstra", AutoSize = true};

            bool AfficherDijkstra = false;
            string stationA = "";
            string stationB = "";

            btnDijkstra.Click += (s, e) =>
            {
                string LectureNoeudA = TextBoxNoeudA.Text;
                string LectureNoeudB = TextBoxNoeudB.Text;
                panelAffichage.Invalidate();
                if (!AfficherDijkstra || LectureNoeudA != stationA || stationB!= LectureNoeudB )
                {
                    Graph<string> chemin = g.Dijkstra(stationA, stationB, false);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);

                    stationA = LectureNoeudA;
                    stationB = LectureNoeudB;

                    AfficherDijkstra = true;
                    chemin = g.Dijkstra(LectureNoeudA, LectureNoeudB, true);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                }
                else if(AfficherDijkstra)
                {
                    Graph<string> chemin = g.Dijkstra(stationA, stationB, false);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherDijkstra = false;

                }
            };

            panelAffichage.Controls.Add(btnDijkstra);
            #endregion

            #region BellmanFord
            System.Windows.Forms.Button btnBellmanFord = new System.Windows.Forms.Button { Text = "BellmanFord", AutoSize = true , Location = new Point(btnDijkstra.Location.X + btnDijkstra.Width+10, btnDijkstra.Location.Y) };

            bool AfficherBellmanFord = false;

            btnBellmanFord.Click += (s, e) =>
            {
                string LectureNoeudA = TextBoxNoeudA.Text;
                string LectureNoeudB = TextBoxNoeudB.Text;
                panelAffichage.Invalidate();
                if (!AfficherBellmanFord || LectureNoeudA != stationA || stationB != LectureNoeudB)
                {
                    Graph<string> chemin = g.BellmanFord(stationA, stationB, false);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);

                    stationA = LectureNoeudA;
                    stationB = LectureNoeudB;

                    AfficherBellmanFord = true;
                    chemin = g.BellmanFord(LectureNoeudA, LectureNoeudB, true);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                }
                else if (AfficherBellmanFord)
                {
                    Graph<string> chemin = g.BellmanFord(stationA, stationB, false);
                    if (chemin != null) chemin.AfficherGraphique(panelAffichage);
                    AfficherBellmanFord = false;

                }
            };

            panelAffichage.Controls.Add(btnBellmanFord);
            #endregion

            Controls.Add(panelAffichage);
            #endregion



        }

    }
}
