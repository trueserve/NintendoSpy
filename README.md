NintendoSpy
======

#### [Download the latest NintendoSpy release here.](https://github.com/trueserve/NintendoSpy/releases/latest) (x64 Windows binary)

This project provides a general solution for displaying controller inputs. NintendoSpy viewer software can be used as a display while speedrunning, or for use in recording tutorials on how to perform tricks. The Windows software supports NES, SNES, Nintendo 64, and GameCube controller signals from an Arduino, as well as any standard gamepad or XBOX 360 controller connected to your PC. This project also supports **true's GrIPdaptor** with a controller extension retrofit and passive listener firmware. XBOX 360 controllers are supported with a skin out of the box.

NintendoSpy supports custom skins using a straight-forward, XML-based skin format. You can also bind controller input combinations to trigger keyboard presses, which can be useful for hitting checkpoints on your splits. If you create your own skins, feel free to submit them as pull requests to this repository.

## Documentation

### Wiring and hardware

The general design of NintendoSpy involves reading joystick data from a DirectInput device on Windows. It can also read data using a poorly designed protocol on an Arduino. For the latter method, you just need to install the Arduino firmware packaged in the NintendoSpy release and run the viewer software.  For more in-depth tutorials on how to do this, check out some of the links below.

![](https://github.com/jeremyaburns/NintendoSpy/raw/master/docs/tutorial-images/wiring-all.png)

[EvilAsh25's SNES hardware building guide](https://github.com/jaburns/NintendoSpy/blob/master/docs/guide-evilash25.md)

[Gamecube hardware tutorial](https://github.com/jaburns/NintendoSpy/blob/master/docs/tutorial-gamecube.md)

[N64 hardware tutorial](https://github.com/jaburns/NintendoSpy/blob/master/docs/tutorial-n64.md)

### Using the viewer software

Once you've unzipped the NintendoSpy release, run NintendoSpy.exe to open the controller viewer.  You'll be greeted by the input source configuration screen, which is fairly straightforward to configure. First select the controller type you are set up to view. For dinput or xinput, select the controller index (for dinput, guess; for xinput, usually the controller will indicate which controller it is, 1-4). For serial, select the serial port of your Arduino adapter.

![](https://github.com/jeremyaburns/NintendoSpy/raw/master/docs/tutorial-images/interface.png)

Once you've selected the input source, choose a skin. Each skin can have multiple backgrounds which are generally used to provide various colors for the controller itself. After choosing, press ``Go!`` and you should see the viewer screen.

### Creating your own skins

Each skin consists of a subfolder in the "skins" directory, which is expected to contain a file called ``skin.xml`` along with all the PNG image assets required by the skin.  The easiest way to create a skin for your target console is probably just to copy+paste the default skin and modify it according to your needs.  The following is documentation of the skin.xml format for reference if you'd like to create more complex skins.

The root node of the ``skin.xml`` file must define the following 3 attributes:
```
<skin
    name="Default PC 360" # This is the name of the skin as it will appear in the selection list.
    author="jaburns"      # Your name or handle.
    type="pc360">         # The input type this skin is used for.
```
Valid values for the ``type`` attribute are as follows: ``nes``, ``snes``, ``n64``, ``gamecube``, ``pc360``, ``generic``. 

Each skin must define at least one ``<background>`` element.  Each background entry will be listed in the skin selector as a separate entry.  Every background for your skin must have the same dimensions.
```
<background
    name="Default"     # The name which will appear in the selection list.
    image="pad.png" /> # The PNG file to use for this background selection.
```
The rest of the ``skin.xml`` file defines how to render button and analog inputs.  Each type exposes a variety of buttons and analog values.  Button inputs are mapped to images at specific locations using the ``<button>`` tag, and there is a small variety of possible ways to map analog inputs.

```
<button
    name="up"          # Name of the source input button to map this image to.
    image="circle.png" # Image file to display when this input is pressed.
    x="101" y="71"     # Location in pixels where the top/left corner of the image should sit.
    width="16"         # Width and height specification can OPTIONALLY be used to scale
    height="16" />     #   an image to a specific size.  The default size is the orignal image size.
```
Analog values can be mapping as sticks, ranges, or range-based buttons.  Ranges are used for things like analog shoulder buttons and render by filling an image by the amount the range is pressed.  Range buttons are useful for creating a button-like display when an analog value is in a certain ranage.  An example of this use case is if you are streaming a SNES game, but playing using a 360 controller, you can bind the analog stick to appear as if you are pressing the d-pad buttons.
```
<stick
    xname="lstick_x"  # Analogs values to bind the image's x 
    yname="lstick_y"  #   and y displacements to.
    image="stick.png" # Image file to use for the stick.
    x="53" y="31"     # Location in pixels where the top/left corner of the image should sit. 
    width="34"        # Width and height specification can OPTIONALLY be used to scale 
    height="35"       #   an image to a specific size.  The default size is the orignal image size.
    xrange="9"        # xrange and yrange specify how much to move the stick image in either axis
    yrange="9"        #   when the stick is deplaced.
    xreverse="false"  # Settings xreverse or yreverse to true will reverse the direction
    yreverse="true"   #   that the stick moves along that axis
    />     
```
```
<analog
    name="trig_l"      # Analog value to bind the display to.
    image="trig-l.png" # Image file to mask over the background as the input changes.
    x="15" y="18"      # Location in pixels where the top/left corner of the image should sit. 
    direction="up"     # The direction to sweep the image.
                         Valid options are 'up', 'down', 'left', 'right'.
    reverse="true"     # 'true' or 'false'. Setting to true will cause the image to clear instead
                         of fill as the analog value is further engaged.
    usenegative="true" # 'true' or 'false'. If set to true, then the image will change when the analog
                         value ranges from 0 to -1 instead of 0 to 1.  Useful for generic gamepad skins
                         who use a single axis for analog L/R buttons.
    />
```
```
<rangebutton
    name="lstick_x"    # Analog value to bind the display to.
    image="d-left.png" # Image file to mask over the background as the input changes.
    x="15" y="18"      # Location in pixels where the top/left corner of the image should sit. 
    from="-1.0"        # From and to attributes specify the range which the specified analog
    to="-0.5"          #   input must be in to display the image.
/>
```

### Binding controller inputs to keyboard key presses

Binding controller inputs to keyboard key presses is achieved by placing ``<binding>`` definitions
in the ``keybindings.xml`` file.  The ``output-key`` attribute on the binding specifies which keyboard key to press when the provided gamepad buttons are pressed.  Values of ``output-key`` can simply be letters, or [see here other for valid key bindings (the text in red are the acceptable values for ``output-key``)](https://github.com/jaburns/NintendoSpy/blob/master/Keybindings.cs#L110).  Each binding must contain at least one child ``<input>`` element.  The input elements specify which buttons on the controller must be depressed in order to send the key press signal.  See below for an example ``keybindings.xml`` file which makes pressing L and R together trigger a ``home`` key press on the keyboard.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<keybindings>
    <binding output-key="home">
        <input button="r" />
        <input button="l" />
    </binding>
</keybindings>
