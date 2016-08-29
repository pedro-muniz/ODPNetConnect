using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess;
using Oracle.DataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace DAO
{
    public class ODPNetConnect
    {

        static private string devConnectionString = "SET YOUR DEV CONNECTION STRING";
        static private string productionConnectionString = "SET YOUR PRODUCTION CONNECTION STRING";

        /// <summary>
        /// Os erros de execução são logados nessa variável.
        /// Errors are logged in this variable
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// Warnings são logados nessa variável.
        /// Warning are logged in this variable.
        /// </summary>
        public string WARNING { get; set; }

        private static Dictionary<string, OracleConnection> connectionCache = new Dictionary<string, OracleConnection>(); //take care with cache, it is not working well
        private OracleConnection myConnection;
        private OracleTransaction myTransaction;
        private OracleCommand myCommand;

        /// <summary>
        /// Limpa as variáveis de log
        /// Clean log variables
        /// </summary>
        private void ClearMessages()
        {
            this.ERROR = null;
            this.WARNING = null;
        }

        /// <summary>
        /// Construtor, por default já conecta no banco de dados, caso não queira passe false no parâmetro.
        /// Construct open database connetion by default, if you don't want set startConnection = false
        /// </summary>
        /// <param name="startConnection">
        ///     Conectar no banco de dados, default true
        ///     Open database connection
        /// </param>
        public ODPNetConnect(bool startConnection = false)
        {
            ClearMessages();
            if (!startConnection)
            {
                return;
            }
            
            myConnection = GetConnection();
            if (!String.IsNullOrWhiteSpace(this.ERROR)) //Erro ao conectar. Connection error
            {
                return;
            }

            try
            {
                if (!myConnection.State.ToString().Equals("Open"))
                {
                    myConnection.Open();
                }

                myCommand = myConnection.CreateCommand();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
            }
        }

        /// <summary>
        /// Destrutor fecha a conexão
        /// Destruct close connections by default
        /// </summary>
        ~ODPNetConnect()
        {
            ClearMessages();
            if (myConnection != null && myConnection.State.ToString().Equals("Open"))
            {
                try
                {
                    myConnection.Close();
                }
                catch (Exception e)
                {
                    ERROR = e.Message;
                }
            }
        }

        /// <summary>
        /// Conecta com o banco de dados
        /// Connect to database
        /// </summary>
        /// <param name="env">
        ///     Ambiente do projeto, "dev" ou "production"
        ///     Project Environment, "dev" or "production"
        /// </param>
        /// <param name="cacheOn">
        ///     FIXME: Grava na memória e não refaz a conexão com o banco de dados, se já existir uma. Não ta funcionando
        ///     FIXME: Keep connection and try reuse, it is not working.
        /// </param>
        /// <returns>
        ///     OracleConnection com a conexão aberta
        ///     Returns OracleConnetion with an opened connection
        /// </returns>
        public OracleConnection GetConnection(string env = "dev", bool cacheOn = false)
        {   
            ClearMessages();
            OracleConnection myODPConnection;
            string connectionString = devConnectionString;
            switch (env)
            {
                case "dev":
                    connectionString = devConnectionString;
                    break;
                case "production":
                    connectionString = productionConnectionString;
                    break;
            }

            if (cacheOn && connectionCache.ContainsKey(connectionString) && connectionCache[connectionString] != null)
            {
                return connectionCache[connectionString]; //esse cache nao funciona bem no autocomplete
            }

            try
            {
                myODPConnection = new OracleConnection(connectionString);
                myODPConnection.Open();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return null;
            }

            connectionCache[connectionString] = myODPConnection;
            return myODPConnection;
        }

        /// <summary>
        /// Fecha a conexão com o banco de dados.
        /// Close database connection
        /// </summary>
        /// <param name="conn">
        ///     Variável com o objeto da conexão.
        ///     Connection variable
        /// </param>
        /// <returns>
        ///     True se conseguir fechar a conexão ou False caso não consiga
        ///     True success and False to fail
        /// </returns>
        public bool CloseConnection(OracleConnection conn)
        {
            ClearMessages();
            try
            {
                conn.Close();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Inicia uma transação para a conexão
        /// Start database connection
        /// </summary>
        /// <returns>
        ///     True caso inicie a transaçao ou False caso não inicie
        ///     True if transaction starts ok, otherwise false 
        /// </returns>
        public bool BeginTransaction()
        {
            ClearMessages();
            try
            {
                myTransaction = myConnection.BeginTransaction();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Commit da transação
        /// Transaction commit
        /// </summary>
        /// <returns>True or False</returns>
        public bool Commit()
        {
            ClearMessages();
            if (this.myTransaction == null)
            {
                WARNING = "No transaction found.";
                return true;
            }

            try
            {
                myTransaction.Commit();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rollback da transação
        /// Transaction rollback
        /// </summary>
        /// <returns>True or False</returns>
        public bool Rollback()
        {
            ClearMessages();
            if (this.myTransaction == null)
            {
                WARNING = "No transaction found.";
                return true;
            }

            try
            {
                myTransaction.Rollback();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Use para ler dados do banco de dados
        /// Use to read database data
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Read(string sql)
        {
            ClearMessages();
            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            if (!String.IsNullOrWhiteSpace(this.ERROR))
            {
                return null;
            }

            myCommand = myConnection.CreateCommand();

            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = sql;

            OracleDataReader myDbDataReader;
            try
            {
                myDbDataReader = myCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return null;
            }

            DataTable resultados = new DataTable();
            resultados.Load(myDbDataReader);

            myConnection.Dispose();
            myConnection = null;
            myCommand.Dispose();
            myDbDataReader.Close();
            myDbDataReader.Dispose();
            
            return resultados;
        }

        /// <summary>
        /// Use para escrever no banco de dados
        /// Use to write at database
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int Write(string sql)
        {
            ClearMessages();
            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            if (!String.IsNullOrWhiteSpace(this.ERROR))
            {
                return 0;
            }

            myCommand = myConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = sql;

            if (this.myTransaction != null)
            {
                myCommand.Transaction = myTransaction;
            }

            int rowsAffected = 0;
            try
            {
                rowsAffected = myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }

                return 0;
            }
            myCommand.Dispose();
            return rowsAffected;
        }

        /// <summary>
        /// Use para fazer consultas parametrizadas
        /// Use to read database with parameterized query
        /// </summary>
        /// <param name="sql">
        /// Sql parametrizada 
        /// parameterized query
        /// Ex.: "SELECT * FROM TABLE WHERE PRIMARY_KEY = :PRIMARY_KEY
        /// </param>
        /// <param name="parameters">
        /// Um Dictionary<string, object> com os valores dos parâmetros
        /// A Dictionary with parameters values
        /// Ex.: 
        ///    Dictionary<string, object> parameters = new Dictionary<string, object>();
        ///    parameters["PRIMARY_KEY"] = 1;
        /// </param>
        /// <returns></returns>
        public DataTable ParameterizedRead(string sql, Dictionary<string, object> parameters)
        {
            ClearMessages();
            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            if (!String.IsNullOrWhiteSpace(this.ERROR))
            {
                return null;
            }

            myCommand = myConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = sql;
            myCommand.BindByName = true;

            OracleDataReader myDbDataReader;
            
            foreach (KeyValuePair<string, object> kvp in parameters)
            {
                myCommand.Parameters.Add(new OracleParameter(kvp.Key, kvp.Value));
            }

            try
            {
                myDbDataReader = myCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                myCommand.Parameters.Clear();
                return null;
            }

            DataTable results = new DataTable();
            results.Load(myDbDataReader);
            myCommand.Parameters.Clear();
            myCommand.Dispose();
            myDbDataReader.Close();
            myDbDataReader.Dispose();
            return results;
        }

        //Utilize para Insert, Update e Delete
        //Feito com parametros do OleDbParameter(), http://msdn.microsoft.com/en-us/library/system.data.oledb.oledbparameter.aspx
        public int ParameterizedWrite(string sql, Dictionary<string, object> parametros)
        {
            ClearMessages();
            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            if (!String.IsNullOrWhiteSpace(this.ERROR))
            {
                return 0;
            }

            myCommand = myConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = sql;
            myCommand.BindByName = true;

            if (this.myTransaction != null)
            {
                myCommand.Transaction = myTransaction;
            }

            foreach (KeyValuePair<string, object> kvp in parametros)
            {
                myCommand.Parameters.Add(new OracleParameter(kvp.Key, kvp.Value));
            }

            int rowsAffected = 0;
            try
            {
                rowsAffected = myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                myCommand.Parameters.Clear();
                return 0;
            }

            myCommand.Parameters.Clear();
            myCommand.Dispose();
            return rowsAffected;
        }

        //Interface para o w(DataTable) do ODP.NET
        //http://docs.oracle.com/html/E10927_01/OracleBulkCopyClass.htm#BIGCDJDD
        public bool WriteToServer(DataTable dados, string tabela)
        {
            ClearMessages();
            OracleBulkCopy bulkCopy = new OracleBulkCopy(myConnection);
            bulkCopy.DestinationTableName = tabela;
            bulkCopy.BulkCopyTimeout = 3600;
            try
            {
                bulkCopy.WriteToServer(dados);
            }
            catch (Exception e) 
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return false;
            }
            bulkCopy.Dispose();
            return true;
        }

        //Use sempre que não utilizar consultas parametrizadas.
        public string SafeSqlLiteral(System.Object theValue, System.Object theLevel)
        {
            ClearMessages();
            // Written by user CWA, CoolWebAwards.com Forums. 2 February 2010
            // http://forum.coolwebawards.com/threads/12-Preventing-SQL-injection-attacks-using-C-NET

            // intLevel represent how thorough the value will be checked for dangerous code
            // intLevel (1) - Do just the basic. This level will already counter most of the SQL injection attacks
            // intLevel (2) - (non breaking space) will be added to most words used in SQL queries to prevent unauthorized access to the database. Safe to be printed back into HTML code. Don't use for usernames or passwords

            string strValue = (string)theValue;
            int intLevel = (int)theLevel;

            if (String.IsNullOrWhiteSpace(strValue))
            {
                return strValue;
            }

            if (intLevel > 0)
            {
                strValue = strValue.Replace("'", "\'"); // Most important one! This line alone can prevent most injection attacks
                strValue = strValue.Replace("--", "");
                strValue = strValue.Replace("[", "[[]");
                strValue = strValue.Replace("%", "[%]");
            }

            if (intLevel > 1)
            {
                string[] myArray = new string[] { "xp_ ", "update ", "insert ", "select ", "drop ", "alter ", "create ", "rename ", "delete ", "replace " };
                int i = 0;
                int i2 = 0;
                int intLenghtLeft = 0;
                for (i = 0; i < myArray.Length; i++)
                {
                    string strWord = myArray[i];
                    Regex rx = new Regex(strWord, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = rx.Matches(strValue);
                    i2 = 0;
                    foreach (Match match in matches)
                    {
                        GroupCollection groups = match.Groups;
                        intLenghtLeft = groups[0].Index + myArray[i].Length + i2;
                        strValue = strValue.Substring(0, intLenghtLeft - 1) + "&nbsp;" + strValue.Substring(strValue.Length - (strValue.Length - intLenghtLeft), strValue.Length - intLenghtLeft);
                        i2 += 5;
                    }
                }
            }

            return strValue;
        }

        //funciona para sequencias de inteiro, pega o proximo valor.
        //Entrada é um sql = "select myseq.nextval as sequence from dual", onde apenas o myseq deve ser alterado para o nome da sequencia.
        public int? GetNextSequence(string sql)
        {
            ClearMessages();
            if (String.IsNullOrWhiteSpace(sql))
            {
                return null;
            }

            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            myCommand = myConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = sql;

            OracleDataReader myDbDataReader;
            try
            {
                myDbDataReader = myCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return null;
            }

            int sequence = 0;
            if (myDbDataReader.Read())
            {
                sequence = Int32.Parse(myDbDataReader["sequence"].ToString());
            }
            else
            {
                return null;
            }
            myCommand.Dispose();
            myDbDataReader.Close();
            myDbDataReader.Dispose();
            return sequence;
        }

        public void ClearPools()
        {
            ClearMessages();
            try
            {
                OracleConnection.ClearAllPools();
            }
            catch (Exception e)
            {
                ERROR += e.InnerException.Message;
            }
        }

        public string GetSysDate()
        {
            ClearMessages();
            if (myConnection == null || !myConnection.State.ToString().Equals("Open"))
            {
                myConnection = this.GetConnection();
            }

            myCommand = myConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "SELECT SYSDATE FROM DUAL";

            OracleDataReader myDbDataReader;
            try
            {
                myDbDataReader = myCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                ERROR = e.Message;
                if (e.InnerException != null)
                {
                    ERROR += e.InnerException.Message;
                }
                return null;
            }

            string sysdate = "";
            if (myDbDataReader.Read())
            {
                sysdate = myDbDataReader["SYSDATE"].ToString();
            }
            else
            {
                return null;
            }
            myCommand.Dispose();

            return sysdate;
        }
    }
}
