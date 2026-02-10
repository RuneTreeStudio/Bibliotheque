using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Data.Classes
{
    public class Livre
    {
        public string Titre { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;

        public DateOnly? DateDePublication { get; set; }

        public string Isbn { get; set; } = string.Empty;
        
        [XmlIgnore]
        public Categorie Categorie { get; set; } = null!;

        // Champ temporaire pour la désérialisation
        [XmlIgnore]
        private string _categorieNomTemp = string.Empty;


        [XmlElement("CategorieNom")]
        public string? CategorieNom 
        { 
            get => Categorie?.Nom;
            set => _categorieNomTemp = value ?? string.Empty;
        }
        
        public DateTime DateAjout { get; set; }

        public Livre()
        {
        }

        /// <summary>
        /// Reconstruit la référence à la catégorie (appelé après désérialisation)
        /// </summary>
        internal void ReconstruireCategorie(List<Categorie> categories)
        {
            if (!string.IsNullOrWhiteSpace(_categorieNomTemp))
            {
                Categorie = categories.Find(c => c.Nom == _categorieNomTemp) ?? Categorie;
            }
        }
    }
}
