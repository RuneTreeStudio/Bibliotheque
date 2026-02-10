using System;
using System.Xml.Serialization;

namespace Data.Classes
{
    public class Emprunt
    {
        public int Id { get; set; }
        
        [XmlIgnore]
        public Livre Livre { get; set; } = null!;
        
        [XmlIgnore]
        public Utilisateur Utilisateur { get; set; } = null!;
        
        [XmlIgnore]
        private string _livreTitreTemp = string.Empty;
        
        [XmlIgnore]
        private string _utilisateurNomTemp = string.Empty;
        
        [XmlElement("LivreTitre")]
        public string? LivreTitre
        {
            get => Livre?.Titre;
            set => _livreTitreTemp = value ?? string.Empty;
        }
        
        [XmlElement("UtilisateurNom")]
        public string? UtilisateurNom
        {
            get => Utilisateur != null ? $"{Utilisateur.Nom} {Utilisateur.Prenom}" : null;
            set => _utilisateurNomTemp = value ?? string.Empty;
        }
        
        public DateTime DateEmprunt { get; set; }
        public DateTime DateRetourPrevue { get; set; }
        public DateTime? DateRetourEffective { get; set; }
        
        [XmlIgnore]
        public bool EstEnCours => DateRetourEffective == null;
        
        [XmlIgnore]
        public bool EstEnRetard => EstEnCours && DateTime.Now > DateRetourPrevue;
        
        [XmlIgnore]
        public string Statut
        {
            get
            {
                if (DateRetourEffective != null)
                    return "Retourné";
                if (EstEnRetard)
                    return "En retard";
                return "En cours";
            }
        }

        public Emprunt()
        {
            DateEmprunt = DateTime.Now;
        }

        /// <summary>
        /// Reconstruit les références après désérialisation
        /// </summary>
        internal void ReconstruireReferences(System.Collections.Generic.List<Livre> livres, 
                                             System.Collections.Generic.List<Utilisateur> utilisateurs)
        {
            if (!string.IsNullOrWhiteSpace(_livreTitreTemp))
            {
                Livre = livres.Find(l => l.Titre == _livreTitreTemp) ?? Livre;
            }

            if (!string.IsNullOrWhiteSpace(_utilisateurNomTemp))
            {
                Utilisateur = utilisateurs.Find(u => 
                    $"{u.Nom} {u.Prenom}" == _utilisateurNomTemp) ?? Utilisateur;
            }
        }

        public override string ToString()
        {
            return $"{LivreTitre} - {UtilisateurNom} ({Statut})";
        }
    }
}
