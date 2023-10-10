using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MathSyntaxTreeBuilder.Visualizer
{
    public static class Extensions
    {
        public static T SetColumn<T>(this T element, int columnIndex)
            where T : UIElement
        {
            Grid.SetColumn(element, columnIndex);
            return element;
        }

        public static T SetRow<T>(this T element, int columnIndex)
            where T : UIElement
        {
            Grid.SetRow(element, columnIndex);
            return element;
        }
    }
}
