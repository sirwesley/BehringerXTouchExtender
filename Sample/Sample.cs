using BehringerXTouchExtender;
using CSCore.CoreAudioAPI;
using KoKo.Property;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Tracing;
using System.Text.Json.Nodes;
using Timer = System.Timers.Timer;

using IBehringerXTouchExtenderControlSurface<IAbsoluteRotaryEncoder> device = BehringerXTouchExtenderControlSurface.CreateWithAbsoluteMode();

Console.WriteLine("Connecting to Behringer X-Touch Extender...");
device.Open();
Console.WriteLine("Connected.");

using MMDeviceEnumerator mmDeviceEnumerator = new();
using MMDevice mmDevice = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
using AudioMeterInformation audioMeterInformation = AudioMeterInformation.FromDevice(mmDevice);
int audioChannelCount = audioMeterInformation.MeteringChannelCount;

int vuMeterLightCount = device.GetVuMeter(1).LightCount;
ManuallyRecalculatedProperty<int[]> audioPeaks = new(() =>
{
    float[] peaks = audioMeterInformation.GetChannelsPeakValues(audioChannelCount);
    int[] ledPositions = new int[audioChannelCount];
    for (int i = 0; i < peaks.Length; i++)
    {
        ledPositions[i] = (int)Math.Round(peaks[i] * vuMeterLightCount);
    }

    return ledPositions;
});

/**
 * Loads the scribble.config.json file to initialize the scribble strips.
 **/ 
void LoadJson()
{
    using (StreamReader r = new StreamReader("scribble.config.json"))
    {
        string json = r.ReadToEnd();
        if (json is not null)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var dynamicObject = JsonConvert.DeserializeObject<dynamic>(json)!;


            if (dynamicObject != null)
            {
                foreach (var item in dynamicObject)
                {
                    int trackId = item.stripIndex;
                    device.GetScribbleStrip(trackId).TopText.Connect($"{item.topText}");
                    device.GetScribbleStrip(trackId).BottomText.Connect($"{item.bottomText}");
                    device.GetScribbleStrip(trackId).TopTextColor.Connect(item.topContrast == "dark" ? ScribbleStripTextColor.Dark : ScribbleStripTextColor.Light);
                    device.GetScribbleStrip(trackId).BottomTextColor.Connect(item.bottomContrast == "dark" ? ScribbleStripTextColor.Dark : ScribbleStripTextColor.Light);

                    switch ((string)(item.backgroundColor))
                    {
                        case "black":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.White);
                            //We can just invert if White Contrast if 'black' is selected. 
                            device.GetScribbleStrip(trackId).TopTextColor.Connect(item.topContrast == "light" ? ScribbleStripTextColor.Dark : ScribbleStripTextColor.Light);
                            device.GetScribbleStrip(trackId).BottomTextColor.Connect(item.bottomContrast == "light" ? ScribbleStripTextColor.Dark : ScribbleStripTextColor.Light);
                            break;
                        case "white":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.White);
                            break;
                        case "red":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Red);
                            break;
                        case "green":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Green);
                            break;
                        case "blue":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Blue);
                            break;
                        case "magenta":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Magenta);
                            break;
                        case "yellow":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Yellow);
                            break;
                        case "cyan":
                            device.GetScribbleStrip(trackId).BackgroundColor.Connect(ScribbleStripBackgroundColor.Cyan);
                            break;

                    }
                }
            }
            
        }
       
    }
}


/**
 * Processes the Button Press
 * Transitions between <ON, BLINKING, OFF>
 */
void processButton(IIlluminatedButton button, int trackId)
{
    StoredProperty<IlluminatedButtonState> state = new();


    button.IlluminationState.Connect(state);
    button.IsPressed.PropertyChanged += (_, eventArgs) =>
    {
        if (eventArgs.NewValue)
        {

            state.Value = state.Value switch
            {
                IlluminatedButtonState.Off => IlluminatedButtonState.On,
                IlluminatedButtonState.On => IlluminatedButtonState.Blinking,
                IlluminatedButtonState.Blinking => IlluminatedButtonState.Off,
                _ => throw new NotImplementedException()
            };

            string typeString = "MUTE ";
            switch (button.ButtonType)
            {
                case IlluminatedButtonType.Mute:
                    typeString = "MUTE ";
                    break;
                case IlluminatedButtonType.Select:
                    typeString = "SEL  ";
                    break;
                case IlluminatedButtonType.Record:
                    typeString = "REC  ";
                    break;
                case IlluminatedButtonType.Solo:
                    typeString = "SOLO ";
                    break;
            }
            string stateString = (state.Value == IlluminatedButtonState.Blinking) ? "<>" : state.Value.ToString() == "Off" ? "--" : "ON";
            device.GetScribbleStrip(trackId).BottomText.Connect($"{typeString}{stateString}");
        }
    };
}



