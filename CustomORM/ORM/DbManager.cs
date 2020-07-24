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
            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();

                SqlCommand insertValuesCommand = new SqlCommand(commandString.ToString(), connection);
                insertValuesCommand.ExecuteNonQuery();
            }
        }

        private string CheckTableName(Type type, out bool flag)
        {
            flag = false;
            string tableName = type.Name;
            if (type.BaseType.Name != "Object")
            {
                flag = true;
                tableName = type.BaseType.Name;
            }

            return tableName;
        }

        private string CreateInsertCommand<T>(object entity)
        {
            bool flag;
            Type type = entity.GetType();
            string tableName = CheckTableName(type, out flag);
           
            StringBuilder commandString = new StringBuilder();
            StringBuilder fieldsString = new StringBuilder();
            StringBuilder valuesString = new StringBuilder();
            commandString.Append($"INSERT INTO {tableName} (");
            // Считывание полей и значений в классе. 
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;
                // Если есть поле со значением ID и при этом такого ID ещё нет в бд или id=0, ID = MaxId+1
                fieldsString.Append($"{fi.Name}, ");
                if (fi.Name == "ID" && (Find<T>(fi.GetValue(entity).ToString()) == null || fi.GetValue(entity).ToString() == "0"))
                {
                    fi.SetValue(entity, GetLastId(tableName) + 1);
                }
                if (fi.GetValue(entity) is string)
                    valuesString.Append($"'{fi.GetValue(entity).ToString()}', ");
                else
                    valuesString.Append($"{fi.GetValue(entity).ToString()}, ");
            }

            if (flag)
            {
                fieldsString.Append("Discriminator");
                valuesString.Append($"'{type.Name}'");
            }
            else
            {
            // Удаление пробела и запятой в конце
            fieldsString.Remove(fieldsString.Length - 2, 2);
            valuesString.Remove(valuesString.Length - 2, 2);
            }
            commandString.Append(fieldsString + ")");
            commandString.Append($" VALUES (" + valuesString + ") ");
            return commandString.ToString();
        }

        private int GetLastId(string tableName)
        {
            object id;
            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();
                string query = $"SELECT MAX(ID) FROM {tableName}";
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
            Type type = typeof(T);
            T obj = default(T);
            type =typeof(T);
            string pkRow = FindPKColumn(type);
            bool flag;
            string tableName = CheckTableName(type, out flag);
            string commandString = $"SELECT * FROM {tableName} WHERE {pkRow} = {pk}";
            if (where != null)
            {
                commandString += $"ADD {where}";
            }

            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();
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
        private string FindPKColumn(Type type)
        {
            foreach (PropertyInfo fi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
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
            Type type = typeof(T);
            // Флаг указывает на то, есть ли у класса наследник, объект которого нужно создать
            bool flag = false;
            string nameOfClass = "";
            // Получение колонок, типов и значений полей
            object[] columns = new object[reader.FieldCount];
            reader.GetValues(columns);
            List<Type> types = new List<Type>();
            List<object> fieldValues = new List<object>();
            // Переменная хранит количество параметров в классе
            var propCount = type.GetProperties().Where(pi => pi.GetCustomAttribute(typeof(IgnoreAttribute), true) == null).Count();
            for (int i = 0, j = 0; i < columns.Length && j < propCount + 1; i++, j++)
            {
                if (reader.GetName(i) != "Discriminator")
                {
                    types.Add(columns[i].GetType());
                    fieldValues.Add(columns[i]);
                }
                else
                {
                    flag = true;
                    nameOfClass = columns[i].ToString();
                }
            }

            T createdObject = default(T);
            if (flag)
            {
                // Считывает со сборки все подклассы указанного класса и выбирает подходящий
                var typesOfClasses = Assembly.GetAssembly(typeof(T)).GetExportedTypes().Where(i => i.IsSubclassOf(typeof(T)));
                foreach (Type t in typesOfClasses)
                {
                    if (t.Name == nameOfClass)
                    {
                        // Вызов соответствующего конструктора
                        createdObject = (T)FindConstructor(t, types.ToArray().Length).Invoke(fieldValues.ToArray());
                    }
                }
            }

            else
            {
                // Вызов соответствующего конструктора
                createdObject = (T)FindConstructor(typeof(T), types.ToArray().Length).Invoke(fieldValues.ToArray());
            }

            propCount = flag ? propCount + 1 : propCount;
            // Ищет в сборке класс подходящий под имя хранящееся в атрибуте FK, вызывает конструктор и создаёт объект, добавляет в найденный объект.
            if (columns.Length > propCount)
            {
                var foreignKey = type.GetProperties().FirstOrDefault(pi => pi.GetCustomAttribute(typeof(FKAttribute), true) != null);
                var typeOfClass = Assembly.GetAssembly(typeof(T)).GetExportedTypes().Where(i => i.Name == FindValueInAttribute(foreignKey)).FirstOrDefault();
                List<object> fieldValuesRelObject = new List<object>();
                for (int i = propCount; i < columns.Length; i++)
                {
                    fieldValuesRelObject.Add(columns[i]);
                }
                var relatedObject = FindConstructor(typeOfClass, (columns.Length - propCount)).Invoke(fieldValuesRelObject.ToArray());
                var relObjectProp = type.GetProperties().FirstOrDefault(pi => pi.Name == relatedObject.GetType().Name);
                relObjectProp.SetValue(createdObject, relatedObject);
            }

            return createdObject;
        }

        private ConstructorInfo FindConstructor(Type type, int paramCount)
        {
            ConstructorInfo[] ci = type.GetConstructors();
            int x;
            for (x = 0; x < ci.Length; x++)
            {
                ParameterInfo[] pi = ci[x].GetParameters();
                if (pi.Length == paramCount) break;
            }
            return ci[x];
        }

        // Получение всех данных с БД
        public IEnumerable<T> FindAll<T>()
        {
            return FindAll<T>(null);
        }

        public IEnumerable<T> FindAll<T>(string where)
        {
            Type type = typeof(T);
            string pkRow = FindPKColumn(type);
            bool flag;
            string tableName = CheckTableName(type, out flag);
            List<T> entities = new List<T>();
            StringBuilder selectionString = new StringBuilder();
            selectionString.Append($"SELECT * FROM {tableName}");
            var foreignKey = type.GetProperties().FirstOrDefault(pi => pi.GetCustomAttribute(typeof(FKAttribute), true) != null);
            if (foreignKey != null)
            {
                string FKtableName = FindValueInAttribute(foreignKey);
                selectionString.Append($" LEFT JOIN {FKtableName} ON {tableName}.{FKtableName}ID = {FKtableName}.ID ");
            }

            if (where != null)
            {
                selectionString.Append($" WHERE {where}");
            }

            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();

                SqlCommand selectionCommand = new SqlCommand(selectionString.ToString(), connection);
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

        private string FindValueInAttribute (PropertyInfo prop)
        {
            Type FKtype = typeof(FKAttribute);
            FKAttribute FKattr = (FKAttribute)
            Attribute.GetCustomAttribute(prop, FKtype);
            string FKTableName = FKattr.nameFKObject;
            return FKTableName;
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
            bool flag;
            string tableName = CheckTableName(type, out flag);
            string commandString = $"DELETE FROM {tableName} WHERE {pkRow} = {pk}";
            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand(commandString, connection);
                int num = command.ExecuteNonQuery();

                if (num == 0)
                {
                    throw new ArgumentException($"PK {pk} doesn't exist");
                }
            }
        }

        public void Update<T>(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity can't be null");
            }

            bool flag;
            Type type = entity.GetType();
            string tableName = CheckTableName(type, out flag);
            int numberUpdatedRows;
            string pkRow = FindPKColumn(type);
            string pk = "";
            StringBuilder commandString = new StringBuilder();
            StringBuilder setString = new StringBuilder();
            commandString.Append($"UPDATE {tableName} SET ");
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

            if (flag)
            {
                setString.Append($"Discriminator = {type.Name}");
            }
            else
            {
            // Удаление пробела и запятой в конце
            setString.Remove(setString.Length - 2, 2);
            }
            commandString.Append(setString + " ");
            commandString.Append($" WHERE {pkRow} = {pk}");
            using (this.connection = new SqlConnection(connectionString))
            {
                this.connection.Open();
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
