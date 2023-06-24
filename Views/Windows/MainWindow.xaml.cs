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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveDataToJson();
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            // Получаем DataContext выбранной группы
            var group = (sender as Button)?.DataContext as TickerGroup;

            // Удаляем группу из коллекции
            if (group != null)
            {
                _viewModel.TickerGroups.Remove(group);

                // Сохраняем обновленные данные в файл data.json
                SaveDataToJson();
            }
        }
        private void SaveDataToJson()
        {
            try
            {
                // Преобразуем коллекцию TickerGroups в JSON строку
                string jsonData = JsonConvert.SerializeObject(_viewModel.TickerGroups);

                // Путь к файлу data.json
                string filePath = "data.json";

                // Сохраняем JSON строку в файл
                File.WriteAllText(filePath, jsonData);

                //MessageBox.Show("Данные сохранены в JSON файл.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить данные в файл data.json, " +
                    $"по причине {ex.Message}");
            }
        }

    }
}
