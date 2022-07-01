using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using regressor2;
using System.Threading;
namespace AGI
{
    public class network
    {
        List<regressor> neurons;
        List<regressor> testneurons;
        List<double[]> inputs;
        List<double> rewards;
        List<List<int>> shape;
        public regressor best1;
        public bool ilinked;
        public int id = 0;
        bool memory;
        public int totalsteps;
        double publicer;
        public Bignetwork bign;
        public network(List<double[]>inputs2,List<double>rewards2,bool ismemorized,List<List<int>> newshape)
        {
            stopacc = 0.98;
            neurons = new List<regressor>();
            testneurons = new List<regressor>();
            shape = new List<List<int>>();
            memory = ismemorized;
            shape = newshape;
            inputs = inputs2;
            rewards = rewards2;
            regressor reg = new regressor(inputs, rewards, shape);
            reg.memory = ismemorized;
            best1 = new regressor(inputs, rewards, shape);
            best1.memory = ismemorized;
            reg.shufflelen = 10;
            reg.expandlimit = 0;
            reg.minpow = -1000;
            reg.maxpow = 1000;
            numthreads = 10;
            reg.neglectionlimit = 0;
            reg.Activationid = 1;
            reg.activationmemory = 0;
            neurons.Add(best1);
            testneurons.Add(reg);
        }//new regressor(inputs, rewards, shape);//
        public double stopacc;
        public double train2(int steps)
        {
            for (int ir = 0; ir < testneurons.Count; ir++) {
                int upsetlevel = 3;
                int q = 400;
                var reg = testneurons[ir];
                best1 = neurons[ir];
                var er = best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1); 
                var er2 = -100000.0;
                var oldr = -100000.0;
                int tsteps = 0;
                for (int i = 0; i < steps; i++)
                {

                    //  for (int i = 0; i < q; i++)
                    {
                        reg.Train();
                        if (memory)
                        {
                            reg.Train(true);
                        }
                        //    reg.shufflelen = 10;
                        reg.sadd = 0;
                        er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                     //   er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        if (er2 - oldr > 0.00001)
                        {
                            reg.sadd = 1;
                            upsetlevel++;
                        }
                        else
                        {
                            upsetlevel -= 1;
                            if (upsetlevel <= 0 && i > 0.35 * q)
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

                        if (er2 > publicer)
                        {
                            upsetlevel = 14;
                            reg.sadd = 1;
                            best1 = reg.copy();
                            
                            neurons[ir] = best1;
                            er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                            if (er2 >= stopacc*reg.binputs.Count)
                            {
                                totalsteps += i;
                                tsteps = 0;
                               // reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                              //  best1.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                                break;
                            }
                            publicer = er2;

                        }

                    }
                    tsteps++;
                    var zz = best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1);
                }
                totalsteps += tsteps;
                
            }
            best1.idofi = 0;
            var z = best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1);
            return z;
        }
        List<Thread> ths;
        public int numthreads;
        int threadi;
        int tosteps;
        List<List<double>> memoryvals;
        public void keepmemory()
        {
            memoryvals = best1.copymemory(best1.memory_vals);
        }
        public double[] calc(double[] input, List<double> timouts, List<double> lastouts,bool record=false)
        {
            var reg = best1;
            if (record)
            {
                reg.timouts.Add(timouts);
                reg.lastouts.Add(lastouts);
                reg.idofi = lastouts.Count - 1;
                reg.inputs.Add(input);
                reg.outputs.Add(0.00001);
            }
            return reg.calc(reg.equation, input, activationid: reg.Activationid, withmemory: memory);
        }
        public void train(int steps)
        {
            int n_threads = numthreads;
            // network ntw = new network(inputs, rewards, ismemorized: true);
            //netws.Add(ntw);
            threadi = 0;
            tosteps = steps;
            ths = new List<Thread>();
            //trainnet();
            publicer = -1000000;
            //   train2(1000);
            bests = new List<regressor>();
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
                wisesteps = steps;
                wreg =  testneurons[0].copy();
                wreg.net = this;
                best1.net = this;
                bests.Add(wreg.copy());
                //wreg.shufflelen = Convert.ToInt32(inputs.Count*0.1);new regressor(inputs, rewards, shape);//
                //wreg.expandlimit = 0;
                //wreg.minpow = -100000;
                //wreg.maxpow = 100000;
                //wreg.neglectionlimit = 0;
                //wreg.memory = best1.memory;
                //wreg.Activationid = 1;
                //wreg.activationmemory = 1;
                ThreadStart thr = new ThreadStart(wisetrain);
                Thread.Sleep(1);
                Thread th = new Thread(thr);
                Thread.Sleep(1);
                ths.Add(th);
                Thread.Sleep(1);
                wisetrain();
                //th.Start();
            }
            while (true && publicer< stopacc * best1.binputs.Count)
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
            }

            
            if(publicer > stopacc * best1.binputs.Count)
            {
                foreach(var th in ths)
                {
                    if (th.ThreadState==ThreadState.Running)
                    {
                        th.Abort();
                    }
                }
            }
            best1 = bests[bestid].copy();
            best1.idofi = 0;
            neurons[0] = best1;
            //best1.reward(best1.equation, inputs, rewards, par: 1);
        }
        int wisesteps;
        regressor wreg;
        List<regressor> bests;
        public int bestid;
        void wisetrain()
        {
            var steps = wisesteps;
            var reg = wreg;
            int id = threadi;
            for (int ir = 0; ir < testneurons.Count; ir++)
            {
                int upsetlevel = 3;
                int q = 400;
               // var reg = testneurons[ir];
                best1 = neurons[ir];
                var er = best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1);
                var er2 = -100000.0;
                var oldr = -100000.0;
                int tsteps = 0;
                for (int i = 0; i < steps; i++)
                {

                    //  for (int i = 0; i < q; i++)
                    {
                        reg.Train();
                        if (memory)
                        {
                            reg.Train(true);
                        }
                        //    reg.shufflelen = 10;
                        reg.sadd = 0;
                        reg.idofi = 0;
                    //    var er22 = 0.0;
                         er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        reg.idofi = reg.idofi2;
                    //    er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        //   er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        //if (er22 -er2>1000)
                        //{
                        //    reg.idofi = 0;
                        //     er22 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        //    reg.idofi = reg.idofi2;
                        //    er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                        //}
                        if (er2 - oldr > 0.00001)
                        {
                            reg.sadd = 1;
                            upsetlevel++;
                        }
                        else
                        {
                            upsetlevel -= 1;
                            if (upsetlevel <= 0 && i > 0.35 * q)
                            {
                                if (er2 > er)
                                {
                                    best1 = reg.copy();
                                }
                                er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                                break;
                            }
                        }
                        if (er2 > 0.7)
                        {
                            reg.neglect = true;
                        }

                        oldr = er2;

                        if (er2 > er)
                        {
                            upsetlevel = 14;
                            reg.sadd = 1;
                            //  best1 = reg.copy();
                            //reg.Inputss_memory[0] = reg.copymemory(reg.Inputss_memory[reg.idofi]);
                            bests[id] = reg.copy();
                            neurons[ir] = reg.copy();
                            best1.idofi = 0;
                            best1 = bests[id];
                            //  er2 = reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                            if (er2 > publicer)
                            {
                                publicer = er2;
                                bestid = id;
                            }
                            er = er2;
                            if (er2 >= stopacc * reg.binputs.Count)
                            {
                                totalsteps += i;
                                tsteps = 0;
                                // reg.idofi = 0;

                           //     reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                            //    best1.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                                //  reg.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                                //    best1.reward(reg.equation, reg.binputs, reg.boutputs, par: 1);
                                break;
                            }
                            
                            

                        }

                    }
                    tsteps++;
                   // var zz = best1.reward(best1.equation, best1.binputs, best1.boutputs, par: 1);
                }
                totalsteps += tsteps;

            }
            best1.idofi = 0;
        }
    }
}
