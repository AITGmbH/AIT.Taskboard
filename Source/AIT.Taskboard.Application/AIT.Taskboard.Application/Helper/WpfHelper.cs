﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AIT.Taskboard.Application.Helper
{
    public static class WpfHelper
    {
        /// <summary>
        /// Finds the visual child matching the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the child to find</typeparam>
        /// <param name="obj">The root of our search.</param>
        /// <returns>The first child of <paramref name="obj"/> matching the given type.</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the visual parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The element.</param>
        /// <returns>The first visual parent matching the specified type or <see langword="null"/> if no matching parent was found</returns>
        public static T FindVisualParent<T>(DependencyObject element) where T : class
        {
            if (element is Visual || element is Visual3D)
            {
                var parent = VisualTreeHelper.GetParent(element) as UIElement;
                while (parent != null)
                {
                    var correctlyTyped = parent as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }
            return null;
        }

        public static T FindVisualChild<T>(DependencyObject obj, object dataContext) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i) as FrameworkElement;
                if (child != null && child is T && child.DataContext == dataContext)
                    return (T)child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child, dataContext);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds all visual children matching the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IList<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            IList<T> matches = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    matches.Add((T)child);
                }
                else
                {
                    foreach (var foundchild in FindVisualChildren<T>(child))
                    {
                        matches.Add(foundchild);
                    }
                }
            }
            return matches;
        }

        /// <summary>
        /// Gets the count of visual children .
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static int GetVisualChildrenCount(DependencyObject source)
        {
            return VisualTreeHelper.GetChildrenCount(source);
        }
    }
}
