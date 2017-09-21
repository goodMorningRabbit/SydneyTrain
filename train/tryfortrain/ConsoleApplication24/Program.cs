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
    class Program
    {
        static string READ_STOP_ID = "stop_all.txt";
        static string READ_ODTABLE ="odTable.txt";
        static string WRITE_FILE = "map//";
        static string ODTABLE_NAME = "odNum";
        static conn myconn = new conn();
        static Dictionary<int, string> stop_id = new Dictionary<int, string>();
        static Dictionary<int, string> stop_id_unchanged = new Dictionary<int, string>();
        static int v0;
        static int start=-1;
        static int route_number = 0;
        static int MMM = 10000000;
        /*static public int Search(int timelow, int timeup, int date, FileStream file)
         * using binary search algorithm to find the the time need to travel between two stops.
         * input: the departure time from a certain stop,the latest time of departure(20 mins) and the file where we store the txts
         * output: the arrival time of the next stop
         * the code is realated to the format,so when the format of the 'Filestream file' changes,the numbers in the following code needs to be changed
         * first find the position of the proper start time
         */
        static public int Search(int timelow, int timeup, int date, FileStream file)
        {
            StreamReader r = new StreamReader(file);
            string ifConnected = r.ReadLine();
            r.Close();
            if (ifConnected=="$") {
                file.Close();
                return timelow + 180;
            }
            byte[] c = new byte[10];
            int g = 2;
            while (c[0] != '\n')
            {
                file.Seek(-g, SeekOrigin.End);
                file.Read(c, 0, g);
                g++;
            }
            int n = 0;
            for (int i = 1; i <= g - 4; i++)
            {
                n = n * 10 + c[i] - '0';
            }
            int min = MMM;
            byte[] byData = new byte[100];
            byte[,] input = new byte[100, 100];
            if (n == 1)
            {
                int total = 0;
                file.Seek(0, SeekOrigin.Begin);
                file.Read(byData, 0, 36);
                for (int i = 0; i < 10; i++)
                {
                    if (byData[i] - '0' <= 9 && byData[i] - '0' >= 0) total = total * 10 + byData[i] - '0';
                }
                 int temp = 0;
                for (int j = 11; j < 21; j++)
                {
                    if (byData[j] - '0' <= 9 && byData[j] - '0' >= 0) temp = temp * 10 + byData[j] - '0';
                }
                if (byData[22 + date * 2] == '1' && temp < min) min = temp;
                if (total >= timelow && total < timeup)
                {
                    return min;
                }
                else
                    return MMM;
            }
            else
            {
                int right = n - 1;
                int left = 0;
                int total = 0;
                while (left < right)
                {
                    int mid = (left + right) / 2;
                    total = 0;
                    file.Seek(mid * 37, SeekOrigin.Begin);//mid
                    file.Read(byData, 0, 36);
                    for (int i = 0; i < 10; i++)
                    {
                        if (byData[i] - '0' <= 9 && byData[i] - '0' >= 0) total = total * 10 + byData[i] - '0';
                    }
                    if (total < timelow)
                        left = mid + 1;
                    else
                        right = mid;
                }
                int rec1 = left;
                right = n - 1;
                left = 0;
                int rec3 = 0;
                while (left < right)
                {
                    int mid = (left + right) / 2;
                    total = 0;
                    file.Seek(mid * 37, SeekOrigin.Begin); //mid
                    file.Read(byData, 0, 36);
                    for (int i = 0; i < 10; i++)
                    {
                        if (byData[i] - '0' <= 9 && byData[i] - '0' >= 0) total = total * 10 + byData[i] - '0';
                    }
                    if (total < timeup)
                        left = mid + 1;
                    else
                        right = mid;
                    rec3 = total;
                }
                int rec2=0;
                if (left != n - 1)
                    rec2 = Math.Max(left - 1, 0);
                else
                    if (rec3 < timeup)
                     rec2 = left;
                for (  int i = rec1; i <= rec2; i++)
                {
                    file.Seek(i * 37, SeekOrigin.Begin); //i
                    file.Read(byData, 0, 36);
                    int temp = 0;
                    for (int j = 11; j < 21; j++)
                    {
                        if (byData[j] - '0' <= 9 && byData[j] - '0' >= 0) temp = temp * 10 + byData[j] - '0';
                    }
                    if (byData[22 + date * 2] == '1' && temp < min) min = temp;
                }
                return min;
            }
        }
        /*static public void insertIntoDic(string str, int MAXNUM, Dictionary<int, string> stop_id)
         * insert all the stop_id into a dictionary
         */
        static public void insertIntoDic(string str, int MAXNUM, Dictionary<int, string> stop_id)
        {
            StreamReader r = new StreamReader(str);

            for (int i = 0; i < MAXNUM; i++)
            {
                string ds = r.ReadLine();
                stop_id.Add(i, ds);
            }
            
        }
        /*static public int getDistance(int a, int b, int starttime, int date)
         * input: the departure stop,the arrival stop,the time(which day in a week)
         * output: the arrival time to the next stop
         */
        static public int getDistance(int a, int b, int starttime, int date)
        {
           string sql_getDistance =WRITE_FILE+stop_id[a]+ "-" + stop_id[b] + ".txt";
            try {
                FileStream file = new FileStream(sql_getDistance, FileMode.Open);

                int endtime = starttime + 20 * 60;

                int ds_distanceTemp = Search(starttime, endtime, date, file);

                return ds_distanceTemp;
            }
            catch(Exception ex) {
                return MMM;
            }

        }
        /*static public int select_stops_from_sql_and_insert_into_txt()
         * select all the stops from database and then put the stops_id in file: READ_STOP_ID which will be used latter
         * output: the number of the stops
         */
        static public int select_stops_from_sql_and_insert_into_txt()
        {
                int k = 0; 
                string sql = "select distinct stop_id from dbo.gap()";
                DataSet ds_temp = new DataSet();
                ds_temp = myconn.Query(sql, "stop_id");
                k = ds_temp.Tables["stop_id"].Rows.Count;
                StreamWriter w = File.AppendText(READ_STOP_ID);//new StreamWriter(READ_STOP_ID);
                for (int i = 0; i <k ; i++)
                {
                    string stop_id = ds_temp.Tables["stop_id"].Rows[i].ItemArray[0].ToString();
                    w.WriteLine(stop_id);
                }
                w.Close();
                return k;
        }
        /*static public int dijkstra_go(string start_stop_id, string end_stop_id, DateTime time_now)
         * the main algorithm
         * the code is a standard dijkstra algorithm, which you can learn at many sources:
         * https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
         * (Chinese Blog:)
         * http://www.cnblogs.com/dolphin0520/archive/2011/08/26/2155202.html
         * 
         * using dijkstra algorithm to calculate the minmum time to travel between two stops
         * initialize
         * choose the minmun edge start from a node V,add that node in the other side of the edge
         * update the distance(from node V to all other nodes),if passing nodeV make the total distance shorter then update it;if not,do nothing
         * do the above two steps until all the nodes has been added.
         * 
         * 假设存在G=<V,E>，源顶点为V0，U={V0},dist[i]记录V0到i的最短距离，path[i]记录从V0到i路径上的i前面的一个顶点。
         * 1.从V-U中选择使dist[i]值最小的顶点i，将i加入到U中；
         * 2.更新与i直接相邻顶点的dist值。(dist[j]=min{dist[j],dist[i]+matrix[i][j]})
         * 3.直到U=V，停止。
         * 
         * input:the start stop,the destination stop,the time
         * output:the time needed to travel from the start stop to the destination stop.
         */
        static public int dijkstra_go(string start_stop_id, string end_stop_id, DateTime time_now)
        {
            int dayOfWeek = -1;
            dayOfWeek = getDayOfWeek(time_now);
            int MAXNUM = select_stops_from_sql_and_insert_into_txt();// the number of all the stops
            int[] dist = new int[MAXNUM];//the distance from the start node to each node
            DateTime[] time_adding = new DateTime[MAXNUM];
            int[] prev = new int[MAXNUM];//record the path
            bool[] S = new bool[MAXNUM];//record whether a node has been added
            int n = MAXNUM;//the number of all the stops
            v0 = -1;

            //insert stop id into a dictionary
            insertIntoDic(READ_STOP_ID, MAXNUM,stop_id);
            insertIntoDic(READ_STOP_ID, MAXNUM, stop_id_unchanged);

            //get the key of start_stop
            int now = time_now.Hour * 3600 + time_now.Minute * 60 + time_now.Second;
            for (int i = 0; i < stop_id.Count; i++)
            {
                dist[i] = MMM;
                if (stop_id[i] == start_stop_id)
                {
                    v0 = i;
                    dist[i] = now;
                }

            }
            start = v0;
            if (v0 == -1)
                return 0;
            for (int i = 0; i < n; ++i)//initialize
            {
                S[i] = false;
                if (dist[i] == MMM)
                    prev[i] = -1;
                else
                    prev[i] = v0;
            }
            int u = v0;
            S[v0] = true;

            for (int i = 1; i < n; i++)//need to do it at most n times
            {
                int mindist = MMM;
                for (int j = 0; j < n; ++j) //choose the minmun edge start from a node
                    if ((S[j]) && dist[j] < mindist)
                    {
                        u = j;
                        mindist = dist[j];
                    }
                S[u] = false;
                for (int j = 0; j < n; j++)// update the distance
                {
                    int ts_temp = getDistance(u, j, dist[u], dayOfWeek);
                    if (ts_temp < MMM)
                    {
                        if (ts_temp < dist[j])
                        {
                            S[j] = true;
                            dist[j] = ts_temp;
                            prev[j] = u;
                        }
                    }
                }
                
            }
            for (int i = 0; i < stop_id.Count; i++)
                if (stop_id[i] == end_stop_id)
                    v0 = i;
            
            int[] route_r = get_route(get_key_of_stop(end_stop_id), prev);//get the route in form of stop index
            int[] route = reverse_route(route_r, route_number);//revere the route,it was back  to the front
            string[] route_name = get_route_name(route);//get the name of the route
            string[] route_time = get_route_time(route, dist);//and the time
            printGroup(route_name);//print
            printGroup(route_time);
            return dist[v0];
            
        }
        /* static public string[] get_route_name(int[] route)
         * input:the route in form of index
         */
        static public string[] get_route_name(int[] route)
        {
            string[] route_name = new string[1000];
            for (int i = 0; i < stop_id.Count; i++)
            {
                string sql_get_name = "select stop_name from stops where stop_id='" + stop_id_unchanged[route[i]] + "'";
                DataSet ds_ans = myconn.Query(sql_get_name, "t");
                route_name[i] = ds_ans.Tables[0].Rows[0].ItemArray[0].ToString();
            }
            return route_name;
        }
        /* static public string[] get_route_time(int[] route, int[] dist)
         * input:the route in form of index,the distance array
         * output:the arrivel time of each stop
         */
        static public string[] get_route_time(int[] route, int[] dist)
        {
            string[] route_time = new string[1000];
            for (int i = 0; i < stop_id.Count; i++)
                route_time[i] = change_to_time_format(dist[route[i]]);
            return route_time;
        }
        /*static public int[] get_route(int dst, int[] prev)
         * trace back to the start node
         * input:the distance array,the pre-node(node before each node in the path) of each node
         * output: the route (back to front)
         */
        static public int[] get_route(int dst, int[] prev)
        {
            int[] route_r = new int[1000];
            int i = dst;
            route_number = 0;
            while (i != start)
            {
                route_r[route_number] = i;
                i = prev[i];
                route_number++;
            }
            route_r[route_number] = i;
            route_number++;
            return route_r;
        }
        static public int[] reverse_route(int[] route_r, int routeNumber)
        {
            int[] route = new int[1000];
            for (int j = 0; j < route_number; j++)
            {
                route[j] = route_r[route_number - j - 1];
            }
            return route;
        }
        static public int get_key_of_stop(string id)
        {
            for (int i = 0; i < stop_id.Count; i++)
                if (stop_id[i] == id)
                    return i;
            return -1;
        }

        static void printGroup(string[] p)
        {
            for (int i = 0; i < route_number; i++)
                Console.WriteLine(p[i]);
        }
        /*static string change_to_time_format(int time)
         * input:the time in forms of int
         * output:the time in normal form
         */
        static string change_to_time_format(int time)
        {
            double h,m, s;

            s = time % 60;
            m = (time - s) % 3600 / 60.0;
            h = (time - m * 60 - s) / 3600;
            
            return h + ":" + m + ":" + s;
        }
        static int getDayOfWeek(DateTime now)
         {
            if (now.DayOfWeek.ToString() == "Monday")
                return 1;
            if (now.DayOfWeek.ToString() == "Tuesday")
                return 2;
            if (now.DayOfWeek.ToString() == "Wednesday")
                return 3;
            if (now.DayOfWeek.ToString() == "Thursday")
                return 4;
            if (now.DayOfWeek.ToString() == "Friday")
                return 5;
            if (now.DayOfWeek.ToString() == "Saturday")
                return 6;
            if (now.DayOfWeek.ToString() == "Sunday")
                return 7;
            return -1;
         }
        /*static public void create_odTxt()
         * export the table odNum in database to READ_ODTABLE
         * change it to txt file
         */
        static public void create_odTxt()
        {
            //string sql = @"EXEC master..xp_cmdshell 'BCP  trans.dbo.odNum out C:\Research\train\tryfortrain\ConsoleApplication24\bin\Debug\"+READ_ODTABLE+@" -c -S DESKTOP-EFH26KM\RABBIT -T'";
            string pathName = System.Environment.CurrentDirectory+"\\"+READ_ODTABLE;
            string sql = @"EXEC master..xp_cmdshell 'BCP  trans.dbo.odNum out "+ pathName +@" -c -S DESKTOP-EFH26KM\RABBIT -T'";
            myconn.nonQuery(sql);
        }
        /* static public void load_odTable()
         * in the beginning the data in database is table stop_times which is provide by GTFS,by using this function,we
         * can find every two adjacent stops in each trip and the travel time needed and then save it in another table odNum(which ODTABLE_NAME stands for)
         */
        static public void load_odTable()
        {
                string sql_pre = "delete from " + ODTABLE_NAME;// odNum;
                myconn.nonQuery(sql_pre);
                string sql = "select  * from dbo.gap() order by trip_id,sequence ";
                DataSet ds = new DataSet();
                ds = myconn.Query(sql, "od_table");
                TimeSpan[] ts = new TimeSpan[200000];
                int i = 0;
                int j = 0;
                string current_trip_id = ds.Tables["od_table"].Rows[0].ItemArray[0].ToString();
                for (i = 0; i < ds.Tables["od_table"].Rows.Count; i++)
                {
                    if (ds.Tables["od_table"].Rows[i].ItemArray[0].ToString() == current_trip_id)
                    {
                        if (i >= 1)
                        {
                            DateTime at = DateTime.Parse(ds.Tables["od_table"].Rows[i].ItemArray[2].ToString());
                            DateTime dt = DateTime.Parse(ds.Tables["od_table"].Rows[i - 1].ItemArray[3].ToString());
                            ts[i] = (at - dt).Duration();
                            string from_stop_id, to_stop_id, trip_id;
                            string mon, tue, wed, thu, fri, sat, sun;
                            int sequence;
                            from_stop_id = ds.Tables["od_table"].Rows[i - 1].ItemArray[1].ToString();
                            to_stop_id = ds.Tables["od_table"].Rows[i].ItemArray[1].ToString();
                            sequence = int.Parse(ds.Tables["od_table"].Rows[i].ItemArray[4].ToString());
                            mon = ds.Tables["od_table"].Rows[i].ItemArray[5].ToString();
                            tue = ds.Tables["od_table"].Rows[i].ItemArray[6].ToString();
                            wed = ds.Tables["od_table"].Rows[i].ItemArray[7].ToString();
                            thu = ds.Tables["od_table"].Rows[i].ItemArray[8].ToString();
                            fri = ds.Tables["od_table"].Rows[i].ItemArray[9].ToString();
                            sat = ds.Tables["od_table"].Rows[i].ItemArray[10].ToString();
                            sun = ds.Tables["od_table"].Rows[i].ItemArray[11].ToString();
                            trip_id = ds.Tables["od_table"].Rows[i].ItemArray[0].ToString();
                            int at_n = at.Hour * 3600 + at.Minute * 60 + at.Second;
                            int dt_n = dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
                            double ts_n = ts[i].TotalSeconds;
                            string sql_add2_odTable = "insert into "+ODTABLE_NAME+ " values('" + from_stop_id + "','" + to_stop_id + "'," + ts_n + ","
                                                       + at_n + "," + dt_n + "," + mon + "," + tue + "," + wed + "," + thu + "," + fri + "," + sat + "," + sun
                                                       + ",'" + trip_id + "')";
                            //once getting the odTable delete the code above;
                            myconn.nonQuery(sql_add2_odTable);
                        }
                    }
                    else
                    {
                        current_trip_id = ds.Tables["od_table"].Rows[i].ItemArray[0].ToString();
                    }

                }
        }

        static void Main(string[] args)
        {
            load_odTable();
            create_odTxt();
            map m = new map(myconn,READ_ODTABLE,WRITE_FILE);
            m.create_txt();
            m.create_txt_combinePlatform();
            string time_now = "";
            Console.WriteLine("input time(hh:mm:ss)");
            time_now=Console.ReadLine();
            DateTime now = DateTime.Parse("2016-10-11 " + time_now);// 20:30:00");
            //int ans=dijkstra_go("2508172", "2515122", now);
            Console.WriteLine("input start stop and destination stop(stop id):\n");
            string start_stop = "";
            string destination_stop = "";
            start_stop = Console.ReadLine();
            destination_stop = Console.ReadLine();
            int ans = 0;
            try
            {
                // ans = dijkstra_go("2000323", "2000325", now);
                ans = dijkstra_go(start_stop, destination_stop,now);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine();
            Console.WriteLine(change_to_time_format( ans));
            Console.ReadLine();
        }
        
    }

}
