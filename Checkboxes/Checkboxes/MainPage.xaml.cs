using Microsoft.Maui.Controls;

namespace Checkboxes;

public partial class MainPage : ContentPage
{
    private List<CheckBox> _checkBoxes = new List<CheckBox>();

    public MainPage()
    {
        InitializeComponent();
        AtualizarContador();
    }

    private void OnAdicionarClicked(object sender, EventArgs e)
    {
        string textoTarefa = TxtNovaTarefa.Text;

        if (string.IsNullOrWhiteSpace(textoTarefa))
            return;

        var checkBox = new CheckBox();
        _checkBoxes.Add(checkBox);

        var label = new Label
        {
            Text = textoTarefa,
            VerticalOptions = LayoutOptions.Center,
            FontSize = 16
        };

        checkBox.CheckedChanged += (s, args) =>
        {
            if (checkBox.IsChecked)
            {
                label.TextDecorations = TextDecorations.Strikethrough;
                label.TextColor = Colors.Gray;
            }
            else
            {
                label.TextDecorations = TextDecorations.None;
                label.ClearValue(Label.TextColorProperty);
            }

            AtualizarContador();
        };

        var stackLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { checkBox, label }
        };

        ListaTarefas.Children.Add(stackLayout);

        TxtNovaTarefa.Text = string.Empty;
        AtualizarContador();
    }

    private void AtualizarContador()
    {
        int totalTarefas = _checkBoxes.Count;
        int tarefasConcluidas = 0;

        foreach (var cb in _checkBoxes)
        {
            if (cb.IsChecked)
            {
                tarefasConcluidas++;
            }
        }

        LblContador.Text = $"{tarefasConcluidas} de {totalTarefas} tarefas concluídas";
    }
}