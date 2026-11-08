using Godot;

public partial class AboutScreenUI : Control
{
	private Button _backButton;

	public override void _Ready()
	{
		_backButton = GetNode<Button>("%BackButton");
		_backButton.Pressed += OnBackPressed;
	}

	private void OnBackPressed()
	{
		GameManager.Instance.LoadScene("MainMenu");
	}
}
