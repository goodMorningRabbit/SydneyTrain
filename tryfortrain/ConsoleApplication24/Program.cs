using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
namespace ConsoleApplication24
{
    class Program
    {
        static Dictionary<int, string> stop_id = new Dictionary<int, string>();
        static int v0;
        int route_number = 0;
        static int MMM = 10000000;
        static public int Search(int timelow, int timeup, int date, FileStream file)
        {
            byte[] c = new byte[10];
            int g = 2;
            while (c[0] != '\n')
            {
                file.Seek(-g, SeekOrigin.End);
                file.Read(c, 0, g);
                g++;
                //Console.WriteLine(Encoding.ASCII.GetString(c));
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
                if (total > timelow && total < timeup)
                    return min;
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
                }
                int rec2 = Math.Max(left - 1, 0);

                for (int i = rec1; i <= rec2; i++)
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
        static public Dictionary<int, string> insertIntoDic(string str, int MAXNUM)
        {
            Dictionary<int, string> stop_id = new Dictionary<int, string>();
            StreamReader r = new StreamReader(str);

            for (int i = 0; i < MAXNUM; i++)
            {
                string ds = r.ReadLine();
                stop_id.Add(i, ds);
            }

            return stop_id;
        }
        static public int getDistance(int a, int b, int starttime, int date)
        {
            string sql_getDistance = "D:/Projects/ConsoleApplication1/ConsoleApplication1/map/" + stop_id[a] + "-" + stop_id[b] + ".txt";
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
        static public int dijkstra_go(string start_stop_id, string end_stop_id, DateTime time_now)
        {
            int MAXNUM = 160;
            int[] dist = new int[MAXNUM];
            DateTime[] time_adding = new DateTime[MAXNUM];
            int[] prev = new int[MAXNUM];
            bool[] S = new bool[MAXNUM];
            int n = MAXNUM;
            v0 = -1;
            //insert stop id into a dictionary

            string sql_getDistance = "D:/Projects/ConsoleApplication1/ConsoleApplication1/map/stops.txt";

            stop_id = insertIntoDic(sql_getDistance, MAXNUM);

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
            if (v0 == -1)
                return 0;
            for (int i = 0; i < n; ++i)
            {
                //dist[i] =getDistance(v0,i,time_now).ts;
                S[i] = false;
                if (dist[i] == MMM)
                    prev[i] = -1;
                else
                    prev[i] = v0;
            }
            //dist[v0] = 0;
            int u = v0;
            S[v0] = true;
            for (int i = 1; i < n; i++)
            {
                int mindist = MMM;
                for (int j = 0; j < n; ++j)
                    if ((S[j]) && dist[j] < mindist)
                    {
                        u = j;
                        mindist = dist[j];
                    }
                S[u] = false;
                for (int j = 0; j < n; j++)
                {
                    int ts_temp = getDistance(u, j, dist[u], 0/*int.Parse(time_now.DayOfWeek.ToString())*/);
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
            return dist[v0];
            //int[] route = reverse_route(get_route(8, prev));
            //string [] route_name=get_route_name(route);
            //string[] route_time = get_route_time(route,time_adding);
            //DateTime arrive_time= time_adding[8];
        }
        static void Main(string[] args)
        {
            //201171-2000361
            DateTime now = DateTime.Parse("1899-12-30 16:30:00");
            int ans=dijkstra_go("202291", "2515142", now);
            Console.WriteLine(ans);
        }
        
    }

}
