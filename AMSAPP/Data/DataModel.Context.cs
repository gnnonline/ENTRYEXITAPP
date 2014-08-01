
namespace AMSAPP
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    
    public partial class DataContext 
    {

        private string ConnectionString { get; set; }

        public DataContext ()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["DataContext"].ToString();
        }

        public virtual IEnumerable<ComputerEvent> ComputerEvents { get {
                
            SqlCeConnection conn = null;
                List<ComputerEvent> list;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    // Create a String to hold the query.
                    string query = "SELECT * FROM ComputerEvents";

                    // Create a SqlCommand object and pass the constructor the connection string and the query string.
                    SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                    // Use the above SqlCommand object to create a SqlDataReader object.
                    SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                    list = (new List<ComputerEvent>()).FromDataReader<ComputerEvent>(queryCommandReader).ToList();
                    // Close the connection
                    conn.Close();
                }
                return list;

            }
        }
        public virtual IEnumerable<AMSEvent> AMSEvents
        {
            get
            {

                SqlCeConnection conn = null;
                List<AMSEvent> list;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    // Create a String to hold the query.
                    string query = "SELECT * FROM AMSEvents";

                    // Create a SqlCommand object and pass the constructor the connection string and the query string.
                    SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                    // Use the above SqlCommand object to create a SqlDataReader object.
                    SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                    list = (new List<AMSEvent>()).FromDataReader<AMSEvent>(queryCommandReader).ToList();
                    // Close the connection
                    conn.Close();
                }
                return list;

            }
        }

     

        public void AddComputerEvent(ComputerEvent event1)
        {
            SqlCeConnection conn = null;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO ComputerEvents ([EventType],[EventOn]) Values('"
                                    + event1.EventType + "','" + event1.EventOn.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                cmd.ExecuteNonQuery();

                // Close the connection
                conn.Close();
            }
    
        }

         public void AddAMSEvent(AMSEvent event1)
        {
            SqlCeConnection conn = null;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO AMSEvents ([EventType],[EventOn] ,[Description],[ElapsedTimeTxt],[IsActual]) Values('"
                                    + event1.EventType + "','" + event1.EventOn.ToString("yyyy-MM-dd HH:mm:ss") + "','"
                                    + event1.Description + "','" + event1.ElapsedTimeTxt + "'," + (event1.IsActual ? "1" : "0") + ")";

                cmd.ExecuteNonQuery();

                // Close the connection
                conn.Close();
            }
    
        }
    }

    public class Reflection
    {
        public void FillObjectWithProperty(ref object objectTo, string propertyName, object propertyValue)
        {
            Type tOb2 = objectTo.GetType();
            tOb2.GetProperty(propertyName).SetValue(objectTo, propertyValue,null);
        }
    }

    public static class IENumerableExtensions
    {
        public static IEnumerable<T> FromDataReader<T>(this IEnumerable<T> list, DbDataReader dr)
        {
            //Instance reflec object from Reflection class coded above
            Reflection reflec = new Reflection();
            //Declare one "instance" object of Object type and an object list
            Object instance;
            List<Object> lstObj = new List<Object>();

            //dataReader loop
            while (dr.Read())
            {
                //Create an instance of the object needed.
                //The instance is created by obtaining the object type T of the object
                //list, which is the object that calls the extension method
                //Type T is inferred and is instantiated
                instance = Activator.CreateInstance(list.GetType().GetGenericArguments()[0]);

                // Loop all the fields of each row of dataReader, and through the object
                // reflector (first step method) fill the object instance with the datareader values
                foreach (DataRow drow in dr.GetSchemaTable().Rows)
                {
                    reflec.FillObjectWithProperty(ref instance,
                            drow.ItemArray[0].ToString(), dr[drow.ItemArray[0].ToString()]);
                }

                //Add object instance to list
                lstObj.Add(instance);
            }

            List<T> lstResult = new List<T>();
            foreach (Object item in lstObj)
            {
                lstResult.Add((T)Convert.ChangeType(item, typeof(T)));
            }

            return lstResult;
        }
    }

}


