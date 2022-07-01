using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AGI;
namespace regressor2
{
    public class regressor
    {
        public List<double[]> inputs;
        public List<double> outputs;
        public List<double[]> binputs;
        public List<double> boutputs;
        public List<List<double[]>> equation;
        public List<List<double[,]>> sides;
        public List<List<double[,]>> sides_m;
        public double expandlimit;
        public int shufflelen;
        double centerpoint;
        bool expandd;
        List<List<int>> Shape;
        double mainacc;
        public double accuracy;
        public bool neglect;
        public double neglectionlimit;
        public bool bracks;
        public double brackelimit;
        public int id;
        public int index;
        public map mp;
        public int Activationid;
        public bool memory;
        public List<List<double>> timouts;
        public List<List<double>> lastouts;
        public List<List<double>> memory_vals;
        public List<List<List<double>>> Inputss_memory;
        public List<List<double[]>> equation_m;
        public double breward;
        public int activationmemory;
        public network net;
        public regressor copy()
        {
            regressor reg = new regressor(binputs, boutputs, Shape,skip:true);
            reg.inputs = inputs;
            reg.outputs = outputs;
            reg.binputs = binputs;
            reg.boutputs = boutputs;
            reg.equation = copier();
            reg.sides = sidecopy();
            reg.equation_m = copier_m();
            reg.sides_m = sidecopy_m();
            reg.Activationid = Activationid;
            reg.activationmemory = activationmemory;
            reg.shufflelen = shufflelen;
            reg.memory = memory;
            reg.timouts = timouts;
            reg.memory_vals =copymemory( memory_vals);
            reg.Inputss_memory =copyinputsmemory(Inputss_memory);
            reg.equation_m = equation_m;
            reg.memoryside1 = memoryside1;
            reg.memoryside2 = memoryside2;
            reg.shufflelen = shufflelen;
            reg.maxpow = maxpow;
            reg.minpow = minpow;
            reg.idofi = 0;
            reg.net = net;
            return reg;
        }
        public double termlimit;
        public int sadd;
        Random r;
        public regressor(List<double[]> Inputs, List<double> Outputs, List<List<int>> shape,bool skip=false)
        {
            expandd = false;
            inputs = Inputs;
            termlimit = 10000;
            ti = Inputs;
            to = Outputs;
            expandlimit = 0.01;
            shufflelen = 200;
            Shape = shape;
            outputs = Outputs;
            equation = new List<List<double[]>>();
            sides = new List<List<double[,]>>();
            binputs = new List<double[]>();
            boutputs = new List<double>();
            mainacc = -1000000;
            neglectionlimit = 0.001;
            neglect = false;
            minpow = -3;
            maxpow = 3;
            //memoryside1 = -1;
            memoryside1 = new List<List<double>>();
            memoryside2 = new List<List<double>>();
            activationmemory = 0;
            //memoryside2 = 1;
            memory_vals = new List<List<double>>();
            timouts = new List<List<double>>();
            Inputss_memory = new List<List<List<double>>>();
            lastouts = new List<List<double>>();
            Random rd = new Random();
            r = new Random();
            if (skip)
            {
                return;
            }

            for (int i = 0; i < inputs.Count; i++)
            {
                binputs.Add(inputs[i]);
                boutputs.Add(outputs[i]);
            }

            for (int i = 0; i < shape.Count; i++)
            {
                List<double[]> temp = new List<double[]>();
                List<double[,]> sides2 = new List<double[,]>();
                List<double> mr = new List<double>();
                List<double> tims = new List<double>();
                for (int i2 = 0; i2 < shape[i].Count; i2++)
                {
                    double[] temp2= new double[shape[i][i2]];
                    double[,] sides3= new double[shape[i][i2],2];
                    for (int i3 = 0; i3 < temp2.Length; i3++)
                    {
                        temp2[i3] = Convert.ToDouble(rd.Next(-1000, 1000))/1000+0.00001;
                        if (i3 == 0)
                        {
                            temp2[i3] = Convert.ToDouble(rd.Next(-1000, 1000)) / 1000 + 0.00001;
                        }
                        sides3[i3, 0] = temp2[i3]-1;
                        sides3[i3, 1] = temp2[i3]+1;
                    }
                    temp.Add(temp2);
                    mr.Add(0.0001);
                    tims.Add(1);
                    //  mr.Add(mr2);
                    sides2.Add(sides3);
                }
                memory_vals.Add(mr);
                timouts.Add(tims);
                equation.Add(temp);
                sides.Add(sides2);
            }
            
            foreach(var g_m in memory_vals)
            {
                List<double> side11=new List<double>();
                List<double> side22=new List<double>();
                foreach(var g_m2 in g_m)
                {
                    side11.Add(g_m2 - 1);
                    side22.Add(g_m2 + 1);
                }
                memoryside1.Add(side11);
                memoryside2.Add(side22);
            }
            for(int i = 0; i < binputs.Count; i++)
            {
                Inputss_memory.Add(copymemory(memory_vals));
            }
            
            equation_m = copier();
            sides_m = sidecopy();
        }
        public static double activationfunc(double val,int id = 0)
        {
            var ans = 0.0;
            switch (id)
            {
                case 0:
                    ans = val;
                    break;
                case 1:
                    ans = Math.Tanh(val);
                    break;
            }
            return ans;
        }
        
