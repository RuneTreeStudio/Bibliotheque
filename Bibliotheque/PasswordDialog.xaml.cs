using System.Windows;

namespace Bibliotheque
{
    /// <summary>
    /// Boîte de dialogue pour la saisie du mot de passe avec gestion des tentatives.
    /// </summary>
    public partial class PasswordDialog : Window
    {
        /// <summary>
        /// Obtient le mot de passe saisi par l'utilisateur.
        /// </summary>
        public string Password { get; private set; } = string.Empty;
        
        /// <summary>
        /// Obtient ou définit le nombre de tentatives restantes.
        /// </summary>
        public int RemainingAttempts { get; set; }

        /// <summary>
        /// Initialise une nouvelle instance de la boîte de dialogue.
        /// </summary>
        /// <param name="remainingAttempts">Le nombre de tentatives restantes.</param>
        public PasswordDialog(int remainingAttempts)
        {
            InitializeComponent();
            RemainingAttempts = remainingAttempts;
            UpdateAttemptsLabel();
        }

        private void UpdateAttemptsLabel()
        {
            if (LblAttempts != null)
            {
                LblAttempts.Content = $"Tentatives restantes : {RemainingAttempts}";
                
                if (RemainingAttempts <= 1)
                {
                    LblAttempts.Foreground = System.Windows.Media.Brushes.Red;
                    LblWarning.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Password = PwdPassword.Password;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PwdPassword.Focus();
        }
    }
}