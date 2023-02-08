using Microsoft.ApplicationBlocks.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace MSSQL.SqlHelper
{
    public sealed class SqlHelper
    {
        private enum SqlConnectionOwnership
        {
            Internal,
            External
        }
        public static string ToQuery(string SpName, params object[] parameterValues)
        {
            string empty = string.Empty;
            empty = empty + SpName + " ";
            for (int i = 0; i < parameterValues.Length; i++)
            {
                empty = empty + " " + parameterValues[i].ToString() + " ,";
            }

            return empty.Trim(',');
        }

        public static void UpdateCommandTimeout(SqlCommand command)
        {
            command.CommandTimeout = 120;
        }

        private SqlHelper()
        {
        }

        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandParameters == null)
            {
                return;
            }

            foreach (SqlParameter sqlParameter in commandParameters)
            {
                if (sqlParameter != null)
                {
                    if ((sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Input) && sqlParameter.Value == null)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }

                    command.Parameters.Add(sqlParameter);
                }
            }
        }

        private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || dataRow == null)
            {
                return;
            }

            int num = 0;
            foreach (SqlParameter sqlParameter in commandParameters)
            {
                if (sqlParameter.ParameterName == null || sqlParameter.ParameterName.Length <= 1)
                {
                    throw new Exception($"Please provide a valid parameter name on the parameter #{num}, the ParameterName property has the following value: '{sqlParameter.ParameterName}'.");
                }

                if (dataRow.Table.Columns.IndexOf(sqlParameter.ParameterName.Substring(1)) != -1)
                {
                    sqlParameter.Value = dataRow[sqlParameter.ParameterName.Substring(1)];
                }

                num++;
            }
        }

        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters == null || parameterValues == null)
            {
                return;
            }

            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            int i = 0;
            for (int num = commandParameters.Length; i < num; i++)
            {
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter dbDataParameter = (IDbDataParameter)parameterValues[i];
                    if (dbDataParameter.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = dbDataParameter.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }

            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }

                command.Transaction = transaction;
            }

            command.CommandType = commandType;
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                return ExecuteNonQuery(sqlConnection, commandType, commandText, commandParameters);
            }
        }

        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteSPWithDefaultValues(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] array = new SqlParameter[parameterValues.Length];
                int num = 0;
                foreach (object obj in parameterValues)
                {
                    array[num] = (SqlParameter)obj;
                    num++;
                }

                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, array);
            }

            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            int result = sqlCommand.ExecuteNonQuery();
            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            int result = sqlCommand.ExecuteNonQuery();
            sqlCommand.Parameters.Clear();
            return result;
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }

        public static long ExecuteNonQueryForTransaction(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                if (ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, hasOutput: true, spParameterSet) > 0)
                {
                    return Convert.ToInt64(spParameterSet[0].Value);
                }

                return -1L;
            }

            return Convert.ToInt64(ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName));
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, bool hasOutput, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            return sqlCommand.ExecuteNonQuery();
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                return ExecuteDataset(sqlConnection, commandType, commandText, commandParameters);
            }
        }

        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            try
            {
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return dataSet;
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }
        }

        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            try
            {
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
                return dataSet;
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTable(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataTable(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public static DataTable ExecuteDataTable(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                return ExecuteDataTable(sqlConnection, commandType, commandText, commandParameters);
            }
        }

        public static DataTable ExecuteDataTable(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataTable(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(connectionString, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTable(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataTable(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static DataTable ExecuteDataTable(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            try
            {
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                sqlCommand.Parameters.Clear();
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return dataTable;
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }
        }

        public static DataTable ExecuteDataTable(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataTable(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(connection, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTable(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataTable(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static DataTable ExecuteDataTable(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            try
            {
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                sqlCommand.Parameters.Clear();
                return dataTable;
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }
        }

        public static DataTable ExecuteDataTable(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataTable(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(transaction, CommandType.StoredProcedure, spName);
        }

        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;
            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
                UpdateCommandTimeout(sqlCommand);
                SqlDataReader result = (connectionOwnership != SqlConnectionOwnership.External) ? sqlCommand.ExecuteReader(CommandBehavior.CloseConnection) : sqlCommand.ExecuteReader();
                bool flag = true;
                foreach (SqlParameter parameter in sqlCommand.Parameters)
                {
                    if (parameter.Direction != ParameterDirection.Input)
                    {
                        flag = false;
                    }
                }

                if (flag)
                {
                    sqlCommand.Parameters.Clear();
                }

                return result;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
        }

        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                return ExecuteReader(sqlConnection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch
            {
                sqlConnection?.Close();
                throw;
            }
        }

        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                return ExecuteScalar(sqlConnection, commandType, commandText, commandParameters);
            }
        }

        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            object result = sqlCommand.ExecuteScalar();
            sqlCommand.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            UpdateCommandTimeout(sqlCommand);
            object result = sqlCommand.ExecuteScalar();
            sqlCommand.Parameters.Clear();
            return result;
        }

        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;
            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
                UpdateCommandTimeout(sqlCommand);
                XmlReader result = sqlCommand.ExecuteXmlReader();
                sqlCommand.Parameters.Clear();
                return result;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            XmlReader result = sqlCommand.ExecuteXmlReader();
            sqlCommand.Parameters.Clear();
            return result;
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] table_names)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                FillDataset(sqlConnection, commandType, commandText, dataSet, table_names);
            }
        }

        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] table_names, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                FillDataset(sqlConnection, commandType, commandText, dataSet, table_names, commandParameters);
            }
        }

        public static void FillDataset(string connectionString, string spName, DataSet dataSet, string[] table_names, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                FillDataset(sqlConnection, spName, dataSet, table_names, parameterValues);
            }
        }

        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] table_names)
        {
            FillDataset(connection, commandType, commandText, dataSet, table_names, (SqlParameter[])null);
        }

        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] table_names, params SqlParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, table_names, commandParameters);
        }

        public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] table_names, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, table_names, spParameterSet);
            }
            else
            {
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, table_names);
            }
        }

        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] table_names)
        {
            FillDataset(transaction, commandType, commandText, dataSet, table_names, (SqlParameter[])null);
        }

        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] table_names, params SqlParameter[] commandParameters)
        {
            FillDataset(transaction.Connection.ConnectionString, commandType, commandText, dataSet, table_names, commandParameters);
        }

        public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] table_names, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, table_names, spParameterSet);
            }
            else
            {
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, table_names);
            }
        }

        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] table_names, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            SqlCommand sqlCommand = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            try
            {
                if (table_names != null && table_names.Length != 0)
                {
                    string text = "Table";
                    for (int i = 0; i < table_names.Length; i++)
                    {
                        if (table_names[i] == null || table_names[i].Length == 0)
                        {
                            throw new ArgumentException("The table_names parameter must contain a list of tables, a value was provided as null or empty string.", "table_names");
                        }

                        sqlDataAdapter.TableMappings.Add(text, table_names[i]);
                        text += i + 1;
                    }
                }

                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }

            if (mustCloseConnection)
            {
                connection.Close();
            }
        }

        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string table_name)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }

            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }

            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }

            if (table_name == null || table_name.Length == 0)
            {
                throw new ArgumentNullException("table_name");
            }

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            try
            {
                sqlDataAdapter.UpdateCommand = updateCommand;
                sqlDataAdapter.InsertCommand = insertCommand;
                sqlDataAdapter.DeleteCommand = deleteCommand;
                sqlDataAdapter.Update(dataSet, table_name);
                dataSet.AcceptChanges();
            }
            finally
            {
                ((IDisposable)(object)sqlDataAdapter)?.Dispose();
            }
        }

        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            SqlCommand sqlCommand = new SqlCommand(spName, connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            if (sourceColumns != null && sourceColumns.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    spParameterSet[i].SourceColumn = sourceColumns[i];
                }

                AttachParameters(sqlCommand, spParameterSet);
            }

            return sqlCommand;
        }

        public static int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQueryTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTableTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataTable(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(connectionString, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTableTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataTable(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(connection, CommandType.StoredProcedure, spName);
        }

        public static DataTable ExecuteDataTableTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataTable(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteDataTable(transaction, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalarTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalarTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length != 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }

            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }
    }
}

