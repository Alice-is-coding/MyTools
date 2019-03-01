using System;
using System.Collections.Generic;
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
            initConnexion();

        }

        /// <summary>
        /// Initialise une nouvelle connexion à la base de données
        /// </summary>
        public void initConnexion()
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
        public static BDConnection GetBDConnexion(string server, string bdd, string user, string pwd)
        {
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

        public Dictionary<string, string> reqSelect(string myQuery)
        {
            //declaration 
            Dictionary<string, string> resultQuery = new Dictionary<string, string>();
            //instanciation d'un nouvel objet de la classe MySqlCommand qui contiendra la requête et la connexion à la BDD
            cmd = GetNewMySqlCommand(myQuery);

            //declaration de l'objet curseur 
            MySqlDataReader cursor = null;

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
                    //Tant qu'il y a des lignes à lire
                    while (cursor.Read())
                    {
                        //actions sur les lignes
                        //parcours des colonnes 
                        for (int x = 0; x < cursor.FieldCount; x++)
                        {
                            //ajout {nomChamp : valeur} dans le dictionnaire des résultats de la requête
                            resultQuery.Add(cursor.GetName(x).ToString(), cursor.GetValue(x).ToString());
                        }
                        //passage à la ligne suivante
                        cursor.NextResult();
                    }
                }
                else
                {
                    Console.WriteLine("0 lignes retournées...");
                }
                //fermeture du curseur
                cursor.Close();
                //fermeture de la connexion MySql
                cnx.Close();
                //on retourne le resultat de la requête sous forme d'un dictionnaire
                return resultQuery;
            }
            catch (Exception e)
            {
                Console.WriteLine("La connexion n'a pas pu s'établir...");
                Console.WriteLine(e.StackTrace);
            };
            return resultQuery;
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
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Une exception s'est produite... ");
                Console.WriteLine(e + "\n" + e.StackTrace);
            }
            //fermeture de la connexion
            cmd.Connection.Close();
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
