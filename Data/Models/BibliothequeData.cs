using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Data.Classes;

namespace Data.Models
{
    [XmlRoot("BibliothequeData")]
    public class BibliothequeData
    {
        [XmlArray("Livres")]
        [XmlArrayItem("Livre")]
        public List<Livre> Livres { get; set; }

        [XmlArray("Utilisateurs")]
        [XmlArrayItem("Utilisateur")]
        public List<Utilisateur> Utilisateurs { get; set; }

        [XmlArray("Categories")]
        [XmlArrayItem("Categorie")]
        public List<Categorie> Categories { get; set; }

        [XmlArray("Emprunts")]
        [XmlArrayItem("Emprunt")]
        public List<Emprunt> Emprunts { get; set; }

        public BibliothequeData()
        {
            Livres = new List<Livre>();
            Utilisateurs = new List<Utilisateur>();
            Categories = new List<Categorie>();
            Emprunts = new List<Emprunt>();
        }

        /// <summary>
        /// Reconstruit toutes les références entre objets après désérialisation.
        /// Cette méthode est appelée automatiquement par BibliothequeFileService.LoadData()
        /// </summary>
        public void ReconstruireReferences()
        {
            // Reconstruire les références livre vers catégorie
            foreach (var livre in Livres)
            {
                livre.ReconstruireCategorie(Categories);
            }

            // Reconstruire les références emprunt vers livre et utilisateur
            foreach (var emprunt in Emprunts)
            {
                emprunt.ReconstruireReferences(Livres, Utilisateurs);
            }
        }

        /// <summary>
        /// Obtient le prochain ID disponible pour un emprunt
        /// </summary>
        public int GetNextEmpruntId()
        {
            return Emprunts.Any() ? Emprunts.Max(e => e.Id) + 1 : 1;
        }
    }
}
