using System;
using System.Collections.Generic;

namespace Data.Classes
{
    public class Categorie
    {
        public string Nom { get; set; } = string.Empty;
        public List<Livre> Livres { get; set; }

        public Categorie()
        {
            Livres = new List<Livre>();
        }
    }
}
