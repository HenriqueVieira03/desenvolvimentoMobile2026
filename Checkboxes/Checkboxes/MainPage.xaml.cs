using Microsoft.Maui.Controls;

namespace Checkboxes
{
    public partial class MainPage : ContentPage
    {
        public float fTotal;

        public MainPage()
        {
            InitializeComponent();

            fTotal = 0.0f;
            lbTotal.Text = $"{fTotal:F2}";
        }

        private void Checkbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            fTotal = Convert.ToSingle(lbTotal.Text);

            CheckBox chk = (CheckBox)sender;

            if (chk.Equals(chk1))
            {
                if (chk.IsChecked)
                    fTotal += Convert.ToSingle(val1.Text);
                else
                    fTotal -= Convert.ToSingle(val1.Text);
            }
            else if (chk.Equals(chk2))
            {
                if (chk.IsChecked)
                    fTotal += Convert.ToSingle(val2.Text);
                else
                    fTotal -= Convert.ToSingle(val2.Text);
            }
            else if (chk.Equals(chk3))
            {
                if (chk.IsChecked)
                    fTotal += Convert.ToSingle(val3.Text);
                else
                    fTotal -= Convert.ToSingle(val3.Text);
            }

            lbTotal.Text = $"{fTotal:F2}";
        }


    }
}
