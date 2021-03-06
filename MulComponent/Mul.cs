﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Component;

namespace MulComponent
{
    public class Mul : IComponent
    {
        public Mul()
        {
            this.ComponentGuid = new Guid();
            this.FriendlyName = "3_Mul";
            this.InputHints = new string[] { "int", "int" };
            this.OutputHints = new string[] { "int" };
        }

        public Guid ComponentGuid
        {
            get;
            private set;
        }

        public IEnumerable<object> Evaluate(IEnumerable<object> values)
        {
            int a;
            int b;
            List<object> intList = new List<object>();
            intList = values.ToList();
            a = (int)intList[0];
            b = (int)intList[1];
            List<object> resultList = new List<object>();
            resultList[0] = a * b;
            return resultList;
        }

        public string FriendlyName
        {
            get;
            private set;
        }

        public IEnumerable<string> InputHints
        {
            get;
            private set;
        }

        public IEnumerable<string> OutputHints
        {
            get;
            private set;
        }
    }
}