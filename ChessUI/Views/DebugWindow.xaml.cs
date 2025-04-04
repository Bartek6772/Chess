﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessUI
{
    /// <summary>
    /// Logika interakcji dla klasy DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            DataContext = AppSettings.Instance;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
