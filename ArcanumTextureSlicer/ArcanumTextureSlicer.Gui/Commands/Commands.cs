﻿using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public static class Commands
    {
        public static readonly RoutedCommand Export = new ExportCommand();

        public static readonly RoutedCommand MoveGridUp1 = new MoveGridCommand(0, -1);
        public static readonly RoutedCommand MoveGridUp10 = new MoveGridCommand(0, -10);

        public static readonly RoutedCommand MoveGridDown1 = new MoveGridCommand(0, 1);
        public static readonly RoutedCommand MoveGridDown10 = new MoveGridCommand(0, 10);

        public static readonly RoutedCommand MoveGridLeft1 = new MoveGridCommand(-1, 0);
        public static readonly RoutedCommand MoveGridLeft10 = new MoveGridCommand(-10, 0);

        public static readonly RoutedCommand MoveGridRight1 = new MoveGridCommand(1, 0);
        public static readonly RoutedCommand MoveGridRight10 = new MoveGridCommand(10, 0);

        public static readonly RoutedCommand ResetGrid = new ResetGridCommand();

        public static readonly RoutedCommand ZoomX1 = new ZoomCommand(1);
        public static readonly RoutedCommand ZoomX2 = new ZoomCommand(2);
        public static readonly RoutedCommand ZoomX3 = new ZoomCommand(3);
        public static readonly RoutedCommand ZoomX4 = new ZoomCommand(4);
    }
}