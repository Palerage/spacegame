using System;
using System.Collections.Generic;
using Godot;

public partial class BuffStatsDisplay : VBoxContainer
{
    [Export] public NodePath PlayerPath;

    private Player _player;
    private Dictionary<ElementType, Label> _buffLabels = new();

    private Label _activeElementLabel; // Label för ActiveElementType

    public override void _Ready()
    {
        _player = GetNode<Player>(PlayerPath);

        // Skapa label för ActiveElementType
        _activeElementLabel = new Label();
        AddChild(_activeElementLabel);

        // Skapa labels för alla element
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            Label label = new Label();
            label.Text = $"{element}: 0%";
            AddChild(label);
            _buffLabels[element] = label;
        }
    }

    public override void _Process(double delta)
    {
        UpdateBuffLabels();
        UpdateActiveElementLabel();
    }

    private void UpdateBuffLabels()
    {
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float buffValue = BuffManager.Instance.GetElementalBuff(element);
            float percentage = (buffValue - 1f) * 100f; // Om 1f = 0%, 1.2f = 20% etc
            string sign = percentage >= 0 ? "+" : "";
            _buffLabels[element].Text = $"{element}: {sign}{percentage:F0}%";
        }
    }

    private void UpdateActiveElementLabel()
    {
        _activeElementLabel.Text = $"{_player.ActiveElementType}";
    }
}
