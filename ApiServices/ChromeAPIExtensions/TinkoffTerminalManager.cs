using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using TinkoffPriceMonitor.Models;

namespace TinkoffPriceMonitor.ApiServices.ChromeAPIExtensions
{
    public class TinkoffTerminalManager
    {
        private IWebDriver _driver = null!;
        private Uri _tinkoffTerminalUrl = new("https://www.tinkoff.ru/terminal/");
        private string? _tickerGroupName = "ALRS";
        private string? _orderAmount = string.Empty;


        public TinkoffTerminalManager(string tickerGroupName, string orderAmount)
        {

            _tickerGroupName = tickerGroupName;
            _orderAmount = orderAmount;
        }

        // Метод, точка входа
        public void Start()
        {
            StartChrome();
            ConnectToChromeDriver();
            OpenTerminal();
        }

        // Основной метод по терминалу
        private void OpenTerminal()
        {
            if (_driver == null)
            {
                return;
            }

            // Перехожу на страницу терминала
            _driver.Navigate().GoToUrl(_tinkoffTerminalUrl);

            // Ожидаю полной загрузки DOM
            WaitForPageLoad();

            // Проверяю наличие кнопки начать инвестировать
            CheckBeginInvestButtonPresent();

            // Ожидание если страница грузится
            WaitWhileSpinner();
            // Проверяю есть ли запроса пинкода
            CheckOrEnterPinCode();

            // Ожидание если страница грузится
            WaitWhileSpinner();
            // Открываю виджеты
            OpenWidgetsWindow();

            // Ожидание если страница грузится
            WaitWhileSpinner();
            // Открываю инструменты
            ClickToolsButton();

            Thread.Sleep(2000);
            // Открываю список для выбора группы тикеров
            OpenChooseTickerGroups();

            Thread.Sleep(2000);
            // Выбираю группу тикеров
            ChooseTickerGroup();

            Thread.Sleep(2000);
            // Вставляю сумму в поле
            InputMoneyValue();
        }

        // Открываю окно Виджеты
        private void OpenWidgetsWindow()
        {
            try
            {
                // Ожидаю загрузки станицы
                WaitForPageLoad();

                // Открываем если не открыто
                var element = _driver.FindElement(By.XPath("//button[contains(@class, 'pro-button pro-minimal pro-small')]/span[text()='Виджеты']"));
                element.Click();
            }
            catch (Exception ex)
            {
                Log.Error($"В методе OpenWidgetsWindow произошла ошибка: {ex.Message}");
                return;
            }

        }

        // Нажмаю на кнопку Инструменты
        private void ClickToolsButton()
        {
            try
            {
                var element = _driver.FindElement(By.XPath("//li[contains(@class, 'pro-menu-item-wrapper')]//div[@class='pro-text-overflow-ellipsis pro-fill' and text()='Заявка']"));
                element.Click();
            }
            catch (Exception ex)
            {
                Log.Error($"В методе ClickToolsButton произошла ошибка: {ex.Message}");
                return;
            }
        }

        // Нажимаю на крестик для выбора группы тикеров
        private void OpenChooseTickerGroups()
        {
            IWebElement popupElement = null!;
            IWebElement spanElement = null!;

            try
            {
                popupElement = _driver.FindElement(By.XPath("//div[contains(@class, 'src-core-components-WidgetBody-WidgetBody-widgetBody-QGsdH')]//div[contains(text(), 'Заявка')]//ancestor::div[contains(@class, 'src-core-components-WidgetBody-WidgetBody-widgetBody-QGsdH')]"));
            }
            catch (Exception ex)
            {
                Log.Error($"В методе OpenChooseTickerGroups произошла ошибка: {ex.Message}");
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
                Log.Error($"В методе OpenChooseTickerGroups произошла ошибка: {ex.Message}");
                return;
            }
        }

