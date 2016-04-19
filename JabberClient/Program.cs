using System;
using System.Collections.Generic;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using agsXMPP.protocol.iq.roster;
using System.Threading;
using agsXMPP.protocol.x.muc;
using agsXMPP.protocol.iq.vcard;
using System.Data.Linq;

namespace JabberClient
{
    class Program
    {
        static bool _wait;
        static XmppClientConnection xmpp;
        static List<userPresence> _userPresence;
        static List<userRoster> _userRoster;


        static void Main(string[] args)
        {
            //create user on xmpp
             // CreateUser();

            // Create User Presence List
            _userPresence = new List<userPresence>();

            // Create User Presence List
            _userRoster = new List<userRoster>();


            /*
             * Starting Jabber Console, setting the Display settings
             * 
             */
            Console.Title = "Jabber Client";
            Console.ForegroundColor = ConsoleColor.White;


            /*
             * Login
             * 
             */
            Console.WriteLine("Login");
            Console.WriteLine();
            Console.WriteLine("JID: ");
            string JID_Sender = "a_1@127.0.0.1";// Console.ReadLine();
            Console.WriteLine("Password: ");
            string Password = "123456";// Console.ReadLine();
            Console.WriteLine(JID_Sender);
            /*
             * Creating the Jid and the XmppClientConnection objects
             */
            Jid jidSender = new Jid(JID_Sender);
            xmpp = new XmppClientConnection(jidSender.Server);

            /*
             * Open the connection
             * and register the OnLogin event handler
             */
            try
            {
                xmpp.Open(jidSender.User, Password, "Home");
                xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /*
             * workaround, jus waiting till the login 
             * and authentication is finished
             * 
             */
            //Console.Write("Wait for Login ");
            //int i = 0;
            //_wait = true;
            //do
            //{
            //    Console.Write(".");
            //    i++;
            //    if (i == 10)
            //        _wait = false;
            //    Thread.Sleep(500);
            //} while (_wait);
            //Console.WriteLine();

            /*
             * 
             * just reading a few information
             * 
             */
            do
            {
                //Console.WriteLine("Login Status:");
                //Console.WriteLine("xmpp Connection State {0}", xmpp.XmppConnectionState);
                //Console.WriteLine("xmpp Authenticated? {0}", xmpp.Authenticated);
                //Console.WriteLine();
            } while (!xmpp.Authenticated);

            /*
             * 
             * tell the world we are online and in chat mode
             * 
             */
            Console.WriteLine("Sending Precence");
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.available;
            xmpp.Send(p);
            Console.WriteLine();

            /*
             * 
             * get the roster (see who's online)
             */
            xmpp.OnPresence += new PresenceHandler(xmpp_OnPresence);

            //wait until we received the list of available contacts            
            Console.WriteLine();
            Thread.Sleep(500);


            //Create room
          //  CreateRoom();





            ///*
            //* 
            //* Join room 
            //* 
            //*/
            //Console.WriteLine("Join room:");
            //MucManager mucManager = new MucManager(xmpp);
            //Jid Room = new Jid("project_300@conference.127.0.0.1");
            //mucManager.AcceptDefaultConfiguration(Room);
            //mucManager.JoinRoom(Room, "adino ticket kiosk app"); 
            // xmpp.OnMessage += xmpp_Group_OnMessage;
            //string outMessageRoom;
            //bool haltRoom = false;
            //do
            //{
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    outMessageRoom = Console.ReadLine();
            //    if (outMessageRoom == "q!")
            //    {
            //        haltRoom = true;
            //    }
            //    else
            //    {
            //        //string XML = "<body>" +
            //        //             "<message to='project_300@conference.74.208.79.74' type='groupchat' projectJID='project_300' projectName='adino ticket kiosk app' xmlns='jabber:client'>" +
            //        //             "<body>Hello from Window console¡12:21¡¡</body>" +
            //        //             "<project xmlns='url:xmpp:project' projectJID='project_300' projectName='adino ticket kiosk app'></project>" +
            //        //             "</message>" +
            //        //             "</body>";
            //        //xmpp.Send(XML);
            //        xmpp.Send(new Message(Room, MessageType.groupchat, outMessageRoom, "Room_OnMessage"));
            //    }
            //} while (!haltRoom);


            /*
             * Get My Vcard
             */
            GetMyVcard();



            /*
             * now we catch the user entry, TODO: who is online
             */
            Console.WriteLine("Enter Chat Partner JID:");
            string JID_Receiver = Console.ReadLine();
            Console.WriteLine();

            /*
             * Chat starts here
             */
            Console.WriteLine("Start Chat");

            /*
             * Catching incoming messages in
             * the MessageCallBack
             */
            xmpp.MessageGrabber.Add(new Jid(JID_Receiver),
                                     new BareJidComparer(),
                                     new MessageCB(MessageCallBack),
                                     null);

            //xmpp.OnReadXml += xmpp_OnReadXml;
           

            /*
             * Sending messages
             * 
             */
            string outMessage;
            bool halt = false;
            do
            {
                //xmpp.Send(new Message(new Jid(JID_Receiver),
                //                 MessageType.headline,
                //                 "Google"));


                Console.ForegroundColor = ConsoleColor.Green;
                outMessage = Console.ReadLine();
                if (outMessage == "q!")
                {
                    halt = true;
                }
                else
                {
                    xmpp.Send(new Message(new Jid(JID_Receiver),
                                  MessageType.chat,
                                  outMessage));
            
                
                }

            } while (!halt);
            Console.ForegroundColor = ConsoleColor.White;

            /*
             * finally we close the connection
             * 
             */
            // xmpp.Close();
            Console.ReadKey();
        }

        //static void xmpp_OnReadXml(object sender, string xml)
        //{
        //    Console.Write("--------xml--------");
        //    Console.Write(xml);
        //    Console.Write(sender);
        //    Console.Write("--------xml--------");
        //   // throw new NotImplementedException();
        //}


        /*
        * 
        * Create room 
        * 
        */
        private static void CreateRoom()
        {
            MucManager muc = new MucManager(xmpp);
            Jid RoomJid = new Jid("rj@conference.127.0.0.1");
            muc.CreateReservedRoom(RoomJid, new IqCB(OnCreateRoom));
            muc.JoinRoom(RoomJid, "a_1@127.0.0.1");
            muc.JoinRoom(RoomJid, "a_3@127.0.0.1");
            muc.JoinRoom(RoomJid, "a_4@127.0.0.1");
            muc.JoinRoom(RoomJid, "a_5@127.0.0.1");
        } 
        private static void OnCreateRoom(object sender, IQ iq, object data)
        {
           
        }

        //Create User
        private static void CreateUser()
        {
            /*
             * Open the connection
             * and register the OnLogin event handler
             */
            try
            {
                Console.WriteLine("Login");
                Console.WriteLine();
                Console.WriteLine("JID: ");
                string JID_Sender = "a_6@127.0.0.1";// Console.ReadLine();
                Console.WriteLine("Password: ");
                string Password = "123456";// Console.ReadLine();

                /*
                 * Creating the Jid and the XmppClientConnection objects
                 */
                Jid jidSender = new Jid(JID_Sender);
                xmpp = new XmppClientConnection(jidSender.Server);

                /*
                 * Open the connection
                 * and register the OnLogin event handler
                 */
                try
                {
                    //xmpp.AutoRoster = true;
                    xmpp.RegisterAccount = true;
                    xmpp.Open(jidSender.User, Password, "Home");
                    //xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                do
                {
                    //Console.WriteLine("Login Status:");
                    //Console.WriteLine("xmpp Connection State {0}", xmpp.XmppConnectionState);
                    //Console.WriteLine("xmpp Authenticated? {0}", xmpp.Authenticated);
                    //Console.WriteLine();
                } while (!xmpp.Authenticated);


                //Set vcard user
                // VcardIq viq = new VcardIq(IqType.get);
                //// viq.Vcard.Name.Value = "Name Test New ";
                // viq.Vcard.Birthday =  DateTime.Now;
                // viq.Vcard.Description = "Description Test user create by agsxmpp";
                // viq.Vcard.Fullname = "Fullname Test New ";
                // viq.Vcard.JabberId = jidSender;
                // viq.Vcard.Nickname = "Nickname Test";
                //// viq.Vcard.Photo = new Photo("http://127.0.0.1:9090/images/login_logo.gif");
                // viq.Vcard.Prefix = "Prefix Mr.";
                // viq.Vcard.Role = "Role User";
                // viq.Vcard.Title = "Title User"; 
                // xmpp.IqGrabber.SendIq(viq, new IqCB(VcardResult), null);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //get MY Vcard 
        static public void GetMyVcard()
        {
            VcardIq viq = new VcardIq(IqType.get);
            xmpp.IqGrabber.SendIq(viq, new IqCB(VcardResult), null);
        }
        static private void VcardResult(object sender, IQ iq, object data)
        {
            if (iq.Type == IqType.result)
            {
                Vcard vcard = iq.Vcard;
                if (vcard != null)
                {
                    string fullname = vcard.Fullname;
                    string nickname = vcard.Nickname;
                    string description = vcard.Description;
                    Photo photo = vcard.Photo;
                }
            }
        }

        //call this function when group message recive
        static void xmpp_Group_OnMessage(object sender, Message msg)
        {
            // throw new NotImplementedException();
            Console.WriteLine("Sending text: '" + msg.Body);
        }

        // Is called, if the precence of a roster contact changed        
        static void xmpp_OnPresence(object sender, Presence pres)
        {
            if (pres.Type.ToString() != "error")
            {
                var iUser = _userPresence.Find(x => x.User == pres.From.User);
                if (iUser != null)
                {
                    iUser.Resource = pres.From.Resource;
                    iUser.Server = pres.From.Server;
                    iUser.User = pres.From.User;
                    iUser.Nickname = pres.Nickname == null ? "" : pres.Nickname.ToString();
                    iUser.PresenceType = pres.Type;
                    iUser.Status = pres.Status;
                    iUser.Show = pres.Show;
                    iUser.Prefix = pres.Prefix == null ? "" : pres.Prefix.ToString();
                }
                else
                {
                    userPresence objuserPresence = new userPresence();
                    objuserPresence.Resource = pres.From.Resource;
                    objuserPresence.Server = pres.From.Server;
                    objuserPresence.User = pres.From.User;
                    objuserPresence.Nickname = pres.Nickname == null ? "" : pres.Nickname.ToString();
                    objuserPresence.PresenceType = pres.Type;
                    objuserPresence.Status = pres.Status;
                    objuserPresence.Show = pres.Show;
                    objuserPresence.Prefix = pres.Prefix == null ? "" : pres.Prefix.ToString();
                    _userPresence.Add(objuserPresence);
                }
                Console.WriteLine("Available Contacts: ");
                Console.WriteLine("{0}@{1}  {2}", pres.From.User, pres.From.Server, pres.Type);
                //Console.WriteLine(pres.From.User + "@" + pres.From.Server + "  " + pres.Type);
                Console.WriteLine();
            }
        }

        // Is raised when login and authentication is finished 
        static void xmpp_OnLogin(object sender)
        {
            _wait = false;
            Console.WriteLine("Logged In");

            /*
           * 
           * tell the world we are online and in chat mode
           * 
           */
            Console.WriteLine("Sending Precence");
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.available;
            xmpp.Send(p);
            Console.WriteLine();

            xmpp.OnRosterStart += new ObjectHandler(objXmpp_OnRosterStart);
            xmpp.OnRosterItem += new XmppClientConnection.RosterHandler(objXmpp_OnRosterItem);
            xmpp.OnRosterEnd += new ObjectHandler(objXmpp_OnRosterEnd);



            string JID_Sender = "a_1@127.0.0.1";
            Jid jidRoster = new Jid(JID_Sender);
            //xmpp.RosterManager.AddRosterItem(jidRoster, "Test");  
            //RosterManager objRosterManager = new RosterManager(xmpp);
            //objRosterManager.AddRosterItem(jidRoster, "Test");
            addNewRoster(jidRoster);
            subcribeRoster(jidRoster);

            //get User roster Items
            RequestRoster();

        }
        static private void addNewRoster(Jid contactJid)
        {
            // xmpp.RosterManager.RemoveRosterItem(contactJid); // remove
            xmpp.RosterManager.AddRosterItem(contactJid); // Add new
            xmpp.RosterManager.UpdateRosterItem(contactJid, "Update Nickname", "Friends"); //update 
        }
        static private void subcribeRoster(Jid contactJid)
        {
            xmpp.PresenceManager.Subscribe(contactJid, "Both");
            xmpp.PresenceManager.ApproveSubscriptionRequest(contactJid);
            //xmpp.PresenceManager.RefuseSubscriptionRequest(contactJid);
        }

        private static void objXmpp_OnRosterStart(object sender)
        {
            //throw new NotImplementedException();
        }
        private static void objXmpp_OnRosterItem(object sender, RosterItem item)
        {
            //throw new NotImplementedException();
        }

        private static void objXmpp_OnRosterEnd(object sender)
        {
            //throw new NotImplementedException();
        }



        //Handles incoming messages
        static void MessageCallBack(object sender,
                                    Message msg,
                                    object data)
        { 
            //MessageBox.Show("BUZZZZZ...");
            if (msg.Body != null)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}>> {1}", msg.From.User, msg.Body);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }


        //get user roster list
        static public void RequestRoster()
        {
            RosterIq iq = new RosterIq(IqType.get);
            xmpp.IqGrabber.SendIq(iq, new IqCB(OnRosterResult), null);
        }
        static private void OnRosterResult(object sender, IQ iq, object data)
        {
            Roster r = iq.Query as Roster;
            if (r != null)
            {
                foreach (RosterItem i in r.GetRoster())
                {
                    // Loops thru all contacts
                    var iUser = _userRoster.Find(x => x.User == i.Jid.User);
                    if (iUser != null)
                    {
                        iUser.User = i.Jid.User;
                        iUser.Server = i.Jid.Server;
                        iUser.Resource = i.Jid.Resource;
                        iUser.Name = i.Name;
                        iUser.Jid = i.Jid.ToString();
                    }
                    else
                    {
                        userRoster objuserRoster = new userRoster();
                        objuserRoster.User = i.Jid.User;
                        objuserRoster.Server = i.Jid.Server;
                        objuserRoster.Resource = i.Jid.Resource;
                        objuserRoster.Name = i.Name;
                        objuserRoster.Jid = i.Jid.ToString();
                        _userRoster.Add(objuserRoster);
                    }
                }
            }

            //get Vcard another User  
            vcardAll();
        }


        //Get Vcard of user
        static private void vcardAll()
        {
            foreach (var uRoster in _userRoster)
            {
                VcardIq viq = new VcardIq(IqType.get, new Jid(uRoster.Jid));
                uRoster.UVcard = viq.Vcard;
                //xmpp.IqGrabber.SendIq(viq, new IqCB(VcardResult), null); 
            }
        }

    }
    class userRoster
    {
        public string User { get; set; }
        public string Server { get; set; }
        public string Resource { get; set; }
        public string Name { get; set; }
        public string Jid { get; set; }
        public Vcard UVcard { get; set; }
    }
    class userPresence
    {
        public string Resource { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public string Nickname { get; set; }
        public PresenceType PresenceType { get; set; }
        public string Status { get; set; }
        public ShowType Show { get; set; }
        public string Prefix { get; set; }
    }
}
