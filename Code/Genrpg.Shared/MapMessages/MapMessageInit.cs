using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Stats.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Genrpg.Shared.MapMessages
{
    [MessagePackObject]
     public class MapMessageInit
    {
        const string ClassAttribute = "[MessagePackObject]";
        const string KeyPrefix = "[Key(";
        const string KeySuffix = ")]";
        const string UsingText = "using MessagePack;";

        const string UnionPrefix = "[Union(";
        const string UnionMiddle = ",typeof(";
        const string UnionSuffix = "))]";

        const string IMapApiMessageType = "public interface IMapApiMessage";

        const string IgnoreTypeMessage = "MessagePackIgnore";

        internal class MessageInitData
        {
            public List<string> MapMessageTypes = new List<string>();
            public string IMapMessagePath = null;

            public MessageInitData()
            {
                MapMessageTypes = new List<string>();
            }
        }

        private static List<string> GetImportantMessages()
        {
            return new List<string>()
            {
                typeof(OnUpdatePos).Name,
                typeof(FX).Name,
                typeof(StatUpd).Name,
            };
        }

        public static void InitMapMessages(string dirName)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(MapMessageInit));

            MessageInitData data = new MessageInitData();

            dirName += "..\\..\\..\\..\\..\\..\\..\\..\\Genrpg.Shared";

            List<string> mapTypeNames = new List<string>();

            AddDirectoryFiles(dirName, data);

            if (!String.IsNullOrEmpty(data.IMapMessagePath))
            {
                string txt = File.ReadAllText(data.IMapMessagePath);
                List<string> lines = txt.Split("\n").ToList();

                lines = lines.Where(x => x.IndexOf(UnionPrefix) < 0).ToList();

                int interfaceLine = -1;
                for (int lid  = 0; lid < lines.Count; lid++)
                {
                    if (lines[lid].Contains(IMapApiMessageType))
                    {
                        interfaceLine = lid;
                        break;
                    }
                }

                List<string> unionLines = GetImportantMessages();

                foreach (string line in data.MapMessageTypes)
                {
                    if (!unionLines.Contains(line))
                    {
                        unionLines.Add(line);
                    }
                }

                int unionIndex = 0;

                foreach (string line in unionLines)
                {
                    lines.Insert(interfaceLine + unionIndex, "    " + UnionPrefix + unionIndex++ + UnionMiddle + line + UnionSuffix);
                }

                bool foundUsing = false;

                foreach (string line in lines)
                {
                    if (line.Contains(UsingText))
                    {
                        foundUsing = true;
                        break;
                    }
                }

                if (!foundUsing)
                {
                    lines.Insert(0, UsingText);
                }

                StringBuilder sb = new StringBuilder();

                for (int lid = 0; lid < lines.Count; lid++)
                {
                    string line = lines[lid];
                    if (lid == lines.Count - 1 && string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    sb.Append(line + "\n");
                }

                File.WriteAllText(data.IMapMessagePath, sb.ToString());
            }
        }

        private static void AddDirectoryFiles(string path, MessageInitData data)
        {
            try
            {
                string[] files = Directory.GetFiles(path);

                foreach (var file in files)
                {
                    if (file.IndexOf(".cs") != file.Length - 3)
                    {
                        continue;
                    }

                    if (file.IndexOf("MapMessageInit") >= 0)
                    {
                        continue;
                    }

                    string fullPath = Path.Combine(new string[] { path, file });

                    string txt = File.ReadAllText(fullPath);

                    List<string> lines = txt.Split("\n").ToList();

                    bool shouldAddClass = false;
                    bool foundUsing = false;

                    int startKeyIndex = 0;

                    int classIndex = 0;

                    bool ignoreType = false;
                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        if (lines[lid].Contains(IgnoreTypeMessage))
                        {
                            ignoreType = true;
                            break;
                        }

                        if (lines[lid].Contains("public class") || lines[lid].Contains("public sealed class"))
                        {
                            lines[lid] = lines[lid].Replace(ClassAttribute + " ", "");
                        }
                        else if (lines[lid].Contains(ClassAttribute))
                        {
                            lines.RemoveAt(lid);
                            lid--;
                        }
                    }

                    if (ignoreType)
                    {
                        continue;
                    }

                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        string line = lines[lid];

                        classIndex = lid;

                        if (line.IndexOf(IMapApiMessageType) >= 0)
                        {
                            data.IMapMessagePath = fullPath;
                            break;
                        }

                        if (line.IndexOf(UsingText) >= 0)
                        {
                            foundUsing = true;
                        }

                        if (line.IndexOf(".Services") >= 0)
                        {
                            break;
                        }

                        if ((line.IndexOf("public class") < 0 && line.IndexOf("public sealed class") < 0) ||
                            line.IndexOf("Helper") >= 0)
                        {
                            continue;
                        }

                        if (line.IndexOf(": BaseMapApiMessage") > 0 || line.IndexOf("IMapApiMessage") > 0)
                        {
                            string[] words = line.Split(' ');

                            foreach (string word in words)
                            {
                                if (!string.IsNullOrEmpty(word) && word != "public" && word != "sealed" && word != "class")
                                {
                                    data.MapMessageTypes.Add(word);
                                    break;
                                }
                            }
                        }

                        int keyIndex = startKeyIndex;

                        while (++lid < lines.Count)
                        {
                            string line2 = lines[lid];
                            if (line2.IndexOf(" class") >= 0)
                            {
                                lid--;
                                break;
                            }

                            if (line2.IndexOf("[IgnoreMember]") >= 0)
                            {
                                continue;
                            }

                            if (line2.IndexOf("public") < 0 || line2.IndexOf("{ get; set; }") < 0 || line2.IndexOf("const") >= 0 ||
                                line2.IndexOf("static") >= 0)
                            {
                                continue;
                            }

                            if (line2.Contains(KeyPrefix))
                            {
                                line2 = line2.Substring(0, line2.IndexOf(KeyPrefix)) +
                                    line2.Substring(line2.IndexOf(KeySuffix) + 3);
                            }

                            lines[lid] = line2.Replace("public ", KeyPrefix + keyIndex++ + KeySuffix + " public ");
                            shouldAddClass = true;
                        }

                        if (shouldAddClass)
                        {
                            lines.Insert(classIndex, "    " + ClassAttribute);
                            lid++;
                        }
                    }

                    if (shouldAddClass && !foundUsing)
                    {
                        lines.Insert(0, UsingText);
                    }

                    StringBuilder newTxt = new StringBuilder();

                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        string line = lines[lid];

                        if (lid == lines.Count-1 && string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        newTxt.Append(line + "\n");
                    }

                    if (shouldAddClass)
                    {
                        File.WriteAllText(fullPath, newTxt.ToString());
                    }

                }
                List<string> directories = Directory.GetDirectories(path).ToList();
                foreach (string dir in directories)
                {
                    AddDirectoryFiles(dir, data);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message + " " + ee.StackTrace);
            }
        }
    }
}