        // Выбираю группу тикеров
        private void ChooseTickerGroup()
        {
            try
            {
                var list = _driver.FindElement(By.CssSelector("ul.pro-menu.pro-small.src-core-components-GroupMenu-GroupMenu-popover-CHTjJ.kvt-menu-load"));
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
                Log.Error($"В методе ChooseTickerGroup произошла ошибка: {ex.Message}");
                return;
            }
        }

        // Проверяю если есть на странице пин код,, если то ввожу
        private void CheckOrEnterPinCode()
        {
            string? _pinCode = GetSettings().PinCode;

            try
            {
                // Ожидаю загрузки станицы
                WaitForPageLoad();

                // Поиск элемента с id "pinCodeField"
                IWebElement pinCodeField = _driver.FindElement(By.Id("pinCodeField"));

                // Получение всех полей ввода пин-кода внутри div
                var inputFields = pinCodeField.FindElements(By.TagName("input"));

                // Ввод пин-кода в каждое поле
                for (int i = 0; i < inputFields.Count; i++)
                {
                    inputFields[i].SendKeys(_pinCode[i].ToString());

                    Thread.Sleep(500);
                }
            }
            catch (Exception) { }
        }

        // Проверяю на странице кнопку Начать инвестировать и кликаю если есть
        private void CheckBeginInvestButtonPresent()
        {
            try
            {
                // Ожидаю загрузки станицы
                WaitForPageLoad();

                IWebElement beginInvestButton = _driver.FindElement(By.ClassName("abou--HIZq.ibou--HIZq.cbou--HIZq"));
                beginInvestButton.Click();
            }
            catch (Exception) { }
        }

        //Ввожу сумму в поле
        private void InputMoneyValue()
        {
            try
            {
                // Получаю элемент для ввода суммы
                IWebElement inputElement = _driver.FindElement(By.CssSelector("input[type='text'][precision='0'][min='0'][max='1000000000'][tabindex='1'][locale='ru'][class='pro-input'][data-qa-tag='input']"));

                // Ввожу сумму
                inputElement.SendKeys("10000");
            }
            catch (Exception ex)
            {
                Log.Error($"В методе InputMoneyValue произошла ошибка: {ex.Message}");
                return;
            }
        }

        // Запускаю Chrome
        private static void StartChrome()
        {
            string? pathToChrome = GetSettings().ChromeLocation;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToChrome,
                Arguments = "--remote-debugging-port=9222"
            };

            Process.Start(startInfo); // Запускаю браузер            
        }

        // Метод ожидания загузки DOM
        private void WaitForPageLoad()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            }
            catch (Exception) { }
        }

        // Если есть спиннер (значит страница грузится) значит ждём
        private void WaitWhileSpinner()
        {
            while (true)
            {
                try
                {
                    IWebElement spinner = _driver.FindElement(By.CssSelector("div.pro-spinner-head"));
                    continue;
                }
                catch (Exception)
                {
                    break;
                }

            }
        }

        // Подключаю драйвер к запущенному браузеру
        private void ConnectToChromeDriver()
        {
            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true; // Скрыть окно командной строки драйвера Chrome

            _driver = new ChromeDriver(service, options);
        }

        // Закрываю драйвер
        private void Close()
        {
            _driver?.Quit();
        }

        // Загружаю путь к chrome
        private static SettingsModel GetSettings()
        {
            string filePath = "settings.json";
            SettingsModel settingsModel = new SettingsModel();

            try
            {
                if (File.Exists(filePath))
                {
                    // Если файл существует, загрузите его содержимое
                    string jsonData = File.ReadAllText(filePath);
                    JObject data = JObject.Parse(jsonData);

                    // Пример загрузки данных из JSON в модель представления
                    settingsModel.TinkoffToken = data["TinkoffToken"]?.ToString();
                    settingsModel.ChromeLocation = data["ChromeLocation"]?.ToString();
                    settingsModel.PinCode = data["PinCode"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }

            return settingsModel;
        }
    }
}
