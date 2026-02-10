using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;
using Data.Models;
using Serialization;

namespace Data.Services
{
    /// <summary>
    /// Service de gestion de la persistance des données de bibliothèque dans des fichiers.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class BibliothequeFileService
    {
        private readonly ISerializationStrategy _serializationStrategy;
        private string _filePath;

        /// <summary>
        /// Initialise le service avec la stratégie de sérialisation spécifiée.
        /// </summary>
        /// <param name="serializationType">Type de sérialisation à utiliser (par défaut : XML).</param>
        public BibliothequeFileService(SerializationType serializationType = SerializationType.Xml)
        {
            _serializationStrategy = SerializationFactory.CreateSerializer(serializationType);
            _filePath = GetFilePath(serializationType);
        }

        private string GetFilePath(SerializationType type)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string bibliothequeDir = Path.Combine(documentsPath, "Bibliotheque");
            
            string userName = WindowsIdentity.GetCurrent().Name.Split('\\')[^1];
            
            string extension = type switch
            {
                SerializationType.Xml => "xml",
                _ => "dat"
            };

            return Path.Combine(bibliothequeDir, $"Bibliotheque_{userName}.{extension}");
        }

        /// <summary>
        /// Sauvegarde les données de bibliothèque dans le fichier.
        /// </summary>
        /// <param name="data">Les données à sauvegarder.</param>
        /// <exception cref="InvalidOperationException">Levée en cas d'erreur lors de la sauvegarde.</exception>
        public void SaveData(BibliothequeData data)
        {
            try
            {
                _serializationStrategy.Serialize(data, _filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Impossible de sauvegarder les données dans {_filePath}. {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge les données de bibliothèque depuis un fichier.
        /// </summary>
        /// <param name="filePath">Le chemin du fichier à charger.</param>
        /// <returns>Les données chargées ou une nouvelle instance si le fichier n'existe pas.</returns>
        /// <exception cref="InvalidOperationException">Levée si le fichier est corrompu.</exception>
        public BibliothequeData LoadData(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new BibliothequeData();
                }
                _filePath = filePath;
                var data = _serializationStrategy.Deserialize<BibliothequeData>(filePath);
                data.ReconstruireReferences();
                return data;
            }
            catch (FileNotFoundException)
            {
                return new BibliothequeData();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Impossible de charger les données depuis {filePath}. Le fichier est peut-être corrompu. {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtient le chemin du fichier actuellement utilisé.
        /// </summary>
        /// <returns>Le chemin du fichier.</returns>
        public string GetCurrentFilePath()
        {
            return _filePath;
        }

        /// <summary>
        /// Vérifie si un fichier existe.
        /// </summary>
        /// <param name="filePath">Le chemin du fichier à vérifier.</param>
        /// <returns>True si le fichier existe, sinon False.</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Obtient le répertoire par défaut pour les fichiers de bibliothèque.
        /// </summary>
        /// <returns>Le chemin du répertoire Bibliotheque dans Mes Documents.</returns>
        public string GetBibliothequeDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, "Bibliotheque");
        }
    }
}
