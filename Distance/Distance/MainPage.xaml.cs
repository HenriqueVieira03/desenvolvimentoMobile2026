namespace Distance
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void btCalcularDistancia_Clicked(object sender, EventArgs e)
        {
            double ax = Convert.ToDouble(Ax.Text);
            double ay = Convert.ToDouble(Ay.Text);
            double az = Convert.ToDouble(Az.Text);

            double bx = Convert.ToDouble(Bx.Text);
            double by = Convert.ToDouble(By.Text);
            double bz = Convert.ToDouble(Bz.Text);

            double distance = Math.Sqrt(Math.Pow(bx - ax, 2) + Math.Pow(by - ay, 2) + Math.Pow(bz - az, 2));

            lbResultado.Text = $"{distance:F4}";
            ;
        }
    }
}
