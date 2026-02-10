using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Classes
{
    public class Utilisateur
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateInscription { get; set; }
        public List<Livre> LivresEmpruntes { get; set; }

        public Utilisateur()
        {
            LivresEmpruntes = new List<Livre>();
        }
    }
}
