using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OptimizacionMatematica.Models;

namespace OptimizacionMatematica.Controllers
{
    public class HookController : Controller
    {
        public IActionResult Hookes()
        {
            int nNumVars = 2;
            double[] fX = new double[] { 0, 0 };
            double[] fParam = new double[] { 0, 0 };
            double[] fStepSize = new double[] { 0.1, 0.1 };
            double[] fMinStepSize = new double[] { 0.0000001, 0.0000001 };
            int nIter = 0;
            double fEpsFx = 0.0000001;
            int i;
            object fBestF;
            string sAnswer;
            Hookes oOpt;
            MyFxDelegate MyFx = new MyFxDelegate(Fx3);
            SayFxDelegate SayFx = new SayFxDelegate(SayFx3);

            oOpt = new Hookes();

            Console.WriteLine("Hooke-Jeeves Search Optimization");
            Console.WriteLine("Finding the minimum of function:");
            Console.WriteLine(SayFx());
            Console.Write("Use default input values? (Y/N) ");
            sAnswer = Console.ReadLine();
            if (sAnswer.ToUpper() == "Y")
            {
                for (i = 0; i < nNumVars; i++)
                {
                    Console.WriteLine("X({0}) = {1}", i + 1, fX[i]);
                    Console.WriteLine("Step size({0}) = {1}", i + 1, fStepSize[i]);
                    Console.WriteLine("Min step Size ({0}) = {1}", i + 1, fMinStepSize[i]);
                }
                Console.WriteLine("Function tolerance = {0}", fEpsFx);
            }
            else
            {
                for (i = 0; i < nNumVars; i++)
                {
                    fX[i] = GetIndexedDblInput("X", i + 1, fX[i]);
                    fStepSize[i] = GetIndexedDblInput("Step size", i + 1, fStepSize[i]);
                    fMinStepSize[i] = GetIndexedDblInput("Min step size", i + 1, fMinStepSize[i]);
                }
                fEpsFx = GetDblInput("Function tolerance", fEpsFx);
            }

            Console.WriteLine("******** FINAL RESULTS *************");
            fBestF = oOpt.CalcOptim(nNumVars, ref fX, ref fParam, ref fStepSize, ref fMinStepSize, fEpsFx, ref nIter, MyFx);
            Console.WriteLine("Optimum at");
            for (i = 0; i < nNumVars; i++)
            {
                Console.WriteLine("X({0}) = {1}", i + 1, fX[i]);
            }
            Console.WriteLine("Function value = {0}", fBestF);
            Console.WriteLine("Number of iterations = {0}", nIter);
            Console.WriteLine();
            Console.Write("Press Enter to end the program ...");
            Console.ReadLine();

            return View();
        }

        #region Metodo
        static public double GetDblInput(string sPrompt, double fDefInput)
        {
            string sInput;

            Console.Write("{0}? ({1}): ", sPrompt, fDefInput);
            sInput = Console.ReadLine();
            if (sInput.Trim(null).Length > 0)
            {
                return double.Parse(sInput);
            }
            else
            {
                return fDefInput;
            }
        }

        static public int GetIntInput(string sPrompt, int nDefInput)
        {
            string sInput;

            Console.Write("{0}? ({1}): ", sPrompt, nDefInput);
            sInput = Console.ReadLine();
            if (sInput.Trim(null).Length > 0)
            {
                return (int)double.Parse(sInput);
            }
            else
            {
                return nDefInput;
            }
        }

        static public double GetIndexedDblInput(string sPrompt, int nIndex, double fDefInput)
        {
            string sInput;

            Console.Write("{0}({1})? ({2}): ", sPrompt, nIndex, fDefInput);
            sInput = Console.ReadLine();
            if (sInput.Trim(null).Length > 0)
            {
                return double.Parse(sInput);
            }
            else
            {
                return fDefInput;
            }
        }

        static public int GetIndexedIntInput(string sPrompt, int nIndex, int nDefInput)
        {
            string sInput;

            Console.Write("{0}({1})? ({2}): ", sPrompt, nIndex, nDefInput);
            sInput = Console.ReadLine();
            if (sInput.Trim(null).Length > 0)
            {
                return (int)double.Parse(sInput);
            }
            else
            {
                return nDefInput;
            }
        }

        static public string SayFx1()
        {
            return "F(X) = 10 + (X(1) - 2) ^ 2 + (X(2) + 5) ^ 2";
        }

        static public double Fx1(int N, ref double[] X, ref double[] fParam)
        {
            return 10 + Math.Pow(X[0] - 2, 2) + Math.Pow(X[1] + 5, 2);
        }

        static public string SayFx2()
        {
            return "F(X) = 100 * (X(1) - X(2) ^ 2) ^ 2 + (X(2) - 1) ^ 2";
        }

        static public double Fx2(int N, ref double[] X, ref double[] fParam)
        {
            return Math.Pow(100 * (X[0] - X[1] * X[1]), 2) + Math.Pow((X[1] - 1), 2);
        }

        static public string SayFx3()
        {
            return "F(X) = X(1) - X(2) + 2 * X(1) ^ 2 + 2 * X(1) * X(2) + X(2) ^ 2";
        }

        static public double Fx3(int N, ref double[] X, ref double[] fParam)
        {
            return X[0] - X[1] + 2 * X[0] * X[0] + 2 * X[0] * X[1] + X[1] * X[1];
        }
        #endregion

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
