using System.Runtime.InteropServices;

namespace LibModPlugSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LibModPlugNote
    {
        public byte Note;
        public byte Instrument;
        public byte VolumeEffect;
        public byte Effect;
        public byte Volume;
        public byte Parameter;
    }
}
