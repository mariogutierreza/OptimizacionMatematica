using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptimizacionMatematica.Models
{ 
    public class SolvedModel
    {
        public List<double> X { get; set; }
        public double Tolerace { get; set; }
        public double MaxCycles { get; set; }
        public string ErrorMsg { get; set; }
        public double Function { get; set; }
        public int Iterations { get; internal set; }

        public SolvedModel()
        {
            X = new List<double>();
        }
    }

    public delegate double MyFxDelegate(int nNumVars, ref double[] fX, ref double[] fParam);
    public delegate string SayFxDelegate();

    public class Fletcher
    {

        MyFxDelegate m_MyFx;
        public double MyFxEx(int nNumVars, ref double[] fX, ref double[] fParam, ref double[] fDeltaX, double fLambda)
        {
            int i;
            double[] fXX = new double[nNumVars];

            for (i = 0; i < nNumVars; i++)
            {
                fXX[i] = fX[i] + fLambda * fDeltaX[i];
            }

            return m_MyFx(nNumVars, ref fXX, ref fParam);
        }

        private void GetGradients(int nNumVars, ref double[] fX, ref double[] fParam, ref double[] fDeriv, ref double fDerivNorm)
        {

            int i;
            double fXX, H, Fp, Fm;

            fDerivNorm = 0;
            for (i = 0; i < nNumVars; i++)
            {
                fXX = fX[i];
                H = 0.01 * (1 + Math.Abs(fXX));
                fX[i] = fXX + H;
                Fp = m_MyFx(nNumVars, ref fX, ref fParam);
                fX[i] = fXX - H;
                Fm = m_MyFx(nNumVars, ref fX, ref fParam);
                fX[i] = fXX;
                fDeriv[i] = (Fp - Fm) / 2 / H;
                fDerivNorm += Math.Pow(fDeriv[i], 2);
            }
            fDerivNorm = Math.Sqrt(fDerivNorm);
        }

        public bool LinSearch_DirectSearch(int nNumVars, ref double[] fX, ref double[] fParam, ref double fLambda, ref double[] fDeltaX, double InitStep, double MinStep)
        {
            double F1, F2;

            F1 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda);

            do
            {
                F2 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda + InitStep);
                if (F2 < F1)
                {
                    F1 = F2;
                    fLambda += InitStep;
                }
                else
                {
                    F2 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda - InitStep);
                    if (F2 < F1)
                    {
                        F1 = F2;
                        fLambda -= InitStep;
                    }
                    else
                    {
                        // reduce search step size
                        InitStep /= 10;
                    }
                }
            } while (!(InitStep < MinStep));

            return true;

        }


        public double CalcOptim(int nNumVars, ref double[] fX, ref double[] fParam, double fEpsFx, int nMaxIter, ref int nIter, ref string sErrorMsg, MyFxDelegate MyFx)
        {

            int i;
            double[] fDeriv = new double[nNumVars];
            double[] fDerivOld = new double[nNumVars];
            double F, fDFNormOld, fLambda, fLastF, fDFNorm = 0;

            m_MyFx = MyFx;

            // calculate and function value at initial point
            fLastF = MyFx(nNumVars, ref fX, ref fParam);

            GetGradients(nNumVars, ref fX, ref fParam, ref fDeriv, ref fDFNorm);

            fLambda = 0.1;
            if (LinSearch_DirectSearch(nNumVars, ref fX, ref fParam, ref fLambda, ref fDeriv, 0.1, 0.000001))
            {
                for (i = 0; i < nNumVars; i++)
                {
                    fX[i] += fLambda * fDeriv[i];
                }
            }
            else
            {
                sErrorMsg = "Failed linear search";
                return fLastF;
            }

            nIter = 1;
            do
            {
                nIter++;
                if (nIter > nMaxIter)
                {
                    sErrorMsg = "Reached maximum iterations limit";
                    break;
                }
                fDFNormOld = fDFNorm;
                for (i = 0; i < nNumVars; i++)
                {
                    fDerivOld[i] = fDeriv[i]; // save old gradient
                }
                GetGradients(nNumVars, ref fX, ref fParam, ref fDeriv, ref fDFNorm);
                for (i = 0; i < nNumVars; i++)
                {
                    fDeriv[i] = Math.Pow((fDFNorm / fDFNormOld), 2) * fDerivOld[i] - fDeriv[i];
                }
                if (fDFNorm <= fEpsFx)
                {
                    sErrorMsg = "Gradient norm meets convergence criteria";
                    break;
                }
                //    For i = 0 To nNumVars - 1
                //      fDeriv(i) = -fDeriv(i) / fDFNorm
                //    Next i
                fLambda = 0;
                //    If LinSearch_Newton(fX, nNumVars, fLambda, fDeriv, 0.0001, 100) Then
                if (LinSearch_DirectSearch(nNumVars, ref fX, ref fParam, ref fLambda, ref fDeriv, 0.1, 0.000001))
                {
                    for (i = 0; i < nNumVars; i++)
                    {
                        fX[i] += fLambda * fDeriv[i];
                    }
                    F = MyFx(nNumVars, ref fX, ref fParam);
                    if (Math.Abs(F - fLastF) < fEpsFx)
                    {
                        sErrorMsg = "Successive function values meet convergence criteria";
                        break;
                    }
                    else
                    {
                        fLastF = F;
                    }

                }
                else
                {
                    sErrorMsg = "Failed linear search";
                    break;
                }
            } while (true);

            return fLastF;

        }


    }
}
