using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MemoryTiles
{
    public class User : IXmlSerializable
    {
        public string ProfilePicturePath { get; private set; }
        public string Name { get; private set; }
        public int GamesWon { get; set; }
        public int GamesPlayed { get; set; }

        public User()
        {
            ProfilePicturePath = string.Empty;
            Name = string.Empty;
            GamesWon = 0;
            GamesPlayed = 0;
        }

        public User(string profilePicturePath, string name, int gamesWon, int gamesPlayed)
        {
            ProfilePicturePath = profilePicturePath;
            Name = name;
            GamesWon = gamesWon;
            GamesPlayed = gamesPlayed;
        }

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("User");
            ProfilePicturePath = reader.ReadElementString("ProfilePicturePath");
            Name = reader.ReadElementString("Name");
            GamesWon = int.Parse(reader.ReadElementString("GamesWon"));
            GamesPlayed = int.Parse(reader.ReadElementString("GamesPlayed"));
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("ProfilePicturePath", ProfilePicturePath);
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("GamesWon", GamesWon.ToString());
            writer.WriteElementString("GamesPlayed", GamesPlayed.ToString());
        }

        public void SerializeUser()
        {
            XmlSerializer serializer = new(typeof(User));

            if (!Directory.Exists("users"))
            {
                Directory.CreateDirectory("users");
            }

            using var xmlWriter = XmlWriter.Create($"users/{Name}.xml");
            serializer.Serialize(xmlWriter, this);
        }
    }
}
