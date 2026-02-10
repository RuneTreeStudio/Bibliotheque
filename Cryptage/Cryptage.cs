using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Cryptage
{
    /// <summary>
    /// Service de cryptage/décryptage des fichiers de bibliothèque
    /// </summary>
    public class Cryptage
    {
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const int Iterations = 10000;
        private const int SaltSize = 32;
        private const byte EncryptionMarker = 0xCE;

        /// <summary>
        /// Crypte un fichier avec une clé fournie ou le SID de l'utilisateur
        /// </summary>
        public static void EncryptFile(string sourceFilePath, string encryptedFilePath, string? encryptionKey = null)
        {
            try
            {
                string key = encryptionKey ?? GetUserSidPlatformSafe();
                byte[] salt = GenerateSalt();

                using var aes = CreateAes(key, salt);
                using var fsInput = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
                using var fsOutput = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write);

                // Écrire le marqueur de cryptage
                fsOutput.WriteByte(EncryptionMarker);

                // Écrire le salt au début du fichier pour le décryptage
                fsOutput.Write(salt, 0, salt.Length);

                using var cryptoStream = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write);
                fsInput.CopyTo(cryptoStream);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Erreur lors du cryptage du fichier : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Décrypte un fichier avec une clé fournie ou le SID de l'utilisateur
        /// </summary>
        public static void DecryptFile(string encryptedFilePath, string decryptedFilePath, string? encryptionKey = null)
        {
            try
            {
                string key = encryptionKey ?? GetUserSidPlatformSafe();

                using var fsInput = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read);

                // Vérifier et lire le marqueur de cryptage
                int marker = fsInput.ReadByte();
                if (marker != EncryptionMarker)
                {
                    throw new CryptographicException("Fichier non crypté ou corrompu : marqueur invalide.");
                }

                // Lire le salt depuis le début du fichier
                byte[] salt = new byte[SaltSize];
                int bytesRead = fsInput.Read(salt, 0, SaltSize);

                if (bytesRead != SaltSize)
                {
                    throw new CryptographicException("Fichier corrompu : salt incomplet.");
                }

                using var aes = CreateAes(key, salt);
                using var cryptoStream = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var fsOutput = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write);

                cryptoStream.CopyTo(fsOutput);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("Erreur de décryptage : Clé de cryptage incorrecte ou fichier corrompu.", ex);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Erreur lors du décryptage du fichier : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crée une instance Aes configurée
        /// </summary>
        private static Aes CreateAes(string password, byte[] salt)
        {
            var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] keyBytes = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize / 8);
            byte[] ivBytes = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, BlockSize / 8);
            
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            return aes;
        }

        /// <summary>
        /// Génère un salt aléatoire
        /// </summary>
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// Obtient le SID de l'utilisateur Windows courant de façon multiplateforme
        /// </summary>
        private static string GetUserSidPlatformSafe()
        {
#if WINDOWS
            return GetUserSid();
#else
            throw new PlatformNotSupportedException("L'obtention du SID utilisateur n'est supportée que sous Windows. Veuillez fournir une clé de cryptage.");
#endif
        }

        /// <summary>
        /// Obtient le SID de l'utilisateur Windows courant
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static string GetUserSid()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User?.Value
                    ?? throw new InvalidOperationException("Impossible d'obtenir le SID de l'utilisateur.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération du SID : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Vérifie si un fichier est crypté en contrôlant le marqueur
        /// </summary>
        public static bool IsFileEncrypted(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // Vérifier la taille minimale du fichier (1 marqueur + 32 octets de salt)
                if (new FileInfo(filePath).Length < SaltSize + 1)
                    return false;

                // Vérifier le marqueur de cryptage
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                int marker = fs.ReadByte();

                return marker == EncryptionMarker;
            }
            catch
            {
                return false;
            }
        }
    }
}