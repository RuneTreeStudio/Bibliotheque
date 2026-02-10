using System;
using System.IO;
using System.Xml.Serialization;

namespace Serialization
{
    public class XmlSerializationStrategy : ISerializationStrategy
    {
        /// <summary>
        /// Sérialise un objet au format XML et l'enregistre dans un fichier.
        /// </summary>
        /// <typeparam name="T">Le type de l'objet à sérialiser.</typeparam>
        /// <param name="data">L'objet à sérialiser.</param>
        /// <param name="filePath">Le chemin du fichier de destination.</param>
        /// <exception cref="InvalidOperationException">Levée lorsqu'une erreur se produit pendant la sérialisation.</exception>
        /// <remarks>
        /// Cette méthode crée automatiquement le répertoire de destination s'il n'existe pas.
        /// Si le fichier existe déjà, il sera écrasé.
        /// </remarks>
        public void Serialize<T>(T data, string filePath)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fileStream, data);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la sérialisation XML : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Désérialise un objet à partir d'un fichier XML.
        /// </summary>
        /// <typeparam name="T">Le type de l'objet à désérialiser.</typeparam>
        /// <param name="filePath">Le chemin du fichier source contenant les données XML.</param>
        /// <returns>L'objet désérialisé de type T.</returns>
        /// <exception cref="FileNotFoundException">Levée si le fichier spécifié n'existe pas.</exception>
        /// <exception cref="InvalidOperationException">Levée lorsqu'une erreur se produit pendant la désérialisation ou si le résultat est null.</exception>
        /// <remarks>
        /// Cette méthode vérifie l'existence du fichier avant de tenter la désérialisation.
        /// </remarks>
        public T Deserialize<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Le fichier {filePath} n'existe pas.");
                }

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var result = serializer.Deserialize(fileStream);
                    if (result == null)
                    {
                        throw new InvalidOperationException("La désérialisation a retourné null.");
                    }
                    return (T)result;
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la désérialisation XML. Le fichier est peut-être corrompu : {ex.Message}", ex);
            }
        }
    }
}
