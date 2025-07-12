using System.Collections.Generic;
using Godot;

public partial class BuffManager : Node
{
    public static BuffManager Instance { get; } = new BuffManager();

    public readonly Dictionary<ElementType, float> ElementalDamageBuffs = new()
    {
        { ElementType.Neutral, 1.0f },
        { ElementType.Fire, 1.05f },
        { ElementType.Ice, 1.10f },
        { ElementType.Poison, 1.20f },
        { ElementType.Electric, 1.06f }
    };

    public float GetElementalBuff(ElementType elementType)
    {
        return ElementalDamageBuffs.TryGetValue(elementType, out var buff) ? buff : 1f;
    }
    public void AddElementalBuff(ElementType elementType, float addBuff)
    {
        if (ElementalDamageBuffs.ContainsKey(elementType))
        {
            ElementalDamageBuffs[elementType] += addBuff;
        }
    }
}
