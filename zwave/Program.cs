using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ControlThink.ZWave;
using ControlThink.ZWave.Devices;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
namespace ZWave
{
    namespace ConfigurationFile
    {
        [DataContract]
        public class Device
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public int NodeId { get; set; }
        }
        [DataContract]
        public class DeviceInScene
        {
            [DataMember]
            public int DeviceId { get; set; }

            [DataMember]
            public byte Level { get; set; }
        }
        [DataContract]
        public class Scene
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Id { get; set; }

            public IEnumerable<DeviceInScene> Devices
            {
                get
                {
                    var a = this.OnDevices != null ? this.OnDevices.Select(d => new DeviceInScene { DeviceId = d, Level = Byte.MaxValue }) : Enumerable.Empty<DeviceInScene>();
                    var b = this.OffDevices != null ? this.OffDevices.Select(d => new DeviceInScene { DeviceId = d, Level = 0 }) : Enumerable.Empty<DeviceInScene>();
                    return a.Concat(b);
                }
            }
            [DataMember]
            public IEnumerable<int> OnDevices { get; set; }

            [DataMember]
            public IEnumerable<int> OffDevices { get; set; }

        }
        [DataContract]
        public class ConfigFile
        {
            [DataMember(Name = "Devices")]
            public IEnumerable<Device> Devices { get; set; }

            [DataMember]
            public IEnumerable<Scene> Scenes { get; set; }

            [DataMember]
            public IEnumerable<Controller> Controllers { get; set; }

            internal Scene GetSceneById(string id)
            {
                if (id.StartsWith("d:"))
                {
                    var idvalues = id.Split(':');
                    if (idvalues.Length != 3)
                    {
                        throw new InvalidDataException();
                    }
                    var deviceId = Int32.Parse(idvalues[1]);
                    var devicesArray = new int[] { deviceId };
                    var resultScene = new Scene { Id = id, Name = id };
                    if (String.Compare(idvalues[2], "on", ignoreCase: true) == 0)
                    {
                        resultScene.OnDevices = devicesArray;
                    }
                    else
                    {
                        resultScene.OffDevices = devicesArray;
                    }
                    return resultScene;
                }
                return this.Scenes.Where(Scene => Scene.Id == id).First();
            }

            internal Device GetDeviceById(int id)
            {
                return Devices.Where(device => device.NodeId == id).FirstOrDefault();
            }

            internal Controller GetControllerByName(string name)
            {
                return Controllers.Where(cont => cont.Name == name).First();
            }
        }

        [DataContract]
        public class SceneToControllerMap
        {
            [DataMember]
            public string SceneId { get; set; }

            [DataMember]
            public byte Channell { get; set; }

            public SceneToControllerMap()
            {
                SceneId = null;
                Channell = 0;
            }
        }
        [DataContract]
        public class Controller
        {
            private string[] channells;

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public int NumberOfChannels { get; set; }

            [IgnoreDataMember]
            public IEnumerable<SceneToControllerMap> Scenes { get; set; }

            [DataMember]
            public string[] Channells
            {
                set
                {
                    channells = value;
                    this.SetMapping(value);
                }
                get
                {
                    return channells;
                }
            }

            public void SetMapping(string[] Scenes)
            {
                List<SceneToControllerMap> mappings = new List<SceneToControllerMap>();
                for (byte i = 0; i < NumberOfChannels && i < Scenes.Length; i++)
                {
                    if (!String.IsNullOrEmpty(Scenes[i]))
                    {
                        mappings.Add(new SceneToControllerMap { Channell = (byte)(i + 1), SceneId = Scenes[i] });
                    }
                }
                this.Scenes = mappings;
            }
        }
    };

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Duplicate the ZWave network and setup channels to a new controller");
                    Console.WriteLine("Asumes that the ControlStick is installed");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("       DuplicateZwaveNetwork.exe <Configuration File Name> <Controller Name>");
                    Console.WriteLine();
                    Console.WriteLine("       Configuration File Name - The path to the JSON file with the network ");
                    Console.WriteLine("                                 configuration details");
                    Console.WriteLine("       Controller Name - The name of the controller to duplicate (The controller");
                    Console.WriteLine("                          is expected to be defined in the config file");
                    return;
                }

                var ser = new DataContractJsonSerializer(typeof(ConfigurationFile.ConfigFile));
                var config = default(ConfigurationFile.ConfigFile);
                using (FileStream stream = new FileStream(args[0], FileMode.Open))
                {
                    // Skip the UTF8 headers in the file
                    while (stream.ReadByte() != '{') ;
                    stream.Seek(-1, SeekOrigin.Current);
                    config = ser.ReadObject(stream) as ConfigurationFile.ConfigFile;
                }

                ZWaveController cont = new ZWaveController();
                Console.WriteLine("Connecting to Think Stick");
                cont.Connect();
                Console.WriteLine("Connected");
                var controllerConfig = config.GetControllerByName(args[1]);
                Console.WriteLine("Copying network settings and channels to controller " + controllerConfig.Name);
                var replicationScenes = new List<ZWaveController.ReplicationScene>();
                foreach (var s in controllerConfig.Scenes)
                {
                    var repScene = new ZWaveController.ReplicationScene(s.Channell);
                    var Scene = config.GetSceneById(s.SceneId);
                    Console.WriteLine("Channel " + s.Channell + " --> " + Scene.Name);
                    foreach (var d in Scene.Devices)
                    {
                        var device = config.GetDeviceById(d.DeviceId);
                        Console.WriteLine("                       " + device.Name + " " + (d.Level == 0 ? "off" : "on"));
                        var deviceFromController = cont.Devices[device.NodeId - 1];
                        repScene.SceneItems.Add(deviceFromController, d.Level);
                    }
                    replicationScenes.Add(repScene);
                }
                Console.WriteLine("Make sure to put the controller in \"Receive Network Configuration\" state ");
                Console.WriteLine("to do so on HA07");
                Console.WriteLine(" * Press and hold the INCLUDE Button for 5 seconds. COPY will flash.");
                Console.WriteLine(" * Release the INCLUDE button.");
                Console.WriteLine(" * Press and release the channel 1 OFF/DIM.");
                Console.WriteLine("   The display will show \"RA\" which means \"Receive All Information\"");
                Console.WriteLine();
                Console.WriteLine("On the HA09 the steps are the same but the lights will flash instead");
                Console.WriteLine("of the display.");
                var addedController = cont.AddController(new ZWaveController.ReplicationGroup[] { }, replicationScenes.ToArray());
                if (addedController == null)
                {
                    throw new Exception("Failed to add controller");
                }
                Console.WriteLine("Done!!!");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
