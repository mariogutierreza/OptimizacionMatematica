using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptimizacionMatematica.Models
{
    //public delegate double MyFxDelegate(int nNumVars, ref double[] fX, ref double[] fParam);
    //public delegate string SayFxDelegate();

    public class Hookes
    {

        MyFxDelegate m_MyFx;

        protected double MyFxEx(int nNumVars, ref double[] fX, ref double[] fParam, ref double[] fDeltaX, double fLambda)
        {
            int i;
            double[] fXX = new double[nNumVars];

            for (i = 0; i < nNumVars; i++)
            {
                fXX[i] = fX[i] + fLambda * fDeltaX[i];
            }

            return m_MyFx(nNumVars, ref fXX, ref fParam);
        }

        protected bool LinSearch_DirectSearch(int nNumVars, ref double[] fX, ref double[] fParam, ref double fLambda, ref double[] fDeltaX, double fInitStep, double fMinStep)
        {
            double F1, F2;

            F1 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda);

            do
            {
                F2 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda + fInitStep);
                if (F2 < F1)
                {
                    F1 = F2;
                    fLambda += fInitStep;
                }
                else
                {
                    F2 = MyFxEx(nNumVars, ref fX, ref fParam, ref fDeltaX, fLambda - fInitStep);
                    if (F2 < F1)
                    {
                        F1 = F2;
                        fLambda -= fInitStep;
                    }
                    else
                    {
                        // reduce search step size
                        fInitStep /= 10;
                    }
                }
            } while (!(fInitStep < fMinStep));

            return true;

        }

        public double CalcOptim(int nNumVars, ref double[] fX, ref double[] fParam, ref double[] fStepSize, ref double[] fMinStepSize, double fEpsFx, ref int nIter, MyFxDelegate MyFx)
        {
            int i;
            double[] fXnew = new double[nNumVars];
            double[] fDeltaX = new double[nNumVars];
            double F, fXX, fLambda, fBestF, fLastBestF;
            bool bStop, bMadeAnyMove;
            bool[] bMoved = new bool[nNumVars];

            m_MyFx = MyFx;

            for (i = 0; i < nNumVars; i++)
            {
                fXnew[i] = fX[i];
            }
            // calculate function value at initial point
            fBestF = MyFx(nNumVars, ref fXnew, ref fParam);
            fLastBestF = 100 * fBestF + 100;

            nIter = 1;
            do
            {

                nIter++;

                for (i = 0; i < nNumVars; i++)
                {
                    fX[i] = fXnew[i];
                }

                for (i = 0; i < nNumVars; i++)
                {
                    bMoved[i] = false;
                    do
                    {
                        fXX = fXnew[i];
                        fXnew[i] = fXX + fStepSize[i];
                        F = MyFx(nNumVars, ref fXnew, ref fParam);
                        if (F < fBestF)
                        {
                            fBestF = F;
                            bMoved[i] = true;
                        }
                        else
                        {
                            fXnew[i] = fXX - fStepSize[i];
                            F = MyFx(nNumVars, ref fXnew, ref fParam);
                            if (F < fBestF)
                            {
                                fBestF = F;
                                bMoved[i] = true;
                            }
                            else
                            {
                                fXnew[i] = fXX;
                                break;
                            }
                        }
                    } while (true);
                }

                // moved in any direction?
                bMadeAnyMove = true;
                for (i = 0; i < nNumVars; i++)
                {
                    if (!bMoved[i])
                    {
                        bMadeAnyMove = false;
                        break;
                    }
                }

                if (bMadeAnyMove)
                {
                    for (i = 0; i < nNumVars; i++)
                    {
                        fDeltaX[i] = fXnew[i] - fX[i];
                    }

                    fLambda = 0;
                    if (LinSearch_DirectSearch(nNumVars, ref fX, ref fParam, ref fLambda, ref fDeltaX, 0.1, 0.0001))
                    {
                        for (i = 0; i < nNumVars; i++)
                        {
                            fXnew[i] = fX[i] + fLambda * fDeltaX[i];
                        }
                    }
                }

                fBestF = MyFx(nNumVars, ref fXnew, ref fParam);

                // reduce the step size for the dimensions that had no moves
                for (i = 0; i < nNumVars; i++)
                {
                    if (!bMoved[i])
                    {
                        fStepSize[i] /= 2;
                    }
                }

                // test function value convergence
                if (Math.Abs(fBestF - fLastBestF) < fEpsFx)
                {
                    break;
                }

                fLastBestF = fBestF;

                bStop = true;
                for (i = 0; i < nNumVars; i++)
                {
                    if (fStepSize[i] >= fMinStepSize[i])
                    {
                        bStop = false;
                        break;
                    }
                }

            } while (!bStop);

            for (i = 0; i < nNumVars; i++)
            {
                fX[i] = fXnew[i];
            }

            return fBestF;

        }
    }

}
