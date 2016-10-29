using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace SerialToLanTransmitter
{
    class SettingsManager
    {
        private const string DATA_FILE_NAME = "Settings.dat";
        private const string FOLDER_NAME = "SerialToLANTransmitter";

        private static SettingsManager _instance;
        public SettingsData data = new SettingsData();

        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsManager();

                return _instance;
            }
        }

        public void SaveData()
        {
            new FileInfo(DataPath).Directory.Create();
            var binFormat = new BinaryFormatter();

            using (var fStream = new FileStream(DataPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                try
                {
                    binFormat.Serialize(fStream, data);
                }
                catch (Exception e)
                {
                    MainWindow.Log(e.ToString());
                }
            }
        }

        public void LoadData()
        {
            if (!File.Exists(DataPath))
                return;

            var binFormat = new BinaryFormatter();

            using (var fStream = File.OpenRead(DataPath))
            {
                try
                {
                    data = (SettingsData)binFormat.Deserialize(fStream);
                }
                catch (Exception e)
                {
                    MainWindow.Log(e.ToString());
                }
            }
        }

        private string DataPath
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments), FOLDER_NAME, DATA_FILE_NAME);
                return path;
            }
        }

        public void SetRemotePort(string value)
        {
            int port = 0;
            if (int.TryParse(value, out port))
                data.remotePort = port;
        }

        public void SetLocalPort(string value)
        {
            int port = 0;
            if (int.TryParse(value, out port))
                data.localPort = port;
        }
    }
}
