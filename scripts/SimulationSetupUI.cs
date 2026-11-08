using Godot;
using System.Collections.Generic;

public partial class SimulationSetupUI : Control
{
	private SpinBox _numPlayersSpinBox;
	private Button _confirmPlayersButton;
	private VBoxContainer _playersContainer;
	private Button _startButton;
	private Button _backButton;

	private int _numPlayerTypes = 2;
	private List<Dictionary<string, object>> _playerConfigs = new List<Dictionary<string, object>>();
	private List<Dictionary<string, object>> _playerTypes = new List<Dictionary<string, object>>();

	public override void _Ready()
	{
		GD.Print("SimulationSetupUI _Ready started");
		try
		{
			_numPlayersSpinBox = GetNode<SpinBox>("%NumPlayersSpinBox");
			_confirmPlayersButton = GetNode<Button>("%ConfirmPlayersButton");
			_playersContainer = GetNode<VBoxContainer>("%PlayersContainer");
			_startButton = GetNode<Button>("%StartSimulationButton");
			_backButton = GetNode<Button>("%BackButton");

			_confirmPlayersButton.Pressed += OnConfirmPlayers;
			_startButton.Pressed += OnStartSimulation;
			_backButton.Pressed += OnBackPressed;
			_numPlayersSpinBox.ValueChanged += OnNumPlayersChanged;
			GD.Print("SimulationSetupUI initialized");
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error in SimulationSetupUI._Ready: {e.Message}\n{e.StackTrace}");
		}
	}

	private void OnNumPlayersChanged(double value) => _numPlayerTypes = (int)value;

	private void OnConfirmPlayers()
	{
		foreach (Node child in _playersContainer.GetChildren())
			child.QueueFree();

		_playerConfigs.Clear();
		_playerTypes.Clear();

		for (int i = 0; i < _numPlayerTypes; i++)
			CreatePlayerConfigUI(i);

		_startButton.Disabled = false;
	}

	private void CreatePlayerConfigUI(int index)
	{
		var panel = new PanelContainer();
		var styleBox = new StyleBoxFlat();
		styleBox.BgColor = new Color(0.15f, 0.15f, 0.2f, 1.0f);
		panel.AddThemeStyleboxOverride("panel", styleBox);

		var vBox = new VBoxContainer();

		var title = new Label();
		title.Text = $"Tip igraca {index + 1}";
		title.AddThemeFontSizeOverride("font_size", 20);
		title.AddThemeColorOverride("font_color", new Color(0.2f, 0.8f, 1.0f, 1.0f));
		vBox.AddChild(title);

		var nameLabel = new Label();
		nameLabel.Text = "Naziv tipa:";
		nameLabel.AddThemeFontSizeOverride("font_size", 14);
		vBox.AddChild(nameLabel);

		var nameInput = new LineEdit();
		nameInput.Text = $"Tip{index + 1}";
		nameInput.CustomMinimumSize = new Vector2(0, 30);
		vBox.AddChild(nameInput);

		var strategiesLabel = new Label();
		strategiesLabel.Text = "Broj strategija:";
		strategiesLabel.AddThemeFontSizeOverride("font_size", 14);
		vBox.AddChild(strategiesLabel);

		var strategiesSpin = new SpinBox();
		strategiesSpin.MinValue = 2;
		strategiesSpin.MaxValue = 4;
		strategiesSpin.Value = 2;
		strategiesSpin.CustomMinimumSize = new Vector2(100, 30);
		vBox.AddChild(strategiesSpin);

		panel.AddChild(vBox);
		_playersContainer.AddChild(panel);

		_playerConfigs.Add(new Dictionary<string, object>
		{
			["name_input"] = nameInput,
			["strategies_spin"] = strategiesSpin,
			["index"] = index
		});
	}

	private void OnStartSimulation()
	{
		_playerTypes.Clear();

		foreach (var config in _playerConfigs)
		{
			var nameInput = (LineEdit)config["name_input"];
			var strategiesSpin = (SpinBox)config["strategies_spin"];
			var index = (int)config["index"];

			_playerTypes.Add(new Dictionary<string, object>
			{
				["name"] = nameInput.Text,
				["num_strategies"] = (int)strategiesSpin.Value,
				["index"] = index
			});
		}

		var gameConfig = new Dictionary<string, object>
		{
			["player_types"] = _playerTypes,
			["payoff_matrices"] = new List<float[,]>()
		};

		GameManager.Instance.SetGameConfig(gameConfig);
		GameManager.Instance.LoadScene("PayoffMatrixSetup");
	}

	private void OnBackPressed()
	{
		GameManager.Instance.LoadScene("MainMenu");
	}
}
