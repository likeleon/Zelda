﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Shell.Controls
{
    public class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            ITool tool = anchorableToShow.Content as ITool;
            if (tool == null)
                return false;

            PaneLocation preferredLocation = tool.PreferredLocation;
            string paneName = GetPaneName(preferredLocation);
            LayoutAnchorablePane toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == paneName);
            if (toolsPane == null)
            {
                switch (preferredLocation)
                {
                    case PaneLocation.Left:
                        toolsPane = CreateAnchorablePane(layout, Orientation.Horizontal, paneName, InsertPosition.Start);
                        break;
                    case PaneLocation.Right:
                        toolsPane = CreateAnchorablePane(layout, Orientation.Horizontal, paneName, InsertPosition.End);
                        break;
                    case PaneLocation.Bottom:
                        toolsPane = CreateAnchorablePane(layout, Orientation.Vertical, paneName, InsertPosition.End);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            toolsPane.Children.Add(anchorableToShow);
            return true;
        }

        private static string GetPaneName(PaneLocation location)
        {
            switch (location)
            {
                case PaneLocation.Left:
                    return "LeftPane";
                case PaneLocation.Right:
                    return "RightPane";
                case PaneLocation.Bottom:
                    return "BottomPane";
                default:
                    throw new ArgumentOutOfRangeException("location");
            }
        }

        private static LayoutAnchorablePane CreateAnchorablePane(LayoutRoot layout, Orientation orientation, string paneName, InsertPosition position)
        {
            LayoutPanel parent = layout.Descendents().OfType<LayoutPanel>().First(d => d.Orientation == orientation);
            LayoutAnchorablePane toolsPane = new LayoutAnchorablePane { Name = paneName };
            if (position == InsertPosition.Start)
                parent.InsertChildAt(0, toolsPane);
            else
                parent.Children.Add(toolsPane);
            return toolsPane;
        }

        private enum InsertPosition
        {
            Start,
            End
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
            ITool tool = anchorableShown.Content as ITool;
            if (tool == null)
                return;

            LayoutAnchorablePane anchorablePane = anchorableShown.Parent as LayoutAnchorablePane;
            if (anchorablePane == null || anchorablePane.ChildrenCount != 1)
                return;

            switch (tool.PreferredLocation)
            {
                case PaneLocation.Left:
                case PaneLocation.Right:
                    anchorablePane.DockWidth = new GridLength(tool.PreferredWidth, GridUnitType.Pixel);
                    break;
                case PaneLocation.Bottom:
                    anchorablePane.DockHeight = new GridLength(tool.PreferredHeight, GridUnitType.Pixel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }
    }
}
