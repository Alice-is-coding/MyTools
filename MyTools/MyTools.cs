/**
 * Script : Bibliothèque de classes contenant un ensemble d'outils utiles et réutilisables : connexion à une base de données MySql et gestion de base de données MySql, opérations basiques sur les dates.
 * Author : Alice B.
 * Date : 31/03/2019
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace MyTools
{
    /// <summary>
    /// Classe BDConnection :
    /// Permet de se connecter à une base de données MySql, 
    /// et d'exécuter des requêtes basiques sur cette base de données :
    /// des requêtes SELECT ou des requêtes d'administration (INSERT, UPDATE, DELETE).
    /// </summary>
    public class BDConnection
    {
        // Propriétés.
        private static String server;
        private static String bdd;
        private static String user;
        private static String pwd;
        private static MySqlConnection cnx;
        private static BDConnection maCnx = null;
        private static MySqlCommand cmd = null;

        /// <summary>
        /// Constructeur privé, crée l'instance de MySqlConnection pour se connecter à la base de données.
        /// </summary>
        /// <param name="server">Nom du serveur (par exemple : localhost).</param>
        /// <param name="bdd">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        private BDConnection(string server, string bdd, string user, string pwd)
        {
            // Valorisation des propriétés privées.
            BDConnection.server = server;
            BDConnection.bdd = bdd;
            BDConnection.user = user;
            BDConnection.pwd = pwd;

            // Initialisation de la connexion à la base de données.
            InitConnection();

        }

        /// <summary>
        /// Initialise une nouvelle connexion à la base de données.
        /// </summary>
        private void InitConnection()
        {
            cnx = new MySqlConnection
            {
                // Chaîne de connexion à la BDD.
                ConnectionString = GenerateConnectionString()
            };
        }

        /// <summary>
        /// Fonction statique qui crée l'unique instance de la classe BDConnexion.
        /// </summary>
        /// <param name="server"> Nom du serveur (par exemple: localhost).</param>
        /// <param name="bdd"> Nom de la base de données.</param>
        /// <param name="user"> Nom d'utilisateur.</param>
        /// <param name="pwd"> Mot de passe.</param>
        /// <returns> Unique objet de la classe BDConnexion.</returns>
        public static BDConnection GetBDConnection(string server, string bdd, string user, string pwd)
        {
            //Console.WriteLine("Initialisation de connexion...");
            if (maCnx == null)
            {
                maCnx = new BDConnection(server, bdd, user, pwd);
            }
            return maCnx;
        }

        /// <summary>
        /// Génère la chaîne de connexion à la base de données.
        /// </summary>
        /// <returns> La chaîne de connexion.</returns>
        private String GenerateConnectionString()
        {
            return "Server=" + BDConnection.server + ";Database=" + BDConnection.bdd + ";user id=" + user + ";Pwd=" + BDConnection.pwd + ";";
        }

        /// <summary>
        /// Destructeur appelé dès qu'il n'y a plus de référence sur un objet donné, 
        /// ou dans n'importe quel ordre pendant la séquence d'arrêt.
        /// </summary>
        ~BDConnection()
        {
            //Console.WriteLine("Destructeur appelé !");
        }

        /// <summary>
        /// Génère une nouvelle instance de la classe MySqlCommand 
        /// commande pour pouvoir manipuler une base de données (contient la requête et la connexion correspondante).
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter sur la base de données.</param>
        /// <returns> Nouvelle instance de la classe MySqlCommand crée pour répondre aux besoins d'une requête et une connexion à une BDD.</returns>
        private MySqlCommand GetNewMySqlCommand(string myQuery)
        {
            return new MySqlCommand(myQuery, cnx);
        }

        /// <summary>
        /// Création d'une DataTable en fonction du résultat d'une requête stockée dans un curseur MySqlDataReader.
        /// </summary>
        /// <param name="cursor"> Curseur contenant un résultat de requête.</param>
        /// <returns> La DataTable construite avec les valeurs du curseur.</returns>
        private DataTable GetDataTable(MySqlDataReader cursor)
        {
            // Declarations.
            // Création d'une table DataTable.
            DataTable table = new DataTable();
            // Compteur. 
            int i = 1;

            while (cursor.Read())
            {
                // Création d'une ligne. 
                DataRow row = table.NewRow();
                // Si on est à la première ligne (compteur == 1) :
                if (i == 1)
                {
                    // Parcours des colonnes du curseur (FieldCount == Nb de colonne dans le curseur).
                    for (int x = 0; x < cursor.FieldCount; x++)
                    {
                        // Création d'une colonne ayant pour nom le nom de la colonne de la table dans la BDD.
                        DataColumn column = new DataColumn(cursor.GetName(x).ToString());
                        // Ajout de la colonne à la DataTable.
                        table.Columns.Add(column);
                        // Affectation d'une valeur à la cellule de la ligne correspondant à la colonne.
                        row[column.ColumnName] = cursor.GetValue(x).ToString();
                    }
                    // Ajout de la ligne à la DataTable.
                    table.Rows.Add(row);
                    // Incrémentation du compteur.
                    i++;
                }
                else
                {
                    // Nous sommes à ligne > 1 du curseur donc inutile d'ajouter des colonnes.
                    for (int x = 0; x < cursor.FieldCount; x++)
                    {
                        // Affectation d'une valeur à la cellule de la ligne correspondant à la colonne.
                        row[table.Columns[x]] = cursor.GetValue(x).ToString();
                    }
                    // Ajout de la ligne à la DataTable.
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        /// <summary>
        /// Effectue une requête SELECT sur la base de données 
        /// puis remplis un Dictionnaire : (clé : nomColonne, valeur : colonneValue).
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter.</param>
        /// <returns> Le Dictionnaire rempli avec le résultat de la requête.</returns>
        public DataTable ReqSelect(string myQuery)
        {
            // Declarations.  
            // Création d'une table DataTable.  
            var table = new DataTable();
            // Declaration de l'objet curseur.
            MySqlDataReader cursor = null;
            // Instanciation d'un nouvel objet de la classe MySqlCommand qui contiendra la requête et la connexion à la BDD.
            cmd = GetNewMySqlCommand(myQuery);

            try
            {
                // Ouverture de connexion MySql.
                cnx.Open();
                Console.WriteLine("Connexion établie.");

                // Liaison DataReader avec la commande (requête).
                cursor = cmd.ExecuteReader();

                // Si le curseur n'est pas vide.
                if (cursor.HasRows)
                {
                    // Valorisation de table avec une DataTable créée.
                    table = GetDataTable(cursor);
                }
                else
                {
                    Console.WriteLine("0 lignes retournées...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur est survenue...");
                Console.WriteLine(e + "\n" + e.StackTrace);
            };
            Console.WriteLine("Fermeture de connexion...");
            // Fermeture du curseur.
            cursor.Close();
            // Fermeture de la connexion MySql.
            cnx.Close();
            Console.WriteLine("Connexion terminée.\n");
            // On retourne le résultat de la requête sous forme d'une DataTable.
            return table;
        }

        /// <summary>
        /// Exécute une requête d'administration de base de données (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter.</param>
        /// <returns>Chaîne contenant les informations sur le bon ou mauvais déroulement de la requête.</returns>
        private string AdministrateBDD(string myQuery)
        {
            // Instanciation d'un nouvel objet de la classe MySqlCommand qui contiendra la requête et la connexion à la BDD.
            cmd = GetNewMySqlCommand(myQuery);
            // Chaîne qui sera retournée.
            string txtResult = "";

            try
            {
                // Connexion à la BDD en passant directement par l'objet Command cmd
                cmd.Connection.Open();
                //Console.WriteLine("Connexion établie.");
                txtResult += "Connexion établie.\n";
                // Exécution de la requête de modification (INSERT, UPDATE, ou DELETE) et stockage dans la variable retVal,
                // qui, en plus d'exécuter la requête, contient le nombre de lignes affectées par la requête.
                var retVal = cmd.ExecuteNonQuery();
                //Console.WriteLine("Requête exécutée. \nNombre de lignes affectées : " + retVal);
                txtResult += "Requête exécutée. \nNombre de lignes affectées : " + retVal + "\n";
            }
            catch (Exception e)
            {
                //Console.WriteLine("Une exception s'est produite... ");
                txtResult += "Une exception s'est produite... \n";
                //Console.WriteLine(e + "\n" + e.StackTrace);
                txtResult += e + "\n" + e.StackTrace;
                return txtResult;
            }
            // Fermeture de la connexion.
            //Console.WriteLine("Fermeture de connexion...");
            txtResult += "Fermeture de connexion...\n";
            cmd.Connection.Close();
            //Console.WriteLine("Connexion terminée.\n");
            txtResult += "Connexion terminée.\n";
            return txtResult;
        }

        /// <summary>
        /// Permet d'exécuter une requête INSERT. 
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter.</param>
        /// <returns>Chaîne contenant les informations sur le bon ou mauvais déroulement de la requête.</returns>
        public string ReqInsert(string myQuery) =>
            // Appel de la méthode qui s'occupera de l'exécution de la requête. 
            AdministrateBDD(myQuery);

        /// <summary>
        /// Permet d'exécuter une requête UPDATE. 
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter.</param>
        /// <returns>Chaîne contenant les informations sur le bon ou mauvais déroulement de la requête.</returns>
        public string ReqUpdate(string myQuery) =>
            // Appel de la méthode qui s'occupera de l'exécution de la requête. 
            AdministrateBDD(myQuery);

        /// <summary>
        /// Permet d'exécuter une requête DELETE. 
        /// </summary>
        /// <param name="myQuery"> Requête à exécuter.</param>
        /// <returns>Chaîne contenant les informations sur le bon ou mauvais déroulement de la requête.</returns>
        public void ReqDelete(string myQuery) =>
            // Appel de la méthode qui s'occupera de l'exécution de la requête. 
            AdministrateBDD(myQuery);
    }

    /// <summary>
    /// Classe DateManagement : 
    /// Permet d'effectuer des actions basiques sur une date, 
    /// telles qu'obtenir le mois précédent, suivant, 
    /// ou de savoir si un jour se trouve entre deux jours.
    /// </summary>
    public abstract class DateManagement
    {
        /// <summary>
        /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois précédent 
        /// par rapport à la date d'aujourd'hui.
        /// </summary>
        /// <returns> Le mois précédent.</returns>
        public static string GetPreviousMonth() => GetPreviousMonth(DateTime.Today);

        /// <summary>
        /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois précédent 
        /// par rapport à la date passée en paramètre.
        /// </summary>
        /// <param name="date"> Date pour laquelle on souhaite obtenir le mois précédent.</param>
        /// <returns> Le mois précédent.</returns>
        public static string GetPreviousMonth(DateTime date)
        {
            // Declarations. 
            string month = date.Month.ToString();

            // Si date == Janvier :
            if (int.Parse(month) == 1)
            {
                // Mois précédent = Décembre.
                month = 12.ToString();
            }
            else
            {
                // Sinon on décrémente de 1 la variable month.
                month = (int.Parse(month) - 1).ToString();
            }
            // Test si la chaîne ne contient qu'un chiffre.
            if (month.Length == 1)
            {
                // Si la variable ne contient qu'un chiffre on ajoute 0 devant.
                month = "0" + month;
            }
            // On retourne le mois précédent.
            return month;
        }

        /// <summary>
        /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois suivant
        /// par rapport à la date d'aujourd'hui.
        /// </summary>
        /// <returns> Le mois suivant.</returns>
        public static string GetNextMonth() => GetNextMonth(DateTime.Today);

        /// <summary>
        /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois suivant
        /// par rapport à la date envoyée en paramètre.
        /// </summary>
        /// <param name="date"> Date pour laquelle il faut obtenir le mois suivant.</param>
        /// <returns> Le mois suivant.</returns>
        public static string GetNextMonth(DateTime date)
        {
            string month = date.Month.ToString();

            // Si mois == Décembre :
            if (int.Parse(month) == 12)
            {
                // Mois == Janvier.
                month = 1.ToString();
            }
            else
            {
                // Sinon incrémentation de 1 de la variable month.
                month = (int.Parse(month) + 1).ToString();
            }
            // Si month ne contient qu'un chiffre :
            if (month.Length == 1)
            {
                // Ajout de 0 devant le chiffre.
                month = "0" + month;
            }
            // On retourne le mois suivant.
            return month;
        }

        /// <summary>
        /// Distingue si la date du jour se trouve entre deux jours passés en paramètre.
        /// </summary>
        /// <param name="day1"> Interval1 (numéro de jour).</param>
        /// <param name="day2"> Interval2 (numéro de jour).</param>
        /// <returns> Vrai si le jour de la date actuelle se situe entre les deux intervalles faux sinon.</returns>
        public static bool Between(int day1, int day2) => Between(day1, day2, DateTime.Today);

        /// <summary>
        /// Distingue si la date passée en paramètre se trouve entre deux jours également passés en paramètre.
        /// </summary>
        /// <param name="day1"> Interval1 (numéro de jour).</param>
        /// <param name="day2"> Interval2 (numéro de jour).</param>
        /// <param name="date"> Date pour laquelle le jour est à tester.</param>
        /// <returns> Vrai si le jour de la date se trouve entre les deux intervalles faux sinon.</returns>
        public static bool Between(int day1, int day2, DateTime date)
        {
            // Declarations.
            // Variable day valorisé avec le jour de la date passée en paramètre.
            int day = date.Day;

            // Si le jour se trouve entre min jour et max jour :
            if ((day >= Math.Min(day1, day2)) && (day <= Math.Max(day1, day2)))
            {
                // On retourne vrai.
                return true;
            }
            else
            {
                // Sinon on retourne faux.
                return false;
            }
        }
    }

    /// <summary>
    /// Classe DirAppend. 
    /// Permet de gérer un système de log en permettant l'écriture dans un ficher txt servant de log.
    /// </summary>
    public abstract class DirAppend
    {
        /// <summary>
        /// Permet d'afficher le contenu du fichier dont le nom est passé en param.
        /// </summary>
        /// <param name="fileNamePlusExtension">Nom du fichier à ouvrir.</param>
        public static void ShowLog(String fileNamePlusExtension)
        {
            using (StreamReader r = File.OpenText(fileNamePlusExtension))
            {
                DumpLog(r);
            }
        }

        /// <summary>
        /// Ajoute du texte à un fichier existant.
        /// </summary>
        /// <param name="path">Chemin vers le fichier.</param>
        /// <param name="logMessage">Informations à écire dans le fichier.</param>
        public static void AppendText(String path, String logMessage)
        {
            using (var sw = File.AppendText(path))
            {
                Log(logMessage, sw);
            }
        }

        /// <summary>
        /// Ecriture dans un fichier d'un chemin précis.
        /// </summary>
        /// <param name="path">Chemin vers le fichier.</param>
        /// <param name="logMessage">Informations à écrire dans le fichier.</param>
        public static void WriteLog(String path, String logMessage)
        {
            if (!File.Exists(path))
            {
                using (var sw = File.CreateText(path))
                {
                    Log(logMessage, sw);
                }
            } else
            {
                AppendText(path, logMessage);
            }
        }

        /// <summary>
        /// Ecriture d'informations dans un fichier de logs.
        /// </summary>
        /// <param name="logMessage">Message à écrire dans le fichier.</param>
        /// <param name="w">Contient le fichier dans lequel il faut écrire.</param>
        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        /// <summary>
        /// Permet d'afficher le contenu d'un fichier de logs.
        /// </summary>
        /// <param name="r">Permet d'accéder au contenu de la ligne.</param>
        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}

