// --------------------------------------------------------------------------
// File: Diet.cs   
// Version 12.8.0  
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
// 5725-A06 5725-A29 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5655-Y21
// Copyright IBM Corporation 2003, 2017. All Rights Reserved.
//
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with
// IBM Corp.
// --------------------------------------------------------------------------
//
// A dietary model.
//
// Input data:
// foodMin[j]          minimum amount of food j to use
// foodMax[j]          maximum amount of food j to use 
// foodCost[j]         cost for one unit of food j
// nutrMin[i]          minimum amount of nutrient i
// nutrMax[i]          maximum amount of nutrient i
// nutrPerFood[i][j]   nutrition amount of nutrient i in food j
//
// Modeling variables:
// Buy[j]          amount of food j to purchase
//
// Objective:
// minimize sum(j) Buy[j] * foodCost[j]
//
// Constraints:
// forall foods i: nutrMin[i] <= sum(j) Buy[j] * nutrPer[i][j] <= nutrMax[j]
//

using ILOG.Concert;
using ILOG.CPLEX;


public class Diet
{
    internal class Data
    {
        internal int nFoods;
        internal int nNutrs;
        internal double[] foodCost;
        internal double[] foodMin;
        internal double[] foodMax;
        internal double[] nutrMin;
        internal double[] nutrMax;
        internal double[][] nutrPerFood;

        internal Data(string filename)
        {
            InputDataReader reader = new InputDataReader(filename);

            foodCost = reader.ReadDoubleArray();
            foodMin = reader.ReadDoubleArray();
            foodMax = reader.ReadDoubleArray();
            nutrMin = reader.ReadDoubleArray();
            nutrMax = reader.ReadDoubleArray();
            nutrPerFood = reader.ReadDoubleArrayArray();

            nFoods = foodMax.Length;
            nNutrs = nutrMax.Length;

            if (nFoods != foodMin.Length ||
                 nFoods != foodMax.Length)
                throw new ILOG.Concert.Exception("inconsistent data in file " + filename);
            if (nNutrs != nutrMin.Length ||
                 nNutrs != nutrPerFood.Length)
                throw new ILOG.Concert.Exception("inconsistent data in file " + filename);
            for (int i = 0; i < nNutrs; ++i)
            {
                if (nutrPerFood[i].Length != nFoods)
                    throw new ILOG.Concert.Exception("inconsistent data in file " + filename);
            }
        }
    }

    internal static void BuildModelByRow(IModeler model,
                                         Data data,
                                         INumVar[] Buy,
                                         NumVarType type)
    {
        int nFoods = data.nFoods;
        int nNutrs = data.nNutrs;

        for (int j = 0; j < nFoods; j++)
        {
            Buy[j] = model.NumVar(data.foodMin[j], data.foodMax[j], type);
        }
        model.AddMinimize(model.ScalProd(data.foodCost, Buy));

        for (int i = 0; i < nNutrs; i++)
        {
            model.AddRange(data.nutrMin[i],
                           model.ScalProd(data.nutrPerFood[i], Buy),
                           data.nutrMax[i]);
        }
    }

    internal static void BuildModelByColumn(IMPModeler model,
                                            Data data,
                                            INumVar[] Buy,
                                            NumVarType type)
    {
        int nFoods = data.nFoods;
        int nNutrs = data.nNutrs;

        IObjective cost = model.AddMinimize();
        IRange[] constraint = new IRange[nNutrs];

        for (int i = 0; i < nNutrs; i++)
        {
            constraint[i] = model.AddRange(data.nutrMin[i], data.nutrMax[i]);
        }

        for (int j = 0; j < nFoods; j++)
        {
            Column col = model.Column(cost, data.foodCost[j]);
            for (int i = 0; i < nNutrs; i++)
            {
                col = col.And(model.Column(constraint[i], data.nutrPerFood[i][j]));
            }
            Buy[j] = model.NumVar(col, data.foodMin[j], data.foodMax[j], type);
        }
    }


    public static void Main(string[] args)
    {

        try
        {
            string filename = "C:/Users/Richard Sobreiro/source/repos/ParetoFrontier_MDVRPTW/ParetoFrontier_MDVRPTW/diet.dat";
            bool byColumn = false;
            NumVarType varType = NumVarType.Float;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToCharArray()[0] == '-')
                {
                    switch (args[i].ToCharArray()[1])
                    {
                        case 'c':
                            byColumn = true;
                            break;
                        case 'i':
                            varType = NumVarType.Int;
                            break;
                        default:
                            Usage();
                            return;
                    }
                }
                else
                {
                    filename = args[i];
                    break;
                }
            }

            Data data = new Data(filename);
            int nFoods = data.nFoods;
            int nNutrs = data.nNutrs;

            // Build model
            Cplex cplex = new Cplex();
            INumVar[] Buy = new INumVar[nFoods];

            if (byColumn) BuildModelByColumn(cplex, data, Buy, varType);
            else BuildModelByRow(cplex, data, Buy, varType);

            // Solve model

            if (cplex.Solve())
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Solution status = " + cplex.GetStatus());
                System.Console.WriteLine();
                System.Console.WriteLine(" cost = " + cplex.ObjValue);
                for (int i = 0; i < nFoods; i++)
                {
                    System.Console.WriteLine(" Buy" + i + " = " + cplex.GetValue(Buy[i]));
                }
                System.Console.WriteLine();
            }
            cplex.End();
        }
        catch (ILOG.Concert.Exception ex)
        {
            System.Console.WriteLine("Concert Error: " + ex);
        }
        catch (InputDataReader.InputDataReaderException ex)
        {
            System.Console.WriteLine("Data Error: " + ex);
        }
        catch (System.IO.IOException ex)
        {
            System.Console.WriteLine("IO Error: " + ex);
        }
    }

    internal static void Usage()
    {
        System.Console.WriteLine(" ");
        System.Console.WriteLine("usage: Diet [options] <data file>");
        System.Console.WriteLine("options: -c  build model by column");
        System.Console.WriteLine("         -i  use integer variables");
        System.Console.WriteLine(" ");
    }
}

/*  Sample output

Solution status = Optimal

cost   = 14.8557
  Buy0 = 4.38525
  Buy1 = 0
  Buy2 = 0
  Buy3 = 0
  Buy4 = 0
  Buy5 = 6.14754
  Buy6 = 0
  Buy7 = 3.42213
  Buy8 = 0
*/
