using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MLAudioManager
{
    class Track
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Name { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string key = "";
            Console.WriteLine("Carpeta base");
            string folderBase = Console.ReadLine();

            while (key.ToLower() != "n")
            {
                List<Track> tracks = new List<Track>();
                
                Console.WriteLine("Nombre del archivo mp3 (sin extensión)");

                string name = Console.ReadLine();
                string mp3Path = Path.Combine(folderBase, name + ".mp3");
                string dataPath = Path.Combine(folderBase, name + ".txt");

                Console.WriteLine("Es un enganchado? y/n (Si no es enganchado se corta 2 segundos antes del inicio del siguiente track)");
                bool mix = (Console.ReadLine().ToLower() == "y");

                if (File.Exists(mp3Path) == false || File.Exists(dataPath) == false)
                    Console.WriteLine("Archivo mp3 o txt con lista de temas no encontrado");
                else
                {
                    using (var fileStream = File.OpenRead(dataPath))
                    {
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
                        {
                            String line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                string time = line.Split(' ')[0];
                                string trackName = line.Replace(line.Split(' ')[0], "");

                                int start = int.Parse(time.Split(':')[0]) * 60 + int.Parse(time.Split(':')[1]);

                                tracks.Add(new Track() { Start = start, Name = trackName });
                            }
                        }
                    }

                    ProcessList(mp3Path, name, tracks, mix);

                    Console.Write("Terminado... Procesar otro archivo? y/n: ");
                    key = Console.ReadLine();
                }
            }
        }

        private static void ProcessList(string mp3Path, string name, List<Track> tracks, bool mix)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Track item = tracks[i];

                if (i < (tracks.Count - 1))
                    item.End = (mix ? tracks[i + 1].Start : tracks[i + 1].Start - 2);
                else
                    item.End = 999999999999;

                Split(mp3Path, name,(i + 1).ToString("D2") + "-" + item.Name.Trim() + ".mp3", item.End, item.Start);
            }
        }

        private static void Split(string mp3Path, string name, string cutFileFilename, double endSecond, double startSecond = 0)
        {
            Console.WriteLine(string.Format("Creando {0}. Desde {1} a {2}", cutFileFilename, (startSecond / 60).ToString("N2"), (endSecond / 60).ToString("N2")));

            var mp3Dir = Path.GetDirectoryName(mp3Path);
            var mp3File = Path.GetFileName(mp3Path);
            var splitFile = Path.Combine(mp3Dir, "musica", name, cutFileFilename);

            if (Directory.Exists(Path.Combine(mp3Dir, "musica", name)) == false)
                Directory.CreateDirectory(Path.Combine(mp3Dir, "musica", name));

            if (File.Exists(splitFile))
                File.Delete(splitFile);

            using (var reader = new Mp3FileReader(mp3Path))
            {
                FileStream writer = File.Create(splitFile);

                Mp3Frame mp3Frame = reader.ReadNextFrame();

                while (mp3Frame != null)
                {
                    if (reader.CurrentTime.TotalSeconds >= startSecond && (reader.CurrentTime.TotalSeconds <= endSecond || endSecond == 0))
                    {
                        writer.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                    }

                    mp3Frame = reader.ReadNextFrame();
                }

                if (writer != null) writer.Dispose();
            }
        }
    }

}
