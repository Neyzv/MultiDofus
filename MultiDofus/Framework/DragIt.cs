using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiDofus.Framework
{
    internal static class DragIt
    {
        internal static event Action<UIElement>? Dragged;

        internal static void DragMove<TUIElement>(this TUIElement element, bool disableXAxis = false, bool disableYAxis = false,
            Func<double>? minXFunc = default, Func<double>? maxXFunc = default, Func<double>? minYFunc = default, Func<double>? maxYFunc = default)
            where TUIElement : UIElement
        {
            var parent = default(UIElement);
            var mousePositionInitial = default(Point?);

            var minX = default(double?);
            var maxX = default(double?);

            var minY = default(double?);
            var maxY = default(double?);

            var mouseMoveAction = new MouseEventHandler((_, e) =>
            {
                if (e.LeftButton is MouseButtonState.Pressed && mousePositionInitial is not null)
                {
                    var mouseLocation = e.GetPosition(parent);
                    var elementLocation = element.TranslatePoint(new(), parent);
                    var transPoint = new TranslateTransform(elementLocation.X, elementLocation.Y);

                    if (!disableXAxis)
                    {
                        var newXPosition = mouseLocation.X - mousePositionInitial.Value.X;

                        transPoint.X = minX is null || minX < newXPosition ? (maxX is null || maxX < minX || maxX > newXPosition ? newXPosition : maxX.Value) : minX.Value;
                    }

                    if (!disableYAxis)
                    {
                        var newYPosition = mouseLocation.Y - mousePositionInitial.Value.Y;

                        transPoint.Y = minY is null || minY < newYPosition ? (maxY is null || maxY < minY || maxY > newYPosition ? newYPosition : maxY.Value) : minY.Value;
                    }

                    element.RenderTransform = transPoint;
                }
            });

            var mouseClickAction = new MouseButtonEventHandler((_, e) =>
            {
                minX = minXFunc is null ? default(double?) : minXFunc();
                maxX = maxXFunc is null ? default(double?) : maxXFunc();

                minY = minYFunc is null ? default(double?) : minYFunc();
                maxY = maxYFunc is null ? default(double?) : maxYFunc();

                parent = (UIElement)LogicalTreeHelper.GetParent(element);
                mousePositionInitial = e.GetPosition(parent);
            });

            var removeMouseAction = new MouseButtonEventHandler((_, __) =>
                Dragged?.Invoke(element));

            element.PreviewMouseLeftButtonDown -= mouseClickAction;
            element.PreviewMouseLeftButtonUp -= removeMouseAction;
            element.PreviewMouseMove -= mouseMoveAction;

            element.PreviewMouseLeftButtonDown += mouseClickAction;
            element.PreviewMouseLeftButtonUp += removeMouseAction;
            element.PreviewMouseMove += mouseMoveAction;
        }
    }
}
