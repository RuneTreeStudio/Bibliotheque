using System;
using System.Windows;
using Data.Models;
using Data.Services;
using Serialization;
using Data.Classes;

namespace Bibliotheque
{
    /// <summary>
    /// Classe pour générer des données initiales d'exemple.
    /// </summary>
    public static class GenererDonneesExemple
    {
        public static BibliothequeData Executer()
        {
            var fileService = new BibliothequeFileService(SerializationType.Xml);
            var data = new BibliothequeData();

            // Créer des catégories
            var categorieRoman = new Categorie { Nom = "Roman" };
            var categorieScience = new Categorie { Nom = "Science-Fiction" };
            var categorieAction = new Categorie { Nom = "Action" };
            var categorieHorreur = new Categorie { Nom = "Horreur" };
            var categorieInformatique = new Categorie { Nom = "Informatique" };

            data.Categories.Add(categorieRoman);
            data.Categories.Add(categorieScience);
            data.Categories.Add(categorieAction);
            data.Categories.Add(categorieHorreur);
            data.Categories.Add(categorieInformatique);

            // Créer des livres
            var livre1 = new Livre
            {
                Titre = "Le Petit Prince",
                Auteur = "Antoine de Saint-Exupéry",
                Isbn = "978-2-07-061275-8",
                DateDePublication = new DateOnly(1943, 4, 6),
                DateAjout = DateTime.Now,
                Categorie = categorieRoman
            };

            var livre2 = new Livre
            {
                Titre = "1984",
                Auteur = "George Orwell",
                Isbn = "978-0-141-03614-4",
                DateDePublication = new DateOnly(1949, 6, 8),
                DateAjout = DateTime.Now,
                Categorie = categorieRoman
            };

            var livre3 = new Livre
            {
                Titre = "Shining",
                Auteur = "Stephen King",
                Isbn = "978-2253151622",
                DateDePublication = new DateOnly(1977, 1, 28),
                DateAjout = DateTime.Now,
                Categorie = categorieHorreur
            };

            var livre4 = new Livre
            {
                Titre = "Clean Code",
                Auteur = "Robert C. Martin",
                Isbn = "978-0132350884",
                DateDePublication = new DateOnly(2008, 8, 1),
                DateAjout = DateTime.Now,
                Categorie = categorieInformatique
            };

            var livre5 = new Livre
            {
                Titre = "La Mémoire dans la peau",
                Auteur = "Robert Ludlum",
                Isbn = "978-2253026746",
                DateDePublication = new DateOnly(1980, 3, 1),
                DateAjout = DateTime.Now,
                Categorie = categorieAction
            };

            var livre6 = new Livre
            {
                Titre = "Dune",
                Auteur = "Frank Herbert",
                Isbn = "978-2266257572",
                DateDePublication = new DateOnly(1965, 8, 1),
                DateAjout = DateTime.Now,
                Categorie = categorieScience
            };

            var livre7 = new Livre
            {
                Titre = "Design Patterns",
                Auteur = "Erich Gamma",
                Isbn = "978-0201633610",
                DateDePublication = new DateOnly(1994, 10, 21),
                DateAjout = DateTime.Now,
                Categorie = categorieInformatique
            };

            data.Livres.Add(livre1);
            data.Livres.Add(livre2);
            data.Livres.Add(livre3);
            data.Livres.Add(livre4);
            data.Livres.Add(livre5);
            data.Livres.Add(livre6);
            data.Livres.Add(livre7);

            // Créer des utilisateurs
            var utilisateur1 = new Utilisateur
            {
                Nom = "Mannheim",
                Prenom = "Rowan",
                Email = "rowan.mannheim@etu.uca.fr",
                DateInscription = DateTime.Now
            };

            var utilisateur2 = new Utilisateur
            {
                Nom = "Duck",
                Prenom = "Donald",
                Email = "donald@duck.splash",
                DateInscription = DateTime.Now
            };

            var utilisateur3 = new Utilisateur
            {
                Nom = "Mouse",
                Prenom = "Mikey",
                Email = "mickey4444@disney.com",
                DateInscription = DateTime.Now
            };

            data.Utilisateurs.Add(utilisateur1);
            data.Utilisateurs.Add(utilisateur2);
            data.Utilisateurs.Add(utilisateur3);

            // Créer des emprunts
            // Emprunt en cours (Le Petit Prince par Rowan)
            var emprunt1 = new Emprunt
            {
                Id = 1,
                Livre = livre1,
                Utilisateur = utilisateur1,
                DateEmprunt = DateTime.Now.AddDays(-5),
                DateRetourPrevue = DateTime.Now.AddDays(9),
                DateRetourEffective = null // En cours
            };
            utilisateur1.LivresEmpruntes.Add(livre1);

            // Emprunt terminé (1984 par Donald)
            var emprunt2 = new Emprunt
            {
                Id = 2,
                Livre = livre2,
                Utilisateur = utilisateur2,
                DateEmprunt = DateTime.Now.AddDays(-20),
                DateRetourPrevue = DateTime.Now.AddDays(-6),
                DateRetourEffective = DateTime.Now.AddDays(-8) // Retourné en avance
            };

            // Emprunt en retard (Shining par Mickey)
            var emprunt3 = new Emprunt
            {
                Id = 3,
                Livre = livre3,
                Utilisateur = utilisateur3,
                DateEmprunt = DateTime.Now.AddDays(-25),
                DateRetourPrevue = DateTime.Now.AddDays(-5), // Retard de 5 jours
                DateRetourEffective = null // Pas encore retourné
            };
            utilisateur3.LivresEmpruntes.Add(livre3);

            // Emprunt en cours (Dune par Donald)
            var emprunt4 = new Emprunt
            {
                Id = 4,
                Livre = livre6,
                Utilisateur = utilisateur2,
                DateEmprunt = DateTime.Now.AddDays(-3),
                DateRetourPrevue = DateTime.Now.AddDays(11),
                DateRetourEffective = null
            };
            utilisateur2.LivresEmpruntes.Add(livre6);

            // Emprunt terminé (Clean Code par Rowan)
            var emprunt5 = new Emprunt
            {
                Id = 5,
                Livre = livre4,
                Utilisateur = utilisateur1,
                DateEmprunt = DateTime.Now.AddDays(-30),
                DateRetourPrevue = DateTime.Now.AddDays(-16),
                DateRetourEffective = DateTime.Now.AddDays(-15) // Retourné avec 1 jour de retard
            };

            data.Emprunts.Add(emprunt1);
            data.Emprunts.Add(emprunt2);
            data.Emprunts.Add(emprunt3);
            data.Emprunts.Add(emprunt4);
            data.Emprunts.Add(emprunt5);

            return data;
        }
    }
}
