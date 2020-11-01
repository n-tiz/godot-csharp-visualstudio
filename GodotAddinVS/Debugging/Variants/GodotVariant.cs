namespace GodotAddinVS.Debugging.Variants
{
    // Incomplete implementation of the Godot's Variant encoder. Add missing parts as needed.

    public class GodotVariant
    {
        public enum Type
        {
            Nil = 0,
            Bool = 1,
            Int = 2,
            Real = 3,
            String = 4,
            Vector2 = 5,
            Rect2 = 6,
            Vector3 = 7,
            Transform2d = 8,
            Quat = 10,
            Aabb = 11,
            Basis = 12,
            Transform = 13,
            Color = 14,
            NodePath = 15,
            Rid = 16,
            Object = 17,
            Dictionary = 18,
            Array = 19,
            RawArray = 20,
            IntArray = 21,
            RealArray = 22,
            StringArray = 23,
            Vector2Array = 24,
            Vector3Array = 25,
            ColorArray = 26,
            Max = 27
        }

        public object Value { get; }
        public Type VariantType { get; }

        public T Get<T>()
        {
            return (T) Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public GodotVariant(Type type, object value)
        {
            Value = value;
            VariantType = type;
        }
    }
}
