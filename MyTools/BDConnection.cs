using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace MyTools
{
    public class BDConnection
    {
        //propriétés
        private static String server;
        private static String bdd;
        private static String user;
        private static String pwd;
        private static MySqlConnection cnx;
        private static BDConnection maCnx = null;
        private static MySqlCommand cmd = null;

        /// <summary>
        /// Constructeur privé, crée l'instance de MySqlConnection pour se connecter à la base de données
        /// </summary>
        /// <param name="server">nom du serveur (par exemple : localhost)</param>
        /// <param name="bdd">nom de la base de données</param>
        /// <param name="user">nom d'utilisateur</param>
        /// <param name="pwd">mot de passe</param>
        private BDConnection(string server, string bdd, string user, string pwd)
        {
            //valorisation des propriétés privées
            BDConnection.server = server;
            BDConnection.bdd = bdd;
            BDConnection.user = user;
            BDConnection.pwd = pwd;

            //initialisation de la connexion à la base de données
            initConnection();

        }

        /// <summary>
        /// Initialise une nouvelle connexion à la base de données
        /// </summary>
        private void initConnection()
        {
            cnx = new MySqlConnection
            {
                //chaîne de connexion à la BDD
                ConnectionString = generateConnectionString()
            };
        }

        /// <summary>
        /// Fonction statique qui crée l'unique instance de la classe BDConnexion
        /// </summary>
        /// <param name="server">nom du serveur (par exemple: localhost)</param>
        /// <param name="bdd">nom de la base de données</param>
        /// <param name="user">nom d'utilisateur</param>
        /// <param name="pwd">mot de passe</param>
        /// <returns>unique objet de la classe BDConnexion</returns>
        public static BDConnection GetBDConnection(string server, string bdd, string user, string pwd)
        {
            Console.WriteLine("Initialisation de connexion...");
            if (maCnx == null)
            {
                maCnx = new BDConnection(server, bdd, user, pwd);
            }
            return maCnx;
        }

        /// <summary>
        /// Génère la chaîne de connexion à la base de données
        /// </summary>
        /// <returns>String   la chaîne de connexion</returns>
        private String generateConnectionString()
        {
            return "Server=" + BDConnection.server + ";Database=" + BDConnection.bdd + ";user id=" + user + ";Pwd=" + BDConnection.pwd + ";";
        }

        /// <summary>
        /// Destructeur appelé dès qu'il n'y a plus de référence sur un objet donné, 
        /// ou dans n'importe quel ordre pendant la séquence d'arrêt
        /// </summary>
        private void destruct()
        {
            cnx = null;
        }

        /// <summary>
        /// Génère une nouvelle instance de la classe MySqlCommand 
        /// commande pour pouvoir manipuler une base de données (la requête et la connexion correspondante)
        /// </summary>
        /// <param name="myQuery">requête à exécuter sur la base de données</param>
        /// <returns>nouvelle instance de la classe MySqlCommand crée pour répondre aux besoins d'une requête et une connexion à une BDD</returns>
        private MySqlCommand GetNewMySqlCommand(string myQuery)
        {
            return new MySqlCommand(myQuery, cnx);
        }

        /// <summary>
        /// Création d'une DataTable en fonction du résultat d'une requête stockée dans un curseur MySqlDataReader
        /// </summary>
        /// <param name="cursor">curseur contenant un résultat de requête</param>
        /// <returns>la DataTable construite avec les valeurs du curseur</returns>
        private DataTable GetDataTable(MySqlDataReader cursor)
        {
            //declarations 
            DataTable table = new DataTable(); //création d'une table DataTable 
            int i = 1; //compteur 

            while (cursor.Read())
            {
                DataRow row = table.NewRow(); //création d'une ligne 
                //si on est à la première ligne (compteur == 1)
                if (i == 1)
                {
                    //parcours des colonnes du curseur (FieldCount == Nb de colonne dans le curseur)
                    for (int x = 0; x < cursor.FieldCount; x++)
                    {
                        DataColumn column = new DataColumn(cursor.GetName(x).ToString()); //création d'une colonne ayant pour nom le nom de la colonne de la table dans la BDD
                        table.Columns.Add(column); //ajout de la colonne à la DataTable 
                        row[column.ColumnName] = cursor.GetValue(x).ToString(); //affectation d'une valeur à la cellule de la ligne correspondant à la colonne 
                    }
                    table.Rows.Add(row); //ajout de la ligne à la DataTable 
                    i++; //incrémentation du compteur 
                }
                else
                {
                    //nous sommes à ligne > 1 du curseur donc inutile d'ajouter des colonnes 
                    for (int x = 0; x < cursor.FieldCount; x++)
                    {
                        row[table.Columns[x]] = cursor.GetValue(x).ToString(); //affectation d'une valeur à la cellule de la ligne correspondant à la colonne 
                    }
                    table.Rows.Add(row); //ajout de la ligne à la DataTable 
                }
            }
            return table; 
        }

        /// <summary>
        /// Effectue une requête SELECT sur la base de données 
        /// puis remplis un Dictionnaire : (clé : nomColonne, valeur : colonneValue)
        /// </summary>
        /// <param name="myQuery">Requête à exécuter</param>
        /// <returns>Le Dictionnaire rempli avec le résultat de la requête</returns>
        public DataTable reqSelect(string myQuery)
        {
            //declaration  
            var table = new DataTable();  //création d'une table DataTable                                      
            MySqlDataReader cursor = null; //declaration de l'objet curseur 
            //instanciation d'un nouvel objet de la classe MySqlCommand qui contiendra la requête et la connexion à la BDD
            cmd = GetNewMySqlCommand(myQuery);

            try
            {
                //ouverture de connexion MySql
                cnx.Open();
                Console.WriteLine("Connexion établie.");

                //liaison DataReader avec la commande (requête)
                cursor = cmd.ExecuteReader();

                //si le curseur n'est pas vide 
                if (cursor.HasRows)
                {
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
            //fermeture du curseur
            cursor.Close();
            //fermeture de la connexion MySql
            cnx.Close();
            Console.WriteLine("Connexion terminée.");
            //on retourne le résultat de la requête sous forme d'un Dictionnaire
            //return resultQuery;
            return table; 
        }

        /// <summary>
        /// Exécute une requête d'administration de base de données (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        private void administrateBDD(string myQuery)
        {
            //instanciation d'un nouvel objet de la classe MySqlCommand qui contiendra la requête et la connexion à la BDD
            cmd = GetNewMySqlCommand(myQuery);

            try
            {
                cmd.Connection.Open();
                Console.WriteLine("Connexion établie.");
                cmd.ExecuteNonQuery();
                Console.WriteLine("Requête exécutée.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Une exception s'est produite... ");
                Console.WriteLine(e + "\n" + e.StackTrace);
            }
            Console.WriteLine("Fermeture de connexion...");
            //fermeture de la connexion
            cmd.Connection.Close();
            Console.WriteLine("Connexion terminée.");
        }

        /// <summary>
        /// Permet d'exécuter une requête INSERT 
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        public void reqInsert(string myQuery) =>
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);

        /// <summary>
        /// Permet d'exécuter une requête UPDATE 
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        public void reqUpdate(string myQuery) =>
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);

        /// <summary>
        /// Permet d'exécuter une requête DELETE 
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        public void reqDelete(string myQuery) =>
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);
    }
}

