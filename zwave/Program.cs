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
        public class Schene
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
            public IEnumerable<Schene> Schenes { get; set; }

            [DataMember]
            public IEnumerable<Controller> Controllers { get; set; }

            internal Schene GetScheneById(string id)
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
                    var resultSchene = new Schene { Id = id, Name = id };
                    if (String.Compare(idvalues[2], "on", ignoreCase: true) == 0)
                    {
                        resultSchene.OnDevices = devicesArray;
                    }
                    else
                    {
                        resultSchene.OffDevices = devicesArray;
                    }
                    return resultSchene;
                }
                return this.Schenes.Where(Schene => Schene.Id == id).First();
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
        public class ScheneToControllerMap
        {
            [DataMember]
            public string ScheneId { get; set; }

            [DataMember]
            public byte Channell { get; set; }

            public ScheneToControllerMap()
            {
                ScheneId = null;
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
            public IEnumerable<ScheneToControllerMap> Schenes { get; set; }

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

            public void SetMapping(string[] Schenes)
            {
                List<ScheneToControllerMap> mappings = new List<ScheneToControllerMap>();
                for (byte i = 0; i < NumberOfChannels && i < Schenes.Length; i++)
                {
                    if (!String.IsNullOrEmpty(Schenes[i]))
                    {
                        mappings.Add(new ScheneToControllerMap { Channell = (byte)(i + 1), ScheneId = Schenes[i] });
                    }
                }
                this.Schenes = mappings;
            }
        }
    };

    class Program
    {
        static ConfigurationFile.ConfigFile SaveConfig()
        {
            var devices = new ConfigurationFile.Device[] {
                
                new ConfigurationFile.Device { Name = "מסדרון למעלה ליד החדרים", NodeId = 1},
                new ConfigurationFile.Device { Name = "מסדרון למטה", NodeId = 4}, 
                new ConfigurationFile.Device { NodeId = 6 , Name ="שקע בסלון"},
                new ConfigurationFile.Device { NodeId =7  , Name ="חדר ארונות הורים"},
                new ConfigurationFile.Device { NodeId =8 , Name ="מיטה יאיר"},
                new ConfigurationFile.Device { NodeId =9 , Name ="מיטה מוריה"},
                new ConfigurationFile.Device { NodeId =11 , Name ="סלון"},
                new ConfigurationFile.Device { NodeId =12 , Name ="שקע בסלום"},
                new ConfigurationFile.Device { NodeId =14 , Name ="חדר ילדים"},
                new ConfigurationFile.Device { NodeId =15 , Name ="חדר משחקים"},
                new ConfigurationFile.Device { NodeId =16 , Name ="מסדרון למעלה קרוב למטבח"},
                new ConfigurationFile.Device { NodeId =17 , Name ="גראז ליד הדלת"},
                new ConfigurationFile.Device { NodeId =18 , Name ="גראז מעל המדפים"},
                new ConfigurationFile.Device { NodeId =19 , Name ="גראז מעל הסולם"},
                new ConfigurationFile.Device { NodeId =20 , Name ="שירותים למטה מעל הכיור"},
                new ConfigurationFile.Device { NodeId =21 , Name ="למטה מעל מכונת הכביסה"},
                new ConfigurationFile.Device { NodeId =22 , Name ="למטה מעל הטלוויזיה"},
                new ConfigurationFile.Device { NodeId =23 , Name ="למטה מעל המחשב"},
                new ConfigurationFile.Device { NodeId =25 , Name ="למטה מעל המיטה"},
                new ConfigurationFile.Device { NodeId =26 , Name ="מטבח מעל הכיור"},
                new ConfigurationFile.Device { NodeId =27 , Name ="מטבח מעל המקרר"},
                new ConfigurationFile.Device { NodeId =28 , Name ="מטבח פינה למעלה (מעל המזווה)"},
            };
            var schems = new ConfigurationFile.Schene[] {
                new ConfigurationFile.Schene { Id = "1", Name = "Downstairs", OnDevices = new int[] { 23, 25, 22 } },
                new ConfigurationFile.Schene { Id = "C1", Name = "Childrens Room", OnDevices = new int[] { 14 }},
                new ConfigurationFile.Schene { Id = "C2", Name = "Toy Room", OnDevices = new int[] { 15 }},
                new ConfigurationFile.Schene { Id = "C3", Name = "Upstairs Corridor", OnDevices = new int[] { 1, 16 }},
                new ConfigurationFile.Schene { Id = "C4", Name = "Upstairs Corridor Night Schene", OnDevices = new int[] { 16 }, OffDevices = new int[] {1}},
                new ConfigurationFile.Schene { Id = "P2", Name = "Yair Bed", OnDevices = new int[] { 8 }},
                new ConfigurationFile.Schene { Id = "P3", Name = "Moriya Bed", OnDevices = new int[] { 9 }},
                new ConfigurationFile.Schene { Id = "P5", Name = "Closet Room", OnDevices = new int[] { 7 }},
                new ConfigurationFile.Schene { Id = "PR1", Name= "Welcome Home", OnDevices = new int[] {1,4,16, 27}},
                new ConfigurationFile.Schene { Id = "PR2", Name= "Night Off", OnDevices = new int[] {16},  OffDevices = new int[] {1, 4, 6, 7, 11, 12, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 25, 26, 27, 28}},
                new ConfigurationFile.Schene { Id = "E1", Name = "Living Room Light", OnDevices = new int[] {6}},
                new ConfigurationFile.Schene { Id = "7", Name = "Kitchen All", OnDevices = new int[] { 26, 27, 28 }},
                new ConfigurationFile.Schene { Id = "8", Name = "Parents Room all", OnDevices = new int[] {7, 8, 9}},
                new ConfigurationFile.Schene { Id = "9", Name = "Childrens Area all", OnDevices = new int[] {1, 16, 14, 15 }},
                new ConfigurationFile.Schene { Id = "10", Name = "Living Room", OnDevices = new int[] { 6, 11, 12 }},
                new ConfigurationFile.Schene { Id = "11", Name = "Downstairs All", OnDevices = new int[] { 20, 21, 22, 23, 25, 4 }},
                new ConfigurationFile.Schene { Id = "12", Name = "Garage", OnDevices = new int[] { 17, 18, 19 }},
            };
            var conrollers = new ConfigurationFile.Controller[] {
                new ConfigurationFile.Controller { Name = "Entrance", NumberOfChannels=12, Channells = new String[] {"PR1", "PR2", "", "", "E1", "", "7", "8", "9", "10", "11", "12"}},
                new ConfigurationFile.Controller { Name = "Childrens Room", NumberOfChannels=12, Channells = new String[] {"C1", "C2", "C3", "C4", "8", "7", "7", "8", "9", "10", "11", "12"}},
                new ConfigurationFile.Controller { Name = "Parents Room", NumberOfChannels=12, Channells = new String[] {"8", "P2", "P3", "C4", "P5", "", "7", "8", "9", "10", "11", "12"}},
                new ConfigurationFile.Controller { Name = "Portable Remote", NumberOfChannels=6, Channells = new String[] {"8", "P2", "P3", "PR2", "PR1"}}
            };
            var config = new ConfigurationFile.ConfigFile();
            config.Devices = devices;
            config.Schenes = schems;
            config.Controllers = conrollers;
            var ser = new DataContractJsonSerializer(typeof(ConfigurationFile.ConfigFile));

            using (FileStream stream = new FileStream(@"c:\temp\homeconfig.json", FileMode.Create))
            {

                var writter = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8);
                ser.WriteObject(writter, config);
                writter.Flush();
            }
            return config;
        }
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
                cont.Connect();

                var controllerConfig = config.GetControllerByName(args[1]);
                Console.WriteLine("Copying network settings and channels to controller " + controllerConfig.Name);
                var replicationSchenes = new List<ZWaveController.ReplicationScene>();
                foreach (var s in controllerConfig.Schenes)
                {
                    var repSchene = new ZWaveController.ReplicationScene(s.Channell);
                    var Schene = config.GetScheneById(s.ScheneId);
                    Console.WriteLine("Channel " + s.Channell + " --> " + Schene.Name);
                    foreach (var d in Schene.Devices)
                    {
                        var device = config.GetDeviceById(d.DeviceId);
                        Console.WriteLine("                       " + device.Name + " " + (d.Level == 0 ? "off" : "on"));
                        var deviceFromController = cont.Devices[device.NodeId - 1];
                        repSchene.SceneItems.Add(deviceFromController, d.Level);
                    }
                    replicationSchenes.Add(repSchene);
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
                var addedController = cont.AddController(new ZWaveController.ReplicationGroup[] { }, replicationSchenes.ToArray());
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
