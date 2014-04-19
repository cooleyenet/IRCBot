using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
/* 
* Coded by Pasi Havia 17.11.2001 http://koti.mbnet.fi/~curupted
* Updated / fixed by Blake 09.10.2010
* Updated by Everett Cooley
*/
namespace IrcBot
{
    class IrcBot
    {
        // Irc server to connect 
        public static string SERVER = ConfigurationManager.AppSettings["SERVER"];
        // Irc server's port (6667 is default port)
        public static int PORT = int.Parse(ConfigurationManager.AppSettings["PORT"]);
        // User information defined in RFC 2812 (Internet Relay Chat: Client Protocol) is sent to irc server 
        public static string USER = ConfigurationManager.AppSettings["USER"];
        // Bot's nickname
        public static string NICK = ConfigurationManager.AppSettings["NICK"];
        // Channel to join
        public static string CHANNEL = ConfigurationManager.AppSettings["CHANNEL"];
        public static StreamWriter writer;
        static void Main()
        {
            NetworkStream stream;
            TcpClient irc;
            string inputLine;
            string inputLowerCase;
            StreamReader reader;
            var JoinedChannel = false;
            try
            {
                irc = new TcpClient(SERVER, PORT);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                IrcInput("NICK " + NICK);
                IrcInput(USER);
                while (true)
                {
                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        Console.WriteLine("OUTPUT: " + inputLine);
                        // Split the lines sent from the server by spaces. This seems the easiest way to parse them.
                        string[] splitInput = inputLine.Split(new Char[] { ' ' });
                        if (splitInput[0] == "PING")
                        {
                            string PongReply = splitInput[1];
                            IrcInput("PONG " + PongReply);
                            continue;
                        }
                        switch (splitInput[1])
                        {
                            // Join CHANNEL after MOTD displays or fails to display
                            case "422": case "376":
                                string JoinString = "JOIN " + CHANNEL;
                                IrcInput(JoinString);
                                Thread.Sleep(2000);
                                JoinedChannel = true;
                                break;
                            default:
                                break;
                        }
                        if (JoinedChannel == true)
                        {
                            if (inputLine.StartsWith(":" + NICK) == false)
                            {
                                inputLowerCase = inputLine.ToLower();

                                //if (Regex.IsMatch(inputLowerCase, @"mag|gun|shoot|nra|shot"))
                                if (Regex.IsMatch(inputLowerCase, ConfigurationManager.AppSettings["NSATRIGGER"]))
                                {
                                    IrcReply();
                                    //ConfigurationManager.RefreshSection("appSettings");
                                }
                                if (Regex.IsMatch(inputLowerCase, @"reload settings"))
                                {
                                    ConfigurationManager.RefreshSection("appSettings");
                                }
                            }
                        }
                    }
                    // Close all streams
                    writer.Close();
                    reader.Close();
                    irc.Close();
                }
            }
            catch (Exception e)
            {
                // Show the exception, sleep for a while and try to establish a new connection to irc server
                Console.WriteLine(e.ToString());
                Thread.Sleep(5000);
                string[] argv = { };
            }
        }
        static void IrcReply()
        {
            //writer.WriteLine("PRIVMSG  " + CHANNEL + " \0001ACTION " + " has logged message" + "\0001");
            //writer.WriteLine(String.Format("PRIVMSG {0} \u0001ACTION {1}\u0001", CHANNEL, "logs message"));
            //writer.Flush();
            IrcInput(String.Format("PRIVMSG {0} \u0001ACTION {1}\u0001", CHANNEL, "logs message"));
        }
        static void IrcInput(string IrcInput)
        {
            writer.WriteLine(IrcInput);
            Console.WriteLine("INPUT:  " + IrcInput);
            writer.Flush();
        }
    }
}