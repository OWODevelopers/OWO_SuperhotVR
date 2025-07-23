using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MelonLoader;
using OWOGame;

namespace OWO_ElvenAssassin
{
    public class OWOSkin
    {
        public bool suitEnabled = false;
        public bool systemInitialized = false;
        private static bool heartBeatIsActive = false;
        private int heartbeatCount = 0;
        public bool stringBowIsActive = false;
        public int stringBowIntensity = 40;

        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }

            }

            systemInitialized = true;
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("57995924");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitEnabled = true;
                LOG("OWO suit connected.");
                Feel("Heart Beat");
            }
            if (!suitEnabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }
        #endregion

        public void Feel(String key, int Priority = 0, int intensity = 0, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                Sensation toSend = FeedbackMap[key];                

                if (intensity != 0) {
                    toSend = toSend.WithMuscles(Muscle.All.WithIntensity(intensity));
                }

                OWO.Send(toSend.WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key);
        }

        public void FeelWithHand(String key, int priority = 0, bool isRightHand = true, int intensity = 0)
        {

            if (isRightHand)
            {
                key += " R";
            }
            else
            {
                key += " L";
            }

            Feel(key, priority, intensity);
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }

        #region heart beat loop
        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartbeatCount = 0;
            heartBeatIsActive = false;
        }

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive && heartbeatCount <= 15)
            {
                heartbeatCount++;
                Feel("Heart Beat", 0);
                await Task.Delay(1000);
            }
            
            StopHeartBeat();
        }
        #endregion


        #region StringBow loop
        public void StartStringBow(bool isRightHanded)
        {
            if (stringBowIsActive) return;

            stringBowIsActive = true;
            StringBowFuncAsync(isRightHanded);
        }

        public void StopStringBow()
        {
            stringBowIsActive = false;
        }

        public async Task StringBowFuncAsync(bool isRightHanded)
        {
            while (stringBowIsActive)
            {
                FeelWithHand("String Bow", 1, isRightHanded, stringBowIntensity);
                await Task.Delay(100);
            }
        }
        #endregion

        public void DeathAction()
        {
            StopAllHapticFeedback();
            Feel("Death", 4);
        }

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();
            StopStringBow();

            OWO.Stop();
        }


    }
}
