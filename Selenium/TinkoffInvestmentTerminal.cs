using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Windows.Shell;

public class ChromeConnector
{
    public static IWebDriver ConnectToRunningChrome()
    {

        // Установите URL-адрес удаленного сервера Selenium WebDriver.
        string remoteDriverUrl = "C:\\Program Files\\Google\\Chrome\\Application\\chromedriver.exe";

        // Создайте экземпляр класса ChromeOptions.
        ChromeOptions options = new ChromeOptions();
        //options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        options.AddArguments("--user-data-dir=C:\\Users\\Goga\\AppData\\Local\\Google\\Chrome\\User", "--password-store=basic");


        // Создайте экземпляр класса RemoteWebDriver, указав URL-адрес удаленного сервера и ChromeOptions.
        ChromeDriver driver = new ChromeDriver(options);

        return driver;
    }
}
