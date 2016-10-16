using NintendoSpy.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintendoSpy
{
    public class InputSource
    {
        static public readonly InputSource NES = new InputSource ("nes", "[serial] NES", 0x01, port => new SerialControllerReader (port, SuperNESandNES.ReadFromPacket_NES));
        static public readonly InputSource SNES = new InputSource ("snes", "[serial] Super NES", 0x01, port => new SerialControllerReader (port, SuperNESandNES.ReadFromPacket_SNES));
        static public readonly InputSource N64 = new InputSource ("n64", "[serial] Nintendo 64", 0x01, port => new SerialControllerReader (port, Nintendo64.ReadFromPacket));
        static public readonly InputSource GAMECUBE = new InputSource ("gamecube", "[serial] GameCube", 0x01, port => new SerialControllerReader (port, GameCube.ReadFromPacket));
        static public readonly InputSource PC360 = new InputSource ("pc360", "[xinput] PC 360", 0x02, port => new XInputReader (port));
        static public readonly InputSource PAD = new InputSource ("generic", "[dinput] PC Gamepad", 0x04, port => new GamepadReader (port));
        

        static public readonly IReadOnlyList <InputSource> ALL = new List <InputSource> {
            PAD, PC360, NES, SNES, N64, GAMECUBE
        };

        static public readonly InputSource DEFAULT = PAD;

        public string TypeTag { get; private set; }
        public string Name { get; private set; }
        public int DevicePortType { get; private set; }

        public Func <string, IControllerReader> BuildReader { get; private set; }

        InputSource (string typeTag, string name, int devicePort, Func <string, IControllerReader> buildReader) {
            TypeTag = typeTag;
            Name = name;
            DevicePortType = devicePort;
            BuildReader = buildReader;
        }
    }
}
