using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MathGame
{
    public partial class MainPage : ContentPage
    {
        int iLB1 = 0;
        int iLB2 = 0;
        int iLB3 = 0;
        float fR = 0.0f;

        int iAcertouCount = 0;
        int iErrouCount = 0;
        int iPontuacao = 0;
        int iQuestaoAtual = 1;

        IDispatcherTimer _timer;
        int _tempoRestante = 30;
        bool _aguardandoProxima = false;

        Random rand = new Random();

        public MainPage()
        {
            InitializeComponent();

            // Configuração do Timer de 30 segundos
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void BtIniciar_Clicked(object sender, EventArgs e)
        {
            // Alterna a interface
            SetupPanel.IsVisible = false;
            GamePanel.IsVisible = true;

            // Zera os contadores da partida
            iAcertouCount = 0;
            iErrouCount = 0;
            iPontuacao = 0;
            iQuestaoAtual = 1;

            AtualizarPlacar();
            GerarJogo();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _tempoRestante--;
            lbTempo.Text = $"{_tempoRestante}s";
            pbTempo.Progress = _tempoRestante / 30.0;

            // Mudança dinâmica de cor (Verde -> Laranja -> Vermelho)
            if (_tempoRestante <= 5)
                pbTempo.ProgressColor = Colors.Red;
            else if (_tempoRestante <= 15)
                pbTempo.ProgressColor = Colors.Orange;
            else
                pbTempo.ProgressColor = Colors.Green;

            // Se o tempo acabar
            if (_tempoRestante <= 0)
            {
                _timer.Stop();
                ProcessarResposta(false, true); // Errou por esgotamento de tempo
            }
        }

        private async void btOK_Clicked(object sender, EventArgs e)
        {
            // Previne múltiplos cliques enquanto aguarda a próxima questão
            if (_aguardandoProxima) return;

            float fResult = 0.0f;

            try
            {
                fResult = Convert.ToSingle(txR.Text);
            }
            catch (Exception)
            {
                await DisplayAlert("Entrada Inválida", "Por favor, digite apenas números válidos!", "OK");
                return;
            }

            _timer.Stop();
            bool acertou = (fR == fResult);
            ProcessarResposta(acertou, false);
        }

        private async void ProcessarResposta(bool acertou, bool tempoEsgotado)
        {
            _aguardandoProxima = true;
            btOK.IsEnabled = false;

            if (acertou)
            {
                iAcertouCount++;
                imR.Source = "win.png";

                // Sistema Inteligente de Pontuação: (Base pela dificuldade) + (Bônus de tempo x2)
                int pontosBase = pkDificuldade.SelectedIndex switch
                {
                    0 => 10,
                    1 => 20,
                    2 => 30,
                    _ => 10
                };
                iPontuacao += pontosBase + (_tempoRestante * 2);
            }
            else
            {
                iErrouCount++;
                imR.Source = "loose.png";
                if (tempoEsgotado)
                {
                    txR.Text = "Tempo Esgotado!";
                }
            }

            AtualizarPlacar();

            // Feedback visual por 1.5 segundos
            await Task.Delay(1500);

            imR.Source = "question.png";
            txR.Text = "";
            btOK.IsEnabled = true;
            _aguardandoProxima = false;

            // Verifica progresso da partida
            iQuestaoAtual++;
            if (iQuestaoAtual > 10)
            {
                FinalizarPartida();
            }
            else
            {
                GerarJogo();
            }
        }

        public void GerarJogo()
        {
            lbProgresso.Text = $"Questão {iQuestaoAtual}/10";

            // Limite numérico de acordo com a Dificuldade Selecionada
            int limite = pkDificuldade.SelectedIndex switch
            {
                0 => 10,
                1 => 50,
                2 => 100,
                _ => 10
            };

            iLB1 = rand.Next(1, limite + 1);
            iLB3 = rand.Next(1, limite + 1);
            iLB2 = rand.Next(1, 5); // 1: +, 2: -, 3: x, 4: ÷

            switch (iLB2)
            {
                case 1:
                    fR = (iLB1 + iLB3);
                    lb2.Text = "+";
                    break;
                case 2:
                    // Inverte os números se necessário para evitar resultado negativo (simplifica a UX)
                    if (iLB1 < iLB3) (iLB1, iLB3) = (iLB3, iLB1);
                    fR = (iLB1 - iLB3);
                    lb2.Text = "-";
                    break;
                case 3:
                    fR = (iLB1 * iLB3);
                    lb2.Text = "x";
                    break;
                case 4:
                    // Para garantir divisões exatas (resultado sempre inteiro)
                    fR = iLB1;
                    int dividendo = iLB1 * iLB3;
                    iLB1 = dividendo; // Muda o lb1 para ser múltiplo exato do lb3
                    lb2.Text = "÷";
                    break;
            }

            lb1.Text = Convert.ToString(iLB1);
            lb3.Text = Convert.ToString(iLB3);

            // Reinicia o timer para 30 segundos na nova questão
            _tempoRestante = 30;
            lbTempo.Text = "30s";
            pbTempo.Progress = 1.0;
            pbTempo.ProgressColor = Colors.Green;
            _timer.Start();
        }

        private void AtualizarPlacar()
        {
            lbAcertou.Text = $"Acertos: {iAcertouCount}";
            lbErrou.Text = $"Erros: {iErrouCount}";
            lbPontuacao.Text = iPontuacao.ToString();
        }

        private async void FinalizarPartida()
        {
            // Classificação de Performance Final
            string classificacao = iAcertouCount switch
            {
                10 => "Perfeito! Você é uma lenda da matemática.",
                >= 8 => "Ótimo desempenho!",
                >= 5 => "Bom trabalho, mas você pode melhorar.",
                _ => "Precisa praticar mais!"
            };

            await DisplayAlert("Fim de Jogo",
                $"Partida Finalizada!\n\nPontuação Final: {iPontuacao}\nAcertos: {iAcertouCount}/10\n\nClassificação: {classificacao}",
                "Voltar ao Menu");

            // Reseta a UI para permitir uma nova partida
            SetupPanel.IsVisible = true;
            GamePanel.IsVisible = false;
        }
    }
}