using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
namespace ConsoleApplication24
{
    public class map
    {
        string WRITE_FILE;
        string READ_ODTABLE;
        conn myconn;
        static int[] from_stop_id = new int[50000];
        static int[] to_stop_id = new int[50000];
        static int[] timespan = new int[50000];
        static int[] dt=new int [50000];
        static int[] at= new int[50000];
        static int[,] available = new int[50000, 7];
        static string[] trip_id = new string[50000];
        static int[] sequence = new int[50000];
        static int [] countt=new int[50000];
        public map(conn myconn,string READ_ODTABLE,string WRITE_FILE)
        {
            this.myconn = myconn;
            this.WRITE_FILE = WRITE_FILE;
            this.READ_ODTABLE = READ_ODTABLE;
        }
        
        /*void quicksort(int left, int right)
        {
            int i, j, t, temp;
            if (left > right)
                return;
            int mid = left;
            int c = from_stop_id[mid]; // from_stop_id[mid];//
            int d = to_stop_id[mid];
            int e = dt[mid];
            int f = at[mid];
            int g = countt[mid];
            temp = from_stop_id[left]; //temp中存的就是基准数 
            i = left;
            j = right;
            while (i != j)
            {
                while ((from_stop_id[j] > c || (from_stop_id[j] == c && to_stop_id[j] > d) || ((from_stop_id[j] == c && to_stop_id[j] == d) && dt[j] >= e)) && i < j)//
                    j--;
                while ((from_stop_id[i] < c || (from_stop_id[i] == c && to_stop_id[i] < d) || ((from_stop_id[i] == c && to_stop_id[i] == d) && dt[i] < e)) && i < j)// 
                    i++;
                if (i < j)
                {
                    exchange(i, j);
                }
            }
            from_stop_id[left] = from_stop_id[i];
            from_stop_id[i] = c;
            to_stop_id[left] = to_stop_id[i];
            to_stop_id[i] = d;
            dt[left] = dt[i];
            dt[i] = e;
            at[left] = at[i];
            at[i] = f;
            countt[left] = countt[i];
            countt[i] = g;
            quicksort(left, i - 1);//继续处理左边的，这里是一个递归的过程 
            quicksort(i + 1, right);//继续处理右边的 ，这里是一个递归的过程 
        }
        */
        public void exchange(int x,int y)
        {
            int s;
            s = from_stop_id[x];
            from_stop_id[x] = from_stop_id[y];
            from_stop_id[y] = s;
            s = to_stop_id[x];
            to_stop_id[x] = to_stop_id[y];
            to_stop_id[y] = s;
            s = dt[x];
            dt[x] = dt[y];
            dt[y] = s;
            s = at[x];
            at[x] = at[y];
            at[y] = s;
            s = countt[x];
            countt[x] = countt[y];
            countt[y] = s;
        }
        /* public void qs(int x, int y)
         * quicksort is uesed when creating the txt file.
         *sort the stop_id
         */
        public void qs(int x, int y)
        {
            int i = x;
            int j = y;
            int mid = (x + y) / 2;
            int c = from_stop_id[mid]; // from_stop_id[mid];//
            int d = to_stop_id[mid];
            int e = dt[mid];
            int f = at[mid];
            int g = countt[mid];
            while (i < j)
            {
                while (((from_stop_id[j] > c) || (from_stop_id[j] == c && to_stop_id[j] > d) || ((from_stop_id[j] == c && to_stop_id[j] == d) && dt[j] > e)))
                    j--; //  
                while (((from_stop_id[i] < c) || (from_stop_id[i] == c && to_stop_id[i] < d) || ((from_stop_id[i] == c && to_stop_id[i] == d) && dt[i] < e)))
                    i++; //) 
                if (i <= j)
                {
                    exchange(i, j);
                    i++;
                    j--;
                }
            }
            if (i < y)
                qs(i, y);
            if (x < j)
                qs(x, j);
        }
        /*public void create_txt()
         * create txt file for each stop pair
         */
        public void create_txt()
        {
            StreamReader f = new StreamReader(READ_ODTABLE);
            int i = 0;
            while (f.Peek()>0)
            {
                string[] line =f.ReadLine().Split('\t');
                from_stop_id[i] = int.Parse(line[0]);
                to_stop_id[i] = int.Parse(line[1]);
                timespan[i] = int.Parse(line[2]);
                at[i] = int.Parse(line[3]);
                dt[i] = int.Parse(line[4]);
                for (int j = 5; j < 11; j++)
                {
                    available[i, j-5] = int.Parse(line[j]);
                }
                //trip_id[i] = line[12];
                //sequence[i] =int.Parse( line[13]);
                countt[i] = i;
                i++;
            }
            i--;
            qs(0, i);
            int start1 = from_stop_id[0];
            int start2 = to_stop_id[0];
            string ss = WRITE_FILE;
            ss = ss + start1.ToString() + "-" + start2.ToString() + ".txt";
            StreamWriter w = new StreamWriter(ss);
            int num = 0;
            for (int j = 1; j < i; j++)
            {
                if ((from_stop_id[j] != start1) || (to_stop_id[j] != start2))
                {
                    w.WriteLine(num);
                    w.Close();
                    start1 = from_stop_id[j];
                    start2 = to_stop_id[j];
                    ss = WRITE_FILE;
                    ss = ss + start1.ToString() + "-" + start2.ToString() + ".txt";
                    w = new StreamWriter(ss);
                    num = 0;
                }
                w.Write("{0,10} {1,10}", dt[j], at[j]);
                for (int k = 0; k < 7; k++)
                    w.Write(" {0}", available[countt[j], k]);
                num++;
                w.WriteLine();
            }
            w.Close();
            f.Close();
        }
        /* public string toNum(string a)
         * there are cases when stop_id includes non-numeric characters,this function change the letter of alphabet into number
         * input:stop_id(string)
         * output:"-"+(char)(((tempbyte - 'A')%10)+'0');
         */
        public string toNum(string a)
        {
            char[] tc = a.ToArray();
            bool flag =true;
            for (int i = 0; i < a.Length; i++)
            {
                byte tempbyte = Convert.ToByte(a[i]);
                if (tempbyte < 48 || tempbyte > 57)
                {
                    tc[i] =(char)(((tempbyte - 'A')%10)+'0');
                    flag = false;
                }
                else
                    tc[i] = a[i];
            }
            if (flag == false)
            {
                return "-"+new string(tc);// + "99";
            }
            return new string(tc);
        }
        /*create txt file for platform stop id pairs
         * only includes two lines now,see the content of string sql_crete_combinePlatform
         */
        public void create_txt_combinePlatform()
        {
            string sql_crete_combinePlatform= @"select distinct stops.stop_id,parent_station from stops, stop_times, calendar, trips, routes 
                                                where stops.parent_station is not null and
                                                stops.stop_id = stop_times.stop_id and
                                                stop_times.trip_id = trips.trip_id and
                                                trips.route_id = routes.route_id  and
                                                calendar.service_id = trips.service_id
                                                and(
                                                routes.route_long_name  like '%Eastern Suburbs and Illawarra Line%' or
                                                routes.route_long_name like '%South Coast Line%')";
            DataSet ds_cp = new DataSet();
            ds_cp= myconn.Query(sql_crete_combinePlatform, "cp");
            //string WRITE_FILE2 = "test//";
            string[] current_station_group = new string[10000];
            int current_station_group_count = 0;

            string current_station = ds_cp.Tables[0].Rows[0].ItemArray[1].ToString();
            current_station_group[0] = ds_cp.Tables[0].Rows[0].ItemArray[0].ToString();
            StreamWriter sw = new StreamWriter("test_combineTestTxt.txt");
            for (int i = 1; i < ds_cp.Tables[0].Rows.Count; i++)
            {
                string temp_station = ds_cp.Tables[0].Rows[i].ItemArray[1].ToString();
                string temp_stop = ds_cp.Tables[0].Rows[i].ItemArray[0].ToString();
                if (temp_station == current_station)
                {
                    current_station_group_count++;
                    current_station_group[current_station_group_count] =temp_stop;
                    string stop_id_i = temp_stop;
                    for (int j = 0; j < current_station_group_count; j++) {
                        string stop_id_j =current_station_group[j];
                        if ( stop_id_i!= stop_id_j)
                        {
                            string txtName = WRITE_FILE + toNum(stop_id_j)+ "-" +toNum(stop_id_i)+ ".txt";
                            sw = new StreamWriter(txtName);
                            sw.Write("$");
                            sw.Close();
                        }
                    }
                }
                else
                {
                    current_station_group = new string[1000];
                    current_station_group_count = 0;
                    current_station = temp_station;
                    current_station_group[0] = current_station;
                }
            }
        }
    }
}
 