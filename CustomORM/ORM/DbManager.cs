using CustomORM.Attributes;
using CustomORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM.ORM
{
    public class DbManager : IDbManager
    {
        private SqlConnection connection;
        private readonly string connectionString;

        public DbManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Добавление новой строки в БД
        public void Add<T>(object entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException("Entity can't be null");
            }

            string commandString = CreateInsertCommand<T>(entity);
            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand insertValuesCommand = new SqlCommand(commandString.ToString(), connection);
                insertValuesCommand.ExecuteNonQuery();
            }
        }

        public string CreateInsertCommand<T>(object entity)
        {
            Type type = entity.GetType();
            StringBuilder commandString = new StringBuilder();
            StringBuilder fieldsString = new StringBuilder();
            StringBuilder valuesString = new StringBuilder();
            commandString.Append($"INSERT INTO {type.Name} (");
            // Считывание полей (и наследуемых полей) и значений в классе. 
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;
                // Если есть поле со значением ID и при этом такого ID ещё нет в бд или id=0, ID = MaxId+1
                fieldsString.Append($"{fi.Name}, ");
                if (fi.Name == "ID" && (Find<T>(fi.GetValue(entity).ToString()) == null || fi.GetValue(entity).ToString() == "0"))
                {
                    fi.SetValue(entity, GetLastId(type) + 1);
                }
                if (fi.GetValue(entity) is string)
                    valuesString.Append($"'{fi.GetValue(entity).ToString()}', ");
                else
                    valuesString.Append($"{fi.GetValue(entity).ToString()}, ");
            }

            // Удаление пробела и запятой в конце
            fieldsString.Remove(fieldsString.Length - 2, 2);
            valuesString.Remove(valuesString.Length - 2, 2);
            commandString.Append(fieldsString + ")");
            commandString.Append($" VALUES (" + valuesString + ") ");
            return commandString.ToString();
        }

        public int GetLastId(Type type)
        {
            object id;
            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT MAX(ID) FROM {type.Name}";
                SqlCommand getLastIdCommand = new SqlCommand(query, connection);
                id = getLastIdCommand.ExecuteScalar();
                if(id.ToString() == "")
                {
                    return 0;
                }
            }
            return (int)id;
        }

        // Поиск по ПК    
        public T Find<T>(string pk)
        {
            return Find<T>(pk, null);
        }

        public T Find<T>(string pk, string where)
        {
            T obj = default(T);
            Type type =typeof(T);
            string pkRow = FindPKColumn(type);
            string commandString = $"SELECT * FROM {type.Name} WHERE {pkRow} = {pk}";
            if (where != null)
            {
                commandString += $"ADD {where}";
            }

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(commandString, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return default(T);
                    }

                    reader.Read();
                    obj = NewEntity<T>(reader);
                }
            }

            return obj;
        }

        // Нахождение PK по атрибуту
        public string FindPKColumn(Type type)
        {
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                Attribute attr = fi.GetCustomAttribute(typeof(PKAttribute));
                if (attr != null)
                    return fi.Name;
            }

            foreach (PropertyInfo fi in type.BaseType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                Attribute attr = fi.GetCustomAttribute(typeof(PKAttribute));
                if (attr != null)
                    return fi.Name;
            }
            throw new InvalidOperationException($"There is no PK in entity");
        }

        // Создание объекта из БД
        public T NewEntity<T>(SqlDataReader reader)
        {
            // Получение колонок, типов и значений полей
            object[] columns = new object[reader.FieldCount];
            reader.GetValues(columns);
            Type[] types = new Type[columns.Length - 1];
            object[] fieldValues = new object[columns.Length - 1];
            
            for (int i = 1; i < columns.Length; i++)
            {
                types[i - 1] = columns[i].GetType();
                fieldValues[i - 1] = columns[i];
            }
            // Вызов соответствующего конструктора
            T createdObject = (T)typeof(T).GetConstructor(types).Invoke(fieldValues);
            return createdObject;
        }

        // Получение всех данных с БД
        public IEnumerable<T> FindAll<T>()
        {
            return FindAll<T>(null);
        }

        public IEnumerable<T> FindAll<T>(string where)
        {
            List<T> entities = new List<T>();
            string selectionString = $"SELECT * FROM {typeof(T).Name}";
            if (where != null)
            {
                selectionString += $" WHERE {where}";
            }

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand selectionCommand = new SqlCommand(selectionString, connection);
                SqlDataReader reader = selectionCommand.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                    {
                        entities.Add(NewEntity<T>(reader));
                    }
                }
            }

            return entities;
        }

        public void Remove<T>(object entity)
        {
            Type type = entity.GetType();
            string pkRow = FindPKColumn(type);
            string pk = "";
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.Name == pkRow)
                    pk = fi.GetValue(entity).ToString();
            }
            Remove<T>(pk);
        }

        // Удаление объекта по PK.
        public void Remove<T>(string pk)
        {
            Type type = typeof(T);
            string pkRow = FindPKColumn(type);
            string commandString = $"DELETE FROM {typeof(T).Name} WHERE {pkRow} = {pk}";
            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(commandString, connection);
                int num = command.ExecuteNonQuery();

                if (num == 0)
                {
                    throw new ArgumentException($"PK {pk} doesn't exist");
                }
            }
        }

        public void Update(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity can't be null");
            }

            int numberUpdatedRows;
            Type type = entity.GetType();
            string pkRow = FindPKColumn(type);
            string pk = "";
            StringBuilder commandString = new StringBuilder();
            StringBuilder setString = new StringBuilder();
            commandString.Append($"UPDATE {type.Name} SET ");
            // Считывание полей (и наследуемых полей) и значений в классе 
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;
                setString.Append($"{fi.Name} = ");
                if (fi.Name == pkRow)
                    pk = fi.GetValue(entity).ToString();
                if (fi.GetValue(entity) is string)
                    setString.Append($"'{fi.GetValue(entity).ToString()}', ");
                else
                    setString.Append($"{fi.GetValue(entity).ToString()}, ");
            }
            // Удаление пробела и запятой в конце
            setString.Remove(setString.Length - 2, 2);
            commandString.Append(setString + " ");
            commandString.Append($" WHERE {pkRow} = {pk}");
            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand insertValuesCommand = new SqlCommand(commandString.ToString(), connection);
                numberUpdatedRows = insertValuesCommand.ExecuteNonQuery();
            }

            if (numberUpdatedRows < 1)
            {
                throw new Exception("Row wasn't updated.");
            }
        }
    }
}
