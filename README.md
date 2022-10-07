# BehringerXTouchExtender
.NET MIDI controller client for Behringer X-Touch Extender audio control surface

Ability to write Text and Colors to Scribble Strips. 

Let's a user configure the Scribble Strip on the Behringer X-Touch Extender with the corresponding scribble.config.json file example below.

## Sample Project
Detects audio and displays channels on VU Meters.  
Displays Colored Scribble Strips with "Track <1-8>".  
Feedback displayed in console when buttons pressed. 

Note: the hardware must be in Ctrl or CtrlRel mode (See users manual)  
_hint: with unit off, hold left-most 'select' and power-on. use left-most rotary to select mode._

### scribble.config.json

Customize this file within the sample project to set the text and colors.

```
[
  {
    "stripIndex":  0,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "light",
    "bottomContrast": "light",
    "backgroundColor": "black"
  },
  {
    "stripIndex": 1,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "light",
    "bottomContrast": "light",
    "backgroundColor": "green"
  },
  {
    "stripIndex": 2,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "blue"
  },
  {
    "stripIndex": 3,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "white"
  },
  {
    "stripIndex": 4,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "magenta"
  },
  {
    "stripIndex": 5,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "cyan"
  },
  {
    "stripIndex": 6,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "yellow"
  },
  {
    "stripIndex": 7,
    "topText": "Hello",
    "bottomText": "World",
    "topContrast": "dark",
    "bottomContrast": "light",
    "backgroundColor": "red"
  }
] 
```
