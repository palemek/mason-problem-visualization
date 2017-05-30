using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace mason
{
    public partial class Form1 : Form
    {
        //input:
        static int pointNum = 8007;

        static int distributionType = 0;

        static double pointsFrom = 0.5;

        static double pointsTo = 0.6;

        //drawing constant data:
        static float r = 480;

        static float margin = 10;


        //anim input:
        static int framesNum = 100;

        //statistics:
        static double longestRoute = 0;

        static int biggestCount = 0;

        static double longestTime = 0;

        static double avLength = 0;

        static double avCount = 0;

        static double avTime = 0;

        //calculation data:


        static int                  calcPerThread = Math.Min(500, pointNum);
        static int                  calcThreadsNum = Math.Min(Math.Max(1,pointNum/(2*calcPerThread)),6);
        static int                  calcWorkingThreads = calcThreadsNum;
        static object               currCalculationsPosition = 0;
        static List<Thread>         calcThreads = new List<Thread>();
        static List<List<route>>    calcResults = new List<List<route>>();
        static List<int>            calcThreadsListPointers = new List<int>();

        //calculated data:
        static List<route> routes = new List<route>();

        //display:

        static int displayStyle = -1;

        //


        public double distribution(int _i)
        {
            switch(distributionType)
            {
                case (0):
                    return (Math.PI / 2)*(((Math.Max(1.0f,_i) / pointNum) * (pointsTo-pointsFrom))+pointsFrom);
                case (1):
                    return (Math.PI / 2) * 3000.0f / (3000 + _i);
                default:
                    return 0;

            }
        }

        public List<route> calcRoutes(int _from, int _to)
        {
            var temp = new List<route>();
            for (int i = _from; i < _to; i++)
            {
                route currRoute = Program.start(distribution(i), r);
                currRoute.points = new List<p>(new p[] { currRoute.points.Last() });
                temp.Add(currRoute);
            }
            return temp;
        }

        void thEnd(int i)
        {
            if (calcResults[i] != null)
                for (int j = 0; j < calcResults[i].Count; j++)
                {
                    routes[calcThreadsListPointers[i] + j] = calcResults[i][j];
                }

            if ((int)currCalculationsPosition >= pointNum)
            {
                calcWorkingThreads--;
                Thread.CurrentThread.Abort();
                return;
            }
            lock (currCalculationsPosition)
            {
                int curr = (int)currCalculationsPosition;
                calcResults[i] = null;
                calcThreads[i] = new Thread(() => { calcResults[i] = calcRoutes(curr, Math.Min(pointNum, curr + calcPerThread)); thEnd(i); });
                calcThreadsListPointers[i] = curr;
                curr += calcPerThread;
                currCalculationsPosition = curr;
                calcThreads[i].Start();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void initialisingCalculations()
        {
            try
            {
                pointNum = Convert.ToInt32(textBoxPointsNum.Text);
            }
            catch
            {
                textBoxPointsNum.Text = pointNum.ToString();
            }

            calcPerThread = Math.Min(500, pointNum);
            calcThreadsNum = Math.Min(Math.Max(1, pointNum / (2 * calcPerThread)), 6);
            calcWorkingThreads = calcThreadsNum;
            currCalculationsPosition = 0;
            calcThreads = new List<Thread>();
            calcResults = new List<List<route>>();
            calcThreadsListPointers = new List<int>();

            routes = new List<route>();

            for (int i = 0; i < calcThreadsNum; i++)
            {
                calcThreads.Add(null);
                calcResults.Add(null);
                calcThreadsListPointers.Add(0);
            }

            for (int i = 0; i < pointNum; i++) routes.Add(new route());

            for (int i = 0; i < calcThreadsNum; i++) thEnd(i);
        }

        public void routesChecking()
        {
            for (int i = 0; i < pointNum; i++)
            {
                route currRoute = routes[i];
                if (currRoute.points == null)
                {
                    Console.WriteLine("doliczam: " + i);
                    currRoute = Program.start(distribution(i), r);
                    routes[i] = currRoute;
                }
            }
        }

        public void calculatingAveragesAndLimits()
        {
            for (int i = 0; i < pointNum; i++)
            {
                route currRoute = routes[i];
                route prevRoute = routes[Math.Max(i - 1,0)];

                if (i > 1)
                {
                    avTime =    (currRoute.time     + (avTime   * (i - 2))) / (i - 1);
                    avCount =   (currRoute.count    + (avCount  * (i - 2))) / (i - 1);
                    avLength =  (currRoute.length   + (avLength * (i - 2))) / (i - 1);
                }
                else
                {
                    avTime = currRoute.time;
                    avCount = currRoute.count;
                    avLength = currRoute.length;
                }

                if (longestRoute < currRoute.length)
                    longestRoute = currRoute.length;

                if (biggestCount < currRoute.count)
                    biggestCount = currRoute.count;

                if (longestTime < currRoute.time)
                    longestTime = currRoute.time;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void pictureBoxesAdjumstments()
        {
            pictureBox1.Size = new Size(Convert.ToInt32(2 * r + 2 * margin), Convert.ToInt32(2 * r + 2 * margin));
            pictureBox2.Location = new Point(pictureBox1.Width, 0);
            pictureBox2.Size = new Size(this.Width-pictureBox1.Width - (int)margin * 4, this.Height/2 - (int)margin * 4);
            pictureBox3.Size = new Size(this.Width - pictureBox1.Width - (int)margin * 4, this.Height / 2 - (int)margin * 4);//new Size(1500, 1000);
            pictureBox3.Location = new Point(pictureBox1.Width, pictureBox2.Height);
        }

        public void pictureBoxesRefresh()
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();
            pictureBox3.Refresh();
        }

        public void savingPictureBoxes()
        {
            Bitmap toSave1 = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.DrawToBitmap(toSave1, new Rectangle(pictureBox1.Location, pictureBox1.Size));
            toSave1.Save("saved" + pointNum + ".png");

            Bitmap toSave2 = new Bitmap(pictureBox2.Size.Width, pictureBox2.Size.Height);
            pictureBox2.DrawToBitmap(toSave2, new Rectangle(new Point(0, 0), pictureBox2.Size));
            toSave2.Save("length" + longestRoute + ".png");

            Bitmap toSave3 = new Bitmap(pictureBox3.Size.Width, pictureBox3.Size.Height);
            pictureBox3.DrawToBitmap(toSave3, new Rectangle(new Point(0, 0), pictureBox3.Size));
            toSave3.Save("count" + biggestCount + ".png");
        }

        public Form1()
        {
            InitializeComponent();

            //savingPictureBoxes();
        }
        
        private void buttStartCalc_Click(object sender, EventArgs e)
        {
            initialisingCalculations();

            try{ pointsFrom = Convert.ToDouble(textBox2.Text); } catch { textBox2.Text = "wrong value"; }

            try{ pointsTo = Convert.ToDouble(textBox3.Text); } catch { textBox3.Text = "wrong value"; }


            while (calcWorkingThreads > 0) progressBar1.Value = Convert.ToInt32(100*(Math.Min(1,(float)(int)currCalculationsPosition/pointNum)));//waiting for calculations to end

            string butText = buttStartCalc.Text;
            buttStartCalc.Text = "finishing";

            routesChecking();
            
            pictureBoxesAdjumstments();

            calculatingAveragesAndLimits();

            pictureBoxesRefresh();

            buttStartCalc.Text = "finished";
            Thread.Sleep(1500);

            buttStartCalc.Text = butText;
        }

        public struct myTag
        {
            public int frame;
            public SolidBrush brush;
        }

        void saveFrames(int _from, int _to)
        {
            var myForm = new Form();
            var picbox = new PictureBox();
            picbox.Location = pictureBox1.Location;
            picbox.Size = pictureBox1.Size;
            myForm.Controls.Add(picbox);
            picbox.Paint += pictureBox1_Paint;
            myTag info = new myTag();
            info.brush = new SolidBrush(Color.White);
            picbox.Tag = info;

            displayStyle = -2;

            for (int i = _from; i < _to; i++)
            {
                
                myTag t = (myTag)(picbox.Tag);
                t.frame = i;
                picbox.Tag = t;
                picbox.Refresh();
                using (Bitmap toSave = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height))
                {
                    picbox.DrawToBitmap(toSave, new Rectangle(pictureBox1.Location, pictureBox1.Size));
                    try
                    {
                        toSave.Save(@"C:\tmp\" + "frame" + i + ".png");
                    }
                    catch
                    {
                        Console.WriteLine("couldnt make " + i + "frame");
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        static private Color trilinearColor(int i)
        {
            return Color.FromArgb(
                        Convert.ToInt32(255 * (Math.Max(Math.Max(0, 1 - (Math.Abs(i - (routes.Count)) / (routes.Count / 3))), 1 - (i / (routes.Count / 3))))),
                        Convert.ToInt32(255 * (Math.Max(0, 1 - (Math.Abs(i - (1f * routes.Count / 3)) / (routes.Count / 3))))),
                        Convert.ToInt32(255 * (Math.Max(0, 1 - (Math.Abs(i - (2f * routes.Count / 3)) / (routes.Count / 3)))))
                        );
        }
        
        static private Color trilinearAlphaColor(int i, float a)
        {
            return Color.FromArgb(Convert.ToInt32(trilinearColor(i).R * a), Convert.ToInt32(trilinearColor(i).G * a), Convert.ToInt32(trilinearColor(i).B * a));
        }
        

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush brush = null;
            int currFrame2 = 5;
            try
            {
                myTag myTag = (myTag)((PictureBox)sender).Tag;
                currFrame2 = myTag.frame;
                brush = myTag.brush;
            }
            catch
            {
                brush = new SolidBrush(Color.AliceBlue);
            }
            brush.Color = Color.White;
            e.Graphics.FillRectangle(brush, 0, 0, 2 * margin + (r * 2), 2 * margin + (r * 2));
            brush.Color = Color.Black;
            e.Graphics.FillEllipse(brush, margin, margin, (r * 2), (r * 2));
            
            if (displayStyle==-1)
            {
                for (int i = 0; i < routes.Count - 1; i++)
                {
                    var currPointPos = routes[i].points.Last().pos;
                    brush.Color = trilinearColor(i);
                    e.Graphics.FillRectangle(brush, (float)currPointPos.x + margin + r, (float)currPointPos.y + margin + r, 2, 2);
                }
            }
            else if (displayStyle == -2)
            {
                for (int i = 0; i < routes.Count - 1; i++)
                {
                    double x = Math.Abs((i * 1f / routes.Count) - (currFrame2 * 1f / framesNum));
                    float a = (float)Math.Pow((1 / (x + 1)), 45);
                    var currPoints = routes[i].points;
                    if (currPoints.Count > 0 && a > 0.004)
                    {

                        float rr = (float)Math.Min(20, 20 * routes[i].length / (avLength * 3)) / 2;
                        brush.Color = trilinearAlphaColor(i, a);
                        var currPos = currPoints.Last().pos;
                        e.Graphics.FillEllipse(brush, (float)currPos.x + margin + r - rr, (float)currPos.y + margin + r - rr, 2 * rr, 2 * rr);

                    }
                }
            }
            else
            {
                route currRoute = Program.start(routes[displayStyle].alpha, r);
                var poi = currRoute.points;
                for (int i = 0; i < poi.Count - 1; i++)
                    e.Graphics.DrawLine(new Pen(Color.White), (float)poi[i].pos.x + margin + r, (float)poi[i].pos.y + margin + r, (float)poi[i + 1].pos.x + margin + r, (float)poi[i + 1].pos.y + margin + r);
                
                e.Graphics.DrawEllipse(new Pen(Color.Red), (float)routes[displayStyle].points.Last().pos.x - 3 + margin + r, (float)routes[displayStyle].points.Last().pos.y - 3 + margin + r, 6, 6);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            v position = new v(pictureBox1.PointToClient(Cursor.Position)) - new v(margin + r, margin + r);

            double closestDist = 100000;

            int closestPoint = 0;

            for(int i = 0; i < routes.Count; i++)
            {
                if (v.dist(routes[i].points.Last().pos, position) < closestDist)
                {
                    closestDist = v.dist(routes[i].points.Last().pos, position);
                    closestPoint = i;
                } 
            }
            displayStyle = closestPoint;
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            displayStyle = -1;
            pictureBox1.Refresh();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush smth = new SolidBrush(Color.AliceBlue);
            for (int i = 0; i < routes.Count; i++)
            {
                smth.Color = trilinearColor(i);
                e.Graphics.FillRectangle(smth, pictureBox2.Width * i * 1f / routes.Count, (float)(pictureBox2.Height - pictureBox2.Height * routes[i].length / (3 * avLength)), 1, 1);
            }
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush smth = new SolidBrush(Color.AliceBlue);
            for (int i = 0; i < routes.Count; i++)
            {
                smth.Color = trilinearColor(i);
                e.Graphics.FillRectangle(smth, pictureBox2.Width * i * 1f / routes.Count, (float)(pictureBox2.Height - pictureBox2.Height * routes[i].time / (3 * avTime)), 1, 1);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            framesNum = Convert.ToInt32(textBox1.Text);
        }

        static int paintThreadNum = 3;

        private void button1_Click(object sender, EventArgs e)
        {
            
            List<Thread> paintThreads = new List<Thread>();
            /*List<int> test = new List<int>();
            for (int i = 0; i < threadnum; i++)
                test.Add(i);*/
            for (int i = 0; i < paintThreadNum; i++)
                paintThreads.Add(null);
            for (int i = 0; i < paintThreadNum; i++)
                paintThreads[i] = new Thread(() => { int temp = paintThreads.IndexOf(Thread.CurrentThread); saveFrames(Convert.ToInt32(temp * framesNum * 1f / paintThreadNum), Convert.ToInt32((temp + 1f) * framesNum / paintThreadNum)); Console.WriteLine(">>t" + temp + " finished"); });
            for (int i = 0; i < paintThreadNum; i++)
                paintThreads[i].Start();
        }

        private void textBoxPointsNum_TextChanged(object sender, EventArgs e)
        {
            int temp = 100;
            try
            {
                temp = Convert.ToInt32(textBoxPointsNum.Text);
                if (temp > 1000000000 || temp < 0)
                {
                    textBoxPointsNum.Text = "value should be less then 10^9 and greater then 0";
                }
            }
            catch
            {
                textBoxPointsNum.Text = "wrong format";
            }
            
        }
    }
}