        public  double[] calc(List<List<double[]>> equationtemp, double[] input,int activationid=0,bool withmemory=false)
        {
            double ans = 0;
            var tempinput = input;
            int gg = 0;
            foreach(var q in equationtemp)
            {
                var tempinput2 = new double[input.Length];
                for (int ii = 0; ii < tempinput2.Length; ii++)
                {
                    tempinput2[ii] = tempinput[ii] + 0.0;
                }
                int bigi = 0;
                foreach(var z in q)
                {
                    ans = 0;
                    int i = 0;
                    foreach(var c in z)
                    {
                        var sd = 1.0;
                        if (i == 0)
                        {
                            Random rd = new Random();
                            sd = memory_vals[gg][bigi];//  Convert.ToDouble(rd.Next(1,300))/3000;
                            if (withmemory)
                            {
                                List<List<double[]>> eqm = new List<List<double[]>>();
                                List<double[]> cf = new List<double[]>();
                                cf.Add(equation_m[gg][bigi]);
                                eqm.Add(cf);
                                sd = calc(eqm, input, activationid: activationmemory)[0];
                                memory_vals[gg][bigi] = sd;
                            }
                        }
                        //else if (i==1)
                        //{
                        //    sd = tempinput[bigi] * tempinput[1];
                        //}
                        else if (i == 1)//last output
                        {
                            if (lastouts.Count > 0)
                            { sd = lastouts[gg][bigi]; }
                        }
                        else if (i == 2)//external timouts
                        {
                            if (timouts.Count > 0)
                            { sd = timouts[gg][bigi]; }
                        }
                        else if (i == 3)
                        {
                            sd = 1;
                        }
                        else
                        {
                            
                            sd =  tempinput[i - 4];
                            if (!withmemory)
                            {
                            //    sd = 0;
                            }
                            if (gg < 0)
                            {
                                sd *= tempinput[bigi];
                            }
                        }
                        
                        ans += c * sd;
                        i++;
                    }
                    tempinput2[bigi] = activationfunc(ans, activationid);
                    bigi++;
                }
                tempinput = tempinput2;
                gg++;
            }
            return tempinput;
        }

