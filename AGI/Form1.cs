using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DeviceId;
using DeviceId.Windows;
using DeviceId.Windows.Wmi;
using regressor2;

namespace AGI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<double[]> inputs;
        List<double> rewards;
        Random rds=new Random();
        double rr = 9;
        double b = 0.7;
        private double circle(double r)
        {
            return Math.Sqrt(rr - Math.Pow(r, 2));
        }
        private double traingle(double r)
        {
            double ans;
          //  double b = 0.7;
            if (r <= 1.5)
            {
                ans = 0 + b * r;
            }
            else
            {
                ans = b*r-2.1;
            }
            return ans;
        }
        private void refreshparams()
        {
            rr = Convert.ToDouble(rds.Next(9, 19));
            b = Convert.ToDouble(rds.Next(1,12))/10;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            netws = new List<network>();
            ths = new List<Thread>();
            rewards = new List<double>();
            inputs = new List<double[]>();
            Random rd = new Random();
            for (int i = 0; i < 100; i++)
            {

                double[] dt = new double[8];
                for (int i2 = 0; i2 < dt.Length; i2++)
                {

                    dt[i2] = rd.Next(0,2)+0.000001;
                }
                double rwd = 0;
                if (i%2 == 0)
                {
                    //triangle
                    List< double[] >n = new List<double[]>();
                    for(int i2 = 0; i2<=3;i2++)
                    {
                        double[] q = new double[2];
                        q[0] = i2 - 0.1;
                        q[1] = 1;// traingle(q[0]);
                        n.Add(q);
                    }
                    List<int> arranges = new List<int>();
                    for (int i3 = 0; i3 < n.Count; i3++)
                    {
                        arranges.Add(i3);
                    }
                    Random r = new Random();
                    for (int y = 0; y < n.Count; y++)
                    {
                        if (arranges.Count == 0)
                        {
                            break;
                        }
                        var x = arranges.Count - 1;// r.Next(0, arranges.Count);
                        int q = arranges[x];
                        dt[q * 2] = n[q][0];
                        dt[q*2+1]= n[q][1];
                        arranges.RemoveAt(x);
                    }
                //    inputs.Add(dt);
                    rewards.Add(1);
                }
                else
                {
                    //circle

                //    double[] dt = new double[12];
                    for (int i2 = 0; i2 < dt.Length; i2++)
                    {

                        dt[i2] = rd.Next(0, 2) - 0.1;
                    }
                 //   double rwd = 0;
                    //if (rd.Next(0, 2) == 0)
                    //{
                        //triangle
                        List<double[]> n = new List<double[]>();
                        for (int i2 = 0; i2 <= 3; i2++)
                        {
                            double[] q = new double[2];
                            q[0] = i2 - 0.1;
                        q[1] = 1;// circle(q[0]);
                            n.Add(q);
                        }
                        List<int> arranges = new List<int>();
                        for (int i3 = 0; i3 < n.Count; i3++)
                        {
                            arranges.Add(i3);
                        }
                        Random r = new Random();
                        for (int y = 0; y < n.Count; y++)
                        {
                            if (arranges.Count == 0)
                            {
                                break;
                            }
                        var x = arranges.Count - 1;// r.Next(0, arranges.Count);
                            int q = arranges[x];
                            dt[q * 2] = n[q][0];
                            dt[q * 2 + 1] = n[q][1];
                            arranges.RemoveAt(x);
                        }
                      //  inputs.Add(dt);
                        rewards.Add(-1);
                }
                //
                refreshparams();
                inputs.Add(dt);
            }
        }
        //نجح لبرنامج في التمييز بين المثلث و الدائرة
        private void button1_Click(object sender, EventArgs e)
        {
        //    var g = new DeviceIdBuilder();
          //  string r = g.OnWindows(windows => windows.AddMotherboardSerialNumber()).ToString()+ g.OnWindows(windows => windows.AddProcessorId()).ToString()+g.AddMachineName().ToString();
//.AddMachineName()
//.AddMacAddress()
//.AddProcessorId()
//.AddMotherboardSerialNumber()
//.ToString();
            List<List<int>> shape = new List<List<int>>();
            for (int i = 0; i < 1; i++)
            {
                List<int> sh = new List<int>();
                for (int i2 = 0; i2 < 1; i2++)
                {
                    sh.Add(1);
                }
                shape.Add(sh);
            }
            regressor reg = new regressor(inputs,rewards,shape);
            regressor best1= new regressor(inputs, rewards, shape);
            reg.shufflelen = 20;
            reg.expandlimit = 0;
            reg.minpow = -1000;
            reg.maxpow = 1000;
            var er = -100000.0;
            var er2 = -100000.0;
            var oldr = -100000.0;
            reg.neglectionlimit = 0.0001;
            reg.Activationid = 1;
            int upsetlevel = 3;
            int q = 400;
            for (int i = 0; i < q; i++)
            {
                reg.Train();
                //    reg.shufflelen = 10;
                reg.sadd = 0;
                er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                if (er2 - oldr > 0.00001)
                {
                    reg.sadd = 1;
                    upsetlevel++;
                }
                else 
                {
                    upsetlevel -= 1;
                    if (upsetlevel <= 0 && i>0.35*q)
                    {
                        if (er2 > er)
                        {
                            best1 = reg.copy();
                        }
                        er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        break;
                    }
                }
                if (er > 0.7)
                {
                    reg.neglect = true;
                }

                oldr = er2;

                if (er2 > er)
                {
                    upsetlevel = 14;
                    reg.sadd = 1;
                    best1 = reg.copy();
                    er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                    if (er2 >= 0.99  )
                    {
                        break;
                    }
                    er = er2;

                }

            }
            var z= best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1);
        }
        List<network> netws;
        List<Thread> ths;
        double bestreward=-1000000;
        int threadi = -110000;
        int bestid;
        [Obsolete]
        private void button2_Click(object sender, EventArgs e)
        {
            //int n_threads = 20;
            //network ntw = new network(inputs, rewards, ismemorized: true);
            ////netws.Add(ntw);
            //threadi = 0;
            ////trainnet();
            //for (int i = 0; i < n_threads; i++)
            //{
            //    Thread.Sleep(10);
            //     ntw = new network(inputs, rewards, ismemorized: true);
            //    Thread.Sleep(10);
            //    netws.Add(ntw);
            //    Thread.Sleep(10);
            //    threadi = i;
            //    //trainnet();
            //    Thread.Sleep(10);
            //    ThreadStart thr = new ThreadStart(trainnet);
            //    Thread.Sleep(10);
            //    Thread th = new Thread(thr);
            //    Thread.Sleep(10);
            //    ths.Add(th);
            //    Thread.Sleep(10);
            //    th.Start();
            //}
        }
        public void trainnet()
        {
            int thid = threadi;
            var ntw = netws[thid];
            var f = 0;// ntw.train(1000);
            if (f > bestreward)
            {
                bestreward = f;
                bestid = thid;
            }
        }
        int counts;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ths.Count > 0)
            {
                counts++;
                if (bestreward > 0.88*netws[0].best1.binputs.Count || counts==10)
                {
                    foreach(var th in ths)
                    {
                        th.Abort();
                    }
                    var b = netws[0].best1;
                    b.idofi = 0;
                    b.reward(b.equation, b.binputs, b.boutputs, par: 1);
                    Text = bestreward + " "+counts;
                    timer1.Stop();
                }
            }
        }
        System.Diagnostics.Stopwatch watch;
        private void button3_Click(object sender, EventArgs e)
        {
            watch = new System.Diagnostics.Stopwatch();
            Bignetwork bign = new Bignetwork(inputs, rewards, true);
            
            watch.Start();
            bign.train(1000);
            
            watch.Stop();
            
           
            MessageBox.Show(watch.ElapsedMilliseconds + " ms");
            bign.expand();
            bign.keepmemory();
            bign.calc(inputs[0]);
            //bign.expand();
           // bign.expand();
        }
    }
}
