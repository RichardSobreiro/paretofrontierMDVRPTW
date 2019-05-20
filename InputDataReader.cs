// --------------------------------------------------------------------------
// File: InputDataReader.cs
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
// This is a helper class used by several examples to read input data files
// containing arrays in the format [x1, x2, ..., x3].  Up to two-dimensional
// arrays are supported.
//

using System;
using System.Globalization;

public class InputDataReader {
   public class InputDataReaderException : System.Exception {
      internal InputDataReaderException(string file) : base("'" + file + "' contains bad data format") {
         
      }
   }
   
   internal string[] _tokens;
   internal int      _current;
   internal string   _fileName;

   internal NumberFormatInfo _nfi = NumberFormatInfo.InvariantInfo;

   internal string NextToken() {
      string token = _tokens[_current++];
      while ( token == "" )
         token = _tokens[_current++];

      return token;
   }

   public InputDataReader(string fileName) {
      _fileName = fileName;
      System.IO.StreamReader reader = new System.IO.StreamReader(fileName);

      string text = reader.ReadToEnd();

      text = text.Replace("[", " [ ");
      text = text.Replace("]", " ] ");
      text = text.Replace(",", " , ");
      text = text.Replace('\"', ' ');

      _tokens = text.Split(null);

      reader.Close();
    
      _current = 0;
   }

   internal double ReadDouble() {
      return Double.Parse(NextToken(), _nfi);
   }
     
   internal int ReadInt() {
      return Int32.Parse(NextToken(), _nfi);
   }

   internal string ReadString() {
      return NextToken();
   }
   
   internal double[] ReadDoubleArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      while (token != "]") {
         values.Add(Double.Parse(token, _nfi));
         token = NextToken();
         
         if ( token == "," ) {
            token = NextToken();
         }
         else if ( token != "]" ) {
            throw new InputDataReaderException(_fileName);
         }
      }
      
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      double[] res = new double[values.Count];
      for (int i = 0; i < values.Count; i++) {
         res[i] = (double)values[i];
      }
      
      return res;
   }

   internal double[][] ReadDoubleArrayArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      
      while (token == "[") {
         _current--;
         
         values.Add(ReadDoubleArray());
         
         token = NextToken();
         if      ( token == "," ) {
           token = NextToken();
         }
         else if ( token != "]" ) {
           throw new InputDataReaderException(_fileName);
         }
      }
    
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      double[][] res = new double[values.Count][];
      for (int i = 0; i < values.Count; i++) 
         res[i] = (double[])values[i];
      
      return res;
   }

   internal int[] ReadIntArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      while (token != "]") {
         values.Add(Int32.Parse(token, _nfi));
         token = NextToken();
         
         if      ( token == "," ) {
            token = NextToken();
         }
         else if ( token != "]" ) {
            throw new InputDataReaderException(_fileName);
         }
      }
      
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      int[] res = new int[values.Count];
      for (int i = 0; i < values.Count; i++)
         res[i] = (int)values[i];
      
      return res;
   }

   internal int[][] ReadIntArrayArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      
      while (token == "[") {
         _current--;
         
         values.Add(ReadIntArray());
         
         token = NextToken();
         if      ( token == "," ) {
            token = NextToken();
         }
         else if ( token != "]" ) {
            throw new InputDataReaderException(_fileName);
         }
      }
    
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      int[][] res = new int[values.Count][];
      for (int i = 0; i < values.Count; i++)
         res[i] = (int[])values[i];
      
      return res;
   }

   internal string[] ReadStringArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      while (token != "]") {
         values.Add(token);
         token = NextToken();
         
         if      ( token == "," ) {
            token = NextToken();
         }
         else if ( token != "]" ) {
            throw new InputDataReaderException(_fileName);
         }
      }
      
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      string[] res = new string[values.Count];
      for (int i = 0; i < values.Count; i++) 
         res[i] = (string)values[i];
      
      return res;
   }

   internal string[][] ReadStringArrayArray() {
      string token = NextToken(); // Read the '['
      
      if ( token != "[" )
         throw new InputDataReaderException(_fileName);
      
      System.Collections.ArrayList values = new System.Collections.ArrayList();
      token = NextToken();
      
      while (token == "[") {
         _current--;
         
         values.Add(ReadStringArray());
         
         token = NextToken();
         if      ( token == "," ) {
            token = NextToken();
         }
         else if ( token != "]" ) {
            throw new InputDataReaderException(_fileName);
         }
      }
    
      if ( token != "]" )
         throw new InputDataReaderException(_fileName);
    
      // Fill the array.
      string[][] res = new string[values.Count][];
      for (int i = 0; i < values.Count; i++)
         res[i] = (string[])values[i];
      
      return res;
   }
}