        public void shuffler()
        {
            inputs = new List<double[]>();
            outputs = new List<double>();
            List<int> arranges = new List<int>();
            for (int i = 0; i < binputs.Count; i++)
            {
                arranges.Add(i);
            }
            Random r = new Random();
            for (int y = 0; y < shufflelen; y++)
            {
                if (arranges.Count == 0)
                {
                    break;
                }
                var x = r.Next(0, arranges.Count);
                int q = arranges[x];
                inputs.Add(binputs[q]);
                outputs.Add(boutputs[q]);
                arranges.RemoveAt(x);
            }
        }
        public  int idofi;
        public  int idofi2;
        public void shuffler_m()
        {
            inputs = new List<double[]>();
            outputs = new List<double>();
            List<int> arranges = new List<int>();
            for (int i = 0; i < binputs.Count; i++)
            {
                arranges.Add(i);
            }
            Random r = new Random();
            var x = r.Next(0, arranges.Count-shufflelen);
            idofi = x;
            idofi2 = x;
            for (int y = 0; y < shufflelen; y++)
            {
                if (arranges.Count == 0)
                {
                    break;
                }
                
                int q = arranges[x];
                inputs.Add(binputs[q]);
                outputs.Add(boutputs[q]);
                x++;
              //  arranges.RemoveAt(x);
            }
        }
        bool record;
        public List<List<double>> copymemory(List<List<double>> m_v)
        {

            List<List<double>> t = new List<List<double>>();
            foreach(var k in m_v)
            {
                List<double> tt = new List<double>();
                foreach (var kk in k)
                {
                    tt.Add(kk + 0);
                }
                t.Add(tt);
            }
            return t;
        }
        public List<List<List<double>>> copyinputsmemory(List<List<List<double>>> m_v2)
        {
            List<List<List<double>>> nlists = new List<List<List<double>>>();
            foreach (var m_v in m_v2)
            {
                List<List<double>> t = new List<List<double>>();
                foreach (var k in m_v)
                {
                    List<double> tt = new List<double>();
                    foreach (var kk in k)
                    {
                        tt.Add(kk + 0);
                    }
                    t.Add(tt);
                }
                nlists.Add(t);
            }
            return nlists;
        }
        public double reward(List<List<double[]>>equationtemp, List<double[]> Inputs, List<double> Outputs, double par = 0.5)
        {
            double acc = 0;
            memory_vals = copymemory(Inputss_memory[idofi]);
            for (int i = 0; i < Inputs.Count; i++)
            {
                var zeft = calc(equationtemp, Inputs[i],Activationid,withmemory:memory);
                zeft = net.bign.calcexistid(net.id, Inputs[i], zeft, false);
                acc += 1 - Math.Pow(Math.Abs((Outputs[i] - zeft[0]) / Outputs[i]), par);
                if (record && idofi + i + 1< Inputss_memory.Count)
                {
                    Inputss_memory[idofi + i+1] = copymemory(memory_vals);
                }
            }
          //  acc /= Inputs.Count;
            return acc;
        }
        public List<List<double[]>> copier()
        {
            List<List<double[]>> cn = new List<List<double[]>>();
            for (int i = 0; i < equation.Count; i++)
            {
                List<double[]> tcc = new List<double[]>();
                for (int i2 = 0; i2 < equation[i].Count; i2++)
                {
                    double[] g = new double[equation[i][i2].Length];
                    for(int i3 = 0; i3 < equation[i][i2].Length; i3++)
                    {
                        g[i3] = equation[i][i2][i3] + 0.0;
                    }
                    tcc.Add(g);
                }
                cn.Add(tcc);
            }
            return cn;
        }
        public List<List<double[]>> copier_m()
        {
            List<List<double[]>> cn = new List<List<double[]>>();
            for (int i = 0; i < equation_m.Count; i++)
            {
                List<double[]> tcc = new List<double[]>();
                for (int i2 = 0; i2 < equation[i].Count; i2++)
                {
                    double[] g = new double[equation[i][i2].Length];
                    for (int i3 = 0; i3 < equation[i][i2].Length; i3++)
                    {
                        g[i3] = equation_m[i][i2][i3] + 0.0;
                    }
                    tcc.Add(g);
                }
                cn.Add(tcc);
            }
            return cn;
        }
        public List<List<double[,]>> sidecopy()
        {
            List<List<double[,]>> sides3 = new List<List<double[,]>>();
            
            for (int i = 0; i < Shape.Count; i++)
            {
                List<double[,]> sides4 = new List<double[,]>();
                for (int i2 = 0; i2 < Shape[i].Count; i2++)
                {
                    double[,] sides2 = new double[sides[0][0].Length/2, 2];
                    for (int i3 = 0; i3 < Shape[i][i2]; i3++)
                    {
                        sides2[i3, 0] = sides[i][i2][i3, 0] + 0;
                        sides2[i3, 1] = sides[i][i2][i3, 1] + 0;
                    }
                    sides4.Add(sides2);
                }
                sides3.Add(sides4);
            }
            return sides3;
        }
        public List<List<double[,]>> sidecopy_m()
        {
            List<List<double[,]>> sides3 = new List<List<double[,]>>();
            double[,] sides2 = new double[sides[0][0].Length / 2, 2];
            for (int i = 0; i < Shape.Count; i++)
            {
                List<double[,]> sides4 = new List<double[,]>();
                for (int i2 = 0; i2 < Shape[i].Count; i2++)
                {
                    for (int i3 = 0; i3 < Shape[i][i2]; i3++)
                    {
                        sides2[i3, 0] = sides_m[i][i2][i3, 0] + 0;
                        sides2[i3, 1] = sides_m[i][i2][i3, 1] + 0;
                    }
                    sides4.Add(sides2);
                }
                sides3.Add(sides4);
            }
            return sides3;
        }
        bool crtb;
        public double pointeval(double side, double avg, int i1, int i2,int i3, double accm)
        {
            double crt = (side + avg) / 2;
            List<List<double[]>> l1;
            List<List<double[]>> l2;
            List<List<double[]>> l3;
            crtb = false;
            var l_m = copier_m();
            var l = copier();
            //var accr = accCalc(l, ti, to, par: 1);
            if (itrainm)
            {
                equation_m[i1][i2][i3] = crt;
                l1 = copier_m();
            }
            else
            {
                l[i1][i2][i3] = crt;
                l1 = l;
                l = copier();
            }
            var acc1 = reward(l, inputs, outputs, par: 1);
            
            if (itrainm)
            {
                equation_m[i1][i2][i3] = side;
                l2 = copier_m();
            }
            else
            {
                l[i1][i2][i3] = side;
                l2 = l;
                l = copier();
            }
            // shuffler();
            var acc2 = reward(l, inputs, outputs, par: 1);
            
            l = equation;
            //l[i1][i2] = avg;
            if (itrainm)
            {
                equation_m[i1][i2][i3] = avg;
                record = true;
                l3 = copier_m(); ;
                //   accm = reward(l, binputs, boutputs, par: 1);
                accm = reward(l, inputs, outputs, par: 1);
                //accm = reward(l, binputs, boutputs, par: 1);
                record = false;
            }
            else
            {
                l3 = l;
            }
            //modifier(l);
            //shuffler();
            
            var acc3 = accm;// accCalc(l, ti1, to1, par: 1);
            for (int i = 0; i < equation.Count; i++)
            {
                //   equation[i][0] = l[i][0]+0;
            }
            //var acc4 = accCalc(l, binputs, boutputs, par: 1);
            //l = copier();
            //modifier(l);
            //myreturn = -1* (Math.Abs(acc1 - acc2)+ Math.Abs(acc1 - acc3))  ;
            var myacc = acc1;
            expandd = true;
            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 2:
                        if (acc1 >= myacc)
                        {
                            centerpoint = crt;
                            myacc = acc1;
                            expandd = false;
                            bestl = l1;
                            crtb = true;
                        }
                        break;
                    case 1:
                        if (acc2 >= myacc)
                        {
                            myacc = acc2;
                            centerpoint = side;
                            expandd = true;
                            bestl = l2;
                            crtb = false;
                        }
                        break;
                    case 0:
                        if (acc3 >= myacc)
                        {
                            myacc = acc3;
                            centerpoint = avg;
                            expandd = false;
                            bestl = l3;
                            crtb = false;
                        }
                        break;
                }
            }
            bestpoint = myacc;
            return acc1;


        }
        List<List<double[]>> bestl1;
        List<List<double[]>> bestl2;
        List<List<double[]>> bestl;
        public double[] expand(double s1, double s2)
        {
            double ss1 = 0;
            double ss2 = 0;
            if ((s1 > 0) == (s2 > 0))
            {
                if (s1 > s2)
                {
                    ss1 = s1 * 2;
                    ss2 = s2 / 2 - ss1 / 2;
                }
                else
                {
                    ss2 = s2 * 2;
                    ss1 = s1 / 2 - ss2 / 2;
                }
            }
            else
            {
                ss1 = s1 * 2;
                ss2 = s2 * 2;
            }
            double[] r = new double[2];
            r[0] = ss1;
            r[1] = ss2;
            return r;
        }
        //public void modifier2(List<double[]> l)
        //{
        //    double[][] mymatrix = Matrices.MatrixCreate(Terms, Terms);
        //    double[][] mymatrix2 = Matrices.MatrixCreate(Terms, 1);
        //    int yy = shufflelen + 0;
        //    shufflelen = Terms;
        //    //shuffler();
        //    for (int z1 = 0; z1 < Terms; z1++)
        //    {
        //        double ans = 0;
        //        var z2 = 0;
        //        foreach (double[] c in l)
        //        {
        //            double ans2 = 1;
        //            for (int i = 0; i < c.Length; i++)
        //            {
        //                if (i == 0)
        //                {
        //                    ans2 *= c[i];
        //                }
        //                else
        //                {
        //                    ans2 *= Math.Pow(inputs[z1][i - 1], c[i]);
        //                }
        //            }
        //            ans += ans2;
        //            mymatrix[z1][z2] = ans2;
        //            z2++;
        //        }
        //        mymatrix2[z1][0] = outputs[z1];
        //    }
        //    try
        //    {
        //        shufflelen = yy;
        //        //   shuffler();
        //        double[][] newterms = Matrices.MatrixProduct(Matrices.MatrixInverse(mymatrix), mymatrix2);
        //        //  bool gr;
        //        bool exit = true;
        //        for (int i = 0; i < Terms; i++)
        //        {
        //            if (!(l[i][0] * newterms[i][0] < termlimit && l[i][0] * newterms[i][0] > -1 * termlimit))
        //            {
        //                exit = false;
        //                break;
        //                //        gr = false;
        //                //Exception ex = new Exception();
        //                // throw ex; 
        //                //  break;
        //            }
        //        }
        //        for (int i = 0; i < Terms; i++)
        //        {
        //            var f = newterms[i][0].ToString();
        //            //if ((f == "NaN" && Nano) || exit)
        //            //{

        //            //}
        //            if (!(f == "NaN" || f == "Infinity" || f == "-Infinity" || newterms[i][0] == 0) && exit)
        //            {
        //                var q = f == "NaN";
        //                l[i][0] *= newterms[i][0];
        //                if (bracks)
        //                {
        //                    if (Math.Abs(l[i][0]) < brackelimit)
        //                    {
        //                        double chh = (l[i][0] > 0) ? 1 : -1;
        //                        l[i][0] += (Math.Abs(l[i][0]) + brackelimit);
        //                        l[i][0] *= chh;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Nano = false;
        //                break;
        //            }
        //        }

        //    }
        //    catch { Nano = false; }

        //}
        double[][] h;
        double[] g;
        void shuffler3()
        {
            ti1 = new List<double[]>();
            to1 = new List<double>();
            inputs = new List<double[]>();
            outputs = new List<double>();
            Random rf = new Random();

            var fq = rf.Next(0, 2);

            if (fq == 0)
            {
                Array.Reverse(g);
                Array.Reverse(h);
            }
            int r = binputs.Count / (shufflelen + rf.Next(0, 5));

            for (int i = 0; i < h.Length; i += r)
            {
                ti1.Add(h[i]);
                to1.Add(g[i]);

            }

            r = binputs.Count / (equation.Count + rf.Next(0, 5));
            for (int i = 0; i < h.Length; i += r)
            {
                inputs.Add(h[i]);
                outputs.Add(g[i]);

            }
        }
        bool Nano;
        List<double[]> ti;
        List<double> to;
        List<double[]> ti1;
        List<double> to1;
        double bestpoint1;
        double bestpoint2;
        double bestpoint;
        public double minpow;
        public double maxpow;
        List<double[]> tii1;
        List<double> too1;
        public int arnge;
        private bool itrainm;
        private List<List<double>> memoryside1;
        private List<List<double>> memoryside2;
        public void traininitialmemory()
        {
            memory_vals = Inputss_memory[0];
            idofi = 0;
            record = true;
            //int i = 0;
            for(int i=0;i<memory_vals.Count;i++)
            {
                //int i2 = 0;
                for (int i2 = 0; i2 < memory_vals[i].Count; i2++)
                {
                     double side1 = memoryside1[i][i2];
                     double side2 = memoryside2[i][i2];
                     double avg= (side2 + side1) / 2; 
                    double expandd123 = Math.Abs(side1 - side2)/2;
                    double crt1 = (avg + side1) / 2;
                    double crt2 = (avg + side2) / 2;

                    memory_vals[i][i2] = side1;
                    var acc1 = reward(equation, binputs, boutputs, par: 1);
                    memory_vals = Inputss_memory[0];

                    memory_vals[i][i2] = side2;
                    var acc2 = reward(equation, binputs, boutputs, par: 1);
                    memory_vals = Inputss_memory[0];

                    memory_vals[i][i2] = (side2+side1)/2;
                    var accm = reward(equation, binputs, boutputs, par: 1);
                    memory_vals = Inputss_memory[0];
                    memory_vals[i][i2] = crt1;
                    var accm1 = reward(equation, binputs, boutputs, par: 1);
                    memory_vals = Inputss_memory[0];
                    memory_vals[i][i2] = crt2;
                    var accm2 = reward(equation, binputs, boutputs, par: 1);
                    memory_vals = Inputss_memory[0];
                    int best1id = 0;
                    double bestacc = -11111111111111111;
                    for(int sw = 0; sw < 5; sw++)
                    {
                        switch (sw)
                        {
                            case 0:
                                if (acc1 >= bestacc)
                                {
                                    bestacc=acc1;
                                    memory_vals[i][i2] =activationfunc( side1, activationmemory );
                                    memoryside1[i][i2] = side1 - expandd123 * 2;
                                    memoryside2[i][i2] = side1 + expandd123 * 2;
                                    best1id = 0;
                                }
                                break;
                            case 1:
                                if (acc2 >= bestacc)
                                {
                                     bestacc=acc2;
                                    memory_vals[i][i2] = activationfunc(side2, activationmemory);
                                    memoryside1[i][i2] = side2 - expandd123*2;
                                    memoryside2[i][i2] = side2 + expandd123 * 2;
                                    best1id = 1;
                                }
                                break;
                            case 2:
                                if (accm >= bestacc)
                                {
                                     bestacc=accm;
                                     avg = (side2 + side1) / 2;
                                    memory_vals[i][i2] = activationfunc(avg, activationmemory );
                                    memoryside1[i][i2] = avg - expandd123/2;
                                    memoryside2[i][i2] = avg + expandd123/2;
                                    best1id = 2;
                                }
                                break;
                            case 3:
                                if (accm1 >= bestacc)
                                {
                                    bestacc= accm1;
                                    memory_vals[i][i2] = activationfunc(crt1, activationmemory ); 
                                    memoryside1[i][i2] = crt1 - expandd123;
                                    memoryside2[i][i2] = crt1 + expandd123;
                                    best1id = 3;
                                }
                                break;
                            case 4:
                                if (accm2 >= bestacc)
                                {
                                     bestacc= accm2;
                                  //   avg = (side2 + side1) / 2;
                                    memory_vals[i][i2] = activationfunc(crt2, activationmemory);
                                    memoryside1[i][i2] = crt2 - expandd123;
                                    memoryside2[i][i2] = crt2 + expandd123;
                                    best1id = 4;
                                }
                                break;
                        }

                    }
                  
                    i2++;
                }
                i++;
            }
             Inputss_memory[0]= memory_vals ;
            reward(equation, binputs, boutputs, par: 1);
            record = false;
        }
        public void Train(bool trainm=false)
        {

            // {
            // tii1 = inputs;
            //too1 = outputs;
            itrainm = trainm;
            if (trainm && !memory)
            {
                return;
            }
            //if (sadd == 1)
            //{
            //    ti1 = tii1;
            //    to1 = too1;
            //}
            //else
            //{
            //    tii1 = ti1;
            //    too1 = to1;
            //}
            //}
            //    var lq = copier();
            Nano = true;
         //   mainacc = reward(equation, binputs, boutputs, par: 1);
           // mainacc = reward(equation, binputs, boutputs, par: 1);
           // mainacc = reward(equation, binputs, boutputs, par: 1);
            //mainacc = reward(equation, binputs, boutputs, par: 1);


            //var ecc = reward(equation, binputs, boutputs, par: 1);

            int refg = r.Next(0, 3) + sadd;

            if ( refg == 0 || ti1==null)
            {
                ti = inputs;
                to = outputs;
                if (memory)
                {
                    
                    shuffler_m();
                }
                else
                {
                    shuffler();
                }
                
                ti1 = inputs;
                to1 = outputs;
                //  mainacc = ecc;
            }
            idofi = 0;
            mainacc = reward(equation, binputs, boutputs, par: 1);
            traininitialmemory();
            idofi = idofi2;
            //else
            //{
            //    inputs = ti;
            //    outputs = to;
            //    //   modifier(equation);
            //    equation = lq;
            //}
            var eqtest=equation;
            var sidetest = sides;
            if (itrainm)
            {
                eqtest = equation_m;
                sidetest = sides_m;
            }
            else
            {

            }
            mainacc = reward(equation, inputs, outputs, par: 1);
            for (int i = 0; i < eqtest.Count; i++)
            {

                for (int i2 = 0; i2 < eqtest[i].Count; i2++)
                {
                    for (int i3 = 0; i3 < eqtest[i][i2].Length; i3++)
                    {
                        int qe = 5;
                        while (qe != 0)
                        {
                            //           shufflelen = rrq.Next(10, 30);


                            if (neglect && Math.Abs(eqtest[i][i2][i3]) <= neglectionlimit)
                            {
                                //if (i2 != 0)
                                {
                                    double side11 = sides[i][i2][i3, 0];
                                    double side22 = sides[i][i2][i3, 1];
                                    var avg2 = (side11 + side22) / 2;
                                    double expand11 = side11 - avg2;
                                    eqtest[i][i2][i3] = 0.00000001;
                                    sidetest[i][i2][i3, 0] = 0.00000001 + expand11;
                                    sidetest[i][i2][i3, 1] = 0.00000001 - expand11;
                                }
                                break;

                            }
                            Nano = false;
                            double side1 = sidetest[i][i2][i3, 0];
                            double side2 = sidetest[i][i2][i3, 1];
                            var avg = (side1 + side2) / 2;

                            double g = 0;
                            double g2 = 0;
                            g = pointeval(side1, avg, i, i2,i3, mainacc);
                            bestl1 = bestl;
                            bestpoint1 = bestpoint;
                            var c1 = centerpoint;
                            bool ex1 = expandd;
                            bool crtb1 = crtb;
                            g2 = pointeval(side2, avg, i, i2,i3, mainacc);
                            bestl2 = bestl;
                            bestpoint2 = bestpoint;
                            var c2 = centerpoint;
                            double expand1 = side1 - avg;

                            if (Math.Abs(expand1) > 2)
                            {
                                expand1 = 2;
                            }
                            bool qr = false;
                            double r0 = 0;
                            if (g > g2)
                            {
                                if (g < mainacc)
                                {
                                    qr = true;
                                    r0 = g;
                                }
                            }
                            else
                            {
                                if (g2 < mainacc)
                                {
                                    qr = true;
                                    r0 = g2;
                                }
                            }
                            if (qr)
                            {
                                if ((mainacc - r0) > 0.01 * Math.Abs(r0))
                                {
                                    //accCalc(equation, binputs, boutputs, par: 1);
                                    // g = pointeval(side1, avg, i, i2);
                                }
                            }
                            if (g > g2 )
                            {
                                var few = Math.Abs(c1);
                                if (!ex1)
                                {
                                    if (!crtb1)
                                    {
                                        expand1 /= 2;
                                    }
                                }
                                else
                                {
                                    expand1 *= 2;
                                }
                               
                                if (Math.Abs(expand1) < expandlimit)
                                {
                                    expand1 += expandlimit;//Math.Abs(expand1) + 
                                }
                                else if (Math.Abs(expand1) > 1)
                                {
                                    expand1 /= 2;
                                }
                                sidetest[i][i2][i3, 0] = c1 + expand1;
                                sidetest[i][i2][i3, 1] = c1 - expand1;
                                if (!itrainm)
                                {
                                    equation = bestl1;//[i][i2] = c1+0.000000001;
                                    eqtest = equation;
                                }
                                else
                                {
                                    equation_m = bestl1;//[i][i2] = c1+0.000000001;
                                    eqtest = equation_m;
                                }
                                                  // mainacc = accCalc(equation, binputs, boutputs, par: 1);
                                mainacc = bestpoint1;
                            }
                            else 
                            {
                                var few = Math.Abs(c2);
                                if (!expandd)
                                {
                                    if (!crtb)
                                    {
                                        expand1 /= 2;
                                    }
                                }
                                else
                                {
                                    expand1 *= 2;
                                }
                               
                                if (Math.Abs(expand1) < expandlimit)
                                {
                                    expand1 += expandlimit;
                                }
                                else if (Math.Abs(expand1) > 1)
                                {
                                    expand1 /= 2;
                                }
                                sidetest[i][i2][i3, 0] = c2 + expand1;
                                sidetest[i][i2][i3, 1] = c2 - expand1;
                                if (!itrainm)
                                {
                                    equation = bestl2;//[i][i2] = c1+0.000000001;
                                    eqtest = equation;
                                }
                                else
                                {
                                    equation_m = bestl2;//[i][i2] = c1+0.000000001;
                                    eqtest = equation_m;
                                }
                                //mainacc = accCalc(equation, binputs, boutputs, par: 1);
                                mainacc = bestpoint2;
                            }
                            // modifier(equation);
                            break;


                        }
                    }
                }
            }
        }
    }
    public class map
    {
        List<regressor> serialnets;
        data dt;
        public double singlereward(List<List<double[]>> test, int indx, int id,int indxofinputs, double[]input)
        {
            double reward = 0;
            switch (id)
            {
                case 0://predict reward ##
                    reward = 0;// regressor.calc(test, input)[0];
                    reward = 1 - Math.Abs(reward - dt.rewards[indxofinputs]) / dt.rewards[indxofinputs];
                    break;
                case 1://predict reaction
                    double[] pred = { 0.0,0.0};// regressor.calc(test, input);
                    reward = singlereward(serialnets[0].equation, indx, 0, indxofinputs, pred);
                    break;
            }
            return reward;
        }
    }
    public class data
    {
        public List<double[]> inputs;
        public List<double> rewards;
        public int lengthofdecision;
        public data()
        {
            lengthofdecision = 2;
        }
        public double[] shapereply(double[]input1,int id)
        {
            double[] input2=new double[1];
            switch(id){
                case 2://decidor
                    input2 = new double[input1.Length + lengthofdecision];
                    break;
            }
            return input2;
        }
    }
}