public abstract class DateManagement
{
    /// <summary>
    /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois précédent 
    /// par rapport à la date d'aujourd'hui
    /// </summary>
    /// <returns>le mois précédent</returns>
    public static string GetPreviousMonth() => GetPreviousMonth(DateTime.Today);

    /// <summary>
    /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois précédent 
    /// par rapport à la date passée en paramètre
    /// </summary>
    /// <param name="date">date pour laquelle on souhaite obtenir le mois précédent</param>
    /// <returns>le mois précédent</returns>
    public static string GetPreviousMonth(DateTime date)
    {
        //declarations 
        string month = date.Month.ToString();

        //si date == Janvier
        if(int.Parse(month) == 1)
        {
            //mois précédent = Décembre
            month = 12.ToString();
        }else
        {
            //sinon on décrémente de 1 la variable previousMonth
            month = (int.Parse(month)- 1).ToString();
        }
        //test si la chaîne ne contient qu'un chiffre
        if(month.Length == 1)
        {
            //si la variable ne contient qu'un chiffre on ajoute 0 devant
            month = "0" + month;
        }
        //on retourne le mois précédent
        return month;
    }

    /// <summary>
    /// Retourne sous forme d'une chaîne de 2 chiffres le numéro du mois suivant 
    /// par rapport à la date d'aujourd'hui
    /// </summary>
    /// <returns>le mois suivant</returns>
    public static string GetNextMonth() => GetNextMonth(DateTime.Today);

    /// <summary>
    /// Retourn sous forme d'une chaîne de 2 chiffres le numéro du mois suivant
    /// par rapport à la date envoyée en paramètre
    /// </summary>
    /// <param name="date">date pour laquelle il faut obtenir le mois suivant</param>
    /// <returns>le mois suivant</returns>
    public static string GetNextMonth(DateTime date)
    {
        string month = date.Month.ToString();

        //si mois == Décembre
        if (int.Parse(month) == 12)
        {
            //mois == Janvier
            month = 1.ToString();
        }
        else
        {
            //sinon incrémentation de 1 de la variable month
            month = (int.Parse(month) + 1).ToString();
        }
        //si month ne contient qu'un chiffre
        if (month.Length == 1)
        {
            //ajout de 0 devant le chiffre
            month = "0" + month;
        }
        //on retourne le mois suivant
        return month;
    }

    /// <summary>
    /// Distingue si la date du jour se trouve entre deux jours passés en paramètre
    /// </summary>
    /// <param name="day1">interval1 (numéro de jour)</param>
    /// <param name="day2">interval2 (numéro de jour)</param>
    /// <returns>vrai si le jour de la date actuelle se situe entre les deux intervalles faux sinon</returns>
    public static bool entre(int day1, int day2) => between(day1, day2, DateTime.Today);

    /// <summary>
    /// Distingue si la date passée en paramètre se trouve entre deux jours également passés en paramètre
    /// </summary>
    /// <param name="day1">interval1 (numéro de jour)</param>
    /// <param name="day2">interval2 (numéro de jour)</param>
    /// <param name="date">date pour laquelle le jour est à tester</param>
    /// <returns>vrai si le jour de la date se trouve entre les deux intervalles faux sinon</returns>
    public static bool between(int day1, int day2, DateTime date)
    {
        //declarations
        int day = date.Day; //valorisé avec le jour de la date passée en paramètre

        //si le jour se trouve entre min jour et max jour
        if(day > Math.Min(day1, day2) && day < Math.Max(day1, day2))
        {
            return true; 
        }else
        {
            return false; 
        }
    }
}
