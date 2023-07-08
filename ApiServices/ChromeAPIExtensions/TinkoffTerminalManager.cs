using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using static System.Net.Mime.MediaTypeNames;

namespace TinkoffPriceMonitor.ApiServices.ChromeAPIExtensions
{
    public class TinkoffTerminalManager
    {
        private IWebDriver _driver;
        private Uri _tinkoffTerminalUrl = new("https://www.tinkoff.ru/terminal/");
        private string? _tickerGroupName;

        public void Start(string tickerGroupName)
        {
            _tickerGroupName = tickerGroupName;

            StartChrome();
            ConnectToChromeDriver();
            OpenTerminal();
        }

        public void OpenTerminal()
        {
            if (_driver == null)
            {
                return;
            }

            // Перехожу в терминал
            _driver.Navigate().GoToUrl(_tinkoffTerminalUrl);

            // Ожидаю полной загрузки DOM
            WaitForPageLoad();

            // Открываю виджеты
            OpenWidgetsWindow();

            // Открываю инструменты
            ClickToolsButton();

            // Открываю список для выбора группы тикеров
            OpenChooseTickerGroups();

            // Выбираю группу тикеров
            ChooseTickerGroup();
        }


        // Открываю окно Виджеты
        private void OpenWidgetsWindow()
        {
            try
            {
                // Открываем если не открыто
                var element = _driver.FindElement(By.XPath("//button[contains(@class, 'pro-button pro-minimal pro-small')]/span[text()='Виджеты']"));
                element.Click();
            }
            catch (Exception)
            {

            }

        }

        // Нажмаю на кнопку Инструменты
        private void ClickToolsButton()
        {
            try
            {
                var element = _driver.FindElement(By.XPath("//li[contains(@class, 'pro-menu-item-wrapper')]//div[@class='pro-text-overflow-ellipsis pro-fill' and text()='Инструменты']"));
                element.Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске и клике на кнопке 'Инструменты': {ex.Message}");
            }
        }

        // Нажимаю на крестик для выбора группы тикеров
        private void OpenChooseTickerGroups()
        {
            IWebElement popupElement;
            IWebElement spanElement;

            try
            {
                popupElement = _driver.FindElement(By.XPath("//div[contains(@class, 'src-core-components-WidgetBody-WidgetBody-widgetBody-QGsdH')]//div[contains(text(), 'Инструменты')]//ancestor::div[contains(@class, 'src-core-components-WidgetBody-WidgetBody-widgetBody-QGsdH')]"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске элемента popup: {ex.Message}");
                return;
            }

            try
            {
                spanElement = popupElement.FindElement(By.CssSelector("span.pro-popover-target"));

                // Выполнение клика на элементе
                spanElement.Click();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске элемента span: {ex.Message}");
                return;
            }
        }


        // Выбираю группу тикеров
        private void ChooseTickerGroup()
        {
            try
            {
                var list = _driver.FindElement(By.CssSelector("body > ul.pro-menu.pro-small.src-core-components-GroupMenu-GroupMenu-popover-CHTjJ.kvt-menu-load"));
                var listItem = list.FindElements(By.CssSelector("li.pro-menu-item-wrapper"));

                foreach (var item in listItem)
                {
                    var elementText = item.FindElement(By.CssSelector("div.pro-text-overflow-ellipsis.pro-fill")).Text;
                    if (elementText.Equals(_tickerGroupName))
                    {
                        item.Click();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске и клике на элементе списка: {ex.Message}");
            }
        }

        private static void StartChrome()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
                Arguments = "--remote-debugging-port=9222"
            };

            Process.Start(startInfo); // Запускаю браузер            
        }

        private void WaitForPageLoad()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        private void ConnectToChromeDriver()
        {
            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";

            _driver = new ChromeDriver(options);
        }

        public void Close()
        {
            _driver?.Quit();
        }
    }
}
