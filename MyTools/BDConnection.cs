using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace MyTools
{
    public class BDConnection
    {
        //propriétés
        private String server;
        private String bdd;
        private String user;
        private String pwd;
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
            this.server = server;
            this.bdd = bdd;
            this.user = user;
            this.pwd = pwd;

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
            return "Server=" + this.server + ";Database=" + this.bdd + ";user id=" + user + ";Pwd=" + this.pwd + ";";
        }

        /// <summary>
        /// Destructeur appelé dès qu'il n'y a plus de référence sur un objet donné, 
        /// ou dans n'importe quel ordre pendant la séquence d'arrêt
        /// </summary>
        public void destruct()
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
        public void reqInsert(string myQuery)
        {
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);
        }

        /// <summary>
        /// Permet d'exécuter une requête UPDATE 
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        public void reqUpdate(string myQuery)
        {
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);
        }

        /// <summary>
        /// Permet d'exécuter une requête DELETE 
        /// </summary>
        /// <param name="myQuery">requête à exécuter</param>
        public void reqDelete(string myQuery)
        {
            //appel de la méthode qui s'occupera de l'exécution de la requête 
            administrateBDD(myQuery);
        }
    }
}
