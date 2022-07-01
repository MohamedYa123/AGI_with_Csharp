using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using regressor2;
namespace AGI
{
    public class Bignetwork
    {
        List<network> nets;
        public List<int[]> links;
        public int maxloops = 10;//when you link to a previous network it's considered a loop
        public List<double[]> inputs;
        public List<double> rewards;
        bool memory;
        public double stopaccnormal;
        public double stopaccbranch;
        public Bignetwork(List<double[]> inputs2, List<double> rewards2, bool ismemorized)
        {
            memory = ismemorized;
            stopaccnormal = 0.995;
            stopaccbranch = 0.995;
            inputs = inputs2;
            rewards = rewards2;
            network nt = new network(inputs2, rewards2, ismemorized,createshape());
            nt.bign = this;
            nt.ilinked = true;
            links = new List<int[]>();
            nets = new List<network>();
            nets.Add(nt);
            int[] flink = { 0, -1, -1 };//type , link1,link2 ##
            links.Add(flink);
        }
        int threadi;
        List<Thread> ths;
        public int tosteps;
        public void train(int totalsteps)
        {
            int n_threads = nets.Count;
           // network ntw = new network(inputs, rewards, ismemorized: true);
            //netws.Add(ntw);
            threadi = 0;
            tosteps = totalsteps;
            ths = new List<Thread>();
            //trainnet();
            for (int i = 0; i < n_threads; i++)
            {
                Thread.Sleep(1);
               // ntw = new network(inputs, rewards, ismemorized: true);
                Thread.Sleep(1);
            //    netws.Add(ntw);
                Thread.Sleep(1);
                threadi = i;
                //trainnet();
                Thread.Sleep(1);
                ThreadStart thr = new ThreadStart(trainnet);
                Thread.Sleep(1);
                Thread th = new Thread(thr);
                Thread.Sleep(1);
                ths.Add(th);
                Thread.Sleep(1);
                //trainnet();
                th.Start();
            }
            while (true)
            {
                int numaborted = 0;
                foreach (Thread th in ths)
                {
                    if (!th.IsAlive)
                    {
                        numaborted++;
                    }
                }
                if (numaborted >= ths.Count)
                {
                    break;
                }
               // Thread.Sleep(1);
                
            }
            for (int i = 0; i <1 ; i++)
            {
                foreach (var nt in nets)
                {
                    
                }
            }
        }
        public double rwd;
        private void trainnet()
        {

            var nt = nets[threadi];
            nt.train(tosteps);

        }
        public void expand()
        {
            int i = 0;
            foreach(var ne in nets)
            {
                ne.best1.breward = ne.best1.reward(ne.best1.equation,inputs, rewards,par:1);
                //  ne.best1.traininitialmemory();
                ne.bign = this;
                ne.id = i;
                i++;
            }
            var sorts = sortworest(nets);
            List<network> wsorts = new List<network>();
            foreach(var e in sorts)
            {
                wsorts.Add(e);
            }
            expandhere(wsorts[wsorts.Count() - 1].id);
            int tsteps = wsorts[wsorts.Count - 1].totalsteps;
            if (tsteps > 500)
            {
                tsteps = 500;
            }
            //train
            int n_threads = 3;
            // network ntw = new network(inputs, rewards, ismemorized: true);
            //netws.Add(ntw);
            threadi = 0;
            tosteps = tsteps;
            ths = new List<Thread>();
            //trainnet();
            for (int ii = 0; ii < n_threads; ii++)
            {
                Thread.Sleep(10);
                // ntw = new network(inputs, rewards, ismemorized: true);
                Thread.Sleep(10);
                //    netws.Add(ntw);
                Thread.Sleep(10);
                threadi =nets.Count- ii-1;
                //trainnet();
                Thread.Sleep(10);
                ThreadStart thr = new ThreadStart(trainnet);
                Thread.Sleep(10);
                Thread th = new Thread(thr);
                Thread.Sleep(10);
                ths.Add(th);
                Thread.Sleep(10);
                trainnet();
                 //th.Start();
            }
            while (true)
            {
                int numaborted = 0;
                foreach (Thread th in ths)
                {
                    if (!th.IsAlive)
                    {
                        numaborted++;
                    }
                }
                if (numaborted >= ths.Count)
                {
                    break;
                }
                Thread.Sleep(1);

            }
            //for(int i2 = 0; i2 < tsteps; i2++)
            {
               // nets[nets.Count - 3].train(1);
             //   nets[nets.Count - 2].train(1);
              //  nets[nets.Count - 1].train(1);
            }
        }
        public void expandhere(int idoflink)
        {
            var nt = nets[idoflink];
            network nt1 = new network(nt.best1.inputs,nt.best1.outputs, memory,createshape());
            network nt2 = new network(nt.best1.inputs, nt.best1.outputs, memory,createshape());
            network ntbranch = new network(nt.best1.inputs, nt.best1.outputs, memory,createshape());
            nt1.stopacc = stopaccnormal;
            nt2.stopacc = stopaccnormal;
            ntbranch.stopacc = stopaccbranch;
            var oldlink = links[idoflink];
            nt1.bign = this;
            nt2.bign = this;
            ntbranch.bign = this;
            nets.Add(ntbranch);
            nets.Add(nt2);
            nets.Add(nt1);
            int[] newlinkbranch = { 1, nets.Count - 2, nets.Count - 1 };
            links.Add(newlinkbranch);
            int[] newlink2 = { 0, oldlink[2], oldlink[2] };
            links.Add(newlink2);
            int[] newlink1 = { 0, oldlink[1], oldlink[1] };
            links.Add(newlink1);
            oldlink[1] = nets.Count - 3;
            oldlink[2] = nets.Count - 3;

        } 
        public void keepmemory()
        {
            foreach(var net in nets)
            {
                net.keepmemory();
            }
        }
        public double[] calc(double[] input)
        {
            int loops = maxloops;
            int id = 0;
            List<double> timouts = new List<double>();
            List<double> lastouts = new List<double>();
            timouts.Add(1.0);
            lastouts.Add(1.0);
            bool record = true;
            double[] output = new double[nets[0].best1.equation[0].Count];
            while (loops > 0)
            {
                var cnet = nets[id];
                var link = links[id];
                var lastid = id;
                output = cnet.calc(input, timouts, lastouts, record);
                if (link[0] == 0)
                {
                    id = link[1];
                    lastouts = copylastouts(output);
                }
                else if (link[0] == 1)
                {
                    if (output[0] >= 0.8)
                    {
                        id = link[1];
                    }
                    else if (output[0] <= -0.8)
                    {
                        id = link[2];
                    }
                    else
                    {
                        var fme = output;
                        var lme = output;
                        if (link[1] != -1)
                        {
                            fme = calcfromhere(input, link[1], loops, timouts, lastouts, record);
                        }
                        if (link[2] != -1)
                        {
                            lme = calcfromhere(input, link[2], loops, timouts, lastouts, record);
                        }
                        output = mergeouts(fme, lme, output[0]);
                        return output;
                    }
                }
                if (id == -1)
                {
                    break;
                }
                if (id < lastid)
                {
                    loops--;
                }

            }
            return output;
        }
        double[] mergeouts(double[] foutput,double[] loutput,double indxofmerge)
        {
            double[] mout = new double[foutput.Length];
            indxofmerge = (1 + indxofmerge) / 2;
            double fme = indxofmerge;
            double lme = 1-indxofmerge;

            for(int i = 0; i < mout.Length; i++)
            {
                mout[i] = (foutput[i] * fme + loutput[i] * lme);
            }
            return mout;
        }
        List<double> copytimlasts(List<double> enter)
        {
            List<double> ans=new List<double>();
            foreach(var a in enter)
            {
                ans.Add(a + 0.0);
            }
            return ans;
        }
        public double[] calcfromhere(double[] input,int id,int loops, List<double> timouts, List<double> lastouts,bool record)
        {
            double[] output = new double[nets[0].best1.equation[0].Count];
            timouts = copytimlasts(timouts);
            lastouts = copytimlasts(lastouts);
            while (loops > 0)
            {
                var cnet = nets[id];
                var link = links[id];
                var lastid = id;
                output = cnet.calc(input, timouts, lastouts, record);
                if (link[0] == 0)
                {
                    id = link[1];
                    lastouts = copylastouts(output);
                }
                else if (link[0] == 1)
                {
                    if (output[0] >= 0.8)
                    {
                        id = link[1];
                    }
                    else if (output[0] <= -0.8)
                    {
                        id = link[2];
                    }
                    else
                    {
                        var fme = output;
                        var lme = output;
                        if (link[1] != -1)
                        {
                            fme = calcfromhere(input, link[1], loops, timouts, lastouts, record);
                        }
                        if (link[2] != -1)
                        {
                            lme= calcfromhere(input, link[2], loops, timouts, lastouts, record);
                        }
                        output = mergeouts(fme, lme, output[0]);
                        return output;
                    }
                }
                if (id == -1)
                {
                    break;
                }
                if (id < lastid)
                {
                    loops--;
                }

            }
            return output;
        }
        public double[] calcexistid(int id,double[] input,double[] output, bool record)
        {
            var link = links[id];
            int nid = 0;
            var loops = maxloops;
            var timouts = new List<double>();
            var lastouts = new List<double>();
            
            if (link[0] == 0)
            {
                nid = link[1];
                lastouts = copylastouts(output);
            }
            else if (link[0] == 1)
            {
                if (output[0] >= 0.8)
                {
                    nid = link[1];
                }
                else if (output[0] <= -0.8)
                {
                    nid = link[2];
                }
                else
                {
                    var fme = output;
                    var lme = output;
                    if (link[1] != -1)
                    {
                        fme = calcfromhere(input, link[1], loops, timouts, lastouts, record);
                    }
                    if (link[2] != -1)
                    {
                        lme = calcfromhere(input, link[2], loops, timouts, lastouts, record);
                    }
                    output = mergeouts(fme, lme, output[0]);
                    return output;
                }
            }
            if (nid != -1) {
                output = calcfromhere(input, nid, loops, timouts, lastouts, record); }
            return output;
        }
        public List<double> copylastouts(double[] output)
        {
            List<double> ans = new List<double>();
            foreach(var o in output)
            {
                ans.Add(o + 0.0);
            }
            return ans;
        }
        List<List<int>> createshape(int f1=1,int f2=1,int f3=11)
        {
            List<List<int>> shape = new List<List<int>>();
            for (int i = 0; i < f1; i++)
            {
                List<int> sh = new List<int>();
                for (int i2 = 0; i2 < f2; i2++)
                {
                    sh.Add(f3);
                }
                shape.Add(sh);
            }
            return shape;
        }
        public void enhance()
        {

        }
        static IEnumerable<network> sortworest(IEnumerable<network> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.best1.breward descending
                         select s;
            return sorted;
        }
    }
}
