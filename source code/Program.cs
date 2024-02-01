using ImGuiNET;
using ClickableTransparentOverlay;
using Un4seen.Bass;
using System.Numerics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MusicPlayer
{
    public class Program : Overlay, IDisposable
    {
        //FILE SETTINGS
        public int streamHandle;
        public int fxHandle;
        public string directoryPath = string.Empty;
        public string selectedFilePath = "/";
        public string[] fileList = Array.Empty<string>();
        public string[] id3v2Tags = Array.Empty<string>();
        //COLOR SETTINGS
        public static float ColorR = 0.17254901960784313f;
        public static float ColorG = 0.0f;
        public static float ColorB = 0.11764705882352941f;
        public static float ColorA = 1.0f;
        public static float White = 1.0f;
        public static Vector4 winColors = new Vector4(0.17254901960784313f, 0.0f, 0.11764705882352941f, 1.0f);
        public static Vector4 txtColors = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public static Vector4 lineColors = new Vector4(0.9137254901960784f, 0.32941176470588235f, 0.12549019607843137f, 1.0f);
        public static Vector4 wgbgcolor = new Vector4(1.0f, 1.0f, 1.0f, 0.1f);
        public static Vector4 framecolor = new Vector4(0f, 0f, 0f, 1.0f);
        //REGULAR SETTINGS
        public float Volume = 1.0f;
        public static float Sence = 80.0f;
        public static float textPos = 37.0f;
        public static float auPos = 37.0f;
        public static float tmp = 90.0f;
        //REVERB SETTINGS
        BASS_DX8_REVERB reverb = new BASS_DX8_REVERB();
        private bool FX = false;
        private bool REVERB_UPDATE = false;
        private bool reverbApplied = false;
        public static float fInGain = 0.0f;
        public static float fReverbMix = 0.0f;
        public static float fReverbtime = 1.0f;
        public static float fHighFreqRTRatio = 0.001f;
        //CHORUS SETTINGS
        BASS_DX8_CHORUS chorus = new BASS_DX8_CHORUS();
        private bool FX2 = false; 
        private bool chorusApplied = false; 
        private bool CHORUS_UPDATE = false;
        public static float fWetDryMix = 50.0f;
        public static float fDepth = 10.0f;
        public static float fFeedback = 25.0f;
        public static float fFrequency = 1.1f;
        public static int lWaveform = 1;
        public static float fDelay = 16.0f;
        //DISPLAY SETTINGS
        public bool showWindow = false;
        public bool showFiles = false;
        public static bool isplayin = false;
        static void Main(string[] args)
        {
            BassNet.Registration("admin@svrecco.com", "2X391417281722");
            Console.Write("Initializing");
            Thread.Sleep(47);
            Console.Write(".");
            Thread.Sleep(45);
            Console.Write(".");
            Thread.Sleep(43);
            Console.WriteLine(".");
            Thread.Sleep(11);
            Console.Write("Drawing Graphics");
            Thread.Sleep(19);
            Console.Write(".");
            Thread.Sleep(53);
            Console.Write(".");
            Thread.Sleep(57);
            Console.WriteLine(".");
            Thread.Sleep(22);
            Console.Write("Loading Functions... (Status: OK)");
            Thread.Sleep(21);
            Console.WriteLine("");
            Console.WriteLine("Initializing...");
            Thread.Sleep(25);
            Console.Write("Loading Music Player...");
            Console.Clear();

            using (Program program = new Program())
            {
                if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                {

                    Console.WriteLine("BASS initialization failed!");
                    program.showWindow = true;
                    program.showFiles = true;
                    return;
                }
                Console.Write("waiting for input...");
                program.Start().Wait();
                Thread logic = new Thread(Logic);
                logic.Start();
            }
        }
        public static void Logic()
        {
        }
        protected override void Render()
        {
            Getstyle();
            DrawSettings();
            DrawDirectory();
            DrawPlayer();
            ImGui.End();
        }
        void DrawSettings()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 5.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4.0f);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(360, 550), ImGuiCond.Appearing);
            ImGui.Begin($"Settings", ImGuiWindowFlags.NoResize);
            ImGui.PushItemWidth(400.0f);
            ImGui.SeparatorText("General");
            ImGui.Text("(VU Sensitivity)");
            ImGui.InputFloat("##AU Sensitivity", ref Sence, 0.0f, 100.0f);
            ImGui.Text("(Debug)");
            ImGui.InputFloat("##Temp Value", ref tmp, -500.0f, 500.0f);
            ImGui.SeparatorText("Sound");
            ImGui.Text("(Volume)");
            ImGui.SliderFloat("##Volume", ref Volume, 0.0f, 1.0f);
            ImGui.SeparatorText("Reverb");
            ImGui.Columns(1);
            ImGui.BeginGroup();
            ImGui.Text("(fInGain)");
            ImGui.SliderFloat("##1", ref fInGain, -96.0f, 0.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Input gain of signal, in decibels (dB)");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fReverbMix)");
            ImGui.SliderFloat("##2", ref fReverbMix, -20.0f, 0.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Reverb mix of signal, in decibels (dB)");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fReverbtime)");
            ImGui.SliderFloat("##3", ref fReverbtime, 0.001f, 2000.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Reverb time, in milliseconds (ms)");
                ImGui.EndTooltip();

            }
            ImGui.Text("(Enable)");
            ImGui.SameLine();
            ImGui.Checkbox("##Enable", ref FX);
            ImGui.EndGroup();
            if (FX)
            {
                if (!reverbApplied)
                {
                    addReverb();
                    Console.WriteLine("");
                    Console.WriteLine("Reverb Enabled");
                    reverbApplied = true;
                    REVERB_UPDATE = true;
                }
            }
            else
            {
                if (reverbApplied)
                {
                    removeReverb();
                    Console.WriteLine("");
                    Console.WriteLine("Reverb Disabled");
                    reverbApplied = false;
                    REVERB_UPDATE = false;
                }
            }
            if (REVERB_UPDATE)
            {
                reverb.fInGain = fInGain;
                reverb.fReverbMix = fReverbMix;
                reverb.fReverbTime = fReverbtime;
                reverb.fHighFreqRTRatio = fHighFreqRTRatio;
                Bass.BASS_FXSetParameters(fxHandle, reverb);
            }

            ImGui.Columns(1);
            ImGui.SeparatorText("Chorus");
            ImGui.BeginGroup();
            ImGui.Text("(fWetDryMix)");
            ImGui.SliderFloat("##4", ref fWetDryMix, 0.0f, 100.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Ratio of wet (processed) signal to dry (unprocessed) signal.");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fDepth)");
            ImGui.SliderFloat("##5", ref fDepth, 0.0f, 100.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Percentage by which the delay time is modulated by the low-frequency oscillator (LFO).");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fFeedback)");
            ImGui.SliderFloat("##6", ref fFeedback, -99.0f, 99.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Percentage of output signal to feed back into the effect's input.");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fFrequency)");
            ImGui.SliderFloat("##7", ref fFrequency, 0f, 10.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Frequency of the LFO.");
                ImGui.EndTooltip();

            }
            ImGui.Text("(lWaveform)");
            ImGui.SliderInt("##8", ref lWaveform, 0, 1);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Waveform of the LFO... 0 = triangle, 1 = sine.");
                ImGui.EndTooltip();

            }
            ImGui.Text("(fDelay)");
            ImGui.SliderFloat("##9", ref fDelay, 0f, 20.0f);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Number of milliseconds the input is delayed before it is played back.");
                ImGui.EndTooltip();

            }
            ImGui.Text("(Enable)");
            ImGui.SameLine();
            ImGui.Checkbox("##Enable2", ref FX2);
            ImGui.EndGroup();
            if (FX2)
            {
                if (!chorusApplied)
                {
                    addChorus();
                    Console.WriteLine("");
                    Console.WriteLine("Chorus Enabled");
                    chorusApplied = true;
                    CHORUS_UPDATE = true;
                }
            }
            else
            {
                if (chorusApplied)
                {
                    removeChorus();
                    Console.WriteLine("");
                    Console.WriteLine("Chorus Disabled");
                    chorusApplied = false;
                    CHORUS_UPDATE = false;
                }
            }
            if (CHORUS_UPDATE)
            {
                chorus.fWetDryMix = fWetDryMix;
                chorus.fDepth = fDepth;
                chorus.fFeedback = fFeedback;
                chorus.fFrequency = fFrequency;
                chorus.lWaveform = lWaveform;
                chorus.fDelay = fDelay;
                Bass.BASS_FXSetParameters(fxHandle, chorus);
            }
            ImGui.Columns(1);
            ImGui.SeparatorText("Appearance");
            ImGui.Text("Themes");
            if (ImGui.Button("Ubuntu"))
            {
                winColors = new Vector4(0.17254901960784313f, 0.0f, 0.11764705882352941f, 1.0f);
                txtColors = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                lineColors = new Vector4(0.9137254901960784f, 0.32941176470588235f, 0.12549019607843137f, 1.0f);
                wgbgcolor = new Vector4(1.0f, 1.0f, 1.0f, 0.1f);
                framecolor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            ImGui.SameLine();
            if (ImGui.Button("Classic Steam"))
            {
                winColors = new Vector4(0.24705882352941178f, 0.2784313725490196f, 0.2196078431372549f, 1.0f);
                txtColors = new Vector4(0.9372549019607843f, 0.9647058823529412f, 0.9333333333333333f, 1.0f);
                lineColors = new Vector4(0.5882352941176471f, 0.5294117647058824f, 0.19607843137254902f, 1.0f);
                wgbgcolor = new Vector4(0.2980392156862745f, 0.34509803921568627f, 0.26666666666666666f, 1.0f);
                framecolor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            ImGui.SameLine();
            if (ImGui.Button("Discord"))
            {
                winColors = new Vector4(0.21176470588235294f, 0.2235294117647059f, 0.24313725490196078f, 1.0f);
                txtColors = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                lineColors = new Vector4(0.4470588235294118f, 0.5372549019607843f, 0.8549019607843137f, 1.0f);
                wgbgcolor = new Vector4(0.25882352941176473f, 0.27058823529411763f, 0.28627450980392155f, 1.0f);
                framecolor = new Vector4(0.11764705882352941f, 0.12941176470588237f, 0.1411764705882353f, 1.0f);
            }
            ImGui.Columns(2);
            ImGui.BeginGroup();
            ImGui.Text("Window Bg Color");
            ImGui.PushItemWidth(-1.0f);
            ImGui.ColorPicker4("##RBGA0", ref winColors, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel);
            ImGui.EndGroup();
            ImGui.NextColumn();
            ImGui.BeginGroup();
            ImGui.Text("Text Color");
            ImGui.PushItemWidth(-1.0f);
            ImGui.ColorPicker4("##RBGA1", ref txtColors, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel);
            ImGui.EndGroup();
            ImGui.NextColumn();
            ImGui.Columns(2);
            ImGui.BeginGroup();
            ImGui.Text("Feedback Color");
            ImGui.PushItemWidth(-1.0f);
            ImGui.ColorPicker4("##RBGA2", ref lineColors, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel);
            ImGui.EndGroup();
            ImGui.NextColumn();
            ImGui.Separator();
            ImGui.BeginGroup();
            ImGui.Text("Slider Bg Color");
            ImGui.PushItemWidth(-1.0f);
            ImGui.ColorPicker4("##RBGA3", ref wgbgcolor, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel);
            ImGui.EndGroup();
        }
        void DrawDirectory()
        {
            string[] fileNames = GetFileNamesFromPaths(fileList);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 5.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5.0f);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(380, 254), ImGuiCond.Appearing);
            ImGui.Begin("Directory", ImGuiWindowFlags.NoResize);
            ImGui.SeparatorText($"({directoryPath})");
            ImGui.PushItemWidth(400.0f);
            ImGui.BeginListBox("");
            for (int i = 0; i < fileNames.Length; i++)
            {
                bool isSelected = ImGui.Selectable(fileNames[i]);
                if (isSelected)
                {
                    string selectedPath = fileList[i];
                    if (Directory.Exists(selectedPath))
                    {
                        directoryPath = selectedPath;
                        fileList = GetFilesInDirectory(directoryPath);
                        fileNames = GetFileNamesFromPaths(fileList);
                    }
                    else
                    {
                        selectedFilePath = selectedPath;
                        FX = false;
                        REVERB_UPDATE = false;
                        Bass.BASS_ChannelPause(streamHandle);
                        Bass.BASS_ChannelSetPosition(streamHandle, 0);
                        LoadAndPlaySongTrack(selectedFilePath);
                    }
                }
            }
            ImGui.EndListBox();
            ImGui.InputTextWithHint("", "D:/", ref directoryPath, 256);
            if (ImGui.IsKeyDown(ImGuiKey.Enter))
            {
                fileList = GetFilesInDirectory(directoryPath);
                fileNames = GetFileNamesFromPaths(fileList);
            }
        }
        void DrawPlayer()
        {
            string selectedFileName = Path.GetFileName(selectedFilePath);
            var ProgressBar_v = new Vector2(0, 2);
            int deviceNum = Bass.BASS_ChannelGetDevice(streamHandle);
            Bass.BASS_ChannelSetAttribute(streamHandle, BASSAttribute.BASS_ATTRIB_VOL, Volume);
            long currentpos = Bass.BASS_ChannelGetPosition(streamHandle);
            long fileLength = Bass.BASS_ChannelGetLength(streamHandle);
            double time = Bass.BASS_ChannelBytes2Seconds(streamHandle, currentpos);
            Bass.BASS_ChannelGetTagsID3V2_2(streamHandle);
            float positionPercentage = (float)currentpos / fileLength;
            double totalT = Convert.ToDouble(fileLength);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 5.0f);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(224, 54), ImGuiCond.Appearing);
            ImGui.Begin($"Music Player", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Columns(1);
            ImGui.Separator();
            int currentVol = Bass.BASS_ChannelGetLevel(streamHandle);
            float normalizedVol = currentVol / 32768.0f;
            float dB = 20.0f * (float)Math.Log10(normalizedVol);
            float[] dBValues = { dB };
            ImGui.PlotHistogram("", ref dBValues[0], dBValues.Length, 0, "", Sence, 97.0f, new Vector2(20, 80));
            ImGui.SameLine();
            ImGui.BeginGroup();
            ImGui.SeparatorText($"Track {deviceNum} ({time:F1})");
            ImGui.SetCursorPosY(52.0f);
            ImGui.Text($"{selectedFileName}");
            ImGui.SetCursorPosY(73.0f);
            if (ImGui.ArrowButton("Play", ImGuiDir.Right))
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Bass.BASS_ChannelPlay(streamHandle, false);
                Console.WriteLine("    _   __                       __            _            \r\n   / | / /___ _      __   ____  / /___ ___  __(_)___  ____ _\r\n  /  |/ / __ \\ | /| / /  / __ \\/ / __ `/ / / / / __ \\/ __ `/\r\n / /|  / /_/ / |/ |/ /  / /_/ / / /_/ / /_/ / / / / / /_/ / \r\n/_/ |_/\\____/|__/|__/  / .___/_/\\__,_/\\__, /_/_/ /_/\\__, /  \r\n                      /_/            /____/        /____/   \r\n\r\n");
                Console.WriteLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                id3v2Tags = Bass.BASS_ChannelGetTagsID3V2(streamHandle);
                if (id3v2Tags != null && id3v2Tags.Length > 0)
                {
                    foreach (string tag in id3v2Tags)
                    {
                        string[] parts = tag.Split('=');
                        if (parts.Length > 1)
                        {
                            string value = parts[1].Trim();
                            Console.WriteLine($"/// {value} ///");

                        }
                    }
                }
                else
                {
                    Console.WriteLine("No ID3v2 tags found for this song.");
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Pause"))
            {
                Bass.BASS_ChannelPause(streamHandle);
            }
            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                Bass.BASS_ChannelPause(streamHandle);
                Bass.BASS_ChannelSetPosition(streamHandle, 0);
            }
            ImGui.PushItemWidth(260.0f);
            ImGui.SetCursorPosY(102.0f);
            ImGui.ProgressBar(positionPercentage, ProgressBar_v);
            ImGui.EndGroup();
            ImGui.Separator();
        }
        public void LoadAndPlaySongTrack(string filePath)
        {
            streamHandle = Bass.BASS_StreamCreateFile(filePath, 0, 0, BASSFlag.BASS_MUSIC_FX);
        }
        public void addReverb()
        {
                fxHandle = Bass.BASS_ChannelSetFX(streamHandle, BASSFXType.BASS_FX_DX8_REVERB, 0);
                reverb.fInGain = fInGain;
                reverb.fReverbMix = fReverbMix;
                reverb.fReverbTime = fReverbtime;
                reverb.fHighFreqRTRatio = fHighFreqRTRatio;
                Bass.BASS_FXSetParameters(fxHandle, reverb);
        }
        public void addChorus()
        {
            fxHandle = Bass.BASS_ChannelSetFX(streamHandle, BASSFXType.BASS_FX_DX8_CHORUS, 1);
            chorus.fWetDryMix = fWetDryMix;
            chorus.fDepth = fDepth;
            chorus.fFeedback = fFeedback;
            chorus.fFrequency = fFrequency;
            chorus.lWaveform = lWaveform;
            chorus.fDelay = fDelay;
            chorus.lPhase = BASSFXPhase.BASS_FX_PHASE_180;
            Bass.BASS_FXSetParameters(fxHandle, chorus);
        }
        public void removeReverb()
        {
            Bass.BASS_ChannelRemoveFX(streamHandle, fxHandle);
        }
        public void removeChorus() 
        {
            Bass.BASS_ChannelRemoveFX(streamHandle, fxHandle);
        }
        private string[] GetFilesInDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    string[] directories = Directory.GetDirectories(path);
                    fileList = files.Concat(directories).ToArray();
                    return fileList;
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return Array.Empty<string>();
        }
        private static string[] GetFileNamesFromPaths(string[] filePaths)
        {
            return filePaths.Select(Path.GetFileName).ToArray();
        }
        static void Getstyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            style.Colors[(int)ImGuiCol.Text] = txtColors;
            style.Colors[(int)ImGuiCol.PlotHistogram] = lineColors;
            style.Colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.1f);
            style.Colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(0.9686274509803922f, 0.7098039215686275f, 0.2196078431372549f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.5f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = winColors;
            style.Colors[(int)ImGuiCol.TitleBg] = framecolor;
            style.Colors[(int)ImGuiCol.FrameBg] = wgbgcolor;
            style.Colors[(int)ImGuiCol.TitleBgActive] = framecolor;
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.7f);
            style.Colors[(int)ImGuiCol.TabActive] = framecolor;
            style.Colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.3f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.2f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.7f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.1f);
        }
    }
}