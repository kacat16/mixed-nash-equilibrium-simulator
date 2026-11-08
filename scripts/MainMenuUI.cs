using Godot;

public partial class MainMenuUI : Control
{
	private Button _startButton;
	private Button _infoButton;
	private static string logPath = "user://debug_log.txt";

	private static void LogToFile(string message)
	{
		try
		{
			string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
			string logMessage = $"[{timestamp}] MainMenuUI: {message}\n";
			
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

	public override void _Ready()
	{
		LogToFile("MainMenuUI _Ready started");
		GD.Print("MainMenuUI _Ready started");
		
		try
		{
			LogToFile("Looking for StartButton using GetNode");
			// Pokušaj sa različitim putanjama
			try
			{
				_startButton = GetNode<Button>("%StartButton");
				LogToFile("Found StartButton with %StartButton");
			}
			catch
			{
				LogToFile("Failed to find StartButton with %StartButton, trying alternative path");
				_startButton = GetNode<Button>("MarginContainer/VBoxContainer/StartButton");
				LogToFile("Found StartButton with MarginContainer/VBoxContainer/StartButton");
			}
			
			LogToFile("Looking for InfoButton using GetNode");
			try
			{
				_infoButton = GetNode<Button>("%InfoButton");
				LogToFile("Found InfoButton with %InfoButton");
			}
			catch
			{
				LogToFile("Failed to find InfoButton with %InfoButton, trying alternative path");
				_infoButton = GetNode<Button>("MarginContainer/VBoxContainer/InfoButton");
				LogToFile("Found InfoButton with MarginContainer/VBoxContainer/InfoButton");
			}

			if (_startButton == null || _infoButton == null)
			{
				LogToFile($"ERROR: Buttons still null! StartButton: {_startButton != null}, InfoButton: {_infoButton != null}");
				GD.PrintErr("Buttons not found!");
				return;
			}

			LogToFile("Connecting signals to buttons");
			_startButton.Pressed += OnStartPressed;
			_infoButton.Pressed += OnInfoPressed;
			
			LogToFile("Checking GameManager instance");
			if (GameManager.Instance == null)
			{
				LogToFile("WARNING: GameManager.Instance is NULL!");
				GD.PrintErr("GameManager.Instance is NULL!");
			}
			else
			{
				LogToFile($"GameManager.Instance found: {GameManager.Instance}");
			}
			
			LogToFile("MainMenuUI initialization completed successfully");
			GD.Print("MainMenuUI buttons connected successfully");
		}
		catch (System.Exception e)
		{
			string errMsg = $"Error in MainMenuUI._Ready: {e.Message}\n{e.StackTrace}";
			LogToFile(errMsg);
			GD.PrintErr(errMsg);
		}
	}

	private void OnStartPressed()
	{
		LogToFile("Start button pressed");
		GD.Print("Start button pressed");
		try
		{
			GameManager.Instance.LoadScene("SimulationSetup");
		}
		catch (System.Exception e)
		{
			string errMsg = $"Error loading SimulationSetup: {e.Message}\n{e.StackTrace}";
			LogToFile(errMsg);
			GD.PrintErr(errMsg);
		}
	}

	private void OnInfoPressed()
	{
		LogToFile("Info button pressed");
		GD.Print("Info button pressed");
		try
		{
			GameManager.Instance.LoadScene("AboutScreen");
		}
		catch (System.Exception e)
		{
			string errMsg = $"Error loading AboutScreen: {e.Message}\n{e.StackTrace}";
			LogToFile(errMsg);
			GD.PrintErr(errMsg);
		}
	}
}
