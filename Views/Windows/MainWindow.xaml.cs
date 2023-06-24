using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TinkoffPriceMonitor.Models;
using TinkoffPriceMonitor.ViewModels;

namespace TinkoffPriceMonitor.Views.Windows
{
    public partial class MainWindow : Window
    {

        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddTickerGroup();
        }

    }
}
