using System;
using System.IO;
using System.Text;

namespace Password
{
    public static class PasswordManager
    {
        private const string DefaultPassword = "Bibliotheque"; // Mot de passe par défaut, mauvaise
                                                               // pratique de l'avoir ici, mais les
                                                               // fichiers sont réellement sécurisés
                                                               // par la clé de cryptage

        /// <summary>
        /// Vérifie si un mot de passe est correct pour un fichier de bibliothèque
        /// </summary>
        public static bool VerifyPassword(string password)
        {
            return password == DefaultPassword;
        }

        /// <summary>
        /// Obtient le mot de passe par défaut
        /// </summary>
        public static string GetDefaultPassword()
        {
            return DefaultPassword;
        }
    }
}