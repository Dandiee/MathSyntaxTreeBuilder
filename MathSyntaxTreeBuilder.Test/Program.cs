using System.Diagnostics;

namespace MathSyntaxTreeBuilder.Test;

public class Program
{
    // Brackets, Exponents, Division/Multiplication, Addition/Subtraction
    public static void Main(string[] args)
    {

        Test("((5+4)*cos(1)-1)", "", 3.86272075281);
        Test("sin((1+cos(3))*4)", "(sin(((1+(cos(3)))*4)))", 0.04001932375);
        Test("sin(cos(1))+1", "((sin((cos(1))))+1)", 1.51439525852);
        Test("sin(1+1)+cos(1)", "((sin((1+1)))+(cos(1)))", 1.44959973269);
        Test("sin(2*3+1)", "(sin(((2*3)+1)))", 0.65698659871);
        Test("max(min(max(min(1, 2), 3), 4), 5)", "(max((min((max((min(1,2)),3)),4)),5))", 5);

        Test("sin(cos(1)*2)", "(sin(((cos(1))*2)))", 0.88224261632);
        
        
        Test("sin(cos(1))", "(sin((cos(1))))", 0.51439525852);
        
        Test("max(min(0-1, 0-5), min(0-2, 0-7))", "(max((min((0-1),(0-5))),(min((0-2),(0-7)))))", -5);
        Test("sin(1)+cos(2)+tan(3)", "((sin(1))+((cos(2))+(tan(3))))", 0.28277760518);
        Test("max(7, 2)", "(max(7,2))", 7);
        Test("min(7, 2)", "(min(7,2))", 2);
        Test("max(cos(1), sin(1))", "(max((cos(1)),(sin(1))))", 0.8414709848);
        Test("min(cos(1), sin(1))", "(min((cos(1)),(sin(1))))", 0.54030230586);
        Test("sin(cos(1))", "(sin((cos(1))))", 0.51439525852);
        Test("x", "(x)");
        //Test("-5(1 + 2)", "(-(5*(1+2)))", -15);
        //Test("5(1 + 2)", "(5*(1+2))", 15);
        Test("-(5 * 2)", "(-(5 * 2))", -10);
        Test("-sin(1)", "(-(sin(1)))", -0.8414709848);
        Test("sin(-(1))", "(sin((-1)))", -0.8414709848);
        //Test("--1", "(-(-1))", 1);
        //Test("--(1)", "(-(-1))", 1);
        Test("-(1)", "(-1)", -1);
        Test("-1", "(-1)", -1);
        Test("-(5 * 4)", "(-(5*4))", -20);
        Test("4 * x + 6", "((4*x)+6)");
        Test("45 + sin(11 + 7)", "(45+(sin((11+7))))", 44.2490127532);
        Test("sin(1 + 7)", "(sin((1+7)))", 0.98935824662);
        Test("sin(1)", "(sin(1))", 0.8414709848);
        Test("4 ^ (5 + 6)", "(4^(5+6))", 4194304);
        Test("4 ^ 5 + 6", "((4^5)+6)", 1030);
        Test("4 + (5 * 6)", "(4+(5*6))", 34);
        Test("(4 + 5) * 6", "((4+5)*6)", 54);
        Test("4 + 5 * 6", "(4+(5*6))", 34);
        Test("4 * 5 + 6", "((4*5)+6)", 26);
        Test("4 + 5 - 6", "((4+5)-6)", 3);
        Test("4 + (5 + 6)", "(4+(5+6))", 15);
        Test("(4 + 5) + 6", "((4+5)+6)", 15);
        Test("4 + 5", "(4+5)", 9);
        Test("5", "(5)", 5);

        Console.WriteLine("A-Okay");
    }

    public static void Test(string input, string expectedOutput = null, double? expectedEval = null)
    {
        var result = MathSyntaxBuilder.GetSyntaxTree(input);
        var output = result.BuildString();

        if (expectedOutput != null)
        {
            expectedOutput = expectedOutput.Replace(" ", "");
            Debug.Assert(output == expectedOutput);
        }

        if (expectedEval.HasValue)
        {
            var eval = result.Eval();
            Debug.Assert(Math.Abs(eval - expectedEval.Value) < 0.0001d);
        }
        

    }
}
