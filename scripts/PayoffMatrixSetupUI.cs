using Godot;
using System;
using System.Collections.Generic;

public partial class PayoffMatrixSetupUI : Control
{
	private List<Dictionary<string, object>> _playerTypes = new List<Dictionary<string, object>>();
	private List<float[,]> _payoffMatrices = new List<float[,]>();
	private int _currentPairIndex = 0;
	private VBoxContainer _matrixInputContainer;
	private Label _titleLabel;
	private Button _prevButton;
	private Button _nextButton;
	private Button _confirmButton;
	private Button _backButton;

	private LineEdit[,] _currentInputFields;

	public override void _Ready()
	{
		var config = GameManager.Instance.GetGameConfig();
		_playerTypes = (List<Dictionary<string, object>>)config["player_types"];

		_titleLabel = GetNode<Label>("%TitleLabel");
		_matrixInputContainer = GetNode<VBoxContainer>("%MatrixInputContainer");
		_prevButton = GetNode<Button>("%PrevButton");
		_nextButton = GetNode<Button>("%NextButton");
		_confirmButton = GetNode<Button>("%ConfirmButton");
		_backButton = GetNode<Button>("%BackButton");

		_prevButton.Pressed += OnPrevious;
		_nextButton.Pressed += OnNext;
		_confirmButton.Pressed += OnConfirm;
		_backButton.Pressed += OnBack;

		var pairs = GetPlayerTypePairs();
		for (int i = 0; i < pairs.Count; i++)
			_payoffMatrices.Add(new float[2, 2]);

		DisplayMatrixPair(0);
	}

	private void DisplayMatrixPair(int index)
	{
		_currentPairIndex = index;

		foreach (Node child in _matrixInputContainer.GetChildren())
			child.QueueFree();

		var pairs = GetPlayerTypePairs();
		if (index >= pairs.Count) return;

		var (type1Idx, type2Idx) = pairs[index];
		var type1 = (string)_playerTypes[type1Idx]["name"];
		var type2 = (string)_playerTypes[type2Idx]["name"];

		_titleLabel.Text = $"Payoff matrica: {type1} vs {type2}";

		_currentInputFields = new LineEdit[2, 2];

		var headerHBox = new HBoxContainer();
		headerHBox.AddChild(new Label { Text = " " });
		headerHBox.AddChild(new Label { Text = $"{type2} Str 0" });
		headerHBox.AddChild(new Label { Text = $"{type2} Str 1" });
		_matrixInputContainer.AddChild(headerHBox);

		for (int i = 0; i < 2; i++)
		{
			var rowHBox = new HBoxContainer();
			rowHBox.AddChild(new Label { Text = $"{type1} Str {i}:" });

			for (int j = 0; j < 2; j++)
			{
				var field = new LineEdit();
				field.Text = _payoffMatrices[index][i, j].ToString();
				field.CustomMinimumSize = new Vector2(80, 30);
				field.Alignment = HorizontalAlignment.Center;
				_currentInputFields[i, j] = field;
				rowHBox.AddChild(field);
			}
			_matrixInputContainer.AddChild(rowHBox);
		}

		var descLabel = new Label();
		descLabel.Text = "Unesi payoff vrednosti (npr: 3)";
		descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1.0f));
		_matrixInputContainer.AddChild(descLabel);

		_prevButton.Disabled = index == 0;
		_nextButton.Disabled = index >= pairs.Count - 1;
	}

	private void OnPrevious()
	{
		SaveCurrentMatrix();
		if (_currentPairIndex > 0)
			DisplayMatrixPair(_currentPairIndex - 1);
	}

	private void OnNext()
	{
		SaveCurrentMatrix();
		var pairs = GetPlayerTypePairs();
		if (_currentPairIndex < pairs.Count - 1)
			DisplayMatrixPair(_currentPairIndex + 1);
	}

	private void OnConfirm()
	{
		SaveCurrentMatrix();
		var config = GameManager.Instance.GetGameConfig();
		config["payoff_matrices"] = _payoffMatrices;
		GameManager.Instance.SetGameConfig(config);
		GameManager.Instance.LoadScene("SimulationScene");
	}

	private void OnBack()
	{
		GameManager.Instance.LoadScene("SimulationSetup");
	}

	private void SaveCurrentMatrix()
	{
		if (_currentInputFields == null) return;
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 2; j++)
				if (float.TryParse(_currentInputFields[i, j].Text, out float val))
					_payoffMatrices[_currentPairIndex][i, j] = val;
	}

	private List<(int, int)> GetPlayerTypePairs()
	{
		var pairs = new List<(int, int)>();
		for (int i = 0; i < _playerTypes.Count; i++)
			for (int j = i + 1; j < _playerTypes.Count; j++)
				pairs.Add((i, j));
		return pairs;
	}
}
