using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GodotAddinVS.Debugging.Variants
{
    public class GodotVariantEncoder
    {
        private readonly List<byte> _buffer = new List<byte>();

        public int Length => _buffer.Count;

        public byte[] ToArray() => _buffer.ToArray();

        public void AddBytes(params byte[] bytes) =>
            _buffer.AddRange(bytes);

        public void AddInt(int value) =>
            AddBytes(BitConverter.GetBytes(value));

        public void AddType(GodotVariant.Type type) =>
            AddInt((int) type);

        public void AddString(string value)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(value);

            AddType(GodotVariant.Type.String);
            AddInt(utf8Bytes.Length);
            AddBytes(utf8Bytes);
            AddBytes(0); // Godot's UTF8 converter adds a trailing whitespace

            while (_buffer.Count % 4 != 0)
                _buffer.Add(0);
        }

        public void AddArray(List<GodotVariant> array)
        {
            AddType(GodotVariant.Type.Array);
            AddInt(array.Count);

            foreach (var element in array)
            {
                if (element.VariantType == GodotVariant.Type.String)
                    AddString(element.Get<string>());
                else
                    throw new NotImplementedException();
            }
        }

        public static void Encode(GodotVariant variant, Stream stream)
        {
            using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                var encoder = new GodotVariantEncoder();
                switch (variant.VariantType)
                {
                    case GodotVariant.Type.String:
                        encoder.AddString((string) variant.Value);
                        break;
                    case GodotVariant.Type.Array:
                        encoder.AddArray((List<GodotVariant>) variant.Value);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // ReSharper disable once RedundantCast
                writer.Write((int) encoder.Length);
                writer.Write(encoder.ToArray());
            }
        }
    }
}