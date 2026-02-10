using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Data.Models;
using Data.Services;
using Serialization;
using Password;
using Data.Classes;

namespace Bibliotheque
{
    /// <summary>
    /// Fenêtre principale de l'application de gestion de bibliothèque.
    /// </summary>
    public partial class MainWindow : Window
    {
        private BibliothequeFileService _fileService = null!;
        private BibliothequeData _bibliothequeData = null!;
        private string? _encryptionKey = null;
        private string? _decryptionKey = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBibliotheque();
            InitializeDateTime();
        }

        /// <summary>
        /// Initialise la bibliothèque avec des données d'exemple et met à jour l'interface.
        /// </summary>
        private void InitializeBibliotheque()
        {
            try
            {
                _fileService = new BibliothequeFileService(SerializationType.Xml);
                _bibliothequeData = GenererDonneesExemple.Executer();

                RefreshBindings();
                UpdateStatusBar($"Données chargées : {_bibliothequeData.Livres.Count} livres, " +
                               $"{_bibliothequeData.Utilisateurs.Count} utilisateurs, " +
                               $"{_bibliothequeData.Categories.Count} catégories, " +
                               $"{_bibliothequeData.Emprunts.Count} emprunts");
                
                UpdateAllStatistics();
            }
            catch (Exception)
            {
                _bibliothequeData = new BibliothequeData();
                RefreshBindings();
                UpdateStatusBar("Nouvelle bibliothèque créée");
            }
        }

        /// <summary>
        /// Rafraîchit les liaisons de données entre les contrôles et les collections.
        /// </summary>
        private void RefreshBindings()
        {
            // Lier les données aux DataGrids
            DgLivres.ItemsSource = null;
            DgLivres.ItemsSource = _bibliothequeData.Livres;
            
            DgCategories.ItemsSource = null;
            DgCategories.ItemsSource = _bibliothequeData.Categories;
            
            DgUtilisateurs.ItemsSource = null;
            DgUtilisateurs.ItemsSource = _bibliothequeData.Utilisateurs;
            
            DgEmprunts.ItemsSource = null;
            DgEmprunts.ItemsSource = _bibliothequeData.Emprunts;

            // Lier les catégories au ComboBox
            CmbCategorieLivre.ItemsSource = null;
            CmbCategorieLivre.ItemsSource = _bibliothequeData.Categories;
            CmbCategorieLivre.DisplayMemberPath = "Nom";
            CmbCategorieLivre.SelectedValuePath = "Nom";

            // Lier les ComboBox des emprunts
            CmbUtilisateurEmprunt.ItemsSource = null;
            CmbUtilisateurEmprunt.ItemsSource = _bibliothequeData.Utilisateurs;
            CmbUtilisateurEmprunt.DisplayMemberPath = "Nom";

            CmbLivreEmprunt.ItemsSource = null;
            CmbLivreEmprunt.ItemsSource = _bibliothequeData.Livres.Where(l => 
                !_bibliothequeData.Emprunts.Any(e => e.Livre == l && e.EstEnCours)).ToList();
            CmbLivreEmprunt.DisplayMemberPath = "Titre";

            // Initialiser les dates d'emprunt
            if (DpDateEmprunt != null)
                DpDateEmprunt.SelectedDate = DateTime.Now;
            
            if (DpDateRetourPrevue != null)
                DpDateRetourPrevue.SelectedDate = DateTime.Now.AddDays(14);
        }

        /// <summary>
        /// Initialise le timer pour l'affichage de la date et l'heure dans la barre d'état.
        /// </summary>
        private void InitializeDateTime()
        {
            // Mise à jour de l'heure dans la barre d'état
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => TxtDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();
        }

        #region Ajout Livres Categories et Utilisateurs
        
