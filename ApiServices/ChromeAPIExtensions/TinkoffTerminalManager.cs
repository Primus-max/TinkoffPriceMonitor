using System;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TinkoffPriceMonitor.ApiServices.ChromeAPIExtensions
{
    public class TinkoffTerminalManager
    {
        private IWebDriver driver;

        public void Start()
        {
            StartChrome();
            ConnectToChromeDriver();
        }

        public void OpenTerminal(string url)
        {
            if (driver == null)
            {
                Console.WriteLine("Ошибка: Браузер Chrome не был запущен и подключен.");
                return;
            }

            driver.Navigate().GoToUrl(url);
            string pageTitle = driver.Title;
            Console.WriteLine($"Открыт терминал Тинькофф. Заголовок: {pageTitle}");
        }

        public void Close()
        {
            driver?.Quit();
        }

        private void StartChrome()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
                Arguments = "--remote-debugging-port=9222"
            };

            Process.Start(startInfo); // Запускаю браузер

            WaitForPageLoad(); // Ожидание загрузки браузера
        }

        private void WaitForPageLoad()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        private void ConnectToChromeDriver()
        {
            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";

            driver = new ChromeDriver(options);
        }
    }
}
