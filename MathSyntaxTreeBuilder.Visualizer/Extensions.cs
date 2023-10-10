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

        public static T SetMargin<T>(this T element, double left, double top, double right, double bottom)
            where T : FrameworkElement
        {
            element.Margin =  new Thickness(left, top, right, bottom);
            return element;
        }

        public static T SetFontSize<T>(this T element, double size)
            where T : FrameworkElement
        {
            TextBlock.SetFontSize(element, size);
            return element;
        }

        public static T SetRow<T>(this T element, int columnIndex)
            where T : UIElement
        {
            Grid.SetRow(element, columnIndex);
            return element;
        }

        public static void AddRange<T>(this T element, IEnumerable<UIElement> elementsToAdd)
            where T : UIElementCollection
        {
            foreach (var elementToAdd in elementsToAdd)
            {
                element.Add(elementToAdd);
            }
        }
    }
}
