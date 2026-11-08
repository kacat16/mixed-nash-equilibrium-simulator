using Godot;
using System;
using System.Collections.Generic;
using FileAccess = Godot.FileAccess;

public partial class GameManager : Node
{
	[Signal]
	public delegate void SceneChangedEventHandler(string sceneName);

	public static GameManager Instance { get; private set; }

	public Dictionary<string, object> CurrentGameConfig { get; set; } = new Dictionary<string, object>();
	public Node CurrentScene { get; set; }

	private static string logPath = "user://debug_log.txt";

	private static void LogToFile(string message)
	{
		try
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			string logMessage = $"[{timestamp}] {message}\n";
			
			if (!FileAccess.FileExists(logPath))
			{
				FileAccess.Open(logPath, FileAccess.ModeFlags.Write).Close();
			}
			
			var file = FileAccess.Open(logPath, FileAccess.ModeFlags.ReadWrite);
			if (file != null)
			{
				file.SeekEnd();
				file.StoreString(logMessage);
			}
		}
		catch { }
	}

	public override void _EnterTree()
	{
		LogToFile("GameManager _EnterTree - Initializing singleton");
		GD.Print("GameManager _EnterTree - Initializing singleton");
		if (Instance != null)
		{
			LogToFile("GameManager already exists, freeing duplicate");
			GD.Print("GameManager already exists, freeing duplicate");
			QueueFree();
			return;
		}
		Instance = this;
		LogToFile("GameManager singleton initialized");
		GD.Print("GameManager singleton initialized");
	}

	public override void _Ready()
	{
		LogToFile("GameManager _Ready - Singleton initialized");
		GD.Print("GameManager _Ready - Singleton initialized");
		// MainMenu će biti učitana kao main_scene, GameManager je samo router
	}

	public void LoadScene(string sceneName)
	{
		LogToFile($"Loading scene: {sceneName}");
		GD.Print($"Loading scene: {sceneName}");
		
		string scenePath = $"res://scenes/{sceneName}.tscn";
		LogToFile($"Scene path: {scenePath}");
		GD.Print($"Scene path: {scenePath}");
		
		try
		{
			var packedScene = GD.Load<PackedScene>(scenePath);
			if (packedScene == null)
			{
				string errMsg = $"Failed to load scene: {scenePath}";
				LogToFile(errMsg);
				GD.PrintErr(errMsg);
				return;
			}
			
			LogToFile($"Instantiating scene {sceneName}");
			Node scene = packedScene.Instantiate();
			
			// Zameni trenutnu scenu u tree-u
			var root = GetTree().Root;
			if (CurrentScene != null && CurrentScene.IsInsideTree())
			{
				LogToFile($"Removing current scene: {CurrentScene.Name}");
				root.RemoveChild(CurrentScene);
				CurrentScene.QueueFree();
			}
			
			LogToFile($"Adding scene {sceneName} to tree root");
			root.AddChild(scene);
			CurrentScene = scene;

			LogToFile($"Emitting signal for {sceneName}");
			EmitSignal(SignalName.SceneChanged, sceneName);
			LogToFile($"Scene {sceneName} loaded successfully");
			GD.Print($"Scene {sceneName} loaded successfully");
		}
		catch (System.Exception e)
		{
			string errMsg = $"Error loading scene {scenePath}: {e.Message}\n{e.StackTrace}";
			LogToFile(errMsg);
			GD.PrintErr(errMsg);
		}
	}

	public void SetGameConfig(Dictionary<string, object> config)
	{
		CurrentGameConfig = config;
		GD.Print("Game config postavljen");
	}

	public Dictionary<string, object> GetGameConfig()
	{
		return CurrentGameConfig;
	}
}