LoadJson();


for (int i = 0; i < device.TrackCount; i++)
{
    int trackId = i; //create closure so it doesn't change between when a callback is defined and executed

    IAbsoluteRotaryEncoder rotaryEncoder = device.GetRotaryEncoder(trackId);
    StoredProperty<int> rotaryEncoderLightPosition = new(trackId);
    rotaryEncoder.LightPosition.Connect(rotaryEncoderLightPosition);
    rotaryEncoder.RotationPosition.PropertyChanged += (_, rotationArgs) =>
    {
        //rotaryEncoderLightPosition.Value = Math.Max(Math.Min(rotaryEncoderLightPosition.Value + (rotationArgs.IsClockwise ? 1 : -1), rotaryEncoder.LightCount - 1), 0);
        Console.WriteLine($"User moved rotary {trackId + 1} to position {rotationArgs.NewValue:P0}");
        device.GetScribbleStrip(trackId).BottomText.Connect($"RT:{rotationArgs.NewValue:P0}");
    };

    rotaryEncoder.IsPressed.PropertyChanged += (_, eventArgs) =>
    {
        device.GetScribbleStrip(trackId).BottomText.Connect($"RT:{(eventArgs.NewValue ? "ON" : "OFF")}");
    };



    int audioChannel = trackId * audioChannelCount / device.TrackCount; //integer truncation is desired here
    device.GetVuMeter(trackId).LightPosition.Connect(DerivedProperty<int>.Create(audioPeaks, peaks => peaks[audioChannel]));

    IIlluminatedButton muteButton = device.GetMuteButton(trackId);
    IIlluminatedButton recordButton = device.GetRecordButton(trackId);
    IIlluminatedButton soloButton = device.GetSoloButton(trackId);
    IIlluminatedButton selectButton = device.GetSelectButton(trackId);

    processButton(muteButton, trackId);
    processButton(recordButton, trackId);
    processButton(soloButton, trackId);
    processButton(selectButton, trackId);

    IFader fader = device.GetFader(trackId);
    fader.IsPressed.PropertyChanged += (_, eventArgs) =>
    {
        if (eventArgs.NewValue)
        {
            Console.WriteLine($"User is touching Fader {trackId}");
            device.GetScribbleStrip(trackId).BottomText.Connect($"FD:{(eventArgs.NewValue ? "ON" : "OFF")}");
        }
    };
    fader.ActualPosition.PropertyChanged += (_, eventArgs) =>
    {
        Console.WriteLine($"User moved fader {trackId + 1} to position {eventArgs.NewValue:P0}");
        device.GetScribbleStrip(trackId).BottomText.Connect(new StoredProperty<string>($"FD:{eventArgs.NewValue:P0}"));
    };

    //device.GetScribbleStrip(trackId).TopText.Connect($"Track {trackId + 1}");
    //device.GetScribbleStrip(trackId).TopTextColor.Connect(ScribbleStripTextColor.Dark);
    //device.GetScribbleStrip(trackId).BottomTextColor.Connect(ScribbleStripTextColor.Light);

    //Property<ScribbleStripBackgroundColor> stripColor = trackId == 0 ? new StoredProperty<ScribbleStripBackgroundColor>(ScribbleStripBackgroundColor.White) : new StoredProperty<ScribbleStripBackgroundColor>((ScribbleStripBackgroundColor)trackId);
    //device.GetScribbleStrip(trackId).BackgroundColor
    //   .Connect(stripColor); //avoid black background because it makes text illegible*/

    fader.DesiredPosition.Connect(trackId / (device.TrackCount - 1.0));
}

const int audioPeakFps = 15;
Timer audioPeakTimer = new(TimeSpan.FromSeconds(1.0 / audioPeakFps).TotalMilliseconds);
audioPeakTimer.Elapsed += (_, _) => audioPeaks.Recalculate();
audioPeakTimer.Start();

Console.WriteLine("Press any key to exit.");
Console.ReadKey();