        /// <summary>
        /// Gère l'ajout d'un nouveau livre après validation des champs.
        /// </summary>
        private void AjoutLivre_Click(object sender, RoutedEventArgs e)
        {
            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(TxtTitreLivre.Text))
            {
                MessageBox.Show("Le titre du livre est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtAuteurLivre.Text))
            {
                MessageBox.Show("L'auteur du livre est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtIsbn.Text) || TxtIsbn.Text.Replace("0", "").Replace("-", "").Replace("_", "").Length == 0)
            {
                MessageBox.Show("L'ISBN du livre est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (!PublicationDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("La date de publication est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (CmbCategorieLivre.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une catégorie.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Vérifier si l'ISBN existe déjà
            if (_bibliothequeData.Livres.Any(l => l.Isbn == TxtIsbn.Text))
            {
                MessageBox.Show("Un livre avec cet ISBN existe déjà.",
                               "ISBN déjà existant",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            string titreLivre = TxtTitreLivre.Text;

            // Ajout
            _bibliothequeData.Livres.Add(new Livre
            {
                Titre = titreLivre,
                Auteur = TxtAuteurLivre.Text,
                Isbn = TxtIsbn.Text,
                DateDePublication = DateOnly.FromDateTime(PublicationDatePicker.SelectedDate.Value),
                Categorie = (Categorie) CmbCategorieLivre.SelectedItem,
                DateAjout = DateTime.Now,
            });

            // Mettre à jour 
            DgLivres.Items.Refresh();
            RefreshBindings();
            UpdateAllStatistics();

            // Vider les champs
            TxtTitreLivre.Clear();
            TxtAuteurLivre.Clear();
            TxtIsbn.Clear();
            PublicationDatePicker.SelectedDate = null;
            CmbCategorieLivre.SelectedIndex = -1;

            // Message de confirmation
            UpdateStatusBar($"Livre '{titreLivre}' ajouté avec succès");

            MessageBox.Show("Livre ajouté avec succès !",
                           "Succès",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        /// <summary>
        /// Gère l'ajout d'un nouvel utilisateur après validation des champs.
        /// </summary>
        private void AjoutUtilisateur_Click(object sender, RoutedEventArgs e)
        {
            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(TxtNomUtilisateur.Text))
            {
                MessageBox.Show("Le nom de l'utilisateur est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtPrenomUtilisateur.Text))
            {
                MessageBox.Show("Le prénom de l'utilisateur est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtEmailUtilisateur.Text))
            {
                MessageBox.Show("L'email de l'utilisateur est obligatoire.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Validation basique de l'email
            if (!TxtEmailUtilisateur.Text.Contains("@") || !TxtEmailUtilisateur.Text.Contains("."))
            {
                MessageBox.Show("L'email n'est pas valide.",
                               "Email invalide",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Vérifier si l'email existe déjà
            if (_bibliothequeData.Utilisateurs.Any(u => u.Email.Equals(TxtEmailUtilisateur.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Un utilisateur avec cet email existe déjà.",
                               "Email déjà existant",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            string nomUtilisateur = TxtNomUtilisateur.Text;

            // Ajout
            _bibliothequeData.Utilisateurs.Add(new Utilisateur
            {
                Nom = nomUtilisateur,
                Prenom = TxtPrenomUtilisateur.Text,
                Email = TxtEmailUtilisateur.Text,
                DateInscription = DateTime.Now,
            });

            // Mettre à jour 
            DgUtilisateurs.Items.Refresh();
            RefreshBindings();
            UpdateAllStatistics();

            // Vider les champs
            TxtNomUtilisateur.Clear();
            TxtPrenomUtilisateur.Clear();
            TxtEmailUtilisateur.Clear();

            // Message de confirmation
            UpdateStatusBar($"Utilisateur '{nomUtilisateur}' ajouté avec succès");

            MessageBox.Show("Utilisateur ajouté avec succès !",
                           "Succès",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

        }

        /// <summary>
        /// Gère l'ajout d'une nouvelle catégorie après validation.
        /// </summary>
        private void AjoutCategorie_Click(object sender, RoutedEventArgs e)
        {
            // Categorie invalide
            if (string.IsNullOrWhiteSpace(TxtNomCategorie.Text))
            {
                MessageBox.Show("Veuillez entrer un nom de catégorie.",
                               "Champ obligatoire",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Vérifier si la catégorie existe déjà
            if (_bibliothequeData.Categories.Exists(c => c.Nom.Equals(TxtNomCategorie.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Cette catégorie existe déjà.",
                               "Catégorie existante",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            string nomCategorie = TxtNomCategorie.Text;

            // Ajout
            _bibliothequeData.Categories.Add(new Categorie
            {
                Nom = nomCategorie,
            });

            // Mettre à jour 
            DgCategories.Items.Refresh();
            RefreshBindings();
            UpdateAllStatistics();
            TxtNomCategorie.Clear();

            // Rafraîchir le ComboBox en réassignant la source
            CmbCategorieLivre.ItemsSource = null;
            CmbCategorieLivre.ItemsSource = _bibliothequeData.Categories;
            CmbCategorieLivre.DisplayMemberPath = "Nom";

            // Message de confirmation
            UpdateStatusBar($"Catégorie '{nomCategorie}' ajoutée avec succès");

            MessageBox.Show("Catégorie ajoutée avec succès !",
                           "Succès",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }
        #endregion

        #region Gestion des Emprunts

        /// <summary>
        /// Crée un nouvel emprunt de livre pour un utilisateur.
        /// </summary>
        private void BtnEmprunter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (CmbUtilisateurEmprunt.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un utilisateur.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                if (CmbLivreEmprunt.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un livre.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                if (!DpDateEmprunt.SelectedDate.HasValue)
                {
                    MessageBox.Show("Veuillez sélectionner une date d'emprunt.",
                                   "Date requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                if (!DpDateRetourPrevue.SelectedDate.HasValue)
                {
                    MessageBox.Show("Veuillez sélectionner une date de retour prévue.",
                                   "Date requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                var utilisateur = CmbUtilisateurEmprunt.SelectedItem as Utilisateur;
                var livre = CmbLivreEmprunt.SelectedItem as Livre;
                var dateEmprunt = DpDateEmprunt.SelectedDate.Value;
                var dateRetourPrevue = DpDateRetourPrevue.SelectedDate.Value;

                // Vérifier que la date de retour est après la date d'emprunt
                if (dateRetourPrevue <= dateEmprunt)
                {
                    MessageBox.Show("La date de retour prévue doit être postérieure à la date d'emprunt.",
                                   "Dates invalides",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                // Créer l'emprunt
                var emprunt = new Emprunt
                {
                    Id = _bibliothequeData.GetNextEmpruntId(),
                    Livre = livre!,
                    Utilisateur = utilisateur!,
                    DateEmprunt = dateEmprunt,
                    DateRetourPrevue = dateRetourPrevue,
                    DateRetourEffective = null
                };

                _bibliothequeData.Emprunts.Add(emprunt);

                // Rafraîchir l'affichage
                DgEmprunts.Items.Refresh();
                RefreshBindings();
                UpdateAllStatistics();

                // Vider les champs
                CmbUtilisateurEmprunt.SelectedIndex = -1;
                CmbLivreEmprunt.SelectedIndex = -1;
                DpDateEmprunt.SelectedDate = null;
                DpDateRetourPrevue.SelectedDate = null;

                UpdateStatusBar($"Emprunt créé : {livre?.Titre} pour {utilisateur?.Nom}");

                MessageBox.Show($"Emprunt enregistré avec succès !\n\n" +
                               $"Livre : {livre?.Titre}\n" +
                               $"Emprunteur : {utilisateur?.Nom} {utilisateur?.Prenom}\n" +
                               $"Date d'emprunt : {dateEmprunt:dd/MM/yyyy}\n" +
                               $"Date de retour prévue : {dateRetourPrevue:dd/MM/yyyy}",
                               "Emprunt créé",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de l'emprunt :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Enregistre le retour d'un livre emprunté.
        /// </summary>
        private void BtnRendreLivre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérifier qu'un emprunt est sélectionné
                if (DgEmprunts.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un emprunt dans la liste.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                var emprunt = DgEmprunts.SelectedItem as Emprunt;

                if (emprunt == null) return;

                // Vérifier que l'emprunt est en cours
                if (!emprunt.EstEnCours)
                {
                    MessageBox.Show("Ce livre a déjà été retourné.",
                                   "Emprunt terminé",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                    return;
                }

                // Confirmation
                var result = MessageBox.Show($"Confirmer le retour du livre ?\n\n" +
                                            $"Livre : {emprunt.Livre.Titre}\n" +
                                            $"Emprunteur : {emprunt.Utilisateur.Nom} {emprunt.Utilisateur.Prenom}\n" +
                                            $"Date d'emprunt : {emprunt.DateEmprunt:dd/MM/yyyy}\n" +
                                            $"Date de retour prévue : {emprunt.DateRetourPrevue:dd/MM/yyyy}",
                                            "Confirmer le retour",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Enregistrer la date de retour
                    emprunt.DateRetourEffective = DateTime.Now;

                    // Calculer le retard éventuel
                    int joursRetard = (DateTime.Now - emprunt.DateRetourPrevue).Days;
                    string messageRetard = joursRetard > 0 
                        ? $"\n\nAttention : Retard de {joursRetard} jour(s)"
                        : "";

                    // Rafraîchir l'affichage
                    DgEmprunts.Items.Refresh();
                    RefreshBindings();
                    UpdateAllStatistics();

                    UpdateStatusBar($"Livre retourné : {emprunt.Livre.Titre}");

                    MessageBox.Show($"Retour enregistré avec succès !\n\n" +
                                   $"Livre : {emprunt.Livre.Titre}\n" +
                                   $"Date de retour : {DateTime.Now:dd/MM/yyyy}{messageRetard}",
                                   "Retour enregistré",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du retour du livre :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filtre la liste des emprunts selon le statut sélectionné.
        /// </summary>
        private void BtnFiltrerEmprunts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filtreItem = CmbFiltreStatutEmprunt.SelectedItem as ComboBoxItem;
                string filtre = filtreItem?.Content.ToString() ?? "Tous";

                var emprunts = filtre switch
                {
                    "En cours" => _bibliothequeData.Emprunts.Where(e => e.EstEnCours && !e.EstEnRetard).ToList(),
                    "Retournés" => _bibliothequeData.Emprunts.Where(e => !e.EstEnCours).ToList(),
                    "En retard" => _bibliothequeData.Emprunts.Where(e => e.EstEnRetard).ToList(),
                    _ => _bibliothequeData.Emprunts
                };

                DgEmprunts.ItemsSource = null;
                DgEmprunts.ItemsSource = emprunts;

                UpdateStatusBar($"Filtre appliqué : {filtre}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du filtrage :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        #endregion

        #region Serialization et Cryptage

        /// <summary>
        /// Sauvegarde les données de la bibliothèque avec option de cryptage.
        /// </summary>
        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer le format sélectionné
                var formatItem = CmbFormatSauvegarde.SelectedItem as ComboBoxItem;
                string format = formatItem?.Content.ToString() ?? "XML";

                // Déterminer le type de sérialisation
                SerializationType serializationType = format switch
                {
                    "XML" => SerializationType.Xml,
                    _ => SerializationType.Xml
                };

                // Créer un nouveau service avec le format choisi
                var serviceTemp = new BibliothequeFileService(serializationType);
                
                // Sauvegarder
                serviceTemp.SaveData(_bibliothequeData);

                // Si cryptage activé, crypter le fichier
                if (ChkActiverCryptage.IsChecked == true)
                {
                    string filePath = serviceTemp.GetCurrentFilePath();
                    string encryptedPath = filePath + ".encrypted";

                    if (!string.IsNullOrWhiteSpace(PwdCleCryptage.Password))
                        _encryptionKey = PwdCleCryptage.Password;
                    else
                    {
                        MessageBox.Show("Clé invalide. Utilisation du SID Windows à la place",
                               "Clé invalide",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                    }

                    Cryptage.Cryptage.EncryptFile(filePath, encryptedPath, _encryptionKey);
                    File.Delete(filePath);
                    File.Move(encryptedPath, filePath);

                    UpdateStatusBar($"Données sauvegardées et cryptées en {format}");
                }
                else 
                    UpdateStatusBar($"Données sauvegardées avec succès en {format}");
                
                MessageBox.Show($"Données sauvegardées avec succès !\n\n" +
                               $"Format: {format}\n" +
                               $"Fichier: {serviceTemp.GetCurrentFilePath()}\n\n" +
                               $"• Livres: {_bibliothequeData.Livres.Count}\n" +
                               $"• Utilisateurs: {_bibliothequeData.Utilisateurs.Count}\n" +
                               $"• Catégories: {_bibliothequeData.Categories.Count}",
                               "Sauvegarde réussie", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateStatusBar("Erreur lors de la sauvegarde");
                MessageBox.Show($"Erreur lors de la sauvegarde :\n\n{ex.Message}", 
                               "Erreur", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charge les données de la bibliothèque depuis un fichier avec authentification.
        /// </summary>
        private void BtnCharger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cheminFichier = TxtCheminFichierChargement.Text;
                
                if (string.IsNullOrEmpty(cheminFichier) || !File.Exists(cheminFichier))
                {
                    MessageBox.Show("Veuillez sélectionner un fichier valide à charger.", 
                                   "Fichier non trouvé", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Warning);
                    return;
                }

                // Confirmation avant de charger
                var result = MessageBox.Show("Êtes-vous sûr de vouloir charger ce fichier ?\n\n" +
                                            "Les données actuelles non sauvegardées seront perdues.",
                                            "Confirmation",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Verification du mot de passe
                    if (!VerifyPasswordWithAttempts())
                    {
                        UpdateStatusBar("Accès refusé - Authentification échouée");
                        return;
                    }

                    // Vérifier si le fichier est crypté
                    if (Cryptage.Cryptage.IsFileEncrypted(cheminFichier))
                    {
                        string decryptedPath = cheminFichier + ".decrypted";

                        // Récupérer le mot de passe
                        _decryptionKey = PwdCleDecryptage.Password;

                        // Vérifier si le mot de passe est vide
                        if (string.IsNullOrWhiteSpace(_decryptionKey))
                        {
                            MessageBox.Show("Veuillez entrer une clé de décryptage.",
                                           "Clé manquante",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Warning);
                            return;
                        }

                        // Effacer le PasswordBox après utilisation
                        PwdCleDecryptage.Clear();

                        Cryptage.Cryptage.DecryptFile(cheminFichier, decryptedPath, _decryptionKey);

                        _bibliothequeData = _fileService.LoadData(decryptedPath);
                        File.Delete(decryptedPath);

                        UpdateStatusBar("Données décryptées et chargées avec succès");
                    }
                    else
                    {
                        // Charger les donnees
                        _bibliothequeData = _fileService.LoadData(cheminFichier);
                        UpdateStatusBar("Données chargées avec succès");

                    }

                    RefreshBindings();
                    UpdateAllStatistics();

                    MessageBox.Show($"Données chargées avec succès !\n\n" +
                                   $"Fichier: {cheminFichier}\n\n" +
                                   $"• Livres: {_bibliothequeData.Livres.Count}\n" +
                                   $"• Utilisateurs: {_bibliothequeData.Utilisateurs.Count}\n" +
                                   $"• Catégories: {_bibliothequeData.Categories.Count}",
                                   "Chargement réussi",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                UpdateStatusBar("Erreur lors du chargement");
                MessageBox.Show($"Erreur lors du chargement :\n\n{ex.Message}\n\n",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ouvre une boîte de dialogue pour sélectionner un fichier à charger.
        /// </summary>
        private void BtnParcourirFichier_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier de données",
                Filter = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                DefaultExt = "xml",
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Bibliotheque")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtCheminFichierChargement.Text = openFileDialog.FileName;
                UpdateStatusBar($"Fichier sélectionné : {System.IO.Path.GetFileName(openFileDialog.FileName)}");
            }
        }

        /// <summary>
        /// Réinitialise toutes les données de la bibliothèque après confirmation.
        /// </summary>
        private void BtnReinitialiserDonnees_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("ATTENTION\n\n" +
                                        "Êtes-vous sûr de vouloir réinitialiser TOUTES les données ?\n\n" +
                                        "Cette action est IRRÉVERSIBLE et supprimera :\n" +
                                        $"• {_bibliothequeData.Livres.Count} livres\n" +
                                        $"• {_bibliothequeData.Utilisateurs.Count} utilisateurs\n" +
                                        $"• {_bibliothequeData.Categories.Count} catégories\n\n" +
                                        "Toutes les données seront perdues définitivement !",
                                        "Confirmation de réinitialisation",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _bibliothequeData = new BibliothequeData();

                RefreshBindings();
                UpdateAllStatistics();
                UpdateStatusBar("Toutes les données ont été réinitialisées");

                MessageBox.Show("Toutes les données ont été réinitialisées.\n\n" +
                                "La bibliothèque est maintenant vide.",
                                "Réinitialisation terminée",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        #endregion

        #region Mise à jour des statistiques

        /// <summary>
        /// Met à jour toutes les statistiques affichées dans l'interface.
        /// </summary>
        private void UpdateAllStatistics()
        {
            // Statistiques principales
            if (LblStatTotalLivres != null)
                LblStatTotalLivres.Content = _bibliothequeData.Livres.Count.ToString();
            
            if (LblStatTotalUtilisateurs != null)
                LblStatTotalUtilisateurs.Content = _bibliothequeData.Utilisateurs.Count.ToString();
            
            if (LblStatTotalCategories != null)
                LblStatTotalCategories.Content = _bibliothequeData.Categories.Count.ToString();

            // Statistiques des emprunts
            var empruntsEnCours = _bibliothequeData.Emprunts.Count(e => e.EstEnCours);
            var empruntsEnRetard = _bibliothequeData.Emprunts.Count(e => e.EstEnRetard);
            var livresDisponibles = _bibliothequeData.Livres.Count - empruntsEnCours;

            if (LblStatEmpruntsActifs != null)
                LblStatEmpruntsActifs.Content = empruntsEnCours.ToString();

            if (LblStatLivresDisponibles != null)
                LblStatLivresDisponibles.Content = livresDisponibles.ToString();

            if (LblStatRetards != null)
                LblStatRetards.Content = empruntsEnRetard.ToString();

            // Labels dans l'onglet emprunts
            if (LblEmpruntsEnCours != null)
                LblEmpruntsEnCours.Content = empruntsEnCours.ToString();

            if (LblEmpruntsEnRetard != null)
                LblEmpruntsEnRetard.Content = empruntsEnRetard.ToString();
        }

        /// <summary>
        /// Actualise manuellement les statistiques affichées.
        /// </summary>
        private void BtnActualiserStatistiques_Click(object sender, RoutedEventArgs e)
        {
            UpdateAllStatistics();
            UpdateStatusBar("Statistiques actualisées");
            MessageBox.Show("Statistiques mises à jour avec succès !",
                           "Actualisation",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Met à jour le message de la barre d'état avec horodatage.
        /// </summary>
        /// <param name="message">Le message à afficher.</param>
        private void UpdateStatusBar(string message)
        {
            TxtStatusBar.Text = $"{message} - {DateTime.Now:HH:mm:ss}";
        }

        #endregion

        #region Password

        /// <summary>
        /// Vérifie le mot de passe avec un nombre limité de tentatives.
        /// </summary>
        /// <returns>True si l'authentification réussit, sinon False.</returns>
        private bool VerifyPasswordWithAttempts()
        {
            int remainingAttempts = 3;

            while (remainingAttempts > 0)
            {
                var passwordDialog = new PasswordDialog(remainingAttempts)
                {
                    Owner = this
                };

                bool? dialogResult = passwordDialog.ShowDialog();

                if (dialogResult != true)
                {
                    return false;
                }

                string enteredPassword = passwordDialog.Password;

                if (PasswordManager.VerifyPassword(enteredPassword))
                {
                    UpdateStatusBar("Authentification réussie");
                    return true;
                }

                remainingAttempts--;

                if (remainingAttempts > 0)
                {
                    MessageBox.Show($"Mot de passe incorrect !\n\n" +
                                   $"Tentatives restantes : {remainingAttempts}\n\n" +
                                   $"Attention : Le fichier sera supprimé après {remainingAttempts} tentative(s) échouée(s) supplémentaire(s).",
                                   "Accès refusé",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                }
            }

            // Toutes les tentatives échouées - Supprimer le fichier
            try
            {
                File.Delete(_fileService.GetCurrentFilePath());
                TxtCheminFichierChargement.Clear();

                MessageBox.Show("Le fichier a été supprimé pour des raisons de sécurité.\n\n" +
                                "Toutes les données associées ont été effacées.",
                                "Fichier supprimé",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                UpdateStatusBar("Fichier supprimé - Tentatives d'accès épuisées");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression du fichier :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }

            return false;
        }

        #endregion

        #region Suppression Livres Categories et Utilisateurs

        /// <summary>
        /// Supprime un livre sélectionné après vérification des emprunts.
        /// </summary>
        private void BtnSupprimerLivre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérifier qu'un livre est sélectionné
                if (DgLivres.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un livre à supprimer.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                var livre = DgLivres.SelectedItem as Livre;

                if (livre == null) return;

                // Vérifier si le livre a des emprunts en cours
                var empruntsEnCours = _bibliothequeData.Emprunts.Where(e => e.Livre == livre && e.EstEnCours).ToList();
                if (empruntsEnCours.Any())
                {
                    MessageBox.Show($"Impossible de supprimer ce livre.\n\n" +
                                   $"Le livre '{livre.Titre}' a {empruntsEnCours.Count} emprunt(s) en cours.\n\n" +
                                   $"Veuillez d'abord enregistrer le(s) retour(s).",
                                   "Emprunts en cours",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                // Vérifier s'il y a des emprunts historiques
                var empruntsHistoriques = _bibliothequeData.Emprunts.Where(e => e.Livre == livre).ToList();
                string messageHistorique = empruntsHistoriques.Any() 
                    ? $"\n\nAttention : Ce livre a {empruntsHistoriques.Count} emprunt(s) dans l'historique qui seront également supprimés." 
                    : "";

                // Confirmation
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer ce livre ?\n\n" +
                                            $"Titre : {livre.Titre}\n" +
                                            $"Auteur : {livre.Auteur}\n" +
                                            $"ISBN : {livre.Isbn}{messageHistorique}",
                                            "Confirmation de suppression",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Supprimer tous les emprunts associés
                    foreach (var emprunt in empruntsHistoriques)
                    {
                        _bibliothequeData.Emprunts.Remove(emprunt);
                    }

                    // Supprimer le livre
                    _bibliothequeData.Livres.Remove(livre);

                    // Mettre à jour l'affichage
                    DgLivres.Items.Refresh();
                    DgEmprunts.Items.Refresh();
                    RefreshBindings();
                    UpdateAllStatistics();

                    UpdateStatusBar($"Livre '{livre.Titre}' supprimé");

                    MessageBox.Show($"Livre supprimé avec succès !\n\n" +
                                   (empruntsHistoriques.Any() ? $"{empruntsHistoriques.Count} emprunt(s) historique(s) également supprimé(s)." : ""),
                                   "Suppression réussie",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Supprime un utilisateur sélectionné après vérification des emprunts.
        /// </summary>
        private void BtnSupprimerUtilisateur_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérifier qu'un utilisateur est sélectionné
                if (DgUtilisateurs.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                var utilisateur = DgUtilisateurs.SelectedItem as Utilisateur;

                if (utilisateur == null) return;

                // Vérifier si l'utilisateur a des emprunts en cours
                var empruntsEnCours = _bibliothequeData.Emprunts.Where(e => e.Utilisateur == utilisateur && e.EstEnCours).ToList();
                if (empruntsEnCours.Any())
                {
                    MessageBox.Show($"Impossible de supprimer cet utilisateur.\n\n" +
                                   $"{utilisateur.Nom} {utilisateur.Prenom} a {empruntsEnCours.Count} emprunt(s) en cours.\n\n" +
                                   $"Veuillez d'abord enregistrer le(s) retour(s).",
                                   "Emprunts en cours",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                // Vérifier s'il y a des emprunts historiques
                var empruntsHistoriques = _bibliothequeData.Emprunts.Where(e => e.Utilisateur == utilisateur).ToList();
                string messageHistorique = empruntsHistoriques.Any() 
                    ? $"\n\nAttention : Cet utilisateur a {empruntsHistoriques.Count} emprunt(s) dans l'historique qui seront également supprimés." 
                    : "";

                // Confirmation
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer cet utilisateur ?\n\n" +
                                            $"Nom : {utilisateur.Nom} {utilisateur.Prenom}\n" +
                                            $"Email : {utilisateur.Email}{messageHistorique}",
                                            "Confirmation de suppression",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Supprimer tous les emprunts associés
                    foreach (var emprunt in empruntsHistoriques)
                    {
                        _bibliothequeData.Emprunts.Remove(emprunt);
                    }

                    // Supprimer l'utilisateur
                    _bibliothequeData.Utilisateurs.Remove(utilisateur);

                    // Mettre à jour l'affichage
                    DgUtilisateurs.Items.Refresh();
                    DgEmprunts.Items.Refresh();
                    RefreshBindings();
                    UpdateAllStatistics();

                    UpdateStatusBar($"Utilisateur '{utilisateur.Nom}' supprimé");

                    MessageBox.Show($"Utilisateur supprimé avec succès !\n\n" +
                                   (empruntsHistoriques.Any() ? $"{empruntsHistoriques.Count} emprunt(s) historique(s) également supprimé(s)." : ""),
                                   "Suppression réussie",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Supprime une catégorie sélectionnée si elle n'est pas utilisée.
        /// </summary>
        private void BtnSupprimerCategorie_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérifier qu'une catégorie est sélectionnée
                if (DgCategories.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner une catégorie à supprimer.",
                                   "Sélection requise",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                var categorie = DgCategories.SelectedItem as Categorie;

                if (categorie == null) return;

                // Vérifier si la catégorie est utilisée par des livres
                var livresAssocies = _bibliothequeData.Livres.Where(l => l.Categorie == categorie).ToList();
                if (livresAssocies.Any())
                {
                    MessageBox.Show($"Impossible de supprimer cette catégorie.\n\n" +
                                   $"La catégorie '{categorie.Nom}' est utilisée par {livresAssocies.Count} livre(s).\n\n" +
                                   $"Veuillez d'abord supprimer ou modifier la catégorie de ces livres :\n" +
                                   string.Join("\n", livresAssocies.Take(5).Select(l => $"• {l.Titre}")) +
                                   (livresAssocies.Count > 5 ? $"\n• ... et {livresAssocies.Count - 5} autre(s)" : ""),
                                   "Catégorie utilisée",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                // Confirmation
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer cette catégorie ?\n\n" +
                                            $"Catégorie : {categorie.Nom}",
                                            "Confirmation de suppression",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Supprimer la catégorie
                    _bibliothequeData.Categories.Remove(categorie);

                    // Mettre à jour l'affichage
                    DgCategories.Items.Refresh();
                    RefreshBindings();
                    UpdateAllStatistics();

                    UpdateStatusBar($"Catégorie '{categorie.Nom}' supprimée");

                    MessageBox.Show("Catégorie supprimée avec succès !",
                                   "Suppression réussie",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                               "Erreur",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        #endregion
    }
}