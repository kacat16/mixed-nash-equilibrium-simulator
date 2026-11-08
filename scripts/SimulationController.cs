using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SimulationController : Node2D
{
	private List<GamePlayer> _players = new List<GamePlayer>();
	private List<Dictionary<string, object>> _playerTypes = new List<Dictionary<string, object>>();
	private List<float[,]> _payoffMatrices = new List<float[,]>();
	private bool _isRunning = false;
	private float _generationTime = 0.0f;
	private float _generationInterval = 2.0f;
	private int _currentGeneration = 0;
	private int _maxGenerations = 100;

	private List<Dictionary<string, object>> _populationHistory = new List<Dictionary<string, object>>();
	private List<GamePlayer> _playersToRemove = new List<GamePlayer>();
	private List<GamePlayer> _playersToAdd = new List<GamePlayer>();

	private Label _generationLabel;
	private Label _populationLabel;

	public override void _Ready()
	{
		_generationLabel = GetNode<Label>("CanvasLayer/StatsPanel/StatsVBox/GenerationLabel");
		_populationLabel = GetNode<Label>("CanvasLayer/StatsPanel/StatsVBox/PopulationLabel");
		InitializeSimulation();
	}

	public override void _Process(double delta)
	{
		if (!_isRunning) return;
		_generationTime += (float)delta;
		if (_generationTime >= _generationInterval)
		{
			_generationTime = 0.0f;
			_currentGeneration++;
			if (_currentGeneration >= _maxGenerations)
				EndSimulation();
			else
				ExecuteGeneration();
		}
	}

	private void InitializeSimulation()
	{
		var config = GameManager.Instance.GetGameConfig();
		if (config.Count == 0)
		{
			GD.PrintErr("Nema konfiguracije igre!");
			return;
		}
		_playerTypes = (List<Dictionary<string, object>>)config["player_types"];
		_payoffMatrices = (List<float[,]>)config["payoff_matrices"];
		CreateInitialPopulation();
		_isRunning = true;
		GD.Print("Simulacija pokrenuta");
	}

	private void CreateInitialPopulation()
	{
		var playerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
		var colors = new Color[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Magenta };
		int playerIndex = 0;
		int playersPerType = 10;

		for (int typeIdx = 0; typeIdx < _playerTypes.Count; typeIdx++)
		{
			for (int i = 0; i < playersPerType; i++)
			{
				var player = playerScene.Instantiate<GamePlayer>();
				player.PlayerTypeIndex = typeIdx;
				player.PlayerIndex = playerIndex;
				player.SetPlayerColor(colors[typeIdx % colors.Length]);
				player.SetStrategy(GD.Randf() < 0.5f ? 0 : 1);
				player.Position = new Vector2(GD.Randf() * 800 + 100, GD.Randf() * 600 + 100);
				AddChild(player);
				_players.Add(player);
				playerIndex++;
			}
		}
		GD.Print($"Pocetna populacija: {_players.Count} igraca");
	}

	private void ExecuteGeneration()
	{
		PlayAllInteractions();
		var payoffByType = CalculatePayoffsByType();
		RecordPopulationHistory();
		EvolvePopulation(payoffByType);
		foreach (var player in _players)
			player.ResetPayoff();
		UpdateStatistics();
		GD.Print($"Generacija {_currentGeneration}");
	}

	private void PlayAllInteractions()
	{
		for (int i = 0; i < _players.Count; i++)
		{
			for (int j = i + 1; j < _players.Count; j++)
			{
				if (_players[i].PlayerTypeIndex != _players[j].PlayerTypeIndex)
					PlayMatch(_players[i], _players[j]);
			}
		}
	}

	private void PlayMatch(GamePlayer p1, GamePlayer p2)
	{
		var payoffs = CalculatePayoff(p1.Strategy, p2.Strategy, p1.PlayerTypeIndex, p2.PlayerTypeIndex);
		p1.AccumulatedPayoff += payoffs["player1"];
		p2.AccumulatedPayoff += payoffs["player2"];
		p1.UpdateVisualColor();
		p2.UpdateVisualColor();
	}

	private Dictionary<string, float> CalculatePayoff(int s1, int s2, int t1, int t2)
	{
		int matrixIdx = 0;
		for (int i = 0; i < _playerTypes.Count; i++)
		{
			for (int j = i + 1; j < _playerTypes.Count; j++)
			{
				if ((t1 == i && t2 == j) || (t1 == j && t2 == i))
				{
					if (_payoffMatrices.Count > matrixIdx && _payoffMatrices[matrixIdx] != null)
					{
						return new Dictionary<string, float>
						{
							["player1"] = _payoffMatrices[matrixIdx][s1, s2],
							["player2"] = _payoffMatrices[matrixIdx][s2, s1]
						};
					}
					break;
				}
				matrixIdx++;
			}
		}
		return new Dictionary<string, float> { ["player1"] = 0, ["player2"] = 0 };
	}

	private Dictionary<int, float> CalculatePayoffsByType()
	{
		var payoffs = new Dictionary<int, float>();
		foreach (var type in _playerTypes)
		{
			int typeIdx = (int)type["index"];
			var playersOfType = _players.Where(p => p.PlayerTypeIndex == typeIdx).ToList();
			float total = playersOfType.Sum(p => p.GetAccumulatedPayoff());
			payoffs[typeIdx] = playersOfType.Count > 0 ? total / playersOfType.Count : 0;
		}
		return payoffs;
	}

	private void RecordPopulationHistory()
	{
		var freq = GetPopulationFrequencies();
		freq["generation"] = _currentGeneration;
		_populationHistory.Add(freq);
	}

	private Dictionary<string, object> GetPopulationFrequencies()
	{
		var freq = new Dictionary<string, object>();
		foreach (var type in _playerTypes)
		{
			int typeIdx = (int)type["index"];
			string typeName = (string)type["name"];
			int count = _players.Count(p => p.PlayerTypeIndex == typeIdx);
			freq[typeName] = _players.Count > 0 ? (float)count / _players.Count : 0f;
		}
		return freq;
	}

	private void EvolvePopulation(Dictionary<int, float> payoffByType)
	{
		_playersToRemove.Clear();
		_playersToAdd.Clear();
		var worst = payoffByType.OrderBy(x => x.Value).First().Key;
		var best = payoffByType.OrderByDescending(x => x.Value).First().Key;
		if (worst == best) return;

		var worstPlayers = _players.Where(p => p.PlayerTypeIndex == worst).ToList();
		var bestPlayers = _players.Where(p => p.PlayerTypeIndex == best).ToList();
		if (worstPlayers.Count == 0 || bestPlayers.Count == 0) return;

		_playersToRemove.Add(worstPlayers[(int)(GD.Randi() % (uint)worstPlayers.Count)]);

		var bp = bestPlayers[(int)(GD.Randi() % (uint)bestPlayers.Count)];
		var clone = ClonePlayer(bp);
		_playersToAdd.Add(clone);

		foreach (var p in _playersToRemove) { _players.Remove(p); p.QueueFree(); }
		foreach (var p in _playersToAdd) { _players.Add(p); }
	}

	private GamePlayer ClonePlayer(GamePlayer orig)
	{
		var scene = GD.Load<PackedScene>("res://scenes/Player.tscn");
		var clone = scene.Instantiate<GamePlayer>();
		clone.PlayerTypeIndex = orig.PlayerTypeIndex;
		clone.PlayerIndex = _players.Count;
		clone.Strategy = orig.Strategy;
		clone.SetPlayerColor(orig.Modulate);
		clone.Position = new Vector2(GD.Randf() * 800 + 100, GD.Randf() * 600 + 100);
		AddChild(clone);
		return clone;
	}

	private void EndSimulation()
	{
		_isRunning = false;
		GD.Print("Simulacija zavrsena");
		var freq = GetPopulationFrequencies();
		foreach (var item in freq)
			GD.Print($"{item.Key}: {((float)item.Value * 100):F1}%");
	}

	private void UpdateStatistics()
	{
		_generationLabel.Text = $"Generacija: {_currentGeneration}";
		_populationLabel.Text = $"Populacija: {_players.Count}";
		var freq = GetPopulationFrequencies();
		foreach (var item in freq)
			_populationLabel.Text += $"\n{item.Key}: {((float)item.Value * 100):F1}%";
	}
}
