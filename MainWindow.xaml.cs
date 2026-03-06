using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfConfetti
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Check mouse down
        }

        public void BurstConfettiButton_Click(object sender, RoutedEventArgs e)
        {
            ConfettiControl.ConfettiMode = ConfettiMode.Burst;
            ConfettiControl.Emit(Mouse.GetPosition(Owner), 125);
        }

        public void RainConfettiButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfettiControl.IsRaining)
            {
                ConfettiControl.StopRain();
            }
            else
            {
                ConfettiControl.StartRain();
            }
        }

        public void CannonConfettiButton_Click(object sender, RoutedEventArgs e)
        {
            ConfettiControl.ConfettiMode = ConfettiMode.Cannon;
            ConfettiControl.Emit(Mouse.GetPosition(Owner), 500);
        }
    }
}