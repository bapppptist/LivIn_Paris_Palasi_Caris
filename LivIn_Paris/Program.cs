namespace LivIn_Paris
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            System.Windows.Forms.Application.Run(new Liv_In_Paris());
        }


        static void ShowPanel(Panel panel)
        {
            panel.BringToFront();
        }



    }
}

