using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mason
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static route start(double _alpha, double _r)
        {
            var stpW = new Stopwatch();
            stpW.Start();
            List<p> points = new List<p>();
            p startP = new p(new v(0, _r), new v(Math.Cos(_alpha ), -Math.Sin(_alpha)));
            int ind = 0;
            int lastEnc = -1;
            double allDist = 0;
            while (true)
            {
                double dist = startP.distToCircle(_r, lastEnc==-1);
                int closestPoint = -1;
                for(int i = 0; i < points.Count-1; i++)
                {
                    if(i != lastEnc)
                    {
                        double currDist = startP.distTo(points[i]);
                        if (currDist < dist & currDist > 0)
                        {
                            closestPoint = i;
                            dist = startP.distTo(points[i]);
                        }
                    }
                }

                allDist += dist;
                ind++;

                if (dist<0.01)
                {
                    stpW.Stop();
                    return new route(points, allDist, ind, _alpha, stpW.Elapsed.TotalMilliseconds);
                }
                    

                v sPos = startP.pos + (startP.dir * dist);
                v sDir = closestPoint==-1? startP.mirror(new p(new v(0, 0), (startP.pos + startP.dir * dist).prost())) : startP.mirror(points[closestPoint]);

                if(startP.dir == -sDir)
                {
                    stpW.Stop();
                    points.Add(startP);
                    startP = new p(sPos, sDir);
                    return new route(points, allDist, ind, _alpha, stpW.Elapsed.TotalMilliseconds);
                }

                lastEnc = closestPoint;

                points.Add(startP);
                startP = new p(sPos, sDir); 
            }
        }

    }

    public struct route
    {
        public List<p> points;
        public double length;
        public int count;
        public double alpha;
        public double time;

        public route(List<p> _points, double _l, int _c, double _a, double _t, int _p = 0)
        {
            points = _points;
            length = _l;
            count = _c;
            alpha = _a;
            time = _t;
        }
    }

    public struct p
    {
        public v pos;
        public v dir;

        public p(v _pos, v _dir)
        {
            pos = _pos;
            dir = _dir / _dir.len();

        }

        public double distToCircle(double _r, bool _string)
        {
            if (_string)
                return 2 * _r * Math.Abs(v.dot(dir, -pos / _r));
            else
                return -v.dot(pos, dir) + Math.Sqrt(_r * _r - Math.Pow(v.dot(pos, dir.prost()), 2));
        }
            

        public double distTo(p p1)=>
            -v.dot(pos - p1.pos, p1.dir.prost()) / v.dot(dir, p1.dir.prost());

        public v mirror(p p1) =>
            2*v.dot(dir, p1.dir)*p1.dir - dir;
    }

    public struct v
    {
        public double x;
        public double y;

        public v(Point _p)
        {
            x = _p.X;
            y = _p.Y;
        }

        public v(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        public static v operator /(v v1, double d) =>
            new v(v1.x / d, v1.y / d);

        public static bool operator ==(v v1, v v2) =>
            Math.Abs(v1.x-v2.x)<0.0001 && Math.Abs(v1.y - v2.y)<0.0001;

        public static bool operator !=(v v1, v v2) =>
            v1.x != v2.x && v1.y != v2.y;

        public static v operator *(v v1, double d) =>
            new v(v1.x * d, v1.y * d);

        public static v operator *(double d, v v1)=>
            new v(v1.x * d, v1.y * d);

        public static v operator +(v v1, v v2)=>
            new v(v1.x + v2.x, v1.y + v2.y);

        public static v operator -(v v1) =>
            new v(-v1.x, -v1.y);

        public static v operator -(v v1, v v2) =>
            new v(v1.x - v2.x, v1.y - v2.y);

        public static double dot(v v1, v v2) =>
            v1.x*v2.x+v1.y*v2.y;

        public double len() =>
            Math.Sqrt(x * x + y * y);

        public double len2() =>
            x * x + y * y;

        public static double len(v v1) =>
            Math.Sqrt(v1.x * v1.x + v1.y * v1.y);

        public static double len2(v v1) =>
            v1.x * v1.x + v1.y * v1.y;

        public v prost() =>
            new v(y, -x);

        public static v prost(v v1) =>
            new v(v1.y, -v1.x);

        public static double dist(v v1, v v2)=>
            Math.Sqrt(((v1.x - v2.x)* (v1.x - v2.x)) + ((v1.y - v2.y)* (v1.y - v2.y)));
    }
}
