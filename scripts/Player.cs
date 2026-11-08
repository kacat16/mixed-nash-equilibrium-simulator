using Godot;

public partial class GamePlayer : CharacterBody2D
{
	public int PlayerTypeIndex { get; set; }
	public int PlayerIndex { get; set; }
	public int Strategy { get; set; }
	public float Speed { get; set; } = 150.0f;
	public float AccumulatedPayoff { get; set; } = 0.0f;

	private Color _playerColor = Colors.White;

	public override void _Ready()
	{
		var circle = new CircleShape2D { Radius = 8.0f };
		GetNode<CollisionShape2D>("CollisionShape2D").Shape = circle;
		Modulate = _playerColor;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GD.Randf() < 0.02f)
		{
			Velocity = new Vector2(
				GD.Randf() * 2 - 1,
				GD.Randf() * 2 - 1
			) * Speed;
		}

		Velocity = Velocity.Lerp(Vector2.Zero, (float)delta * Speed);
		MoveAndSlide();
	}

	public void SetStrategy(int strategyIndex)
	{
		Strategy = strategyIndex;
	}

	public float GetAccumulatedPayoff()
	{
		return AccumulatedPayoff;
	}

	public void ResetPayoff()
	{
		AccumulatedPayoff = 0.0f;
		UpdateVisualColor();
	}

	public void SetPlayerColor(Color color)
	{
		_playerColor = color;
		Modulate = color;
	}

	public void UpdateVisualColor()
	{
		float hue = Mathf.Clamp(AccumulatedPayoff / 100.0f, 0.0f, 1.0f);
		_playerColor = Color.FromHsv(hue * 0.3f + 0.5f, 0.8f, 0.9f);
		Modulate = _playerColor;
	}
}
