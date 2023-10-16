using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathSyntaxTreeBuilder
{
    public static class Extensions
    {
        public static NodeVariable AsVariable(this Node node)
        {
            return node as NodeVariable;
        }

        public static NodeUserConstant AsUserConst(this Node node)
        {
            return node as NodeUserConstant;
        }
    }
}
