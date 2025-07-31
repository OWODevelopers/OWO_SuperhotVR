using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using MelonLoader;
using OWOGame;

namespace OWO_SuperhotVR
{
    public class OWOSkin
    {
        public bool suitEnabled = false;
        public bool systemInitialized = false;
        private static bool mindwaveChargeIsActive = false;
        private int heartbeatCount = 0;

        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();
        private readonly Muscle[] rightRecoilMuscles = { Muscle.Arm_R, Muscle.Pectoral_R.WithIntensity(80), Muscle.Dorsal_R.WithIntensity(40) };
        private readonly Muscle[] leftRecoilMuscles = { Muscle.Arm_L, Muscle.Pectoral_L.WithIntensity(80), Muscle.Dorsal_L.WithIntensity(40) };

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\owo";
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

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("84329488");

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
                Feel("Grab Pyramid");
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
            string filePath = Directory.GetCurrentDirectory() + "\\Mods" + filename;
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

                if (intensity != 0)
                {
                    toSend = toSend.WithMuscles(Muscle.All.WithIntensity(intensity));
                }

                OWO.Send(toSend.WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key);
        }

        public void FeelWithHand(String key, bool isRightHand = true, int Priority = 0, int intensity = 100)
        {
            Sensation toSend = GetBackedId(key);
            if (toSend == null)
            {
                LOG("Feedback not registered: " + key);
                return;
            }

            toSend = toSend.WithMuscles(isRightHand ? rightRecoilMuscles.WithIntensity(intensity) : leftRecoilMuscles.WithIntensity(intensity));

            OWO.Send(toSend.WithPriority(Priority));
        }

        public void FeelGunfire(PickableItems guntype, HandType hand)
        {
            switch (guntype)
            {
                case PickableItems.Pistol:
                    FeelWithHand("Pistol Recoil", hand == HandType.RightHand || hand == HandType.Empty_RightHand, Priority: 2);
                    break;
                case PickableItems.Uzi:
                    FeelWithHand("Uzi Recoil", hand == HandType.RightHand || hand == HandType.Empty_RightHand, Priority: 2);
                    break;
                case PickableItems.Shotgun:
                    FeelWithHand("Shotgun Recoil", hand == HandType.RightHand || hand == HandType.Empty_RightHand, Priority: 2);
                    break;
                case PickableItems.Gun_NoAmmo:
                    FeelWithHand("No Ammo", hand == HandType.RightHand || hand == HandType.Empty_RightHand, Priority: 2);
                    break;
                case PickableItems.Knife:
                case PickableItems.Throwable:
                case PickableItems.Pyramid:
                    break;
            }
        }

        private Sensation GetBackedId(string sensationKey)
        {
            if (FeedbackMap.ContainsKey(sensationKey))
            {
                return FeedbackMap[sensationKey];
            }
            else
            {
                LOG($"Feedback not registered: {sensationKey}");
                return null;
            }
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }

        #region mindwave loop
        public void StartMindWaveCharge()
        {
            if (mindwaveChargeIsActive) return;

            mindwaveChargeIsActive = true;
            MindWaveChargeFuncAsync();
        }

        public void StopMindWaveCharge()
        {
            mindwaveChargeIsActive = false;
        }

        public async Task MindWaveChargeFuncAsync()
        {
            while (mindwaveChargeIsActive)
            {
                Feel("Mindwave Charge", 0);
                await Task.Delay(1000);
            }

            StopMindWaveCharge();
        }
        public void MindWaveSkill()
        {
            StopMindWaveCharge();
            Feel("Mindwave", 2);
        }
        #endregion

        public void DeathAction()
        {
            StopAllHapticFeedback();
            Feel("Death", 4);
        }

        public void StopAllHapticFeedback()
        {
            OWO.Stop();
        }
    }
}
