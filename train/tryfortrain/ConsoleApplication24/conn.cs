using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
/*This class is the connection class between this program and the databanse program
 * change the connection string :string constr; when use this code in different computer
 */
namespace ConsoleApplication24
{
    public class conn
    {
        string constr;
        SqlConnection myconn;
        public conn()
        {
            this.constr = "Data Source=DESKTOP-EFH26KM\\RABBIT;Initial Catalog=trans;Integrated Security=True";
            myconn = new SqlConnection(this.constr);
            //this connection code need to be changed according to different data source(pc name) and database name;
        }
        public DataSet Query(string sql, string name)
        {
            try
            {
                SqlDataAdapter ada = new SqlDataAdapter(sql, constr);
                DataSet dt = new DataSet();
                ada.Fill(dt, name);
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string nonQuery(string sql)
        {

            SqlConnection myconn = new SqlConnection(this.constr);
            try
            {
                SqlCommand cmd = new SqlCommand(sql, myconn);
                myconn.Open();
                cmd.ExecuteNonQuery();
                return 1.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {
                myconn.Close();
            }
        }
        public int executeScalar(string sql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, myconn);
                myconn.Open();
                int k=(int)(cmd.ExecuteScalar());
                myconn.Close();
                return k;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}
